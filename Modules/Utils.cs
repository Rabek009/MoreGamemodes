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
using System.Security.Cryptography;
using System.Text;

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
            if (!ShipStatus.Instance.Systems.ContainsKey(type))
            {
                return false;
            }

            int mapId = Main.RealOptions.GetByte(ByteOptionNames.MapId);
            switch (type)
            {
                case SystemTypes.Electrical:
                    if (mapId == 5) return false;
                    var SwitchSystem = ShipStatus.Instance.Systems[type].TryCast<SwitchSystem>();
                    return SwitchSystem != null && SwitchSystem.IsActive;
                case SystemTypes.Reactor:
                    if (mapId == 2) return false; 
                    else
                    {
                        var ReactorSystemType = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                        return ReactorSystemType != null && ReactorSystemType.IsActive;
                    }
                case SystemTypes.Laboratory:
                    if (mapId != 2) return false;
                    var ReactorSystemType2 = ShipStatus.Instance.Systems[type].TryCast<ReactorSystemType>();
                    return ReactorSystemType2 != null && ReactorSystemType2.IsActive;
                case SystemTypes.LifeSupp:
                    if (mapId is 2 or 4 or 5) return false;
                    var LifeSuppSystemType = ShipStatus.Instance.Systems[type].TryCast<LifeSuppSystemType>();
                    return LifeSuppSystemType != null && LifeSuppSystemType.IsActive;
                case SystemTypes.HeliSabotage:
                    if (mapId != 4) return false;
                    var HeliSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<HeliSabotageSystem>();
                    return HeliSabotageSystem != null && HeliSabotageSystem.IsActive;
                case SystemTypes.Comms:
                    if (mapId is 1 or 5)
                    {
                        var HqHudSystemType = ShipStatus.Instance.Systems[type].TryCast<HqHudSystemType>();
                        return HqHudSystemType != null && HqHudSystemType.IsActive;
                    }
                    else
                    {
                        var HudOverrideSystemType = ShipStatus.Instance.Systems[type].TryCast<HudOverrideSystemType>();
                        return HudOverrideSystemType != null && HudOverrideSystemType.IsActive;
                    }
                case SystemTypes.MushroomMixupSabotage:
                    if (mapId != 5) return false;
                    var MushroomMixupSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<MushroomMixupSabotageSystem>();
                    return MushroomMixupSabotageSystem != null && MushroomMixupSabotageSystem.IsActive;
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
            Il2CppStructArray<byte> byteArray = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(opt, false);
            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage(targetClientId == -1 ? Tags.GameData : Tags.GameDataTo);
            {
                writer.Write(AmongUsClient.Instance.GameId);
                if (targetClientId != -1) writer.WritePacked(targetClientId);
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

        public static void SendSpam()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            for (int i = 1; i <= 30; ++i)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, "");
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == PlayerControl.LocalPlayer) continue;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SendChat, SendOption.None, pc.GetClientId());
                    writer.Write("");
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }

        public static void SendGameData()
        {
            foreach (var playerinfo in GameData.Instance.AllPlayers)
            {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage(5);
                {
                    writer.Write(AmongUsClient.Instance.GameId);
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerinfo.NetId);
                        playerinfo.Serialize(writer, false);
                    }
                    writer.EndMessage();
                }
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
            }
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
            sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetRole)
                .Write((ushort)RoleTypes.Crewmate)
                .Write(true)
                .EndRpc();
            PlayerControl.LocalPlayer.NetTransform.lastSequenceId += 328;
            sender.StartRpc(PlayerControl.LocalPlayer.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(PlayerControl.LocalPlayer.transform.position)
                .Write((ushort)(PlayerControl.LocalPlayer.NetTransform.lastSequenceId + 8))
                .EndRpc();
            sender.StartRpc(deadBodyParent.NetId, (byte)RpcCalls.SetColor)
                .Write(deadBodyParent.Data.NetId)
                .Write(deadBodyParent.CurrentOutfit.ColorId)
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
                case TabGroup.CrewmateRoles:
                    return "Crewmate Roles";
                case TabGroup.ImpostorRoles:
                    return "Impostor Roles";
                case TabGroup.NeutralRoles:
                    return "Neutral Roles";
                case TabGroup.AddOns:
                    return "Add Ons";
                default:
                    return "";
            }
        }

        public static string RoleToString(RoleTypes role, Gamemodes gamemode = Gamemodes.Classic)
        {
            if (role == RoleTypes.Crewmate && gamemode == Gamemodes.HideAndSeek) return "Hider";
            if (role == RoleTypes.Crewmate && gamemode == Gamemodes.FreezeTag) return "Runner";
            if (role == RoleTypes.Crewmate) return "Crewmate";
            if (role == RoleTypes.Scientist) return "Scientist";
            if (role == RoleTypes.Engineer && gamemode == Gamemodes.ShiftAndSeek) return "Hider";
            if (role == RoleTypes.Engineer) return "Engineer";
            if (role == RoleTypes.GuardianAngel) return "Guardian Angel";
            if (role == RoleTypes.CrewmateGhost && (gamemode == Gamemodes.HideAndSeek || gamemode == Gamemodes.ShiftAndSeek)) return "Hider Ghost";
            if (role == RoleTypes.CrewmateGhost) return "Crewmate Ghost";
            if (role == RoleTypes.Impostor && gamemode == Gamemodes.HideAndSeek) return "Seeker";
            if (role == RoleTypes.Impostor && gamemode == Gamemodes.FreezeTag) return "Tagger";
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
            if (reason == DeathReasons.Guessed) return "Guessed";
            if (reason == DeathReasons.Eaten) return "Eaten";
            if (reason == DeathReasons.Cursed) return "Cursed";
            if (reason == DeathReasons.Shot) return "Shot";
            if (reason == DeathReasons.Heartbroken) return "Heartbroken";
            if (reason == DeathReasons.Burned) return "Burned";
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

        public static void SetChatVisible(bool visible)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.SetChatVisible(visible);
        }

        public static void ShowExileAnimation()
        {
            MeetingHud.Instance = Object.Instantiate(HudManager.Instance.MeetingPrefab);
            MeetingHud.Instance.ServerStart(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.Spawn(MeetingHud.Instance, -2, SpawnFlags.None);
            new LateTask(() => MeetingHud.Instance.RpcClose(), 0.5f);
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

        public static Explosion RpcCreateExplosion(float size, float duration, bool createHole, int holeSpeedDecrease, Vector2 position)
        {
            if (!createHole) holeSpeedDecrease = 0;
            return new Explosion(size, duration, createHole, holeSpeedDecrease, position);
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

        public static ExplosionHole RpcCreateExplosionHole(float size, int speedDecrease, Vector2 position)
        {
            return new ExplosionHole(size, speedDecrease, position);
        }

        public static Drone RpcCreateDrone(PlayerControl owner, Vector2 position)
        {
            if (ClassicGamemode.instance == null) return null;
            return new Drone(owner, position);
        }

        public static void RpcSetDesyncRoles(RoleTypes selfRole, RoleTypes othersRole)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                SetDesyncRoleForPlayer(pc, selfRole, othersRole);
        }

        public static void SetDesyncRoleForPlayer(PlayerControl player, RoleTypes selfRole, RoleTypes othersRole)
        {
            RpcSetRolePatch.RoleAssigned[player.PlayerId] = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner)
                {
                    player.StartCoroutine(player.CoSetRole(pc == player ? selfRole : othersRole, true));
                    continue;
                }
                CustomRpcSender sender = CustomRpcSender.Create("RpcSetRole fix blackscreen", SendOption.Reliable);
                MessageWriter writer = sender.stream;
                sender.StartMessage(pc.GetClientId());
                bool disconnected = player.Data.Disconnected;
                player.Data.Disconnected = true;
                writer.StartMessage(1);
                writer.WritePacked(player.Data.NetId);
                player.Data.Serialize(writer, false);
                writer.EndMessage();
                player.Data.Disconnected = disconnected;
                sender.StartRpc(player.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)(pc == player ? selfRole : othersRole))
                    .Write(true)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
            new LateTask(() => {
                player.Data.MarkDirty();
            }, 0.5f);
        }

        public static void SetDesyncRoleForPlayers(PlayerControl player, List<PlayerControl> list, RoleTypes listRole, RoleTypes othersRole)
        {
            RpcSetRolePatch.RoleAssigned[player.PlayerId] = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner)
                {
                    player.StartCoroutine(player.CoSetRole(list.Contains(pc) ? listRole : othersRole, true));
                    continue;
                }
                CustomRpcSender sender = CustomRpcSender.Create("RpcSetRole fix blackscreen", SendOption.Reliable);
                MessageWriter writer = sender.stream;
                sender.StartMessage(pc.GetClientId());
                bool disconnected = player.Data.Disconnected;
                player.Data.Disconnected = true;
                writer.StartMessage(1);
                writer.WritePacked(player.Data.NetId);
                player.Data.Serialize(writer, false);
                writer.EndMessage();
                player.Data.Disconnected = disconnected;
                sender.StartRpc(player.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)(list.Contains(pc) ? listRole : othersRole))
                    .Write(true)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
            new LateTask(() => {
                player.Data.MarkDirty();
            }, 0.5f);
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
                Messages[tab] += option.GetString();
            }
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                if (tab == TabGroup.GamemodeSettings && Options.CurrentGamemode == Gamemodes.Classic) continue;
                if (tab is TabGroup.CrewmateRoles or TabGroup.ImpostorRoles or TabGroup.NeutralRoles or TabGroup.AddOns) continue;
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
        
        public static bool IsValidHexCode(string hex)
        {
             if (string.IsNullOrWhiteSpace(hex) || (hex.Length != 6 && hex.Length != 3)) return false;
             foreach (char c in hex)
             {
                if (!Uri.IsHexDigit(c)) 
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
            var ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
            if (ventilationSystem != null)
                VentilationSystemDeterioratePatch.SerializeV2(ventilationSystem);
        }

        public static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        public static void SetupRoleInfo(CustomRole role)
        {
            role.RoleName = CustomRolesHelper.RoleNames[role.Role];
            role.RoleDescription = CustomRolesHelper.RoleDescriptions[role.Role];
            role.RoleDescriptionLong = CustomRolesHelper.RoleDescriptionsLong[role.Role];
            role.Color = CustomRolesHelper.RoleColors[role.Role];
            role.CustomRoleType = CustomRolesHelper.CRoleTypes[role.Role];
        }

        public static void SetupAddOnInfo(AddOn addOn)
        {
            addOn.AddOnName = AddOnsHelper.AddOnNames[addOn.Type];
            addOn.AddOnDescription = AddOnsHelper.AddOnDescriptions[addOn.Type];
            addOn.AddOnDescriptionLong = AddOnsHelper.AddOnDescriptionsLong[addOn.Type];
            addOn.Color = AddOnsHelper.AddOnColors[addOn.Type];
        }

        public static string GetRoleOptions(CustomRoles role)
        {
            if (role is CustomRoles.Crewmate or CustomRoles.Impostor) return "";
            string text = "";
            switch (role)
            {
                case CustomRoles.Scientist:
                    text += ColorString(Palette.CrewmateBlue, "Scientist") + ": " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Scientist) + "%";
                    text += "\n → Max: " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Scientist);
                    text += "\n → Vitals display cooldown: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.ScientistCooldown) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ScientistCooldown)) + "s";
                    text += "\n → Battery duration: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.ScientistBatteryCharge) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ScientistBatteryCharge)) + "s";
                    return text;
                case CustomRoles.Engineer:
                    text += ColorString(Palette.CrewmateBlue, "Engineer") + ": " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Engineer) + "%";
                    text += "\n → Max: " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Engineer);
                    text += "\n → Vent use cooldown: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.EngineerCooldown) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.EngineerCooldown)) + "s";
                    text += "\n → Max time in vents: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.EngineerInVentMaxTime) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.EngineerInVentMaxTime)) + "s";
                    return text;
                case CustomRoles.Noisemaker:
                    text += ColorString(Palette.CrewmateBlue, "Noisemaker") + ": " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Noisemaker) + "%";
                    text += "\n → Max: " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Noisemaker);
                    text += "\n → Impostors get alert: " + ((Main.RealOptions != null ? Main.RealOptions.GetBool(BoolOptionNames.NoisemakerImpostorAlert) : GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.NoisemakerImpostorAlert)) ? "✓" : "X");
                    text += "\n → Alert duration: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.NoisemakerAlertDuration) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.NoisemakerAlertDuration)) + "s";
                    return text;
                case CustomRoles.Tracker:
                    text += ColorString(Palette.CrewmateBlue, "Tracker") + ": " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Tracker) + "%";
                    text += "\n → Max: " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Tracker);
                    text += "\n → Tracking cooldown: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.TrackerCooldown) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.TrackerCooldown)) + "s";
                    text += "\n → Tracking delay: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.TrackerDelay) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.TrackerDelay)) + "s";
                    text += "\n → Tracking duration: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.TrackerDuration) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.TrackerDuration)) + "s";
                    return text;
                case CustomRoles.Shapeshifter:
                    text += ColorString(Palette.ImpostorRed, "Shapeshifter") + ": " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter) + "%";
                    text += "\n → Max: " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter);
                    text += "\n → Leave shapeshifting evidence: " + ((Main.RealOptions != null ? Main.RealOptions.GetBool(BoolOptionNames.ShapeshifterLeaveSkin) : GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.ShapeshifterLeaveSkin)) ? "✓" : "X");
                    text += "\n → Shapeshift duration: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.ShapeshifterDuration) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ShapeshifterDuration)) + "s";
                    text += "\n → Shapeshift cooldown: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.ShapeshifterCooldown) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ShapeshifterCooldown)) + "s";
                    return text;
                case CustomRoles.Phantom:
                    text += ColorString(Palette.ImpostorRed, "Phantom") + ": " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Phantom) + "%";
                    text += "\n → Max: " + GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Phantom);
                    text += "\n → Vanish duration: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.PhantomDuration) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PhantomDuration)) + "s";
                    text += "\n → Vanish cooldown: " + (Main.RealOptions != null ? Main.RealOptions.GetFloat(FloatOptionNames.PhantomCooldown) : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PhantomCooldown)) + "s";
                    return text;
            }
            for (int index = 0; index < OptionItem.AllOptions.Count; index++)
            {
                var option = OptionItem.AllOptions[index];
                var enabled = option.Id == Options.RolesChance[role].Id || (option.Id > Options.RolesChance[role].Id && option.Id < Options.RolesChance[role].Id + 100 && (option.Parent.Parent == null || option.Parent.GetBool()));
                if (!enabled) continue;
                if (text != "")
                    text += "\n";

                if (option.Parent?.Parent?.Parent != null)
                    text += "           → ";
                else if (option.Parent?.Parent != null)
                    text += "      → ";
                else if (option.Parent != null)
                    text += " → ";
                text += option.GetName(option.NameColor == Color.white);

                text += ": ";
                text += option.GetString();
            }
            return text;
        }

        public static string GetAddOnOptions(AddOns addOn)
        {
            string text = "";
            for (int index = 0; index < OptionItem.AllOptions.Count; index++)
            {
                var option = OptionItem.AllOptions[index];
                var enabled = option.Id == Options.AddOnsChance[addOn].Id || (option.Id > Options.AddOnsChance[addOn].Id && option.Id < Options.AddOnsChance[addOn].Id + 100 && (option.Parent.Parent == null || option.Parent.GetBool()));
                if (!enabled) continue;
                if (text != "")
                    text += "\n";

                if (option.Parent?.Parent?.Parent != null)
                    text += "           → ";
                else if (option.Parent?.Parent != null)
                    text += "      → ";
                else if (option.Parent != null)
                    text += " → ";
                text += option.GetName(option.NameColor == Color.white);

                text += ": ";
                text += option.GetString();
            }
            return text;
        }

        // https://github.com/Gurge44/EndlessHostRoles/blob/main/Modules/BanManager.cs#L81
         public static string GetHashedPuid(this ClientData player)
        {
            if (player == null) return string.Empty;
            string puid = player.ProductUserId;
            using var sha256 = SHA256.Create();
            byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
            string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();
            return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
		}
    }
}