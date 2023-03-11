using System.Linq;
using InnerNet;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Data;

namespace MoreGamemodes
{
    static class ExtendedPlayerControl
    {
        public static void RpcTeleport(this PlayerControl player, Vector2 location)
        {
            if (player.inVent)
                player.MyPhysics.RpcBootFromVent(0);
            if (AmongUsClient.Instance.AmHost) player.NetTransform.SnapTo(location);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None);
            NetHelpers.WriteVector2(location, writer);
            writer.Write(player.NetTransform.lastSequenceId);         
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcRandomVentTeleport(this PlayerControl player)
        {
            var vents = UnityEngine.Object.FindObjectsOfType<Vent>();
            var rand = new System.Random();
            var vent = vents[rand.Next(0, vents.Count)];
            player.RpcTeleport(new Vector2(vent.transform.position.x, vent.transform.position.y + 0.3636f));
        }
        public static void RpcSendMessage(this PlayerControl player, string message, string messageName)
        {
            var sender = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            var name = sender.Data.PlayerName;

            if (player.AmOwner)
            {
                sender.SetName(Utils.ColorString(Color.blue, "MG.SystemMessage." + messageName));
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, message);
                sender.SetName(name);
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SetName, SendOption.None, player.GetClientId());
                writer.Write(Utils.ColorString(Color.blue, "MG.SystemMessage." + messageName));
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SendChat, SendOption.None, player.GetClientId());
                writer2.Write(message);
                AmongUsClient.Instance.FinishRpcImmediately(writer2);

