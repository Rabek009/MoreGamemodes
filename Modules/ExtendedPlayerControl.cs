using System.Linq;
using InnerNet;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Data;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    static class ExtendedPlayerControl
    {
        public static CustomRole GetRole(this PlayerControl player)
        {
            if (ClassicGamemode.instance == null || !ClassicGamemode.instance.AllPlayersRole.ContainsKey(player.PlayerId)) return null;
            return ClassicGamemode.instance.AllPlayersRole[player.PlayerId];
        }

        public static CustomRole GetRole(this NetworkedPlayerInfo playerInfo)
        {
            if (ClassicGamemode.instance == null || !ClassicGamemode.instance.AllPlayersRole.ContainsKey(playerInfo.PlayerId)) return null;
            return ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId];
        }

        public static List<AddOn> GetAddOns(this PlayerControl player)
        {
            if (ClassicGamemode.instance == null || !ClassicGamemode.instance.AllPlayersAddOns.ContainsKey(player.PlayerId)) return null;
            return ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId];
        }

        public static List<AddOn> GetAddOns(this NetworkedPlayerInfo playerInfo)
        {
            if (ClassicGamemode.instance == null || !ClassicGamemode.instance.AllPlayersAddOns.ContainsKey(playerInfo.PlayerId)) return null;
            return ClassicGamemode.instance.AllPlayersAddOns[playerInfo.PlayerId];
        }

        public static bool HasAddOn(this PlayerControl player, AddOns addOn)
        {
            foreach (var addon in player.GetAddOns())
            {
                if (addon.Type == addOn)
                    return true;
            }
            return false;
        }

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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && player.GetRole().Role == CustomRoles.Droner)
            {
                var dronerRole = player.GetRole() as Droner;
                if (dronerRole != null)
                {
                    player.RpcSetDronerRealPosition(position);
                    if (AmongUsClient.Instance.AmClient && !player.AmOwner)
                    {
                        player.NetTransform.SnapTo(position, (ushort)(player.NetTransform.lastSequenceId + 328));
                    }
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.AmOwner || pc == player) continue;
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.Reliable, pc.GetClientId());
                        NetHelpers.WriteVector2(position, writer2);
                        writer2.Write((ushort)(player.NetTransform.lastSequenceId + 16383 + 8));
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    }
                }
            }
            if (AmongUsClient.Instance.AmClient)
            {
                player.NetTransform.SnapTo(position, (ushort)(player.NetTransform.lastSequenceId + 328));
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.Reliable);
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
            player.RpcSetVentInteraction();
            AntiCheat.TimeSinceRoleChange[player.PlayerId] = 0f;
            if (player == seer)
                Main.KillCooldowns[player.PlayerId] = 10f;
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
            if (player == seer)
                Main.KillCooldowns[player.PlayerId] = 10f;
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
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.Reliable, target.GetClientId());
                writer.WriteNetObject(target);
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            Main.ProtectCooldowns[target.PlayerId] = Main.OptionProtectCooldowns[target.PlayerId];
        }

        public static void RpcSetAbilityCooldown(this PlayerControl target, float time)
        {
            if (PlayerControl.LocalPlayer == target)
            {
                var options = target.BuildGameOptions(abilityCooldown: time);
                foreach (var com in GameManager.Instance.LogicComponents)
                {
                    if (com.TryCast<LogicOptions>(out var lo))
                        lo.SetGameOptions(options);
                }
                GameOptionsManager.Instance.CurrentGameOptions = options;
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
                var options2 = target.BuildGameOptions();
                foreach (var com in GameManager.Instance.LogicComponents)
                {
                    if (com.TryCast<LogicOptions>(out var lo))
                        lo.SetGameOptions(options2);
                }
                GameOptionsManager.Instance.CurrentGameOptions = options2;
                return;
            }
            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(target.GetClientId());
            var opt = target.BuildGameOptions(abilityCooldown: time);
            Il2CppStructArray<byte> byteArray = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt, false);
            writer.StartMessage(1);
            {
                writer.WritePacked(GameManager.Instance.NetId);
                writer.StartMessage(GameManager.Instance.TryCast<NormalGameManager>() ? (byte)4 : (byte)5);
			    writer.WriteBytesAndSize(byteArray);
			    writer.EndMessage();
            }
            writer.EndMessage();
            writer.StartMessage(2);
            {
                writer.WritePacked(target.NetId);
                writer.Write((byte)RpcCalls.ProtectPlayer);
                writer.WriteNetObject(target);
                writer.Write(0);
            }
            writer.EndMessage();
            var opt2 = target.BuildGameOptions();
            Il2CppStructArray<byte> byteArray2 = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt2, false);
            writer.StartMessage(1);
            {
                writer.WritePacked(GameManager.Instance.NetId);
                writer.StartMessage(GameManager.Instance.TryCast<NormalGameManager>() ? (byte)4 : (byte)5);
			    writer.WriteBytesAndSize(byteArray2);
			    writer.EndMessage();
            }
            writer.EndMessage();
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public static PlayerControl GetClosestPlayer(this PlayerControl player, bool forTarget = false)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && (!forTarget || !(p.inVent || p.MyPhysics.Animations.IsPlayingEnterVentAnimation() || p.onLadder || p.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || p.inMovingPlat)))
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            if (pcdistance.Count == 0) return null;
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
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, killer.GetClientId());
                writer.WriteNetObject(target);
                writer.Write((int)MurderResultFlags.FailedProtected);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static void RpcUnmoddedSetKillTimer(this PlayerControl player, float time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(player.GetClientId());
            if (time != float.MaxValue)
            {
                var opt = player.BuildGameOptions(killCooldown: time * 2f);
                Il2CppStructArray<byte> byteArray = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt, false);
                writer.StartMessage(1);
                {
                    writer.WritePacked(GameManager.Instance.NetId);
                    writer.StartMessage(GameManager.Instance.TryCast<NormalGameManager>() ? (byte)4 : (byte)5);
				    writer.WriteBytesAndSize(byteArray);
				    writer.EndMessage();
                }
                writer.EndMessage();
            }
            writer.StartMessage(2);
            {
                writer.WritePacked(player.NetId);
                writer.Write((byte)RpcCalls.MurderPlayer);
                writer.WriteNetObject(player);
                writer.Write((int)MurderResultFlags.FailedProtected);
            }
            writer.EndMessage();
            if (time != float.MaxValue)
            {
                var opt = player.BuildGameOptions();
                Il2CppStructArray<byte> byteArray = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt, false);
                writer.StartMessage(1);
                {
                    writer.WritePacked(GameManager.Instance.NetId);
                    writer.StartMessage(GameManager.Instance.TryCast<NormalGameManager>() ? (byte)4 : (byte)5);
				    writer.WriteBytesAndSize(byteArray);
				    writer.EndMessage();
                }
                writer.EndMessage();
            }
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public static void RpcExileV2(this PlayerControl player)
        {
            player.Exiled();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcUnmoddedReactorFlash(this PlayerControl pc, float duration)
        {
            if (pc == null) return;
            int clientId = pc.GetClientId();
            byte reactorId = 3;
            if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 2) reactorId = 21;
            if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 4) reactorId = 58;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
            writer.Write(reactorId);
            writer.WriteNetObject(pc);
            writer.Write((byte)128);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            new LateTask(() =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                writer.Write(reactorId);
                writer.WriteNetObject(pc);
                writer.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }, duration, "Fix Desync Reactor");

            if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 4)
                new LateTask(() =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                    writer.Write(reactorId);
                    writer.WriteNetObject(pc);
                    writer.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
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
            if (ClassicGamemode.instance != null && player.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = player.GetRole() as Droner;
                if (dronerRole != null && dronerRole.RealPosition != null)
                    playerpos = dronerRole.DronePosition;
            }
            Dictionary<Vent, float> ventdistance = new();
            float dis;
            foreach (Vent vent in ShipStatus.Instance.AllVents)
            {
                dis = Vector2.Distance(playerpos, vent.transform.position);
                ventdistance.Add(vent, dis);
            }
            if (ventdistance.Count == 0) return null;
            var min = ventdistance.OrderBy(c => c.Value).FirstOrDefault();
            Vent target = min.Key;
            return target;
        }

        public static DeadBody GetClosestDeadBody(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<DeadBody, float> bodydistance = new();
            float dis;
            foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
            {
                dis = Vector2.Distance(playerpos, body.transform.position);
                bodydistance.Add(body, dis);
            }
            if (bodydistance.Count == 0) return null;
            var min = bodydistance.OrderBy(c => c.Value).FirstOrDefault();
            DeadBody target = min.Key;
            return target;
        }

        public static IGameOptions BuildGameOptions(this PlayerControl player, float killCooldown = -1f, float abilityCooldown = -1f)
        {
            IGameOptions opt = Main.RealOptions.Restore(new NormalGameOptionsV08(new UnityLogger().Cast<Hazel.ILogger>()).Cast<IGameOptions>());
            opt = CustomGamemode.Instance.BuildGameOptions(player, opt);
            if (ExplosionHole.LastSpeedDecrease[player.PlayerId] > 0)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, opt.GetFloat(FloatOptionNames.PlayerSpeedMod) * ((100f - ExplosionHole.LastSpeedDecrease[player.PlayerId]) / 100f));
            if (opt.GetByte(ByteOptionNames.MapId) == 3)
                opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
            if (killCooldown >= 0) opt.SetFloat(FloatOptionNames.KillCooldown, killCooldown);
            if (abilityCooldown >= 0)
            {
                opt.SetFloat(FloatOptionNames.EngineerCooldown, abilityCooldown);
                opt.SetFloat(FloatOptionNames.GuardianAngelCooldown, abilityCooldown);
                opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, abilityCooldown);
                opt.SetFloat(FloatOptionNames.PhantomCooldown, abilityCooldown);
                opt.SetFloat(FloatOptionNames.TrackerCooldown, abilityCooldown);
                opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
                opt.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
            }
            return opt;
        }

        public static string BuildPlayerName(this PlayerControl player, PlayerControl seer, bool isMeeting = false, bool classicMeeting = false)
        {
            string name = Main.StandardNames[Main.AllShapeshifts[player.PlayerId]];
            if (isMeeting)
                name = Main.StandardNames[player.PlayerId];
            if (Main.NameColors[(player.PlayerId, seer.PlayerId)] != Color.clear)
                name = Utils.ColorString(Main.NameColors[(player.PlayerId, seer.PlayerId)], name);
            if (isMeeting)
            {
                if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && !classicMeeting)
                {
                    var prefix = "";
                    var postfix = "";
                    if (player == seer || seer.Data.IsDead || (player.GetRole().IsImpostor() && seer.GetRole().IsImpostor() && Options.SeeTeammateRoles.GetBool()))
                    {
                        foreach (var addOn in player.GetAddOns())
                            prefix += "<size=1.6>" + Utils.ColorString(addOn.Color, "(" + addOn.AddOnName + ")") + " </size>";
                    }
                    if (player == seer || seer.Data.IsDead || (player.GetRole().IsImpostor() && seer.GetRole().IsImpostor() && Options.SeeTeammateRoles.GetBool()))
                        prefix += "<size=1.6>" + Utils.ColorString(player.GetRole().Color, player.GetRole().RoleName + player.GetRole().GetProgressText()) + "\n</size>";
                    if (seer.GetRole().Role == CustomRoles.EvilGuesser || seer.GetRole().Role == CustomRoles.NiceGuesser)
                        prefix += Utils.ColorString(seer.GetRole().Color, player.PlayerId.ToString()) + " ";
                    if (player.Data.IsDead && (seer.GetRole().Role == CustomRoles.Mortician || seer.Data.IsDead))
                        postfix += " " + Utils.ColorString(seer.GetRole().Color, "(" + Utils.DeathReasonToString(player.GetDeathReason()) + ")");
                    name = prefix + name + postfix;
                }
                return name;
            }
            name = CustomGamemode.Instance.BuildPlayerName(player, seer, name);
            if (player == seer)
            {
                foreach (var message in Main.NameMessages[player.PlayerId])
                    name += "\n" + message.Item1;
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
                    pc.Notify(Utils.ColorString(Color.white, toSend));
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
            Main.OptionKillCooldowns[player.PlayerId] = opt.GetFloat(FloatOptionNames.KillCooldown);
            Main.OptionProtectCooldowns[player.PlayerId] = opt.GetFloat(FloatOptionNames.GuardianAngelCooldown);
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
            new LateTask(() => player.RpcSetVentInteraction(), 0.1f);
            AntiCheat.TimeSinceRoleChange[player.PlayerId] = 0f;
            Main.KillCooldowns[player.PlayerId] = 0f;
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
            Main.KillCooldowns[player.PlayerId] = 0f;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                new LateTask(() => {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        ClassicGamemode.instance.FreezeTimer[pc.PlayerId] = 0f;
                        ClassicGamemode.instance.BlindTimer[pc.PlayerId] = 0f;
                        ClassicGamemode.instance.RoleblockTimer[pc.PlayerId] = 0f;
                        ClassicGamemode.instance.IsFrozen[pc.PlayerId] = false;
                        ClassicGamemode.instance.IsBlinded[pc.PlayerId] = false;
                        if (ClassicGamemode.instance.IsRoleblocked[pc.PlayerId])
                            pc.RpcSetRoleblock(false);
                        pc.GetRole().OnMeeting();
                        foreach (var addOn in pc.GetAddOns())
                            addOn.OnMeeting();
                        Utils.SyncAllSettings();
                        Utils.SetAllVentInteractions();
                    }
                }, 0.01f);
                new LateTask(() => {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, true, true), ar, true);
                    }
                }, 0.5f);
            }
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
            else if (Options.CurrentGamemode is Gamemodes.BattleRoyale or Gamemodes.ColorWars)
            {
                player.RpcSetDesyncRole(RoleTypes.Impostor, player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != player)
                        player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                }
                return;
            }
            else if (Options.CurrentGamemode is Gamemodes.Classic)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.DesyncRoles.ContainsKey((player.PlayerId, pc.PlayerId)))
                        Main.DesyncRoles.Remove((player.PlayerId, pc.PlayerId));
                    if (Main.DesyncRoles.ContainsKey((pc.PlayerId, player.PlayerId)))
                        Main.DesyncRoles.Remove((pc.PlayerId, player.PlayerId));
                }
                switch (player.GetRole().BaseRole)
                {
                    case BaseRoles.Crewmate:
                        player.RpcSetRoleV2(RoleTypes.Crewmate);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Scientist:
                        player.RpcSetRoleV2(RoleTypes.Scientist);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Scientist;
                        break;
                    case BaseRoles.Engineer:
                        player.RpcSetRoleV2(RoleTypes.Engineer);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Engineer;
                        break;
                    case BaseRoles.Noisemaker:
                        player.RpcSetRoleV2(RoleTypes.Noisemaker);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Noisemaker;
                        break;
                    case BaseRoles.Tracker:
                        player.RpcSetRoleV2(RoleTypes.Tracker);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Tracker;
                        break;
                    case BaseRoles.Impostor:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Impostor;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Impostor, pc);
                        }
                        break;
                    case BaseRoles.Shapeshifter:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Shapeshifter;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Shapeshifter, pc);
                        }
                        break;
                    case BaseRoles.Phantom:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Phantom;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Phantom, pc);
                        }
                        break;
                    case BaseRoles.DesyncImpostor:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player)
                                player.RpcSetDesyncRole(RoleTypes.Impostor, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                        }
                        break;
                    case BaseRoles.DesyncShapeshifter:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player)
                                player.RpcSetDesyncRole(RoleTypes.Shapeshifter, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                        }
                        break;
                    case BaseRoles.DesyncPhantom:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player)
                                player.RpcSetDesyncRole(RoleTypes.Phantom, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                        }
                        break;
                }
                switch (player.GetRole().BaseRole)
                {
                    case BaseRoles.DesyncImpostor:
                    case BaseRoles.DesyncShapeshifter:
                    case BaseRoles.DesyncPhantom:
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc != player)
                            {
                                if (pc.GetRole().BaseRole is BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom)
                                    pc.RpcSetDesyncRole(RoleTypes.Crewmate, player);
                                else
                                    pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], player);
                            }
                        }
                        break;
                    default:
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc != player)
                                pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], player);
                        }
                        break;
                }
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
                player.RpcSetDeathReason(DeathReasons.Alive);
                PlayerControl.LocalPlayer.RpcRemoveDeadBody(player.Data);
            }
            player.RpcSetRoleV2(Main.StandardRoles[player.PlayerId]);
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
            player.RpcSetDeathReason(DeathReasons.Alive);
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
            AmongUsClient.Instance.NetIdCnt += 1U;
            MessageWriter msg = MessageWriter.Get(SendOption.None);
			msg.StartMessage(5);
			msg.Write(AmongUsClient.Instance.GameId);
			AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
			msg.EndMessage();
			msg.StartMessage(6);
			msg.Write(AmongUsClient.Instance.GameId);
			msg.WritePacked(int.MaxValue);
			for (uint i = 1; i <= 3; ++i)
			{
			    msg.StartMessage(4);
			    msg.WritePacked(2U);
			    msg.WritePacked(-2);
			    msg.Write((byte)SpawnFlags.None);
			    msg.WritePacked(1);
		        msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
			    msg.StartMessage(1);
			    msg.EndMessage();
			    msg.EndMessage();
			}
			msg.EndMessage();
			AmongUsClient.Instance.SendOrDisconnect(msg);
			msg.Recycle();
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
                player.RpcSetNamePrivate(player.BuildPlayerName(player, false), player, true);
                return;
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Shapeshift, SendOption.Reliable, player.GetClientId());
		    writer.WriteNetObject(PlayerControl.LocalPlayer);
		    writer.Write(false);
		    AmongUsClient.Instance.FinishRpcImmediately(writer);
            new LateTask(() => {
                player.RpcSetNamePrivate(player.BuildPlayerName(player, false), player, true);
                var outfit = player.Data.Outfits[PlayerOutfitType.Default];
                var sender = CustomRpcSender.Create("Set Unshift Button", SendOption.None);
                sender.StartMessage(player.GetClientId());
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
            }, 0.2f);
        }

        public static void RpcDesyncUpdateSystem(this PlayerControl target, SystemTypes systemType, int amount)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, target.GetClientId());
            messageWriter.Write((byte)systemType);
            messageWriter.WriteNetObject(target);
            messageWriter.Write((byte)amount);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }

        public static RoleTypes GetSelfRole(this PlayerControl player)
        {
            if (CustomGamemode.Instance.Gamemode is Gamemodes.BattleRoyale or Gamemodes.PaintBattle or Gamemodes.BaseWars or Gamemodes.ColorWars) return RoleTypes.Impostor;
            if (CustomGamemode.Instance.Gamemode is Gamemodes.BombTag or Gamemodes.KillOrDie or Gamemodes.Jailbreak) return RoleTypes.Shapeshifter;
            return Main.DesyncRoles.ContainsKey((player.PlayerId, player.PlayerId)) ? Main.DesyncRoles[(player.PlayerId, player.PlayerId)] : Main.StandardRoles[player.PlayerId];
        }

        public static List<Vent> GetVentsFromClosest(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            if (ClassicGamemode.instance != null && player.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = player.GetRole() as Droner;
                if (dronerRole != null && dronerRole.RealPosition != null)
                    playerpos = dronerRole.DronePosition;
            }
            List<Vent> vents = new();
            foreach (var vent in ShipStatus.Instance.AllVents)
                vents.Add(vent);
            vents.Sort((v1, v2) => Vector2.Distance(playerpos, v1.transform.position).CompareTo(Vector2.Distance(playerpos, v2.transform.position)));
            return vents;
        }

        public static void RpcSetVentInteraction(this PlayerControl player)
        {
            var ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
            if (ventilationSystem != null)
                VentilationSystemDeterioratePatch.SerializeV2(ventilationSystem, player);
        }

        public static PlayerControl GetClosestImpostor(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p.Data.Role.IsImpostor && p != player)
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            if (pcdistance.Count == 0) return null;
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

        public static void Notify(this PlayerControl player, string message)
        {
            Main.NameMessages[player.PlayerId].Add((message, 0f));
        }

        public static void RpcResetInvisibility(this PlayerControl player)
        {
            if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic || player == null) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.AmOwner && !pc.GetRole().IsImpostor() && pc.GetRole().BaseRole != BaseRoles.Tracker && pc != player)
                {
                    CustomRpcSender sender = CustomRpcSender.Create("ResetInvisibility", SendOption.None);
                    sender.StartMessage(pc.GetClientId());
                    sender.StartRpc(player.NetId, (byte)RpcCalls.Exiled)
                        .EndRpc();
                    var role = Main.DesyncRoles.ContainsKey((player.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(player.PlayerId, pc.PlayerId)] : Main.StandardRoles[player.PlayerId];
                    sender.StartRpc(player.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)role)
                        .Write(true)
                        .EndRpc();
                    sender.StartRpc(player.NetId, (byte)RpcCalls.CancelPet)
                        .EndRpc();
                    sender.StartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(new Vector2(50f, 50f))
                        .Write(player.NetTransform.lastSequenceId)
                        .EndRpc();
                    sender.StartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(new Vector2(50f, 50f))
                        .Write((ushort)(player.NetTransform.lastSequenceId + 16383))
                        .EndRpc();
                    sender.StartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(new Vector2(50f, 50f))
                        .Write((ushort)(player.NetTransform.lastSequenceId + 32767))
                        .EndRpc();
                    sender.StartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(new Vector2(50f, 50f))
                        .Write((ushort)(player.NetTransform.lastSequenceId + 32767 + 16383))
                        .EndRpc();
                    sender.StartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(new Vector2(50f, 50f))
                        .Write(player.NetTransform.lastSequenceId)
                        .EndRpc();
                    sender.StartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(new Vector2(50f, 50f))
                        .Write((ushort)(player.NetTransform.lastSequenceId + 16383))
                        .EndRpc();
                    sender.EndMessage();
                    sender.SendMessage();
                }
            }
        }

        public static void SetChatVisible(this PlayerControl player, bool visible)
        {
            if (player.AmOwner)
            {
                HudManager.Instance.Chat.SetVisible(visible);
		        HudManager.Instance.Chat.HideBanButton();
                return;
            }
            bool isDead = player.Data.IsDead;
            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(player.GetClientId());
            writer.StartMessage(4);
			writer.WritePacked(HudManager.Instance.MeetingPrefab.SpawnId);
			writer.WritePacked(-2);
			writer.Write((byte)SpawnFlags.None);
			writer.WritePacked(1);
            uint netIdCnt = AmongUsClient.Instance.NetIdCnt;
			AmongUsClient.Instance.NetIdCnt = netIdCnt + 1U;
			writer.WritePacked(netIdCnt);
			writer.StartMessage(1);
            writer.WritePacked(0);
			writer.EndMessage();
			writer.EndMessage();
            player.Data.IsDead = visible;
            writer.StartMessage(1);
            writer.WritePacked(player.Data.NetId);
            player.Data.Serialize(writer, true);
            writer.EndMessage();
            writer.StartMessage(2);
            writer.WritePacked(netIdCnt);
			writer.Write((byte)RpcCalls.CloseMeeting);
            writer.EndMessage();
            player.Data.IsDead = isDead;
            writer.StartMessage(1);
            writer.WritePacked(player.Data.NetId);
            player.Data.Serialize(writer, true);
            writer.EndMessage();
            writer.StartMessage(5);
            writer.WritePacked(netIdCnt);
            writer.EndMessage();
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }
    }
}