using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;

namespace MoreGamemodes
{
    public static class Utils
    {
        public static int GetClientId(this PlayerControl player)
        {
            var client = player.GetClient();
            return client == null ? -1 : client.Id;
        }
        public static InnerNet.ClientData GetClient(this PlayerControl player)
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

        public static string ColorString(Color32 color, string str) => $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";

        public static Items RandomItemCrewmate()
        {
            List<Items> items = new();
            var rand = new System.Random();
            if (Options.EnableTimeSlower.GetBool()) items.Add(Items.TimeSlower);
            if (Options.EnableKnowledge.GetBool()) items.Add(Items.Knowledge);
            if (Options.EnableShield.GetBool()) items.Add(Items.Shield);
            if (Options.EnableGun.GetBool()) items.Add(Items.Gun);
            if (Options.EnableIllusion.GetBool()) items.Add(Items.Illusion);
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool() && Options.CanBeGivenToCrewmate.GetBool()) items.Add(Items.Stop);

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
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool()) items.Add(Items.Stop);

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
                    return "Knowledge(Crewmate only): You can investigate nearest player. Green name means that he's crewmate, red name means impostor. Depending on options target can see that you investigated him. Black name means that this person investigated you";
                case Items.Shield:
                    return "Shield(Crewmate only): You grant yourself a shield for some time. If someone try kill you in this time, he can't. You will see that this person tried to kill you.";
                case Items.Gun:
                    return "Gun(Crewmate only): If nearest player is impostor, you kill him. Otherwise you die.";
                case Items.Illusion:
                    return "Illusion(Crewmate only): If nearest player is impostor, he kills you.";
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
                default:
                    return "INVALID DESCRIPTION LONG";
            }
        }

        public static void SyncSettingsToAll(IGameOptions opt)
        {
            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.Write(opt.Version);
            writer.StartMessage(0);
            writer.Write((byte)opt.GameMode);
            if (opt.TryCast<NormalGameOptionsV07>(out var normalOpt))
                NormalGameOptionsV07.Serialize(writer, normalOpt);
            else if (opt.TryCast<HideNSeekGameOptionsV07>(out var hnsOpt))
                HideNSeekGameOptionsV07.Serialize(writer, hnsOpt);
            else
            {
                writer.Recycle();
            }
            writer.EndMessage();

            var ByteArray = new byte[0];

            Span<byte> writerSpan = new(writer.Buffer, 1, writer.Length - 1);
            if (ByteArray == null || ByteArray.Length != writerSpan.Length) ByteArray = new byte[writerSpan.Length];
            for (int i = 0; i < ByteArray.Length; ++i)
                ByteArray[i] = writerSpan[i];

            for (byte i = 0; i < GameManager.Instance.LogicComponents.Count; ++i)
            {
                if (GameManager.Instance.LogicComponents[i].TryCast<LogicOptions>(out _))
                {
                    var writer2 = MessageWriter.Get(SendOption.Reliable);

                    writer2.StartMessage(Tags.GameData);
                    {
                        writer2.Write(AmongUsClient.Instance.GameId);
                        writer2.StartMessage(1);
                        {
                            writer2.WritePacked(GameManager.Instance.NetId);
                            writer2.StartMessage(i);
                            {
                                writer2.WriteBytesAndSize(ByteArray);
                            }
                            writer2.EndMessage();
                        }
                        writer2.EndMessage();
                    }
                    writer2.EndMessage();

                    AmongUsClient.Instance.SendOrDisconnect(writer2);
                    writer2.Recycle();
                }
            }
        }
        public static bool IsActive(SystemTypes type)
        {
            int mapId = Main.RealOptions.GetByte(ByteOptionNames.MapId);
            switch (type)
            {
                case SystemTypes.Electrical:
                    {
                        var SwitchSystem = ShipStatus.Instance.Systems[type].Cast<SwitchSystem>();
                        return SwitchSystem != null && SwitchSystem.IsActive;
                    }
                case SystemTypes.Reactor:
                    {
                        if (mapId == 2) return false;
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
                        if (mapId == 1)
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
                default:
                    return false;
            }
        }

        public static void Camouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;

            PlayerControl.LocalPlayer.RpcSetColor(15);
            PlayerControl.LocalPlayer.RpcSetName(Utils.ColorString(Color.clear, "Player"));
            PlayerControl.LocalPlayer.RpcSetHat("");
            PlayerControl.LocalPlayer.RpcSetSkin("");
            PlayerControl.LocalPlayer.RpcSetPet("pet_test");
            PlayerControl.LocalPlayer.RpcSetVisor("");

            var ShapeshifterLeaveSkin = GameOptionsManager.Instance.currentGameOptions.GetBool(BoolOptionNames.ShapeshifterLeaveSkin);
            GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
            SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead)
                    player.RpcShapeshiftV2(PlayerControl.LocalPlayer, true);
            }
            GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, ShapeshifterLeaveSkin);
            SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.Role == RoleTypes.Shapeshifter)
                    pc.RpcSetAbilityCooldown(255f);
            }
        }
        public static void RevertCamouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;

            PlayerControl.LocalPlayer.RpcSetColor((byte)Main.StandardColors[PlayerControl.LocalPlayer.PlayerId]);
            PlayerControl.LocalPlayer.RpcSetName(Main.StandardNames[PlayerControl.LocalPlayer.PlayerId]);
            PlayerControl.LocalPlayer.RpcSetHat(Main.StandardHats[PlayerControl.LocalPlayer.PlayerId]);
            PlayerControl.LocalPlayer.RpcSetSkin(Main.StandardSkins[PlayerControl.LocalPlayer.PlayerId]);
            PlayerControl.LocalPlayer.RpcSetPet("pet_clank");
            PlayerControl.LocalPlayer.RpcSetVisor(Main.StandardVisors[PlayerControl.LocalPlayer.PlayerId]);

            var ShapeshifterLeaveSkin = GameOptionsManager.Instance.currentGameOptions.GetBool(BoolOptionNames.ShapeshifterLeaveSkin);
            GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
            SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);        
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead)
                    player.RpcRevertShapeshiftV2(true);
            }
            GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, ShapeshifterLeaveSkin);
            SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.Role == RoleTypes.Shapeshifter)
                    pc.RpcSetAbilityCooldown(255f);
            }

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)], ar, true);
            }          
        }

        public static void SendChat(string message, string messageName)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcSendMessage(message, messageName);
        }

        public static byte[] ToBytes(this IGameOptions gameOptions)
        {
            return GameOptionsManager.Instance.gameOptionsFactory.ToBytes(gameOptions);
        }

        public static IGameOptions DeepCopy(this IGameOptions opt)
        {
            return GameOptionsManager.Instance.gameOptionsFactory.FromBytes(opt.ToBytes());
        }

        public static void SendGameData()
        {
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage(5);
            {
                writer.Write(AmongUsClient.Instance.GameId);
                writer.StartMessage(1);
                {
                    writer.WritePacked(GameData.Instance.NetId);
                    GameData.Instance.Serialize(writer, true);

                }
                writer.EndMessage();
            }
            writer.EndMessage();

            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public static void CreateNPC(byte playerId, Vector3 position, byte[] tasks, byte colorId, string name, string hatId, string skinId, string petId, string visorId, uint level, string namePlateId)
        {
            var bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            bot.PlayerId = playerId;
            GameData.Instance.AddPlayer(bot);
            AmongUsClient.Instance.Spawn(bot, -2, SpawnFlags.None);
            bot.transform.position = position;
            bot.NetTransform.enabled = true;
            GameData.Instance.RpcSetTasks(bot.PlayerId, tasks);
            bot.RpcSetColor(colorId);
            bot.RpcSetName(name);
            bot.RpcSetHat(hatId);
            bot.RpcSetSkin(skinId);
            bot.RpcSetPet(petId);
            bot.RpcSetVisor(visorId);
            bot.RpcSetLevel(level);
            bot.RpcSetNamePlate(namePlateId);
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
    }
}