                MessageWriter writer3 = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SetName, SendOption.None, player.GetClientId());
                writer3.Write(Main.GameStarted ? Main.LastNotifyNames[(sender.PlayerId, player.PlayerId)] : name);
                AmongUsClient.Instance.FinishRpcImmediately(writer3);

                MessageWriter writer4 = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SetName, SendOption.None, player.GetClientId());
                writer4.Write(Main.GameStarted ? Main.LastNotifyNames[(sender.PlayerId, player.PlayerId)] : name);
                AmongUsClient.Instance.FinishRpcImmediately(writer4);
            }
        }
        public static void RpcSendChatV2(this PlayerControl player, string message)
        {
            if (player == null) return;
            var name = player.Data.PlayerName;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.None);
            writer.Write(Main.GameStarted ? Main.StandardNames[player.PlayerId] : name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SendChat, SendOption.None);
            writer2.Write(message);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                MessageWriter writer3 = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.None, pc.GetClientId());
                writer3.Write(Main.GameStarted ? Main.LastNotifyNames[(player.PlayerId, pc.PlayerId)] : name);
                AmongUsClient.Instance.FinishRpcImmediately(writer3);

                MessageWriter writer4 = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.None, pc.GetClientId());
                writer4.Write(Main.GameStarted ? Main.LastNotifyNames[(player.PlayerId, pc.PlayerId)] : name);
                AmongUsClient.Instance.FinishRpcImmediately(writer4);
            }
        }
        public static void RpcSetDesyncRole(this PlayerControl player, RoleTypes role, int clientId)
        {
            if (player == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcSetNamePrivate(this PlayerControl player, string name, PlayerControl seer = null, bool isRaw = false)
        {
            if (player == null || name == null || !AmongUsClient.Instance.AmHost) return;
            if (seer == null) seer = player;
            if (Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] == name && !isRaw) return;

            if (seer.AmOwner)
            {
                player.cosmetics.nameText.SetText(name);
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
                return;
            }
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, Hazel.SendOption.Reliable, clientId);
            writer.Write(name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            if (!isRaw)
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
        }
        public static void SyncCustomSettingsRPC()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 80, Hazel.SendOption.Reliable, -1);
            foreach (var co in OptionItem.AllOptions)
            {
                writer.Write(co.GetValue());
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static bool TryCast<T>(this Il2CppObjectBase obj, out T casted)
        where T : Il2CppObjectBase
        {
            casted = obj.TryCast<T>();
            return casted != null;
        }
        public static void RpcCancelPetV2(this PlayerPhysics playerPhysics)
        {
            if (playerPhysics.AmOwner)
                playerPhysics.RpcCancelPet();
            else
                AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(playerPhysics.NetId, (byte)RpcCalls.CancelPet, SendOption.None));
        }
        public static void RpcShapeshiftV2(this PlayerControl shifter, PlayerControl target, bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (shifter.Data.IsDead)
            {
                shifter.RpcSendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", "Anticheat");
                return;
            }
            var role = shifter.Data.Role.Role;
            shifter.RpcSetRoleV2(RoleTypes.Shapeshifter);
            new LateTask(() =>
            {
                shifter.RpcShapeshift(target, shouldAnimate);
            }, 0.1f, "Shapeshift");
        }
        public static void RpcRevertShapeshiftV2(this PlayerControl shifter, bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (shifter.Data.IsDead)
            {
                shifter.RpcSendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", "Anticheat");
                return;
            }
            var role = shifter.Data.Role.Role;
            shifter.RpcSetRoleV2(RoleTypes.Shapeshifter);
            new LateTask(() =>
            {
                shifter.RpcRevertShapeshift(shouldAnimate);
            }, 0.1f, "Revert Shapeshift");
        }
        public static void RpcResetAbilityCooldown(this PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.LocalPlayer == target)
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.None, target.GetClientId());
                writer.WriteNetObject(target);
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void RpcSetAbilityCooldown(this PlayerControl target, float timer)
        {
            switch (target.Data.Role.Role)
            {
                case RoleTypes.Scientist:
                    var scientistCooldown = GameOptionsManager.Instance.currentGameOptions.GetFloat(FloatOptionNames.ScientistCooldown);
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, timer);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    target.RpcResetAbilityCooldown();
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, scientistCooldown);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    break;
                case RoleTypes.Engineer:
                    var engineerCooldown = GameOptionsManager.Instance.currentGameOptions.GetFloat(FloatOptionNames.EngineerCooldown);
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, timer);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    target.RpcResetAbilityCooldown();
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, engineerCooldown);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    break;
                case RoleTypes.GuardianAngel:
                    var guardianAngelCooldown = GameOptionsManager.Instance.currentGameOptions.GetFloat(FloatOptionNames.GuardianAngelCooldown);
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.GuardianAngelCooldown, timer);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    target.RpcResetAbilityCooldown();
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.GuardianAngelCooldown, guardianAngelCooldown);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    break;
                case RoleTypes.Shapeshifter:
                    var shapeshifterCooldown = GameOptionsManager.Instance.currentGameOptions.GetFloat(FloatOptionNames.ShapeshifterCooldown);
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, timer);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    target.RpcResetAbilityCooldown();
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, shapeshifterCooldown);
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    break;
            }
        }
        public static void RpcSetRoleV2(this PlayerControl player, RoleTypes role)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable);
            writer.Write((byte)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static bool HasBomb(this PlayerControl player)
        {
            var hasBomb = false;
            if (player == null)
                return hasBomb;
            var hasBombFound = Main.HasBomb.TryGetValue(player.PlayerId, out hasBomb);
            return hasBombFound ? hasBomb : false;
        }
        public static Items GetItem(this PlayerControl player)
        {
            var item = Items.None;
            if (player == null)
                return item;
            var itemFound = Main.AllPlayersItems.TryGetValue(player.PlayerId, out item);
            return itemFound ? item : Items.None;
        }
        public static bool CanVent(this PlayerControl player)
        {
            if (((Options.CurrentGamemode == Gamemodes.HideAndSeek && !Options.HnSImpostorsCanVent.GetBool()) || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && !Options.SnSImpostorsCanVent.GetBool())) && Main.Impostors.Contains(player.PlayerId))
                return false;
            if (Options.CurrentGamemode == Gamemodes.BombTag)
                return false;
            if (Options.CurrentGamemode == Gamemodes.RandomItems && Main.HackTimer > 1f && (Main.Impostors.Contains(player.PlayerId) == false || Options.HackAffectsImpostors.GetBool()))
                return false;
            return player.Data.Role.Role == RoleTypes.Engineer || player.Data.Role.IsImpostor;
        }
        public static PlayerControl GetClosestPlayer(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player)
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

    }
}