using System.Collections.Generic;
using System.Data;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using System.IO;
using System.Reflection;
using System;

using Object = UnityEngine.Object;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace MoreGamemodes
{
    public static class Utils
    {
        public static int GetClientId(this PlayerControl player)
        {
            var client = player.GetClient();
            return client == null ? -1 : client.Id;
        }
        public static ClientData GetClient(this PlayerControl player)
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }
        public static PlayerControl GetPlayerById(byte id)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.PlayerId == id)
                    return pc;
            }
            return null;
        }

        public static ClientData GetClientById(int id)
        {
            try
            {
                var client = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Id == id);
                return client;
            }
            catch
            {
                return null;
            }
        }

        public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return "";
        string puid = player.ProductUserId;
        using SHA256 sha256 = SHA256.Create();
        
        // get sha-256 hash
        byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
        string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();

        // pick front 5 and last 4
        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }

        public static string ColorString(Color32 color, string str) => $"<#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
        public static string ColorToHex(Color32 color) => $"#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}";
        public static bool CheckColorHex(string ColorCode)
        {
            Regex regex = new("^[0-9A-Fa-f]{6}$");
            if (!regex.IsMatch(ColorCode)) return false;
            return true;
        }
        public static bool CheckGradientCode(string ColorCode)
        {
            Regex regex = new(@"^[0-9A-Fa-f]{6}\s[0-9A-Fa-f]{6}$");
             if (!regex.IsMatch(ColorCode)) return false;
             return true;
        } 
        public static Items RandomItemCrewmate()
        {
            List<Items> items = new();
            var rand = new System.Random();
            if (Options.EnableTimeSlower.GetBool()) items.Add(Items.TimeSlower);
            if (Options.EnableKnowledge.GetBool()) items.Add(Items.Knowledge);
            if (Options.EnableShield.GetBool()) items.Add(Items.Shield);
            if (Options.EnableGun.GetBool()) items.Add(Items.Gun);
            if (Options.EnableIllusion.GetBool()) items.Add(Items.Illusion);
            if (Options.EnableRadar.GetBool()) items.Add(Items.Radar);
            if (Options.EnableSwap.GetBool()) items.Add(Items.Swap);
            if (Options.EnableMedicine.GetBool()) items.Add(Items.Medicine);
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool() && Options.CanBeGivenToCrewmate.GetBool()) items.Add(Items.Stop);
            if (Options.EnableNewsletter.GetBool()) items.Add(Items.Newsletter);
            if (Options.EnableCompass.GetBool()) items.Add(Items.Compass);
            if (Options.EnableBooster.GetBool()) items.Add(Items.Booster);

            return items[rand.Next(0, items.Count)];
        }

        public static Items RandomItemImpostor()
        {
            List<Items> items = new();
            var rand = new System.Random();
            if (Options.EnableTimeSpeeder.GetBool()) items.Add(Items.TimeSpeeder);
            if (Options.EnableFlash.GetBool()) items.Add(Items.Flash);
            if (Options.EnableHack.GetBool()) items.Add(Items.Hack);
            if (Options.EnableCamouflage.GetBool()) items.Add(Items.Camouflage);
            if (Options.EnableMultiTeleport.GetBool()) items.Add(Items.MultiTeleport);
            if (Options.EnableBomb.GetBool()) items.Add(Items.Bomb);
            if (Options.EnableTrap.GetBool()) items.Add(Items.Trap);
            if (Options.EnableTeamChanger.GetBool()) items.Add(Items.TeamChanger);
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool()) items.Add(Items.Stop);
            if (Options.EnableNewsletter.GetBool()) items.Add(Items.Newsletter);
            if (Options.EnableCompass.GetBool()) items.Add(Items.Compass);
            if (Options.EnableBooster.GetBool()) items.Add(Items.Booster);

            return items[rand.Next(0, items.Count)];
        }

        public static string ItemString(Items item)
        {
            switch (item)
            {
                case Items.TimeSlower:
                    return "Time Slower";
                case Items.Knowledge:
                    return "Knowledge";
                case Items.Shield:
                    return "Shield";
                case Items.Gun:
                    return "Gun";
                case Items.Illusion:
                    return "Illusion";
                case Items.Radar:
                    return "Radar";
                case Items.Swap:
                    return "Swap";
                case Items.Medicine:
                    return "Medicine";
                case Items.TimeSpeeder:
                    return "Time Speeder";
                case Items.Flash:
                    return "Flash";
                case Items.Hack:
                    return "Hack";
                case Items.Camouflage:
                    return "Camouflage";
                case Items.MultiTeleport:
                    return "Multi Teleport";
                case Items.Bomb:
                    return "Bomb";
                case Items.Trap:
                    return "Trap";
                case Items.TeamChanger:
                    return "Team Changer";
                case Items.Teleport:
                    return "Teleport";
                case Items.Button:
                    return "Button";
                case Items.Finder:
                    return "Finder";
                case Items.Rope:
                    return "Rope";
                case Items.Stop:
                    return "Stop";
                case Items.Newsletter:
                    return "Newsletter";
                case Items.Compass:
                    return "Compass";
                case Items.Booster:
                    return "Booster";
                default:
                    return "INVALID ITEM";
            }
        }

        public static string ItemDescription(Items item)
        {
            switch (item)
            {
                case Items.TimeSlower:
                    return "Increase voting time";
                case Items.Knowledge:
                    return "Find out if someone is bad";
                case Items.Shield:
                    return "Grant yourself a shield";
                case Items.Gun:
                    return "Shoot impostor";
                case Items.Illusion:
                    return "Make impostor kill you";
                case Items.Radar:
                    return "See if impostors are near";
                case Items.Swap:
                    return "Swap tasks with someone";
                case Items.Medicine:
                    return "Revive a dead player (report)";
                case Items.TimeSpeeder:
                    return "Decrease voting time";
                case Items.Flash:
                    return "Blind all crewmates";
                case Items.Hack:
                    return "Prevent everyone from doing anything";
                case Items.Camouflage:
                    return "Make everyone look the same";
                case Items.MultiTeleport:
                    return "Teleport everyone to you";
                case Items.Bomb:
                    return "Sacrifice yourself to kill nearby players";
                case Items.Trap:
                    return "Create deadly trap";
                case Items.TeamChanger:
                    return "Turn someone into impostor";
                case Items.Teleport:
                    return "Teleport to random vent";
                case Items.Button:
                    return "Call emergency from anywhere";
                case Items.Finder:
                    return "Teleport to nearest player";
                case Items.Rope:
                    return "Teleport nearest player to you";
                case Items.Stop:
                    return "Type /stop command to end meeting";
                case Items.Newsletter:
                    return "Type /info to get extra informations";
                case Items.Compass:
                    return "Track other players";
                case Items.Booster:
                    return "Increase your speed";
                default:
                    return "INVALID DESCRIPTION";
            }
        }

        public static string ItemDescriptionLong(Items item)
        {
            switch (item)
            {
                case Items.TimeSlower:
                    return "Time Slower(Crewmate only): Increase discussion and voting time by amount in settings.";
                case Items.Knowledge:
                    return "Knowledge(Crewmate only): You can investigate nearby player. Green name means that he's crewmate, red name means impostor. Depending on options target can see that you investigated him. Black name means that this person investigated you";
                case Items.Shield:
                    return "Shield(Crewmate only): You grant yourself a shield for some time. If someone try kill you in this time, he can't. You will see that this person tried to kill you.";
                case Items.Gun:
                    return "Gun(Crewmate only): If nearby player is impostor, you kill him. Otherwise you die.";
                case Items.Illusion:
                    return "Illusion(Crewmate only): If nearby player is impostor, he kills you.";
                case Items.Radar:
                    return "Radar(Crewmate only): You see reactor flash if impostor is nearby.";
                case Items.Swap:
                    return "Swap(Crewmate only): Swap your tasks with nearby player tasks.";
                case Items.Medicine:
                    return "Medicine(Crewmate only): Use report button to bring back dead player. But depending on options you die after using it";
                case Items.TimeSpeeder:
                    return "Time Speeder(Impostor only): Increase discussion and voting time by amount in settings.";
                case Items.Flash:
                    return "Flash(Impostor only): Throws flash for few seconds. During that time crewmates are blind, but impostor's vision is decreased.";
                case Items.Hack:
                    return "Hack(Impostor only): For some time crewmates can't do anything. Depending on options hack affects impostor. Yellow name means hack active. Hack prevents from: reporting, opening doors, repairing sabotages, venting, using items, using role abilities, killing, sabotaging, calling meetings or even doing tasks.";
                case Items.Camouflage:
                    return "Camouflage(Impostor only): Everyone turns into gray bean for few seconds.";
                case Items.MultiTeleport:
                    return "Multi Teleport(Impostor only): Everyone gets teleported to you.";
                case Items.Bomb:
                    return "Bomb(Impostor only): Everyone near you die, but you sacrifice yourself. Depending on options explosion can kill other impostors. If no one is alive after explosion impostors still win! You can't use bomb 10 seconds after meeting or multi teleport.";
                case Items.Trap:
                    return "Trap(Impostor only): Place trap that kills first player touches it. Trap is completely invisible and works after few seconds from placing.";
                case Items.TeamChanger:
                    return "Team Changer(Impostor only): You can turn nearby crewmate into impostor, but you die after doing it.";
                case Items.Teleport:
                    return "Teleport(Both): Teleports you to random vents.";
                case Items.Button:
                    return "Button(Both): Call meeting instantly. Depending on options you can use this during sabotage or not.";
                case Items.Finder:
                    return "Finder(Both): Teleports you to nearest player.";
                case Items.Rope:
                    return "Rope(Both): Teleports nearest player to you.";
                case Items.Stop:
                    return "Stop(Both/Impostor only): You can instantly end meeting without anyone ejected by typing /stop command.";
                case Items.Newsletter:
                    return "Newsletter(Both): Sends you information about how amny roles are alive, how people died. Use this item by typing /info in chat";
                case Items.Compass:
                    return "Compass(Both): Show arrow to all players for short period of time.";
                case Items.Booster:
                    return "Booster(Both): Increases your speed temporarily.";
                default:
                    return "INVALID DESCRIPTION LONG";
            }
        }

        public static bool IsActive(SystemTypes type)
        {
            int mapId = Main.RealOptions.GetByte(ByteOptionNames.MapId);
            switch (type)
            {
                case SystemTypes.Electrical:
                    {
                        if (mapId == 5) return false;
                        var SwitchSystem = ShipStatus.Instance.Systems[type].Cast<SwitchSystem>();
                        return SwitchSystem != null && SwitchSystem.IsActive;
                    }
                case SystemTypes.Reactor:
                    {
                        if (mapId == 2 || mapId == 4) return false;
                        else if (mapId == 4)
                        {
                            var HeliSabotageSystem = ShipStatus.Instance.Systems[type].Cast<HeliSabotageSystem>();
                            return HeliSabotageSystem != null && HeliSabotageSystem.IsActive;
                        }
                        else
                        {
                            var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                            return ReactorSystemType != null && ReactorSystemType.IsActive;
                        }
                    }
                case SystemTypes.Laboratory:
                    {
                        if (mapId != 2) return false;
                        var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                        return ReactorSystemType != null && ReactorSystemType.IsActive;
                    }
                case SystemTypes.LifeSupp:
                    {
                        if (mapId is 2 or 4) return false;
                        var LifeSuppSystemType = ShipStatus.Instance.Systems[type].Cast<LifeSuppSystemType>();
                        return LifeSuppSystemType != null && LifeSuppSystemType.IsActive;
                    }
                case SystemTypes.Comms:
                    {
                        if (mapId is 1 or 5)
                        {
                            var HqHudSystemType = ShipStatus.Instance.Systems[type].Cast<HqHudSystemType>();
                            return HqHudSystemType != null && HqHudSystemType.IsActive;
                        }
                        else
                        {
                            var HudOverrideSystemType = ShipStatus.Instance.Systems[type].Cast<HudOverrideSystemType>();
                            return HudOverrideSystemType != null && HudOverrideSystemType.IsActive;
                        }
                    }
                case SystemTypes.MushroomMixupSabotage:
                    {
                        if (mapId != 5) return false;
                        var MushroomMixupSabotageSystemType = ShipStatus.Instance.Systems[type].Cast<MushroomMixupSabotageSystem>();
                        return MushroomMixupSabotageSystemType != null && MushroomMixupSabotageSystemType.IsActive;
                    }
                case SystemTypes.HeliSabotage:
                    {
                        if (mapId != 4) return false;
                        var HeliSabotageSystemType = ShipStatus.Instance.Systems[type].Cast<HeliSabotageSystem>();
                        return HeliSabotageSystemType != null && HeliSabotageSystemType.IsActive;
                    }
                default:
                    return false;
            }
        }

        public static bool IsSabotage()
        {
            return IsActive(SystemTypes.LifeSupp) || IsActive(SystemTypes.Reactor) || IsActive(SystemTypes.Laboratory) || IsActive(SystemTypes.Electrical) || IsActive(SystemTypes.Comms) || IsActive(SystemTypes.MushroomMixupSabotage) || IsActive(SystemTypes.HeliSabotage);
        }

        public static void SyncSettings(IGameOptions opt, int targetClientId = -1)
        {
            for (int i = 0; i < GameManager.Instance.LogicComponents.Count; ++i)
            {
                if (GameManager.Instance.LogicComponents[i].TryCast<LogicOptions>(out var _))
                {
                    Il2CppStructArray<byte> byteArray = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt, false);
                    MessageWriter writer = MessageWriter.Get(SendOption.None);
                    writer.StartMessage(targetClientId == -1 ? Tags.GameData : Tags.GameDataTo);
                    {
                        writer.Write(AmongUsClient.Instance.GameId);
                        if (targetClientId != -1) writer.WritePacked(targetClientId);
                        writer.StartMessage(1);
                        {
                            writer.WritePacked(GameManager.Instance.NetId);
                            writer.StartMessage((byte)i);
				            writer.WriteBytesAndSize(byteArray);
				            writer.EndMessage();
                        }
                        writer.EndMessage();
                    }
                    writer.EndMessage();

                    AmongUsClient.Instance.SendOrDisconnect(writer);
                    writer.Recycle();
                }
            }
        }

        public static void SyncAllSettings()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.SyncPlayerSettings();
        }

        public static void Camouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.Role == RoleTypes.Shapeshifter)
                    pc.RpcShapeshift(pc, false);
                pc.RpcSetOutfit(15, "", "", "pet_test", "");
            }
        }
        
        public static void RevertCamouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcSetOutfit(Main.StandardColors[pc.PlayerId], Main.StandardHats[pc.PlayerId], Main.StandardSkins[pc.PlayerId], Main.StandardPets[pc.PlayerId], Main.StandardVisors[pc.PlayerId]);
        }

        public static void SendChat(string message, string title)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (message.Length > 1000)
            {
                foreach (var text in message.SplitMessage())
                    SendChat(text, title);
                return;
            }
            Main.MessagesToSend.Add((message, 255, title));
        }

        public static void SendSpam(string message)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            for (int i = 1; i <= 20; ++i)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, message);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == PlayerControl.LocalPlayer) continue;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SendChat, SendOption.None, pc.GetClientId());
                    writer.Write(message);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }

        public static void SendGameData()
        {
            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);
            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                writer.StartMessage(1);
                writer.WritePacked(playerInfo.NetId);
                playerInfo.Serialize(writer, false);
                writer.EndMessage();
            }
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public static void CreateDeadBody(Vector3 position, byte colorId, PlayerControl deadBodyParent)
        {
            var baseColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId;
            PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId = colorId;
            DeadBody deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
		    deadBody.enabled = false;
		    deadBody.ParentId = deadBodyParent.PlayerId;
            foreach (SpriteRenderer b in deadBody.bodyRenderers)
            {
                PlayerControl.LocalPlayer.SetPlayerMaterialColors(b);
            }
		    PlayerControl.LocalPlayer.SetPlayerMaterialColors(deadBody.bloodSplatter);
		    Vector3 vector = position + PlayerControl.LocalPlayer.KillAnimations[0].BodyOffset;
		    vector.z = vector.y / 1000f;
		    deadBody.transform.position = vector;
            deadBody.enabled = true;
            PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId = baseColorId;
        }

        public static void RpcCreateDeadBody(Vector3 position, byte colorId, PlayerControl deadBodyParent)
        {
            if (deadBodyParent == null || !Main.GameStarted) return;
            CreateDeadBody(position, colorId, deadBodyParent);
            var sender = CustomRpcSender.Create("Create Dead Body", SendOption.None);
            MessageWriter writer = sender.stream;
            sender.StartMessage(-1);
            sender.StartRpc(deadBodyParent.NetId, (byte)RpcCalls.SetColor)
                .Write(deadBodyParent.Data.NetId)
                .Write(colorId)
                .EndRpc();
            PlayerControl.LocalPlayer.NetTransform.lastSequenceId += 328;
            sender.StartRpc(PlayerControl.LocalPlayer.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(position)
                .Write((ushort)(PlayerControl.LocalPlayer.NetTransform.lastSequenceId + 8))
                .EndRpc();
            if (deadBodyParent != PlayerControl.LocalPlayer)
            {
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.NetId);
                    writer.Write(deadBodyParent.PlayerId);
                }
                writer.EndMessage();
            }
            sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.MurderPlayer)
                .WriteNetObject(PlayerControl.LocalPlayer)
                .Write((int)MurderResultFlags.Succeeded)
                .EndRpc();
            if (deadBodyParent != PlayerControl.LocalPlayer)
            {
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.NetId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                } 
                writer.EndMessage();
            }
            sender.StartRpc(deadBodyParent.NetId, (byte)RpcCalls.SetColor)
                .Write(deadBodyParent.Data.NetId)
                .Write(deadBodyParent.CurrentOutfit.ColorId)
                .EndRpc();
            NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId);
            writer.StartMessage(1);
            writer.WritePacked(playerInfo.NetId);
            playerInfo.Serialize(writer, false);
            writer.EndMessage();
            sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.Shapeshift)
                .WriteNetObject(GetPlayerById(Main.AllShapeshifts[PlayerControl.LocalPlayer.PlayerId]))
                .Write(false)
                .EndRpc();
            sender.EndMessage();
            sender.SendMessage();
        }

        public static string GetArrow(Vector3 from, Vector3 to)
        {
            var dir = to - from;
            byte index;
            if (dir.magnitude < 2)
            {
                index = 8;
            }
            else
            {
                var angle = Vector3.SignedAngle(Vector3.down, dir, Vector3.back) + 180 + 22.5;
                index = (byte)(((int)(angle / 45)) % 8);
            }
            return "↑↗→↘↓↙←↖・"[index].ToString();
        }

        public static string BodyTypeString(SpeedrunBodyTypes bodyType)
        {
            switch (bodyType)
            {
                case SpeedrunBodyTypes.Crewmate:
                    return "Crewmate";
                case SpeedrunBodyTypes.Engineer:
                    return "Engineer";
                case SpeedrunBodyTypes.Ghost:
                    return "Ghost";
                default:
                    return "INVALID BODY TYPE";
            }
        }

        public static Sprite GetTabSprite(TabGroup tab)
        {
            switch (tab)
            {
                case TabGroup.ModSettings:
                    return LoadSprite("MoreGamemodes.Resources.TabIcon_ModSettings.png", 100f);
                case TabGroup.GamemodeSettings:
                    return LoadSprite("MoreGamemodes.Resources.TabIcon_GamemodeSettings.png", 100f);
                case TabGroup.AdditionalGamemodes:
                    return LoadSprite("MoreGamemodes.Resources.TabIcon_AdditionalGamemodes.png", 100f);
                default:
                    return null;
            }
        }

        public static string GetTabName(TabGroup tab)
        {
            switch (tab)
            {
                case TabGroup.ModSettings:
                    return "Mod Settings";
                case TabGroup.GamemodeSettings:
                    return "Gamemode Settings";
                case TabGroup.AdditionalGamemodes:
                    return "Additional Gamemodes";
                default:
                    return "";
            }
        }

        public static void EndPaintBattleGame()
        {
            if (PaintBattleGamemode.instance == null) return;
            List<byte> winners = new();
            List<byte> bestPlayers = new();
            float bestRate = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item2 == 0)
                    winners.Add(pc.PlayerId);
                else if ((float)PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item1 / (float)PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item2 > bestRate)
                {
                    bestRate = (float)PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item1 / (float)PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item2;
                    bestPlayers.Clear();
                    bestPlayers.Add(pc.PlayerId);
                }
                else if ((float)PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item1 / (float)PaintBattleGamemode.instance.PlayerVotes[pc.PlayerId].Item2 == bestRate)
                    bestPlayers.Add(pc.PlayerId);
            }
            foreach (var id in bestPlayers)
                winners.Add(id);
            CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.HumansByTask, winners);
        }

        public static string RoleToString(RoleTypes role, Gamemodes gamemode = Gamemodes.Classic)
        {
            if (role == RoleTypes.Crewmate && gamemode == Gamemodes.HideAndSeek) return "Hider";
            if (role == RoleTypes.Crewmate) return "Crewmate";
            if (role == RoleTypes.Scientist) return "Scientist";
            if (role == RoleTypes.Engineer && gamemode == Gamemodes.ShiftAndSeek) return "Hider";
            if (role == RoleTypes.Engineer) return "Engineer";
            if (role == RoleTypes.GuardianAngel) return "Guardian Angel";
            if (role == RoleTypes.CrewmateGhost && (gamemode == Gamemodes.HideAndSeek || gamemode == Gamemodes.ShiftAndSeek)) return "Hider Ghost";
            if (role == RoleTypes.CrewmateGhost) return "Crewmate Ghost";
            if (role == RoleTypes.Impostor && gamemode == Gamemodes.HideAndSeek) return "Seeker";
            if (role == RoleTypes.Impostor) return "Impostor";
            if (role == RoleTypes.Shapeshifter && gamemode == Gamemodes.ShiftAndSeek) return "Shifter";
            if (role == RoleTypes.Shapeshifter) return "Shapeshifter";
            if (role == RoleTypes.ImpostorGhost && (gamemode == Gamemodes.HideAndSeek || gamemode == Gamemodes.ShiftAndSeek)) return "Seeker Ghost";
            if (role == RoleTypes.ImpostorGhost) return "Impostor Ghost";
            if (role == RoleTypes.Noisemaker) return "Noisemaker";
            if (role == RoleTypes.Phantom) return "Phantom";
            if (role == RoleTypes.Tracker) return "Tracker";
            return "???";
        }

        public static string DeathReasonToString(DeathReasons reason)
        {
            if (reason == DeathReasons.Alive) return "Alive";
            if (reason == DeathReasons.Killed) return "Killed";
            if (reason == DeathReasons.Exiled) return "Exiled";
            if (reason == DeathReasons.Disconnected) return "Disconnected";
            if (reason == DeathReasons.Command) return "Command";
            if (reason == DeathReasons.Bombed) return "Bombed";
            if (reason == DeathReasons.Misfire) return "Misfire";
            if (reason == DeathReasons.Suicide) return "Suicide";
            if (reason == DeathReasons.Trapped) return "Trapped";
            if (reason == DeathReasons.Escaped) return "Escaped";
            return "???";
        }

        public static bool IsImpostor(this RoleTypes role)
        {
            if (role == RoleTypes.Impostor) return true;
            if (role == RoleTypes.Shapeshifter) return true;
            if (role == RoleTypes.Phantom) return true;
            if (role == RoleTypes.ImpostorGhost) return true;
            return false;
        }

        public static void SetChatVisible()
        {
            if (GameManager.Instance.LogicFlow.IsGameOverDueToDeath()) return;
            MeetingHud.Instance = Object.Instantiate(HudManager.Instance.MeetingPrefab);
            MeetingHud.Instance.ServerStart(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.Spawn(MeetingHud.Instance, -2, SpawnFlags.None);
            MeetingHud.Instance.RpcClose();
        }

        public static string TrackingZombiesModeString(TrackingZombiesModes trackingZombiesMode)
        {
            switch (trackingZombiesMode)
            {
                case TrackingZombiesModes.None:
                    return "None";
                case TrackingZombiesModes.Nearest:
                    return "Nearest";
                case TrackingZombiesModes.Every:
                    return "All";
                default:
                    return "INVALID TRACKING ZOMBIES MODE";
            }
        }

        public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
        {
            Sprite sprite = null;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray());
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f), pixelsPerUnit);
            return sprite;
        }

        public static void RpcCreateExplosion(float size, float duration, Vector2 position)
        {
            new Explosion(size, duration, position);
        }

        public static void RpcCreateTrapArea(float radius, float waitDuration, Vector2 position, List<byte> visibleList)
        {
            new TrapArea(radius, waitDuration, position, visibleList);
        }

        public static void RpcSetDesyncRoles(RoleTypes selfRole, RoleTypes othersRole)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                new LateTask(() => SetDesyncRoleForPlayer(pc, selfRole, othersRole), 0f);
        }

        public static void SetDesyncRoleForPlayer(PlayerControl player, RoleTypes selfRole, RoleTypes othersRole)
        {
            int assignedRoles = 0;
            int playerCount = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == player) continue;
                ++playerCount;
                if (pc.Data != null && (RpcSetRolePatch.RoleAssigned[pc.PlayerId] || pc.Data.Disconnected))
                    ++assignedRoles;
            }
            if (assignedRoles >= playerCount)
            {
                RpcSetRolePatch.RoleAssigned[player.PlayerId] = true;
                new LateTask(() => {
                    Dictionary<byte, bool> Disconnected = new();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        Disconnected[pc.PlayerId] = pc.Data.Disconnected;
                        pc.Data.Disconnected = true;
                    }
                    SendGameData();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        pc.Data.Disconnected = Disconnected[pc.PlayerId];
                }, 0.5f);
                new LateTask(() => {
                    player.RpcSetDesyncRoleV2(selfRole, player);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc == player) continue;
                        player.RpcSetDesyncRoleV2(othersRole, pc);
                    }
                }, 1f);
                new LateTask(() => {
                    foreach (var pc in PlayerControl.AllPlayerControls)
						PlayerNameColor.Set(pc);
					DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
					DestroyableSingleton<HudManager>.Instance.HideGameLoader();
                }, 1.2f);
                new LateTask(() => SendGameData(), 1.5f);
                return;
            }
            RpcSetRolePatch.RoleAssigned[player.PlayerId] = true;
            player.RpcSetDesyncRoleV2(selfRole, player);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == player) continue;
                player.RpcSetDesyncRoleV2(othersRole, pc);
            }
        }

        public static void DestroyTranslator(this GameObject obj)
        {
            var translator = obj.GetComponent<TextTranslatorTMP>();
            if (translator != null)
            {
                Object.Destroy(translator);
            }
        }

        public static void DestroyTranslator(this MonoBehaviour obj) => obj.gameObject.DestroyTranslator();

        public static void SendGameOptionsMessage(PlayerControl player = null)
        {
            Dictionary<TabGroup, string> Messages = new();
            foreach (var tab in Enum.GetValues<TabGroup>())
                Messages.Add(tab, "");
            for (int index = 0; index < OptionItem.AllOptions.Count; index++)
            {
                var option = OptionItem.AllOptions[index];
                var tab = option.Tab;
                var enabled = !option.IsHiddenOn(Options.CurrentGamemode) && (option.Parent == null || (!option.Parent.IsHiddenOn(Options.CurrentGamemode) && option.Parent.GetBool()));
                if (!enabled || option is PresetOptionItem) continue;
                if (Messages[tab] != "")
                    Messages[tab] += "\n";
                if (option.IsHeader || option is TextOptionItem)
                {
                    if (Messages[tab] != "")
                        Messages[tab] += "\n";
                    Messages[tab] += "<b><u>";
                }

                if (option.Parent?.Parent?.Parent != null)
                    Messages[tab] += "           → ";
                else if (option.Parent?.Parent != null)
                    Messages[tab] += "      → ";
                else if (option.Parent != null)
                    Messages[tab] += " → ";
                Messages[tab] += option.GetName(option.NameColor == Color.white);

                if (option.IsHeader || option is TextOptionItem)
                    Messages[tab] += "</b></u>";
                if (option is TextOptionItem) continue;

                Messages[tab] += ": ";
                if (option is BooleanOptionItem)
                    Messages[tab] += option.GetBool() ? "✓" : "X";
                else if (option is IntegerOptionItem)
                    Messages[tab] += option.ApplyFormat(option.GetInt().ToString());
                else if (option is FloatOptionItem)
                    Messages[tab] += option.ApplyFormat(option.GetFloat().ToString());
                else if (option is StringOptionItem)
                    Messages[tab] += option.GetString();
            }
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                Messages[tab] = "<b><size=125%>" + GetTabName(tab) + "</size></b>\n\n" + Messages[tab];
                if (player == null)
                    SendChat(Messages[tab], "Options");
                else
                    player.RpcSendMessage(Messages[tab], "Options");
            }
        }

        public static List<string> SplitMessage(this string LongMsg)
        {
            List<string> result = new();
            var lines = LongMsg.Split('\n');
            var shortenedtext = string.Empty;

            foreach (var line in lines)
            {

                if (shortenedtext.Length + line.Length < 1000)
                {
                    shortenedtext += line + "\n";
                    continue;
                }

                if (shortenedtext.Length >= 1000) result.AddRange(shortenedtext.Chunk(1000).Select(x => new string(x)));
                else result.Add(shortenedtext);
                shortenedtext = line + "\n";
            }
            if (shortenedtext.Length > 0) result.Add(shortenedtext);
            return result;
        }

        public static bool IsValidFriendCode(string friendCode)
        {
            if (string.IsNullOrEmpty(friendCode)) return false;
            if (friendCode.Length < 7) return false;
            if (friendCode[friendCode.Length - 5] != '#') return false;

            for (int i = 0; i < friendCode.Length - 5; ++i)
            {
                if (friendCode[i] < 'a' || friendCode[i] > 'z')
                    return false;
            }

            for (int i = friendCode.Length - 4; i < friendCode.Length; ++i)
            {
                if (friendCode[i] < '0' || friendCode[i] > '9')
                    return false;
            }
            return true;
        }
    }
}