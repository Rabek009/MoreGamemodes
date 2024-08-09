using System.Linq;
using InnerNet;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Data;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    static class ExtendedPlayerControl
    {
        public static void RpcTeleport(this PlayerControl player, Vector2 position)
        {
            if (MeetingHud.Instance) return;
            if ((player.inVent || player.MyPhysics.Animations.IsPlayingEnterVentAnimation()) && !player.inMovingPlat)
            {
                player.MyPhysics.RpcExitVent(player.GetClosestVent().Id);
                new LateTask(() => player.RpcTeleport(position), 0.5f, "Retry Teleport");
                return;
            }
            if (player.onLadder || player.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || player.inMovingPlat)
            {
                new LateTask(() => player.RpcTeleport(position), 0.1f, "Retry Teleport");
                return;
            }
            if (AmongUsClient.Instance.AmClient)
            {
                player.NetTransform.SnapTo(position, (ushort)(player.NetTransform.lastSequenceId + 328));
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None);
            NetHelpers.WriteVector2(position, writer);
            writer.Write((ushort)(player.NetTransform.lastSequenceId + 8));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRandomVentTeleport(this PlayerControl player)
        {
            var vents = Object.FindObjectsOfType<Vent>();
            var rand = new System.Random();
            var vent = vents[rand.Next(0, vents.Count)];
            player.RpcTeleport(new Vector2(vent.transform.position.x, vent.transform.position.y + 0.3636f));
        }

        public static void RpcSendMessage(this PlayerControl player, string message, string title)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (message.Length > 1000)
            {
                foreach (var text in message.SplitMessage())
                    player.RpcSendMessage(text, title);
                return;
            }
            Main.MessagesToSend.Add((message, player.PlayerId, title));
        }

        public static void RpcSetDesyncRole(this PlayerControl player, RoleTypes role, PlayerControl seer)
        {
            if (player == null || seer == null) return;
            if (seer.AmOwner)
            {
                player.StartCoroutine(player.CoSetRole(role, true));
                return;
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, seer.GetClientId());
            writer.Write((ushort)role);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            if (Main.DesyncRoles.ContainsKey((player.PlayerId, seer.PlayerId)) && role == Main.StandardRoles[player.PlayerId])
                Main.DesyncRoles.Remove((player.PlayerId, seer.PlayerId));
            if (!Main.DesyncRoles.ContainsKey((player.PlayerId, seer.PlayerId)) && role != Main.StandardRoles[player.PlayerId])
                Main.DesyncRoles.Add((player.PlayerId, seer.PlayerId), role);
            AntiCheat.TimeSinceRoleChange[player.PlayerId] = 0f;
        }

        public static void RpcSetDesyncRoleV2(this PlayerControl player, RoleTypes role, PlayerControl seer)
        {
            if (player == null || seer == null) return;
            if (seer.AmOwner)
            {
                player.StartCoroutine(player.CoSetRole(role, true));
                return;
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, seer.GetClientId());
            writer.Write((ushort)role);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetNamePrivate(this PlayerControl player, string name, PlayerControl seer = null, bool isRaw = false)
        {
            if (player == null || name == null || !AmongUsClient.Instance.AmHost) return;
            if (seer == null) seer = player;
            if (AntiCheat.BannedPlayers.Contains(player.NetId)) return;
            if (Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] == name && !isRaw) return;
            
            if (seer.AmOwner)
            {
                player.cosmetics.nameText.SetText(name);
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
                return;
            }
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.None, clientId);
            writer.Write(player.Data.NetId);
            writer.Write(name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
        }

        public static bool TryCast<T>(this Il2CppObjectBase obj, out T casted)
        where T : Il2CppObjectBase
        {
            casted = obj.TryCast<T>();
            return casted != null;
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

        public static PlayerControl GetClosestPlayer(this PlayerControl player, bool forTarget = false)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && (!forTarget || !(p.inVent || p.MyPhysics.Animations.IsPlayingEnterVentAnimation() || p.onLadder || p.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || p.inMovingPlat || (p.Data.Role.Role == RoleTypes.Phantom && (p.Data.Role as PhantomRole).IsInvisible))))
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

        public static void RpcGuardAndKill(this PlayerControl killer, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (killer.AmOwner)
            {
                killer.MurderPlayer(target, MurderResultFlags.FailedProtected);
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, killer.GetClientId());
                writer.WriteNetObject(target);
                writer.Write((int)MurderResultFlags.FailedProtected);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static void RpcUnmoddedSetKillTimer(this PlayerControl player, float time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (time != float.MaxValue)
            {
                var opt = player.BuildGameOptions(time * 2);
                Utils.SyncSettings(opt, player.GetClientId());
            }
            player.RpcGuardAndKill(player);
        }

        public static void RpcExileV2(this PlayerControl player)
        {
            player.Exiled();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcFixedMurderPlayer(this PlayerControl killer, PlayerControl target)
        {
            new LateTask(() => 
            {
                killer.RpcMurderPlayer(target, true);
                killer.MyPhysics.RpcCancelPet();
            }, 0.01f, "Late Murder");
        }

        public static void RpcUnmoddedReactorFlash(this PlayerControl pc, float duration)
        {
            if (pc == null) return;
            int clientId = pc.GetClientId();
            byte reactorId = 3;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) reactorId = 21;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4) reactorId = 58;

            MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
            SabotageWriter.Write(reactorId);
            MessageExtensions.WriteNetObject(SabotageWriter, pc);
            SabotageWriter.Write((byte)128);
            AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);

            new LateTask(() =>
            {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, duration, "Fix Desync Reactor");

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                new LateTask(() =>
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, duration, "Fix Desync Reactor 2");
        }

        public static void RpcSetDeathReason(this PlayerControl player, DeathReasons reason)
        {
            Main.AllPlayersDeathReason[player.PlayerId] = reason;
        }

        public static DeathReasons GetDeathReason(this PlayerControl player)
        {
            return Main.AllPlayersDeathReason[player.PlayerId];
        }

        public static Vent GetClosestVent(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<Vent, float> ventdistance = new();
            float dis;
            foreach (Vent vent in ShipStatus.Instance.AllVents)
            {
                dis = Vector2.Distance(playerpos, vent.transform.position);
                ventdistance.Add(vent, dis);
            }
            var min = ventdistance.OrderBy(c => c.Value).FirstOrDefault();
            Vent target = min.Key;
            return target;
        }

        public static IGameOptions BuildGameOptions(this PlayerControl player, float killCooldown = -1f)
        {
            IGameOptions opt = Main.RealOptions.Restore(new NormalGameOptionsV08(new UnityLogger().Cast<Hazel.ILogger>()).Cast<IGameOptions>());
            opt = CustomGamemode.Instance.BuildGameOptions(player, opt);
            if (killCooldown >= 0) opt.SetFloat(FloatOptionNames.KillCooldown, killCooldown);
            return opt;
        }

        public static string BuildPlayerName(this PlayerControl player, PlayerControl seer, bool isMeeting = false)
        {
            string name = Main.StandardNames[Main.AllShapeshifts[player.PlayerId]];
            if (isMeeting)
                name = Main.StandardNames[player.PlayerId];
            if (Main.NameColors[(player.PlayerId, seer.PlayerId)] != Color.clear)
                name = Utils.ColorString(Main.NameColors[(player.PlayerId, seer.PlayerId)], name);
            if (isMeeting) return name;
            name = CustomGamemode.Instance.BuildPlayerName(player, seer, name);
            if (Options.EnableMidGameChat.GetBool() && Options.ProximityChat.GetBool() && player == seer)
            {
                foreach (var message in Main.ProximityMessages[player.PlayerId])
                    name += "\n" + Utils.ColorString(Color.white, message.Item1);
            }
            if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 5 && Utils.IsActive(SystemTypes.MushroomMixupSabotage) && player != seer && !seer.Data.Role.IsImpostor)
                name = Utils.ColorString(Color.clear, "Player");
            return name;
        }

        public static void SendProximityMessage(this PlayerControl player, string appearance, string message)
        {
            string toSend = appearance + ": " + message;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Vector2.Distance(player.transform.position, pc.transform.position) <= Options.MessagesRadius.GetFloat() * 2.5f && pc != player)
                {
                    if (player.Data.IsDead && !pc.Data.IsDead) continue;
                    Main.ProximityMessages[pc.PlayerId].Add((toSend, 0f));
                }
            }
        }

        public static void SyncPlayerSettings(this PlayerControl player)
        {
            var opt = player.BuildGameOptions();
            if (player.AmOwner)
            {
                foreach (var com in GameManager.Instance.LogicComponents)
                {
                    if (com.TryCast<LogicOptions>(out var lo))
                        lo.SetGameOptions(opt);
                }
                GameOptionsManager.Instance.CurrentGameOptions = opt;
            }
            else
                Utils.SyncSettings(opt, player.GetClientId());
        }

        public static void RpcSetOutfit(this PlayerControl player, byte colorId, string hatId, string skinId, string petId, string visorId)
        {
            player.RpcSetColor(colorId);
            player.RpcSetHat(hatId);
            player.RpcSetSkin(skinId);
            player.RpcSetVisor(visorId);
            player.RpcSetPet(player.Data.IsDead ? "" : petId);
        }

        public static void RpcSetRoleV2(this PlayerControl player, RoleTypes role)
        {
            if (player == null) return;
            Main.StandardRoles[player.PlayerId] = role;
            player.StartCoroutine(player.CoSetRole(role, true));
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, -1);
            writer.Write((ushort)role);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Main.DesyncRoles.ContainsKey((player.PlayerId, pc.PlayerId)))
                    Main.DesyncRoles.Remove((player.PlayerId, pc.PlayerId));
            }
            AntiCheat.TimeSinceRoleChange[player.PlayerId] = 0f;
        }

        public static void RpcSetRoleV3(this PlayerControl player, RoleTypes role, bool forEndGame)
        {
            if (player == null) return;
            if (forEndGame)
                RoleManager.Instance.SetRole(player, role);
            else
                player.StartCoroutine(player.CoSetRole(role, true));
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, -1);
            writer.Write((ushort)role);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static PlainShipRoom GetPlainShipRoom(this PlayerControl pc)
        {
            if (pc.Data.IsDead) return null;
            var Rooms = ShipStatus.Instance.AllRooms;
            if (Rooms == null) return null;
            foreach (var room in Rooms)
            {
                if (!room.roomArea) continue;
                if (pc.Collider.IsTouching(room.roomArea))
                    return room;
            }
            return null;
        }

        public static void ForceReportDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
        {
            if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
		    {
			    return;
		    }
            MeetingRoomManager.Instance.AssignSelf(player, target);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(player);
		    player.RpcStartMeeting(target);
        }

        public static void RpcRevive(this PlayerControl player)
        {
            if (!Main.StandardRoles.ContainsKey(player.PlayerId))
            {
                player.RpcSetRoleV3(RoleTypes.Crewmate, false);
                PlayerControl.LocalPlayer.RpcRemoveDeadBody(player.Data);
                return;
            }
            if (Options.CurrentGamemode is Gamemodes.BombTag or Gamemodes.KillOrDie or Gamemodes.Jailbreak or Gamemodes.BaseWars)
            {
                player.RpcSetDesyncRole(RoleTypes.Shapeshifter, player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != player)
                        player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                }
                return;
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                player.RpcSetDesyncRole(RoleTypes.Impostor, player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != player)
                        player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                }
                return;
            }
            player.RpcSetRoleV2(Main.StandardRoles[player.PlayerId]);
            player.RpcSetKillTimer(10f);
            player.RpcResetAbilityCooldown();
            foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == player.PlayerId)
                {
                    var position = deadBody.transform.position;
                    player.RpcTeleport(new Vector2(position.x, position.y + 0.3636f));
                    break;
                }
            }
            player.RpcSetPet(Main.StandardPets[player.PlayerId]);
            PlayerControl.LocalPlayer.RpcRemoveDeadBody(player.Data);
        }

        public static bool HasTask(this PlayerControl player, TaskTypes taskType)
        {
            foreach (var task in player.myTasks)
            {
                if (task.TaskType == taskType)
                    return true;
            }
            return false;
        }

        // It doesn't work for phantom and tracker
        // Makes role really weird, but also gives new possibilities to host only mods
        public static void RpcSetWeirdRole(this PlayerControl player, RoleTypes role, bool despawnFakePlayer, PlayerControl seer = null)
        {
            if (player.AmOwner || Main.IsModded[player.PlayerId])
            {
                if (seer == null)
                    player.RpcSetRoleV2(role);
                else
                    player.RpcSetDesyncRole(role, seer);
                return;
            }
            PlayerControl playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = player.PlayerId;
            playerControl.isNew = false;
            playerControl.notRealPlayer = true;
            AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
            if (seer == null)
                playerControl.RpcSetRoleV2(role);
            else
                playerControl.RpcSetDesyncRole(role, seer);
            if (PlayerControl.AllPlayerControls.Contains(playerControl))
                PlayerControl.AllPlayerControls.Remove(playerControl);
            if (despawnFakePlayer)
            {
                new LateTask(() => playerControl.Despawn(), 0.5f);
            }
            else
            {
                Main.RoleFakePlayer[player.PlayerId] = playerControl.NetId;
                playerControl.MyPhysics.RpcExitVent(0);
                playerControl.MyPhysics.RpcEnterVent(0);
            }
        }

        public static void RpcSetUnshiftButton(this PlayerControl player)
        {
            if (Main.AllShapeshifts[player.PlayerId] != player.PlayerId) return;
            if (player.AmOwner)
            {
                player.RawSetOutfit(player.Data.Outfits[PlayerOutfitType.Default], PlayerOutfitType.Shapeshifted);
                return;
            }
            new LateTask(() => {
                var outfit = player.Data.Outfits[PlayerOutfitType.Default];
                var sender = CustomRpcSender.Create("Set Unshift Button", SendOption.None);
                sender.StartMessage(player.GetClientId());
                sender.StartRpc(player.NetId, RpcCalls.Shapeshift)
                    .WriteNetObject(PlayerControl.LocalPlayer)
                    .Write(false)
                    .EndRpc();
                sender.StartRpc(player.NetId, RpcCalls.SetColor)
                    .Write(player.Data.NetId)
                    .Write((byte)outfit.ColorId)
                    .EndRpc();
                sender.StartRpc(player.NetId, RpcCalls.SetHatStr)
                    .Write(outfit.HatId)
                    .Write(player.GetNextRpcSequenceId(RpcCalls.SetHatStr))
                    .EndRpc();
                sender.StartRpc(player.NetId, RpcCalls.SetSkinStr)
                    .Write(outfit.SkinId)
                    .Write(player.GetNextRpcSequenceId(RpcCalls.SetSkinStr))
                    .EndRpc();
                sender.StartRpc(player.NetId, RpcCalls.SetVisorStr)
                    .Write(outfit.VisorId)
                    .Write(player.GetNextRpcSequenceId(RpcCalls.SetVisorStr))
                    .EndRpc();
                sender.StartRpc(player.NetId, RpcCalls.SetPetStr)
                    .Write(outfit.PetId)
                    .Write(player.GetNextRpcSequenceId(RpcCalls.SetPetStr))
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }, 0f);
            new LateTask(() => player.RpcSetNamePrivate(player.BuildPlayerName(player, false), player, true), 0.2f);
        }

        public static void RpcDesyncUpdateSystem(this PlayerControl target, SystemTypes systemType, int amount)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, target.GetClientId());
            messageWriter.Write((byte)systemType);
            messageWriter.WriteNetObject(target);
            messageWriter.Write((byte)amount);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }
}