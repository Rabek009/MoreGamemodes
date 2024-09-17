using HarmonyLib;
using Hazel;
using UnityEngine;
using AmongUs.GameOptions;
using InnerNet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace MoreGamemodes
{
    class ExileControllerWrapUpPatch
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        static void WrapUpPostfix(NetworkedPlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CustomGamemode.Instance.OnExile(exiled);

            if (exiled == null || exiled.Object == null) return;
            if (exiled.Object.GetDeathReason() == DeathReasons.Alive)
                exiled.Object.RpcSetDeathReason(DeathReasons.Exiled);
        }
    }

    [HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
    class PolusExileHatFixPatch
    {
        public static void Prefix(PbExileController __instance)
        {
            __instance.Player.cosmetics.hat.transform.localPosition = new(-0.2f, 0.6f, 1.1f);
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    class ReEnableGameplayPatch
    {
        public static void Postfix()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Main.KillCooldowns[pc.PlayerId] = Main.OptionKillCooldowns[pc.PlayerId];
                if (pc.AmOwner || Main.IsModded[pc.PlayerId]) continue;
                pc.RpcReactorFlash(0.4f, Color.red);
                new LateTask(() => pc.RpcReactorFlash(0.4f, Color.red), 0.9f);
                new LateTask(() => {
                    pc.NetTransform.SnapTo(pc.transform.position, (ushort)(pc.NetTransform.lastSequenceId + 328));
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(pc.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None);
                    NetHelpers.WriteVector2(pc.transform.position, writer);
                    writer.Write((ushort)(pc.NetTransform.lastSequenceId + 8));
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }, 0.2f);
                CustomRpcSender sender = CustomRpcSender.Create("AntiBlackout", SendOption.None);
                MessageWriter writer = sender.stream;
                sender.StartMessage(pc.GetClientId());
                PlayerControl.LocalPlayer.NetTransform.lastSequenceId += 328;
                sender.StartRpc(PlayerControl.LocalPlayer.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                    .WriteVector2(Utils.GetOutsideMapPosition())
                    .Write((ushort)(PlayerControl.LocalPlayer.NetTransform.lastSequenceId + 8))
                    .EndRpc();
                var role = Main.DesyncRoles.ContainsKey((PlayerControl.LocalPlayer.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(PlayerControl.LocalPlayer.PlayerId, pc.PlayerId)] : Main.StandardRoles[PlayerControl.LocalPlayer.PlayerId];
                if (CustomGamemode.Instance.Gamemode is Gamemodes.BombTag or Gamemodes.BattleRoyale or Gamemodes.KillOrDie or Gamemodes.Jailbreak or Gamemodes.BaseWars)
                    role = RoleTypes.Crewmate;
                if (role == RoleTypes.Noisemaker && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Crewmate)
                        .Write(true)
                        .EndRpc();
                }
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.NetId);
                    writer.Write(pc.PlayerId);
                }
                writer.EndMessage();
                sender.StartRpc(pc.NetId, (byte)RpcCalls.MurderPlayer)
                    .WriteNetObject(PlayerControl.LocalPlayer)
                    .Write((int)MurderResultFlags.Succeeded)
                    .EndRpc();
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.NetId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                }
                writer.EndMessage();
                sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)role)
                    .Write(true)
                    .EndRpc();
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)PlayerControl.LocalPlayer.Data.Role.Role)
                        .Write(true)
                        .EndRpc();
                }
                PlayerControl.LocalPlayer.NetTransform.lastSequenceId += 328;
                sender.StartRpc(PlayerControl.LocalPlayer.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                    .WriteVector2(PlayerControl.LocalPlayer.transform.position)
                    .Write((ushort)(PlayerControl.LocalPlayer.NetTransform.lastSequenceId + 8))
                    .EndRpc();
                sender.StartRpc(pc.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)(!pc.Data.IsDead ? pc.GetSelfRole() : pc.Data.Role.Role))
                    .Write(true)
                    .EndRpc();
                if (pc.GetSelfRole().IsImpostor() && !pc.Data.IsDead)
                {
                    var opt = pc.BuildGameOptions(Main.KillCooldowns[pc.PlayerId] * 2f);
                    Il2CppStructArray<byte> byteArray = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt, false);
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(GameManager.Instance.NetId);
                        writer.StartMessage(GameManager.Instance.TryCast<NormalGameManager>() ? (byte)4 : (byte)5);
				        writer.WriteBytesAndSize(byteArray);
				        writer.EndMessage();
                    }
                    writer.EndMessage();
                    sender.StartRpc(pc.NetId, (byte)RpcCalls.MurderPlayer)
                        .WriteNetObject(pc)
                        .Write((int)MurderResultFlags.FailedProtected)
                        .EndRpc();
                    var opt2 = pc.BuildGameOptions();
                    Il2CppStructArray<byte> byteArray2 = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt2, false);
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(GameManager.Instance.NetId);
                        writer.StartMessage(GameManager.Instance.TryCast<NormalGameManager>() ? (byte)4 : (byte)5);
				        writer.WriteBytesAndSize(byteArray2);
				        writer.EndMessage();
                    }
                    writer.EndMessage();
                }
                sender.EndMessage();
                sender.SendMessage();
            }
        }
    }
}