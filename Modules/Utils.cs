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

        public static Vent GetVentById(int id)
        {
            if (!ShipStatus.Instance) return null;
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                if (vent.Id == id)
                    return vent;
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

        public static string ColorString(Color32 color, string str) => $"<#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
        public static string ColorToHex(Color32 color) => $"#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}";

        public static bool IsActive(SystemTypes type)
        {
            int mapId = Main.RealOptions.GetByte(ByteOptionNames.MapId);
            switch (type)
            {
                case SystemTypes.Electrical:
                    {
                        if (mapId >= 5) return false;
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
                        if (mapId == 1 || mapId >= 5)
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
                        if (mapId < 5) return false;
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
            PlayerControl playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = deadBodyParent.PlayerId;
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
            if (PlayerControl.AllPlayerControls.Contains(playerControl))
                PlayerControl.AllPlayerControls.Remove(playerControl);
            new LateTask(() => {
                byte colorId2 = (byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId;
                var sender = CustomRpcSender.Create("Create Dead Body", SendOption.None);
                MessageWriter writer = sender.stream;
                sender.StartMessage(-1);
                writer.StartMessage(1);
                writer.WritePacked(playerControl.Data.NetId);
                playerControl.Data.Serialize(writer, false);
                writer.EndMessage();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.SetColor)
                    .Write(playerControl.Data.NetId)
                    .Write(colorId)
                    .EndRpc();
                sender.StartRpc(playerControl.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                    .WriteVector2(position)
                    .Write((ushort)10)
                    .EndRpc();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.MurderPlayer)
                    .WriteNetObject(playerControl)
                    .Write((int)MurderResultFlags.Succeeded)
                    .EndRpc();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.SetColor)
                    .Write(playerControl.Data.NetId)
                    .Write(colorId2)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }, 0.1f);
            new LateTask(() => playerControl.Data.MarkDirty(), 0.3f);
            new LateTask(() => playerControl.Despawn(), 0.5f);
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

        public static Explosion RpcCreateExplosion(float size, float duration, Vector2 position)
        {
            return new Explosion(size, duration, position);
        }

        public static TrapArea RpcCreateTrapArea(float radius, float waitDuration, Vector2 position, List<byte> visibleList, byte ownerId)
        {
            if (RandomItemsGamemode.instance == null) return null;
            return new TrapArea(radius, waitDuration, position, visibleList, ownerId);
        }

        public static Turret RpcCreateTurret(BaseWarsTeams team, SystemTypes room, Vector2 position)
        {
            if (BaseWarsGamemode.instance == null) return null;
            return new Turret(team, room, position);
        }

        public static Base RpcCreateBase(BaseWarsTeams team, Vector2 position)
        {
            if (BaseWarsGamemode.instance == null) return null;
            return new Base(team, position);
        }

        public static Display RpcCreateDisplay(string text, Vector2 position)
        {
            return new Display(text, position);
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

        public static void CustomSettingsChangeMessageLogic(this NotificationPopper notificationPopper, OptionItem optionItem, string text, bool playSound)
        {
            if (notificationPopper.lastMessageKey == 10000 + optionItem.Id && notificationPopper.activeMessages.Count > 0)
            {
                notificationPopper.activeMessages[notificationPopper.activeMessages.Count - 1].UpdateMessage(text);
            }
            else
            {
                notificationPopper.lastMessageKey = 10000 + optionItem.Id;
                LobbyNotificationMessage settingmessage = Object.Instantiate(notificationPopper.notificationMessageOrigin, Vector3.zero, Quaternion.identity, notificationPopper.transform);
                settingmessage.transform.localPosition = new Vector3(0f, 0f, -2f);
                settingmessage.SetUp(text, notificationPopper.settingsChangeSprite, notificationPopper.settingsChangeColor, new Action(() =>
                {
                    notificationPopper.OnMessageDestroy(settingmessage);
                }));
                notificationPopper.ShiftMessages();
                notificationPopper.AddMessageToQueue(settingmessage);
            }
            if (playSound)
            {
                SoundManager.Instance.PlaySoundImmediate(notificationPopper.settingsChangeSound, false, 1f, 1f, null);
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

        public static string GetOptionNameSCM(this OptionItem optionItem)
        {
            if (optionItem.Name == "Enable")
            {
                int id = optionItem.Id;
                while (id % 10 != 0)
                    --id;
                var optionItem2 = OptionItem.AllOptions.FirstOrDefault(opt => opt.Id == id);
                return optionItem2 != null ? optionItem2.GetName() : optionItem.GetName();
            }
            else
                return optionItem.GetName();
        }

        public static Vector2 GetOutsideMapPosition()
        {
            return Main.RealOptions.GetByte(ByteOptionNames.MapId) switch
            {
                0 => new(-27f, 3.3f),
                1 => new(-11.4f, 8.2f),
                2 => new(42.6f, -19.9f),
                3 => new(27f, 3.3f),
                4 => new(-16.8f, -6.2f),
                _ => new(9.6f, 23.2f),
            };
        }

        public static void SetAllVentInteractions()
        {
            VentilationSystemDeterioratePatch.SerializeV2(ShipStatus.Instance.Systems[SystemTypes.Ventilation].Cast<VentilationSystem>());
        }
    }
}