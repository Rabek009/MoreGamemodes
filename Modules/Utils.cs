using System.Collections.Generic;
using System.Data;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
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
            if (Options.EnableRadar.GetBool()) items.Add(Items.Radar);
            if (Options.EnableSwap.GetBool()) items.Add(Items.Swap);
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool() && Options.CanBeGivenToCrewmate.GetBool()) items.Add(Items.Stop);
            if (Options.EnableNewsletter.GetBool()) items.Add(Items.Newsletter);
            if (Options.EnableCompass.GetBool()) items.Add(Items.Compass);

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
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool()) items.Add(Items.Stop);
            if (Options.EnableNewsletter.GetBool()) items.Add(Items.Newsletter);
            if (Options.EnableCompass.GetBool()) items.Add(Items.Compass);

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
                case Items.TimeSpeeder:
                    return "Decrease voting time";
                case Items.Flash:
                    return "Blind all crewmates";
                case Items.Hack:
                    return "Prevent everyone from doing anything";
                case Items.Camouflage:
                    return "Make everyone look the same";
                case Items.Trap:
                    return "Create deadly trap";
                case Items.MultiTeleport:
                    return "Teleport everyone to you";
                case Items.Bomb:
                    return "Sacrifice yourself to kill nearby players";
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
                    return "Get extra informations";
                case Items.Compass:
                    return "Track other players";
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
                case Items.Radar:
                    return "Radar(Crewmate only): You see reactor flash if impostor is nearby.";
                case Items.Swap:
                    return "Swap(Crewmate only): Swap your tasks with nearest player tasks.";
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
                    return "Newsletter(Both): Sends you information about how amny roles are alive, how people died. Informations refers to moment when item was used.";
                case Items.Compass:
                    return "Compass(Both): Show arrow to all players for short period of time.";
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
            CustomRpcSender sender = CustomRpcSender.Create("Camouflage", SendOption.None);
            foreach (var pc in PlayerControl.AllPlayerControls) 
            {
                sender.RpcSetOutfit(pc, 15, "", "", "pet_test", "");
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(Utils.ColorString(Color.clear, "Player"), ar, true);
            }
            sender.SendMessage();       
        }
        
        public static void RevertCamouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CustomRpcSender sender = CustomRpcSender.Create("RevertCamouflage", SendOption.None);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                sender.RpcSetOutfit(pc, Main.StandardColors[pc.PlayerId], Main.StandardHats[pc.PlayerId], Main.StandardSkins[pc.PlayerId], Main.StandardPets[pc.PlayerId], Main.StandardVisors[pc.PlayerId]);
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)], ar, true);
            }
            sender.SendMessage();
        }

        public static void SendChat(string message, string title)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Main.MessagesToSend.Add((message, 255, title));
        }

        public static void SendSpam(string message)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            for (int i = 1; i <= 20; ++i)
                SendChat(message, "Spam");
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

        public static void CreateDeadBody(Vector2 position, byte colorId)
        {       
            if (!AmongUsClient.Instance.AmHost) return;
            var baseColorId = PlayerControl.LocalPlayer.CurrentOutfit.ColorId;
            var basePos = PlayerControl.LocalPlayer.transform.position;

            Main.IsCreatingBody = true;
            var sender = CustomRpcSender.Create("Create Dead Body", SendOption.None);
            PlayerControl.LocalPlayer.SetColor(colorId);
            sender.StartMessage(-1);
            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetColor)
                .Write(colorId)
                .EndRpc();
            PlayerControl.LocalPlayer.NetTransform.SnapTo(position);
            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(position)
                .Write(PlayerControl.LocalPlayer.NetTransform.lastSequenceId)
                .EndRpc();
            PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer);
            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.MurderPlayer)
                .WriteNetObject(PlayerControl.LocalPlayer)
                .EndRpc();
            PlayerControl.LocalPlayer.NetTransform.SnapTo(basePos);
            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(basePos)
                .Write(PlayerControl.LocalPlayer.NetTransform.lastSequenceId)
                .EndRpc();
            PlayerControl.LocalPlayer.SetColor(baseColorId);
            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetColor)
                .Write(baseColorId)
                .EndRpc();
            sender.EndMessage();
            sender.SendMessage();
            Main.IsCreatingBody = false;
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
                case TabGroup.MainSettings:
                    return HudManager.Instance.UseButton.graphic.sprite;
                case TabGroup.GamemodeSettings:
                    return HudManager.Instance.KillButton.graphic.sprite;
                case TabGroup.AdditionalGamemodes:
                    return HudManager.Instance.ImpostorVentButton.graphic.sprite;
                default:
                    return new Sprite();
            }
        }

        public static void EndPaintBattleGame()
        {
            List<byte> winners = new();
            List<byte> bestPlayers = new();
            float bestRate = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Main.PlayerVotes[pc.PlayerId].Item2 == 0)
                    winners.Add(pc.PlayerId);
                else if ((float)Main.PlayerVotes[pc.PlayerId].Item1 / (float)Main.PlayerVotes[pc.PlayerId].Item2 > bestRate)
                {
                    bestRate = (float)Main.PlayerVotes[pc.PlayerId].Item1 / (float)Main.PlayerVotes[pc.PlayerId].Item2;
                    bestPlayers.Clear();
                    bestPlayers.Add(pc.PlayerId);
                }
                else if ((float)Main.PlayerVotes[pc.PlayerId].Item1 / (float)Main.PlayerVotes[pc.PlayerId].Item2 == bestRate)
                    bestPlayers.Add(pc.PlayerId);
            }
            foreach (var id in bestPlayers)
                winners.Add(id);
            CheckEndCriteriaPatch.StartEndGame(GameOverReason.HumansByTask, winners);
        }
    }
}