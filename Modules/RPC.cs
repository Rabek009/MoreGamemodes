using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using System;
using System.Linq;
using AmongUs.GameOptions;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    public enum CustomRPC
    {
        VersionCheck = 70,
        SyncCustomOptions,
        SyncGamemode,
        ReactorFlash,
        SetKillTimer,
        PetAction,
        StartGamemode,
        RemoveDeadBody,
        RequestVersionCheck,
        AddCustomSettingsChangeMessage,
        SetStandardName,
        SendNoisemakerAlert,
        SetCustomRole,
        SetRoleblock,
        SyncCustomWinner,
        SetAbilityUses,
        SetPetAbilityCooldown,
        GuessPlayer,
        SetAddOn,
        SyncCustomRole,
        MakeInvisible,
        MakeVisible,
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
    class ShipStatusHandleRpc
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.UpdateSystem:
                    SystemTypes system = (SystemTypes)subReader.ReadByte();
                    PlayerControl player = subReader.ReadNetObject<PlayerControl>();
                    byte amount = subReader.ReadByte();
                    if (AntiCheat.RpcUpdateSystemCheck(player, system, amount, subReader)) return false;
                    return true;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class PlayerControlHandleRpc
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            if (!AmongUsClient.Instance.AmHost) return true;
            if (AntiCheat.PlayerControlReceiveRpc(__instance, callId, reader)) return false;
            switch (rpcType)
            {
                case RpcCalls.SendChat:
                    var text = subReader.ReadString();
                    if (!SendChatPatch.OnReceiveChat(__instance, text)) return false;
                    if (text[0] == '/' && Main.IsModded[__instance.PlayerId]) return false;
                    break;
                case RpcCalls.UsePlatform:
                    if (Options.DisableGapPlatform.GetBool()) return false;
                    if (!CustomGamemode.Instance.OnUsePlatform(__instance)) return false;
                    break;
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId == 13 && !AmongUsClient.Instance.AmHost)
                ChatUpdatePatch.SendingSystemMessage = false;
            if (callId < 70) return;
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.VersionCheck:
                    if (!AmongUsClient.Instance.AmHost) break;
                    if (reader.ReadString() != Main.CurrentVersion)
                    {
                        Utils.SendChat(__instance.Data.PlayerName + " was kicked for having other version of More Gamemodes.", "AutoKick");
                        AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
                    }
                    else
                    {
                        Main.IsModded[__instance.PlayerId] = true;
                        bool doSend = false;
                        CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                        sender.StartMessage(__instance.GetClientId());
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!Main.StandardNames.ContainsKey(pc.PlayerId)) continue;
                            sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetStandardName)
                                .WriteNetObject(pc)
                                .Write(Main.StandardNames[pc.PlayerId])
                                .EndRpc();
                            doSend = true;
                        }
                        sender.EndMessage();
                        sender.SendMessage(doSend);
                    }
                    break;
                case CustomRPC.SyncGamemode:
                    __instance.SyncGamemode(reader);
                    break;
                case CustomRPC.ReactorFlash:
                    if (!__instance.AmOwner) break;
                    HudManager.Instance.ReactorFlash(reader.ReadSingle(), reader.ReadColor());
                    break;
                case CustomRPC.SetKillTimer:
                    if (!__instance.AmOwner) break;
                    __instance.SetKillTimer(reader.ReadSingle());
                    break;
                case CustomRPC.PetAction:
                    if (!AmongUsClient.Instance.AmHost) return;
                    ExternalRpcPetPatch.Prefix(__instance.MyPhysics, (byte)CustomRPC.PetAction, new MessageReader());
                    break;
                case CustomRPC.RemoveDeadBody:
                    NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
                    __instance.RemoveDeadBody(playerById);
                    break;
                case CustomRPC.RequestVersionCheck:
                    if (__instance.GetClientId() != AmongUsClient.Instance.HostId) break;
                    PlayerControl.LocalPlayer.RpcVersionCheck(Main.CurrentVersion);
                    break;
                case CustomRPC.SetStandardName:
                    PlayerControl player = reader.ReadNetObject<PlayerControl>();
                    player.SetStandardName(reader.ReadString());
                    break;
                case CustomRPC.SendNoisemakerAlert:
                    __instance.SendNoisemakerAlert();
                    break;
                case CustomRPC.SetCustomRole:
                    __instance.SetCustomRole((CustomRoles)reader.ReadInt32());
                    break;
                case CustomRPC.SetRoleblock:
                    __instance.SetRoleblock(reader.ReadBoolean());
                    break;
                case CustomRPC.SetAbilityUses:
                    __instance.SetAbilityUses(reader.ReadSingle());
                    break;
                case CustomRPC.SetPetAbilityCooldown:
                    __instance.SetPetAbilityCooldown(reader.ReadBoolean());
                    break;
                case CustomRPC.GuessPlayer:
                    __instance.GuessPlayer();
                    break;
                case CustomRPC.SetAddOn:
                    __instance.SetAddOn((AddOns)reader.ReadInt32());
                    break;
                case CustomRPC.SyncCustomRole:
                    __instance.SyncCustomRole(reader);
                    break;
                case CustomRPC.MakeInvisible:
                    __instance.MakeInvisible();
                    Main.IsInvisible[__instance.PlayerId] = true;
                    break;
                case CustomRPC.MakeVisible:
                    __instance.MakeVisible();
                    Main.IsInvisible[__instance.PlayerId] = false;
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.HandleRpc))]
    class GameManagerHandleRpc
    {
        public static void Postfix(GameManager __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId < 70) return;
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.SyncCustomOptions:
                    foreach (var co in OptionItem.AllOptions)
                    {
                        if (co is TextOptionItem) continue;
                        if (co.Id == 0)
                        {
                            co.SetValue(9);
                            continue;
                        }
                        if (co.Id >= 1000 && co.IsHiddenOn(Options.CurrentGamemode)) continue;
                        co.SetValue(reader.ReadInt32());
                    }
                    var viewSettingsPane = Object.FindObjectOfType<LobbyViewSettingsPane>();
                    if (viewSettingsPane != null)
                    {
                        if (viewSettingsPane.currentTab != StringNames.OverviewCategory && viewSettingsPane.currentTab != StringNames.RolesCategory)
                            viewSettingsPane.RefreshTab();
                        LobbyViewPatch.ReCreateButtons(viewSettingsPane);
                    }
                    break;
                case CustomRPC.SyncGamemode:
                    __instance.SyncGamemode(reader);
                    break;
                case CustomRPC.StartGamemode:
                    __instance.StartGamemode((Gamemodes)reader.ReadInt32());
                    break;
                case CustomRPC.AddCustomSettingsChangeMessage:
                    int optionId = reader.ReadInt32();
                    var optionItem = OptionItem.AllOptions.FirstOrDefault(opt => opt.Id == optionId);
                    if (optionItem == null) break;
                    __instance.AddCustomSettingsChangeMessage(optionItem, reader.ReadString(), reader.ReadBoolean());
                    break;
                case CustomRPC.SyncCustomWinner:
                    if (ClassicGamemode.instance == null) break;
                    ClassicGamemode.instance.Winner = (CustomWinners)reader.ReadInt32();
                    ClassicGamemode.instance.AdditionalWinners.Clear();
                    int num = reader.ReadInt32();
                    for (int i = 0; i < num; ++i)
                        ClassicGamemode.instance.AdditionalWinners.Add((AdditionalWinners)reader.ReadInt32());
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
    class CustomNetworkTransformHandleRpc
    {
        public static bool Prefix(CustomNetworkTransform __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            if (AntiCheat.CustomNetworkTransformReceiveRpc(__instance, callId, reader)) return false;
            switch (rpcType)
            {
                case RpcCalls.SnapTo:
                    if (CoEnterVentPatch.PlayersToKick.Contains(__instance.myPlayer.PlayerId)) return false;
                    break;
            }
            return true;
        }
    }

    static class RPC
    {
        public static void SyncGamemode(this PlayerControl player, MessageReader reader)
        {
            if (CustomGamemode.Instance == null) return;
            CustomGamemode.Instance.ReceiveRPC(player, reader);
        }

        public static void SyncGamemode(this GameManager manager, MessageReader reader)
        {
            if (CustomGamemode.Instance == null) return;
            CustomGamemode.Instance.ReceiveRPC(manager, reader);
        }

        public static void ReactorFlash(this HudManager hud, float duration, Color color)
        {
            if (hud.FullScreen == null) return;
            var obj = hud.transform.FindChild("FlashColor_FullScreen")?.gameObject;
            if (obj == null)
            {
                obj = Object.Instantiate(hud.FullScreen.gameObject, hud.transform);
                obj.name = "FlashColor_FullScreen";
            }
            hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((t) =>
            {
                obj.SetActive(t != 1f);
                obj.GetComponent<SpriteRenderer>().color = new(color.r, color.g, color.b, Mathf.Clamp01((-2f * Mathf.Abs(t - 0.5f) + 1) * color.a));
            })));
        }

        public static void StartGamemode(this GameManager manager, Gamemodes gamemode)
        {
            CustomGamemode.Instance = null;
            ClassicGamemode.instance = null;
            UnmoddedGamemode.instance = null;
            HideAndSeekGamemode.instance = null;
            ShiftAndSeekGamemode.instance = null;
            BombTagGamemode.instance = null;
            RandomItemsGamemode.instance = null;
            BattleRoyaleGamemode.instance = null;
            SpeedrunGamemode.instance = null;
            PaintBattleGamemode.instance = null;
            KillOrDieGamemode.instance = null;
            ZombiesGamemode.instance = null;
            JailbreakGamemode.instance = null;
            DeathrunGamemode.instance = null;
            BaseWarsGamemode.instance = null;
            FreezeTagGamemode.instance = null;
            ColorWarsGamemode.instance = null;
            switch (gamemode)
            {
                case Gamemodes.Classic:
                    ClassicGamemode.instance = new ClassicGamemode();
                    CustomGamemode.Instance = ClassicGamemode.instance;
                    break;
                case Gamemodes.HideAndSeek:
                    HideAndSeekGamemode.instance = new HideAndSeekGamemode();
                    CustomGamemode.Instance = HideAndSeekGamemode.instance;
                    break;
                case Gamemodes.ShiftAndSeek:
                    ShiftAndSeekGamemode.instance = new ShiftAndSeekGamemode();
                    CustomGamemode.Instance = ShiftAndSeekGamemode.instance;
                    break;
                case Gamemodes.BombTag:
                    BombTagGamemode.instance = new BombTagGamemode();
                    CustomGamemode.Instance = BombTagGamemode.instance;
                    break;
                case Gamemodes.RandomItems:
                    RandomItemsGamemode.instance = new RandomItemsGamemode();
                    CustomGamemode.Instance = RandomItemsGamemode.instance;
                    break;
                case Gamemodes.BattleRoyale:
                    BattleRoyaleGamemode.instance = new BattleRoyaleGamemode();
                    CustomGamemode.Instance = BattleRoyaleGamemode.instance;
                    break;
                case Gamemodes.Speedrun:
                    SpeedrunGamemode.instance = new SpeedrunGamemode();
                    CustomGamemode.Instance = SpeedrunGamemode.instance;
                    break;
                case Gamemodes.PaintBattle:
                    PaintBattleGamemode.instance = new PaintBattleGamemode();
                    CustomGamemode.Instance = PaintBattleGamemode.instance;
                    break;
                case Gamemodes.KillOrDie:
                    KillOrDieGamemode.instance = new KillOrDieGamemode();
                    CustomGamemode.Instance = KillOrDieGamemode.instance;
                    break;
                case Gamemodes.Zombies:
                    ZombiesGamemode.instance = new ZombiesGamemode();
                    CustomGamemode.Instance = ZombiesGamemode.instance;
                    break;
                case Gamemodes.Jailbreak:
                    JailbreakGamemode.instance = new JailbreakGamemode();
                    CustomGamemode.Instance = JailbreakGamemode.instance;
                    break;
                case Gamemodes.Deathrun:
                    DeathrunGamemode.instance = new DeathrunGamemode();
                    CustomGamemode.Instance = DeathrunGamemode.instance;
                    break;
                case Gamemodes.BaseWars:
                    BaseWarsGamemode.instance = new BaseWarsGamemode();
                    CustomGamemode.Instance = BaseWarsGamemode.instance;
                    break;
                case Gamemodes.FreezeTag:
                    FreezeTagGamemode.instance = new FreezeTagGamemode();
                    CustomGamemode.Instance = FreezeTagGamemode.instance;
                    break;
                case Gamemodes.ColorWars:
                    ColorWarsGamemode.instance = new ColorWarsGamemode();
                    CustomGamemode.Instance = ColorWarsGamemode.instance;
                    break;
            }
        }

        public static void RemoveDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
        {
            if (target == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                AntiCheat.RemovedBodies.Add(target.PlayerId);
            }
            foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == target.PlayerId)
                    Object.Destroy(deadBody.gameObject);
            }
        }

        public static void AddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            string optionName = "";
            if (optionItem.Parent?.Parent?.Parent != null)
                optionName += optionItem.Parent.Parent.Parent.GetOptionNameSCM() + " → ";
            if (optionItem.Parent?.Parent != null)
                optionName += optionItem.Parent.Parent.GetOptionNameSCM() + " → ";
            if (optionItem.Parent != null)
                optionName += optionItem.Parent.GetOptionNameSCM() + " → ";
            optionName += optionItem.GetOptionNameSCM();
            string text = $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{optionName}</font>: <font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{value}</font>";
            HudManager.Instance.Notifier.CustomSettingsChangeMessageLogic(optionItem, text, playSound);
        }

        public static void SetStandardName(this PlayerControl player, string name)
        {
            Main.StandardNames[player.PlayerId] = name;
        }

        public static void SendNoisemakerAlert(this PlayerControl player)
        {
            if (player.Data.Role.Role != RoleTypes.Noisemaker) return;
            player.Data.Role.OnDeath(DeathReason.Kill);
        }

        public static void SetRoleblock(this PlayerControl player, bool roleblock)
        {
            if (ClassicGamemode.instance == null) return;
            ClassicGamemode.instance.IsRoleblocked[player.PlayerId] = roleblock;
            if (roleblock && Minigame.Instance)
			{
				try
				{
					Minigame.Instance.Close();
					Minigame.Instance.Close();
				}
				catch
				{
				}
			}
            if (AmongUsClient.Instance.AmHost && !player.AmOwner && !Main.IsModded[player.PlayerId])
            {
                if (roleblock)
                    player.RpcDesyncUpdateSystem(SystemTypes.Comms, 128);
                else if (!Utils.IsActive(SystemTypes.Comms))
                {
                    player.RpcDesyncUpdateSystem(SystemTypes.Comms, 16);
                    if (Main.RealOptions.GetByte(ByteOptionNames.MapId) is 1 or 5)
                        player.RpcDesyncUpdateSystem(SystemTypes.Comms, 17);
                }
            }
                
        }

        public static void SetAbilityUses(this PlayerControl player, float uses)
        {
            if (ClassicGamemode.instance == null) return;
            player.GetRole().AbilityUses = uses;
        }

        public static void SetPetAbilityCooldown(this PlayerControl player, bool onCooldown)
        {
            if (ClassicGamemode.instance == null) return;
            ClassicGamemode.instance.IsOnPetAbilityCooldown[player.PlayerId] = onCooldown;
        }

        public static void GuessPlayer(this PlayerControl player)
        {
            SoundManager.Instance.PlaySound(player.KillSfx, false, 0.8f);
            HudManager.Instance.KillOverlay.ShowKillAnimation(player.Data, player.Data);
        }

        public static void SyncCustomRole(this PlayerControl player, MessageReader reader)
        {
            if (ClassicGamemode.instance == null) return;
            player.GetRole().ReceiveRPC(reader);
        }

        public static void MakeInvisible(this PlayerControl player)
        {
            if (player.AmOwner)
                player.invisibilityAlpha = 0.5f;
            else
                player.invisibilityAlpha = PlayerControl.LocalPlayer.Data.Role.IsDead ? 0.5f : 0f;
            player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
            if (!player.AmOwner && !PlayerControl.LocalPlayer.Data.Role.IsDead)
            {
                player.shouldAppearInvisible = true;
                player.Visible = false;
            }
        }

        public static void MakeVisible(this PlayerControl player)
        {
            if (!player.AmOwner)
            {
                player.shouldAppearInvisible = false;
			    player.Visible = true;
            }
            player.invisibilityAlpha = 1f;
            player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
            if (!player.AmOwner)
            {
                player.shouldAppearInvisible = false;
                player.Visible = !player.inVent;
            }
        }

        public static void RpcVersionCheck(this PlayerControl player, string version)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.VersionCheck, SendOption.Reliable, AmongUsClient.Instance.HostId);
            writer.Write(version);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSyncCustomOptions(this GameManager manager)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable, -1);
            foreach (var co in OptionItem.AllOptions)
            {
                if (co.Id == 0 || co is TextOptionItem) continue;
                if (co.Id >= 1000 && co.IsHiddenOn(Options.CurrentGamemode)) continue;
                writer.Write(co.CurrentValue);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcReactorFlash(this PlayerControl player, float duration, Color color)
        {
            if (player.AmOwner)
            {
                HudManager.Instance.ReactorFlash(duration, color);
                return;
            }
            if (Main.IsModded[player.PlayerId])
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.ReactorFlash, SendOption.Reliable, -1);
                writer.Write(duration);
                writer.WriteColor(color);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return;
            }
            player.RpcUnmoddedReactorFlash(duration);
        }

        public static void RpcSetKillTimer(this PlayerControl player, float time = float.MaxValue)
        {
            Main.KillCooldowns[player.PlayerId] = time != float.MaxValue ? time : Main.OptionKillCooldowns[player.PlayerId] / 2f;
            if (player.AmOwner)
            {
                player.SetKillTimer(time != float.MaxValue ? time : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                return;
            }
            if (Main.IsModded[player.PlayerId])
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetKillTimer, SendOption.Reliable, -1);
                writer.Write(time != float.MaxValue ? time : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return;
            }
            player.RpcUnmoddedSetKillTimer(time);
        }

        public static void RpcPetAction(this PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.PetAction, SendOption.Reliable, AmongUsClient.Instance.HostId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcStartGamemode(this GameManager manager, Gamemodes gamemode)
        {
            manager.StartGamemode(gamemode);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.StartGamemode, SendOption.Reliable, -1);
            writer.Write((int)gamemode);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRemoveDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
        {
            player.RemoveDeadBody(target);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.RemoveDeadBody, SendOption.Reliable, -1);
            writer.Write((target != null) ? target.PlayerId : byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRequestVersionCheck(this PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.RequestVersionCheck, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcAddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            manager.AddCustomSettingsChangeMessage(optionItem, value, playSound);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.AddCustomSettingsChangeMessage, SendOption.Reliable, -1);
            writer.Write(optionItem.Id);
            writer.Write(value);
            writer.Write(playSound);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetStandardName(this PlayerControl player, string name)
        {
            player.SetStandardName(name);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetStandardName, SendOption.Reliable, -1);
            writer.WriteNetObject(player);
            writer.Write(name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSendNoisemakerAlert(this PlayerControl player)
        {
            player.SendNoisemakerAlert();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SendNoisemakerAlert, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetCustomRole(this PlayerControl player, CustomRoles role)
        {
            player.SetCustomRole(role);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetCustomRole, SendOption.Reliable, -1);
            writer.Write((int)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetRoleblock(this PlayerControl player, bool roleblock)
        {
            player.SetRoleblock(roleblock);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetRoleblock, SendOption.Reliable, -1);
            writer.Write(roleblock);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetAbilityUses(this PlayerControl player, float uses)
        {
            player.SetAbilityUses(uses);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetAbilityUses, SendOption.Reliable, -1);
            writer.Write(uses);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSyncCustomWinner(this GameManager manager)
        {
            if (ClassicGamemode.instance == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncCustomWinner, SendOption.Reliable, -1);
            writer.Write((int)ClassicGamemode.instance.Winner);
            writer.Write(ClassicGamemode.instance.AdditionalWinners.Count);
            for (int i = 0; i < ClassicGamemode.instance.AdditionalWinners.Count; ++i)
                writer.Write((int)ClassicGamemode.instance.AdditionalWinners[i]);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetPetAbilityCooldown(this PlayerControl player, bool onCooldown)
        {
            player.SetPetAbilityCooldown(onCooldown);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetPetAbilityCooldown, SendOption.Reliable, -1);
            writer.Write(onCooldown);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcGuessPlayer(this PlayerControl player)
        {
            player.GuessPlayer();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.GuessPlayer, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetAddOn(this PlayerControl player, AddOns addOn)
        {
            player.SetAddOn(addOn);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetAddOn, SendOption.Reliable, -1);
            writer.Write((int)addOn);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcMakeInvisibleModded(this PlayerControl player)
        {
            player.MakeInvisible();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.MakeInvisible, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcMakeVisibleModded(this PlayerControl player)
        {
            player.MakeVisible();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.MakeVisible, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}