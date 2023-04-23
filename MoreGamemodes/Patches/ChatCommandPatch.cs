using HarmonyLib;
using System.Linq;
using System;
using Assets.CoreScripts;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    class SendChatPatch
    {
        public static bool Prefix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (__instance.TextArea.text == "") return false;
            __instance.TimeSinceLastMessage = 3f;
            var text = __instance.TextArea.text;
            string[] args = text.Split(' ');
            string subArgs = "";
            var canceled = false;
            switch (args[0])
            {
                case "/cs":
                case "/changesetting":
                    canceled = true;
                    if (Main.GameStarted) break;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "map":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "theskeld":
                                    GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 0);
                                    break;
                                case "mirahq":
                                    GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 1);
                                    break;
                                case "polus":
                                    GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 2);
                                    break;
                                case "dlekseht":
                                    GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 3);
                                    break;
                                case "airship":
                                    GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 4);
                                    break;
                                case "custom":
                                    subArgs = args.Length < 4 ? "" : args[3];
                                    GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, byte.Parse(subArgs));
                                    break;
                            }
                            break;
                        case "impostors":
                            subArgs = args.Length < 3 ? "" : args[2];
                            if (int.Parse(subArgs) < 1)
                            {
                                var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                PlayerControl.LocalPlayer.RpcSendMessage("Number of impostors is lower than 1. If you want to change other settings set impostors to more than 0!", "Warning");
                            }
                            if (int.Parse(subArgs) > 3)
                            {
                                var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                    PlayerControl.LocalPlayer.RpcSendMessage("Number of impostors is greater than 3. If you want to change other settings set impostors to 3 or less!", "Warning");
                            }
                            GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.NumImpostors, int.Parse(subArgs));
                            break;
                        case "players":
                            subArgs = args.Length < 3 ? "" : args[2];
                            if (int.Parse(subArgs) > 15)
                            {
                                var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                PlayerControl.LocalPlayer.RpcSendMessage("Max players are greater than 15. If you want to change other settings set maximum players to 15 or lower!", "Warning");
                            }
                            GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, int.Parse(subArgs));
                            break;
                        case "recommended":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.IsDefaults, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.IsDefaults, false);
                                    break;
                            }
                            break;
                        case "confirmejects":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetBool(BoolOptionNames.ConfirmImpostor, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetBool(BoolOptionNames.ConfirmImpostor, false);
                                    break;
                            }
                            break;
                        case "emergencymeetings":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, int.Parse(subArgs));
                            break;
                        case "anonymousvotes":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetBool(BoolOptionNames.AnonymousVotes, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetBool(BoolOptionNames.AnonymousVotes, false);
                                    break;
                            }
                            break;
                        case "emergencycooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.EmergencyCooldown, int.Parse(subArgs));
                            break;
                        case "discussiontime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.DiscussionTime, int.Parse(subArgs));
                            break;
                        case "votingtime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.VotingTime, int.Parse(subArgs));
                            break;
                        case "playerspeed":
                            subArgs = args.Length < 3 ? "" : args[2];
                            if (float.Parse(subArgs) <= 0)
                            {
                                var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                PlayerControl.LocalPlayer.RpcSendMessage("Player speed is lower or equals 0. If you want to change other settings set player speed to more than 0!", "Warning");
                            }
                            if (float.Parse(subArgs) > 3)
                            {
                                var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                PlayerControl.LocalPlayer.RpcSendMessage("Player speed is greater than 3. If you want to change other settings set player speed to 3 or less!", "Warning");
                            }
                            GameOptionsManager.Instance.currentNormalGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, float.Parse(subArgs));
                            break;
                        case "crewmatevision":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetFloat(FloatOptionNames.CrewLightMod, float.Parse(subArgs));
                            break;
                        case "impostorvision":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, float.Parse(subArgs));
                            break;
                        case "killcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.SetFloat(FloatOptionNames.KillCooldown, float.Parse(subArgs));
                            break;
                        case "killdistance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "short":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.KillDistance, 0);
                                    break;
                                case "medium":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.KillDistance, 1);
                                    break;
                                case "long":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.KillDistance, 2);
                                    break;
                                case "custom":
                                    subArgs = args.Length < 4 ? "" : args[3];
                                    if (int.Parse(subArgs) < 0)
                                    {
                                        var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                        PlayerControl.LocalPlayer.RpcSendMessage("Kill distance is lower than 0. If you want to change other settings set kill distance to short, medium or long!", "Warning");
                                    }
                                    if (int.Parse(subArgs) > 2)
                                    {
                                        var players = GameOptionsManager.Instance.currentGameOptions.MaxPlayers;
                                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, -1);
                                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, players);
                                        PlayerControl.LocalPlayer.RpcSendMessage("Kill distance is greater than 2. If you want to change other settings set kill distance to short, medium or long!", "Warning");
                                    }
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.KillDistance, int.Parse(subArgs));
                                    break;
                            }
                            break;
                        case "taskbarupdates":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "always":
                                    GameOptionsManager.Instance.currentNormalGameOptions.TaskBarMode = AmongUs.GameOptions.TaskBarMode.Normal;
                                    break;
                                case "meetings":
                                    GameOptionsManager.Instance.currentNormalGameOptions.TaskBarMode = AmongUs.GameOptions.TaskBarMode.MeetingOnly;
                                    break;
                                case "never":
                                    GameOptionsManager.Instance.currentNormalGameOptions.TaskBarMode = AmongUs.GameOptions.TaskBarMode.Invisible;
                                    break;
                            }
                            break;
                        case "visualtasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetBool(BoolOptionNames.VisualTasks, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentNormalGameOptions.SetBool(BoolOptionNames.VisualTasks, true);
                                    break;
                            }
                            break;
                        case "commontasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumCommonTasks, int.Parse(subArgs));
                            break;
                        case "longtasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumLongTasks, int.Parse(subArgs));
                            break;
                        case "shorttasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumShortTasks, int.Parse(subArgs));
                            break;
                        case "scientistcount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, int.Parse(subArgs), GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Scientist));
                            break;
                        case "scientistchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Scientist), int.Parse(subArgs));
                            break;
                        case "vitalsdisplaycooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, float.Parse(subArgs));
                            break;
                        case "batteryduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistBatteryCharge, float.Parse(subArgs));
                            break;
                        case "engineercount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, int.Parse(subArgs), GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Engineer));
                            break;
                        case "engineerchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Engineer), int.Parse(subArgs));
                            break;
                        case "ventusecooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, float.Parse(subArgs));
                            break;
                        case "maxtimeinvents":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerInVentMaxTime, float.Parse(subArgs));
                            break;
                        case "guardianangelcount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, int.Parse(subArgs), GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel));
                            break;
                        case "guardianangelchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel), int.Parse(subArgs));
                            break;
                        case "protectcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.GuardianAngelCooldown, float.Parse(subArgs));
                            break;
                        case "protectduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ProtectionDurationSeconds, float.Parse(subArgs));
                            break;
                        case "protectvisibletoimpostors":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
                                    break;
                            }
                            break;
                        case "shapeshiftercount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, int.Parse(subArgs), GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter));
                            break;
                        case "shapeshifterchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter), int.Parse(subArgs));
                            break;
                        case "shapeshiftduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterDuration, float.Parse(subArgs));
                            break;
                        case "shapeshiftcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, float.Parse(subArgs));
                            break;
                        case "leaveshapeshiftevidence":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
                                    break;
                            }
                            break;
                        case "ghostdotasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.GhostsDoTasks, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.GhostsDoTasks, false);
                                    break;
                            }
                            break;
                        case "gamemode":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "classic":
                                    GameOptionsManager.Instance.currentGameMode = GameModes.Normal;
                                    break;
                                case "hidenseek":
                                    GameOptionsManager.Instance.currentGameMode = GameModes.HideNSeek;
                                    break;
                            }
                            break;
                        case "hidingtime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EscapeTime, float.Parse(subArgs));
                            break;
                        case "finalhidetime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.FinalEscapeTime, float.Parse(subArgs));
                            break;
                        case "maxventuses":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.CrewmateVentUses, int.Parse(subArgs));
                            break;
                        case "maxtimeinvent":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.CrewmateTimeInVent, float.Parse(subArgs));
                            break;
                        case "flashlightmode":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.UseFlashlight, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.UseFlashlight, false);
                                    break;
                            }
                            break;
                        case "crewmateflashlightsize":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.CrewmateFlashlightSize, float.Parse(subArgs));
                            break;
                        case "impostorflashlightsize":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ImpostorFlashlightSize, float.Parse(subArgs));
                            break;
                        case "finalhideimpostorspeed":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.SeekerFinalSpeed, float.Parse(subArgs));
                            break;
                        case "finalhideseekmap":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.SeekerFinalMap, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.SeekerFinalMap, false);
                                    break;
                            }
                            break;
                        case "finalhidepings":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.SeekerPings, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.SeekerPings, false);
                                    break;
                            }
                            break;
                        case "pinginterval":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.MaxPingTime, float.Parse(subArgs));
                            break;
                        case "shownames":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShowCrewmateNames, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShowCrewmateNames, false);
                                    break;
                            }
                            break;
                        case "impostor":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.ImpostorPlayerID, int.Parse(subArgs));
                            break;

                    }
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    break;
                case "/gm":
                case "/gamemode":
                    canceled = true;
                    if (Main.GameStarted) break;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "classic":
                            Options.Gamemode.SetValue(0);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is classic", "GamemodesChanger");
                            break;
                        case "hideandseek":
                            Options.Gamemode.SetValue(1);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is hide and seek", "GamemodesChanger");
                            break;
                        case "shiftandseek":
                            Options.Gamemode.SetValue(2);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is shift and seek", "GamemodesChanger");
                            break;
                        case "bombtag":
                            Options.Gamemode.SetValue(3);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is bomb tag", "GamemodesChanger");
                            break;
                        case "randomitems":
                            Options.Gamemode.SetValue(4);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is random items", "GamemodesChanger");
                            break;
                        case "battleroyale":
                            Options.Gamemode.SetValue(5);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is battle royale", "GamemodesChanger");
                            break;
                        case "speedrun":
                            Options.Gamemode.SetValue(6);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is speedrun", "GamemodesChanger");
                            break;
                    }
                    break;
                case "/color":
                case "/colour":
                    canceled = true;
                    if (Main.GameStarted) break;
                    subArgs = args.Length < 2 ? "" : args[1];
                    Main.StandardColors[PlayerControl.LocalPlayer.PlayerId] = byte.Parse(subArgs);
                    PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId = Main.StandardColors[PlayerControl.LocalPlayer.PlayerId];
                    PlayerControl.LocalPlayer.RpcSetColor((byte)Main.StandardColors[PlayerControl.LocalPlayer.PlayerId]);
                    break;
                case "/name":
                    canceled = true;
                    if (Main.GameStarted) break;
                    var name = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        name += subArgs;
                    }
                    Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] = name;
                    PlayerControl.LocalPlayer.Data.PlayerName = Main.StandardNames[PlayerControl.LocalPlayer.PlayerId];
                    PlayerControl.LocalPlayer.RpcSetName(Main.StandardNames[PlayerControl.LocalPlayer.PlayerId]);
                    break;
                case "/h":
                case "/help":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "gm":
                        case "gamemode":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "classic":
                                    Utils.SendChat("Classic: Standard among us game.", "Gamemodes", __instance);
                                    break;
                                case "hideandseek":
                                    Utils.SendChat("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes", __instance);
                                    break;
                                case "shiftandseek":
                                    Utils.SendChat("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes", __instance);
                                    break;
                                case "bombtag":
                                    Utils.SendChat("Bomb Tag: Game lasts for few round. Random player/players gets bomb at start of the round. Player with bomb need to give away a bomb. After the round players with bombs die. Click kill to give bomb away. Black players have bomb. Last standing alive wins!", "Gamemodes", __instance);
                                    break;
                                case "randomitems":
                                    Utils.SendChat("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick.", "Gamemodes", __instance);
                                    break;
                                case "battleroyale":
                                    Utils.SendChat("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes", __instance);
                                    break;
                                case "speedrun":
                                    Utils.SendChat("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes", __instance);
                                    break;
                                default:
                                    switch (Options.CurrentGamemode)
                                    {
                                        case Gamemodes.Classic:
                                            Utils.SendChat("Classic: Standard among us game.", "Gamemodes", __instance);
                                            break;
                                        case Gamemodes.HideAndSeek:
                                            Utils.SendChat("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes", __instance);
                                            break;
                                        case Gamemodes.ShiftAndSeek:
                                            Utils.SendChat("Shift And Seek: Everyone is engineer or shapeshifter.Impostors are visible to other players.Crewmates wins by finishing their tasks, impostors by killing every single crewmate.Impostor must shapeshift into person he want kill.", "Gamemodes", __instance);
                                            break;
                                        case Gamemodes.BombTag:
                                            Utils.SendChat("Bomb Tag: Game lasts for few round. Random player/players gets bomb at start of the round. Player with bomb need to give away a bomb. After the round players with bombs die. Click kill to give bomb away. Black players have bomb. Last standing alive wins!", "Gamemodes", __instance);
                                            break;
                                        case Gamemodes.RandomItems:
                                            Utils.SendChat("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick.", "Gamemodes", __instance);
                                            break;
                                        case Gamemodes.BattleRoyale:
                                            Utils.SendChat("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes", __instance);
                                            break;
                                        case Gamemodes.Speedrun:
                                            Utils.SendChat("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes", __instance);
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "item":
                        case "i":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "timeslower":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.TimeSlower), "Items");
                                    break;
                                case "knowledge":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Knowledge), "Items");
                                    break;
                                case "gun":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Gun), "Items");
                                    break;
                                case "shield":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Shield), "Items");
                                    break;
                                case "illusion":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Illusion), "Items");
                                    break;
                                case "radar":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Radar), "Items");
                                    break;
                                case "timespeeder":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.TimeSpeeder), "Items");
                                    break;
                                case "flash":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Flash), "Items");
                                    break;
                                case "hack":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Hack), "Items");
                                    break;
                                case "camouflage":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Camouflage), "Items");
                                    break;
                                case "multiteleport":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.MultiTeleport), "Items");
                                    break;
                                case "bomb":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Bomb), "Items");
                                    break;
                                case "teleport":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Teleport), "Items");
                                    break;
                                case "button":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Button), "Items");
                                    break;
                                case "finder":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Finder), "Items");
                                    break;
                                case "rope":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Rope), "Items");
                                    break;
                                case "stop":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Stop), "Items");
                                    break;
                                case "newsletter":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Newsletter), "Items");
                                    break;
                                default:
                                    if (PlayerControl.LocalPlayer.GetItem() == Items.None)
                                    {
                                        PlayerControl.LocalPlayer.RpcSendMessage("You don't have any item. Do your task or kill someone to get item!", "Items");
                                        break;
                                    }
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(PlayerControl.LocalPlayer.GetItem()), "Items");
                                    break;
                            }
                            break;
                    }
                    break;
                case "/stop":
                    if (Options.CurrentGamemode != Gamemodes.RandomItems || !Main.GameStarted || PlayerControl.LocalPlayer.Data.IsDead) break;
                    canceled = true;
                    List<string> spam = new List<string>();
                    if (PlayerControl.LocalPlayer.GetItem() == Items.Stop)
                    {
                        MeetingHud.Instance.RpcClose();
                        PlayerControl.LocalPlayer.RpcSetItem(Items.None);
                        for (int i = 1; i <= 20; ++i)
                            spam.Add("Someone used /stop command!");
                    }
                    Utils.SendChatV2(spam, "Spam", __instance);
                    break;
                case "/n":
                case "/now":
                    canceled = true;
                    List<string> messages = new List<string>();
                    var message = "";
                    message += "No game end: "; message += Options.NoGameEnd.GetBool() ? "ON\n" : "OFF\n";
                    message += "Can use /color command: "; message += Options.CanUseColorCommand.GetBool() ? "ON\n" : "OFF\n";
                    message += "Can use /name command: "; message += Options.CanUseNameCommand.GetBool() ? "ON\n" : "OFF\n";
                    messages.Add(message);

                    message = "";
                    switch (Options.CurrentGamemode)
                    {
                        case Gamemodes.Classic:
                            message = "Gamemode: Classic\n";
                            break;
                        case Gamemodes.HideAndSeek:
                            message = "Gamemode: Hide and seek\n\n";
                            message += "Impostors blind time: " + Options.HnSImpostorsBlindTime.GetFloat() + "s\n";
                            message += "Impostors can kill during blind: "; message += Options.HnSImpostorsCanKillDuringBlind.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can vent: "; message += Options.HnSImpostorsCanVent.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can close doors: "; message += Options.HnSImpostorsCanCloseDoors.GetBool() ? "ON\n" : "OFF\n";
                            break;
                        case Gamemodes.ShiftAndSeek:
                            message = "Gamemode: Shift and seek\n\n";
                            message += "Impostors blind time: " + Options.SnSImpostorsBlindTime.GetFloat() + "s\n";
                            message += "Impostors can kill during blind: "; message += Options.SnSImpostorsCanKillDuringBlind.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can vent: "; message += Options.SnSImpostorsCanVent.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can close doors: "; message += Options.SnSImpostorsCanCloseDoors.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors are visible: "; message += Options.ImpostorsAreVisible.GetBool() ? "ON\n" : "OFF\n";
                            break;
                        case Gamemodes.BombTag:
                            message = "Gamemode: Bomb tag\n\n";
                            message += "Teleport after explosion: "; message += Options.TeleportAfterExplosion.GetBool() ? "ON\n" : "OFF\n";
                            message += "Explosion delay: " + Options.ExplosionDelay.GetInt() + "s\n";
                            message += "Players with bomb: " + Options.PlayersWithBomb.GetInt() + "%\n";
                            message += "Max players with bomb: " + Options.MaxPlayersWithBomb.GetInt() + " players\n";
                            break;
                        case Gamemodes.RandomItems:
                            message = "Gamemode: Random items\n\n";
                            message += "\nTime slower: "; message += Options.EnableTimeSlower.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableTimeSlower.GetBool())
                            {
                                message += "Discussion time increase: " + Options.DiscussionTimeIncrease.GetInt() + "s\n";
                                message += "Voting time increase: " + Options.VotingTimeIncrease.GetInt() + "s\n";
                            }
                            message += "\nKnowledge: "; message += Options.EnableKnowledge.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableKnowledge.GetBool())
                            {
                                message += "Crewmates see reveal: "; message += Options.CrewmatesSeeReveal.GetBool() ? "ON\n" : "OFF\n";
                                message += "Impostors see reveal: "; message += Options.ImpostorsSeeReveal.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nShield: "; message += Options.EnableShield.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableShield.GetBool())
                            {
                                message += "Shield duration: " + Options.ShieldDuration.GetFloat() + "s\n";
                                message += "See who tried kill: "; message += Options.SeeWhoTriedKill.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nGun: "; message += Options.EnableGun.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableGun.GetBool())
                            {
                                message += "Can kill crewmate: "; message += Options.CanKillCrewmate.GetBool() ? "ON\n" : "OFF\n";
                                message += "Misfire kills crewmate: "; message += Options.MisfireKillsCrewmate.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nIllusion: "; message += Options.EnableIllusion.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nRadar: "; message += Options.EnableRadar.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableRadar.GetBool())
                            {
                                message += "Radar range: " + Options.RadarRange.GetFloat() + "x\n";
                            }
                            message += "\nTime speeder: "; message += Options.EnableTimeSpeeder.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableTimeSpeeder.GetBool())
                            {
                                message += "Discussion time decrease: " + Options.DiscussionTimeDecrease.GetInt() + "s\n";
                                message += "Voting time decrease: " + Options.VotingTimeDecrease.GetInt() + "s\n";
                            }
                            message += "\nFlash: "; message += Options.EnableFlash.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableFlash.GetBool())
                            {
                                message += "Flash duration: " + Options.FlashDuration.GetFloat() + "s\n";
                                message += "Impostor vision in flash: " + Options.ImpostorVisionInFlash.GetFloat() + "x\n";
                            }
                            message += "\nHack: "; message += Options.EnableHack.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableHack.GetBool())
                            {
                                message += "Hack duration: " + Options.HackDuration.GetFloat() + "s\n";
                                message += "Hack affects impostors: "; message += Options.HackAffectsImpostors.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nCamouflage: "; message += Options.EnableCamouflage.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableCamouflage.GetBool())
                            {
                                message += "Camouflage duration: " + Options.CamouflageDuration.GetFloat() + "s\n";
                            }
                            message += "\nMulti teleport: "; message += Options.EnableMultiTeleport.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nBomb: "; message += Options.EnableBomb.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableBomb.GetBool())
                            {
                                message += "Bomb duration: " + Options.BombRadius.GetFloat() + "x\n";
                                message += "Can kill impostors: "; message += Options.CanKillImpostors.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nTeleport: "; message += Options.EnableTeleport.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nButton: "; message += Options.EnableButton.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableButton.GetBool())
                            {
                                message += "Can use during sabotage: "; message += Options.CanUseDuringSabotage.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nFinder: "; message += Options.EnableFinder.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nRope: "; message += Options.EnableRope.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nStop: "; message += Options.EnableStop.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableStop.GetBool())
                            {
                                message += "Can be given to crewmate: "; message += Options.CanBeGivenToCrewmate.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nNewsletter: "; message += Options.EnableNewsletter.GetBool() ? "ON\n" : "OFF\n";
                            break;
                        case Gamemodes.BattleRoyale:
                            message = "Gamemode: Battle royale\n\n";
                            message += "Lives: " + Options.Lives.GetInt() + "\n";
                            message += "Lives Visible To Others: "; message += Options.LivesVisibleToOthers.GetBool() ? "ON\n" : "OFF\n";
                            message += "Arrow To Nearest Player: "; message += Options.ArrowToNearestPlayer.GetBool() ? "ON\n" : "OFF\n";
                            message += "Grace Period: " + Options.GracePeriod.GetFloat() + "s\n";
                            break;
                        case Gamemodes.Speedrun:
                            message = "Gamemode: Speedrun\n\n";
                            message += "Body Type: " + Utils.BodyTypeString(Options.CurrentBodyType) + "\n";
                            message += "Tasks Visible To Others: "; message += Options.TasksVisibleToOthers.GetBool() ? "ON\n" : "OFF\n";
                            break;
                    }
                    messages.Add(message);

                    message = "Additional Gamemodes:\n";
                    if (Options.RandomSpawn.GetBool())
                    {
                        message += "\nRandom Spawn\n";
                        message += "Teleport After Meeting: "; message += Options.TeleportAfterMeeting.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (message != "Additional Gamemodes:\n")
                        messages.Add(message);
                    Utils.SendChatV2(messages, "Options", __instance);
                    break;
                case "/id":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "players":
                            var player_ids = "";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                player_ids += pc.Data.PlayerName;
                                player_ids += " - ";
                                player_ids += pc.PlayerId;
                                player_ids += "\n";
                            }
                            PlayerControl.LocalPlayer.RpcSendMessage(player_ids, "Ids");
                            break;
                        case "colors":
                        case "colours":
                            PlayerControl.LocalPlayer.RpcSendMessage("red - 0\nblue - 1\ngreen - 2\npink - 3\norange - 4\nyellow - 5\nblack - 6\nwhite - 7\npurple - 8\nbrown - 9\ncyan - 10\nlime - 11\nmaroon - 12\nrose - 13\nbanana - 14\ngray - 15\ntan - 16\ncoral - 17\nfortegreen - >17", "Ids");
                            break;
                    }
                    break;
                case "/commands":
                case "/cm":
                    canceled = true;
                    PlayerControl.LocalPlayer.RpcSendMessage("Commands:\n/color COLOR_ID - changes your color\n/name NAME - changes your name\n/help gamemode - show gamemode description\n" +
                        "/now - show active settings\n/id (players, colors) - show ids\n/help item - show item description\n/commands - show list of commands\n/changesetting SETTING VALUE - changes setting value\n" +
                        "/gamemode GAMEMODE - changes gamemode\n/kick PLAYER_ID - kick player\n/ban PLAYER_ID - ban player\n/announce MESSAGE - send message", "Command");
                    break;
                case "/kick":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    AmongUsClient.Instance.KickPlayer(Utils.GetPlayerById(byte.Parse(subArgs)).GetClientId(), false);
                    break;
                case "/ban":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    AmongUsClient.Instance.KickPlayer(Utils.GetPlayerById(byte.Parse(subArgs)).GetClientId(), true);
                    break;
                case "/announce":
                case "/ac":
                    canceled = true;
                    var announce = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        announce += subArgs;
                    }
                    Utils.SendChat(announce, "HostMessage", __instance);
                    break;
                default:
                    break;
            }
            if (canceled)
            {
                __instance.TextArea.Clear();
                __instance.TextArea.SetText("");
                __instance.quickChatMenu.ResetGlyphs();
            }
            return !canceled;
        }
        public static bool OnReceiveChat(PlayerControl player, string text)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var canceled = false;
            string[] args = text.Split(' ');
            string subArgs = "";   
            switch (args[0])
            {
                case "/color":
                case "/colour":
                    canceled = true;
                    if (!Options.CanUseColorCommand.GetBool()) break;
                    if (Main.GameStarted) break;
                    subArgs = args.Length < 2 ? "" : args[1];
                    Main.StandardColors[player.PlayerId] = byte.Parse(subArgs);
                    player.Data.DefaultOutfit.ColorId = Main.StandardColors[player.PlayerId];
                    player.RpcSetColor((byte)Main.StandardColors[player.PlayerId]);
                    break;
                case "/name":
                    canceled = true;
                    if (!Options.CanUseNameCommand.GetBool()) break;
                    if (Main.GameStarted) break;
                    var name = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        name += subArgs;
                    }
                    Main.StandardNames[player.PlayerId] = name;
                    player.Data.PlayerName = Main.StandardNames[player.PlayerId];
                    player.RpcSetName(Main.StandardNames[player.PlayerId]);
                    break;
                case "/h":
                case "/help":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "gm":
                        case "gamemode":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "classic":
                                    player.RpcSendMessage("Classic: Standard among us game.", "Gamemodes");
                                    break;
                                case "hideandseek":
                                    player.RpcSendMessage("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                    break;
                                case "shiftandseek":
                                    player.RpcSendMessage("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes");
                                    break;
                                case "bombtag":
                                    player.RpcSendMessage("Bomb Tag: Game lasts for few round. Random player/players gets bomb at start of the round. Player with bomb need to give away a bomb. After the round players with bombs die. Click kill to give bomb away. Black players have bomb. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "randomitems":
                                    player.RpcSendMessage("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick.", "Gamemodes");
                                    break;
                                case "battleroyale":
                                    player.RpcSendMessage("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                    break;
                                default:
                                    switch (Options.CurrentGamemode)
                                    {
                                        case Gamemodes.Classic:
                                            player.RpcSendMessage("Classic: Standard among us game.", "Gamemodes");
                                            break;
                                        case Gamemodes.HideAndSeek:
                                            player.RpcSendMessage("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                            break;
                                        case Gamemodes.ShiftAndSeek:
                                            player.RpcSendMessage("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes");
                                            break;
                                        case Gamemodes.BombTag:
                                            player.RpcSendMessage("Bomb Tag: Game lasts for few round. Random player/players gets bomb at start of the round. Player with bomb need to give away a bomb. After the round players with bombs die. Click kill to give bomb away. Black players have bomb. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.RandomItems:
                                            player.RpcSendMessage("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick.", "Gamemodes");
                                            break;
                                        case Gamemodes.BattleRoyale:
                                            player.RpcSendMessage("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "item":
                        case "i":
                            subArgs = args.Length < 3 ? "" : args[2];
                            switch (subArgs)
                            {
                                case "timeslower":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.TimeSlower), "Items");
                                    break;
                                case "knowledge":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Knowledge), "Items");
                                    break;
                                case "gun":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Gun), "Items");
                                    break;
                                case "shield":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Shield), "Items");
                                    break;
                                case "illusion":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Illusion), "Items");
                                    break;
                                case "radar":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Radar), "Items");
                                    break;
                                case "timespeeder":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.TimeSpeeder), "Items");
                                    break;
                                case "flash":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Flash), "Items");
                                    break;
                                case "hack":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Hack), "Items");
                                    break;
                                case "camouflage":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Camouflage), "Items");
                                    break;
                                case "multiteleport":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.MultiTeleport), "Items");
                                    break;
                                case "bomb":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Bomb), "Items");
                                    break;
                                case "teleport":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Teleport), "Items");
                                    break;
                                case "button":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Button), "Items");
                                    break;
                                case "finder":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Finder), "Items");
                                    break;
                                case "rope":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Rope), "Items");
                                    break;
                                case "stop":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Stop), "Items");
                                    break;
                                case "newsletter":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Newsletter), "Items");
                                    break;
                                default:
                                    if (player.GetItem() == Items.None)
                                    {
                                        player.RpcSendMessage("You don't have any item. Do your task or kill someone to get item!", "Items");
                                        break;
                                    }
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(player.GetItem()), "Items");
                                    break;
                            }
                            break;
                    }
                    break;
                case "/stop":
                    if (Options.CurrentGamemode != Gamemodes.RandomItems || !Main.GameStarted || player.Data.IsDead) break;
                    canceled = true;
                    List<string> spam = new List<string>();
                    if (player.GetItem() == Items.Stop)
                    {
                        MeetingHud.Instance.RpcClose();
                        player.RpcSetItem(Items.None);
                        for (int i = 1; i <= 20; ++i)
                            spam.Add("Someone used /stop command!");
                        Utils.SendChatV2(spam, "Spam");
                    }          
                    break;
                case "/n":
                case "/now":
                    canceled = true;
                    List<string> messages = new List<string>();
                    var message = "";
                    message += "No game end: "; message += Options.NoGameEnd.GetBool() ? "ON" : "OFF";
                    message += "\nCan use /color command: "; message += Options.CanUseColorCommand.GetBool() ? "ON" : "OFF";
                    message += "\nCan use /name command: "; message += Options.CanUseNameCommand.GetBool() ? "ON" : "OFF";
                    messages.Add(message);

                    message = "";
                    switch (Options.CurrentGamemode)
                    {
                        case Gamemodes.Classic:
                            message = "Gamemode: Classic\n";
                            break;
                        case Gamemodes.HideAndSeek:
                            message = "Gamemode: Hide and seek\n\n";
                            message += "Impostors blind time: " + Options.HnSImpostorsBlindTime.GetFloat() + "s\n";
                            message += "Impostors can kill during blind: "; message += Options.HnSImpostorsCanKillDuringBlind.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can vent: "; message += Options.HnSImpostorsCanVent.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can close doors: "; message += Options.HnSImpostorsCanCloseDoors.GetBool() ? "ON\n" : "OFF\n";
                            break;
                        case Gamemodes.ShiftAndSeek:
                            message = "Gamemode: Shift and seek\n\n";
                            message += "Impostors blind time: " + Options.SnSImpostorsBlindTime.GetFloat() + "s\n";
                            message += "Impostors can kill during blind: "; message += Options.SnSImpostorsCanKillDuringBlind.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can vent: "; message += Options.SnSImpostorsCanVent.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors can close doors: "; message += Options.SnSImpostorsCanCloseDoors.GetBool() ? "ON\n" : "OFF\n";
                            message += "Impostors are visible: "; message += Options.ImpostorsAreVisible.GetBool() ? "ON\n" : "OFF\n";
                            break;
                        case Gamemodes.BombTag:
                            message = "Gamemode: Bomb tag\n\n";
                            message += "Teleport after explosion: "; message += Options.TeleportAfterExplosion.GetBool() ? "ON\n" : "OFF\n";
                            message += "Explosion delay: " + Options.ExplosionDelay.GetInt() + "s\n";
                            message += "Players with bomb: " + Options.PlayersWithBomb.GetInt() + "%\n";
                            message += "Max players with bomb: " + Options.MaxPlayersWithBomb.GetInt() + " players\n";
                            break;
                        case Gamemodes.RandomItems:
                            message = "Gamemode: Random items\n\n";
                            message += "\nTime slower: "; message += Options.EnableTimeSlower.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableTimeSlower.GetBool())
                            {
                                message += "Discussion time increase: " + Options.DiscussionTimeIncrease.GetInt() + "s\n";
                                message += "Voting time increase: " + Options.VotingTimeIncrease.GetInt() + "s\n";
                            }
                            message += "\nKnowledge: "; message += Options.EnableKnowledge.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableKnowledge.GetBool())
                            {
                                message += "Crewmates see reveal: "; message += Options.CrewmatesSeeReveal.GetBool() ? "ON\n" : "OFF\n";
                                message += "Impostors see reveal: "; message += Options.ImpostorsSeeReveal.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nShield: "; message += Options.EnableShield.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableShield.GetBool())
                            {
                                message += "Shield duration: " + Options.ShieldDuration.GetFloat() + "s\n";
                                message += "See who tried kill: "; message += Options.SeeWhoTriedKill.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nGun: "; message += Options.EnableGun.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableGun.GetBool())
                            {
                                message += "Can kill crewmate: "; message += Options.CanKillCrewmate.GetBool() ? "ON\n" : "OFF\n";
                                message += "Misfire kills crewmate: "; message += Options.MisfireKillsCrewmate.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nIllusion: "; message += Options.EnableIllusion.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nRadar: "; message += Options.EnableRadar.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableRadar.GetBool())
                            {
                                message += "Radar range: " + Options.RadarRange.GetFloat() + "x\n";
                            }
                            message += "\nTime speeder: "; message += Options.EnableTimeSpeeder.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableTimeSpeeder.GetBool())
                            {
                                message += "Discussion time decrease: " + Options.DiscussionTimeDecrease.GetInt() + "s\n";
                                message += "Voting time decrease: " + Options.VotingTimeDecrease.GetInt() + "s\n";
                            }
                            message += "\nFlash: "; message += Options.EnableFlash.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableFlash.GetBool())
                            {
                                message += "Flash duration: " + Options.FlashDuration.GetFloat() + "s\n";
                                message += "Impostor vision in flash: " + Options.ImpostorVisionInFlash.GetFloat() + "x\n";
                            }
                            message += "\nHack: "; message += Options.EnableHack.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableHack.GetBool())
                            {
                                message += "Hack duration: " + Options.HackDuration.GetFloat() + "s\n";
                                message += "Hack affects impostors: "; message += Options.HackAffectsImpostors.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nCamouflage: "; message += Options.EnableCamouflage.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableCamouflage.GetBool())
                            {
                                message += "Camouflage duration: " + Options.CamouflageDuration.GetFloat() + "s\n";
                            }
                            message += "\nMulti teleport: "; message += Options.EnableMultiTeleport.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nBomb: "; message += Options.EnableBomb.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableBomb.GetBool())
                            {
                                message += "Bomb duration: " + Options.BombRadius.GetFloat() + "x\n";
                                message += "Can kill impostors: "; message += Options.CanKillImpostors.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nTeleport: "; message += Options.EnableTeleport.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nButton: "; message += Options.EnableButton.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableButton.GetBool())
                            {
                                message += "Can use during sabotage: "; message += Options.CanUseDuringSabotage.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nFinder: "; message += Options.EnableFinder.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nRope: "; message += Options.EnableRope.GetBool() ? "ON\n" : "OFF\n";
                            message += "\nStop: "; message += Options.EnableStop.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableStop.GetBool())
                            {
                                message += "Can be given to crewmate: "; message += Options.CanBeGivenToCrewmate.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nNewsletter: "; message += Options.EnableNewsletter.GetBool() ? "ON\n" : "OFF\n";
                            break;
                        case Gamemodes.BattleRoyale:
                            message = "Gamemode: Battle royale\n\n";
                            message += "Lives: " + Options.Lives.GetInt() + "\n";
                            message += "Lives Visible To Others: "; message += Options.LivesVisibleToOthers.GetBool() ? "ON\n" : "OFF\n";
                            message += "Arrow To Nearest Player: "; message += Options.ArrowToNearestPlayer.GetBool() ? "ON\n" : "OFF\n";
                            message += "Grace Period: " + Options.GracePeriod.GetFloat() + "s\n";
                            break;
                    }
                    messages.Add(message);

                    message = "Additional Gamemodes:\n";
                    if (Options.RandomSpawn.GetBool())
                    {
                        message += "\nRandom Spawn\n";
                        message += "Teleport After Meeting: "; message += Options.TeleportAfterMeeting.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (message != "Additional Gamemodes:\n")
                        messages.Add(message);
                    player.RpcSendMessages(messages, "Options");
                    break;
                case "/id":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "players":
                            var player_ids = "";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                player_ids += pc.Data.PlayerName;
                                player_ids += " - ";
                                player_ids += pc.PlayerId;
                                player_ids += "\n";
                            }
                            player.RpcSendMessage(player_ids, "Ids");
                            break;
                        case "colors":
                        case "colours":
                            player.RpcSendMessage("red - 0\nblue - 1\ngreen - 2\npink - 3\norange - 4\nyellow - 5\nblack - 6\nwhite - 7\npurple - 8\nbrown - 9\ncyan - 10\nlime - 11\nmaroon - 12\nrose - 13\nbanana - 14\ngray - 15\ntan - 16\ncoral - 17\nfortegreen - >17", "Ids");
                            break;
                    }
                    break;
                case "/commands":
                case "/cm":
                    canceled = true;
                    player.RpcSendMessage("Commands:\n/color COLOR_ID - changes your color\n/name NAME - changes your name\n/help gamemode - show gamemode description\n/now - show active settings\n" +
                        "/id (players, colors) - show ids\n/help item - show item description\n/commands - show list of commands", "Commands");
                    break;
                default:
                    break;
            }
            return !canceled;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    class RpcSendChatPatch
    {
        public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (string.IsNullOrWhiteSpace(chatText))
            {
                __result = false;
                return false;
            }
            int return_count = PlayerControl.LocalPlayer.name.Count(x => x == '\n');
            chatText = new StringBuilder(chatText).Insert(0, "\n", return_count).ToString();
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                DestroyableSingleton<Telemetry>.Instance.SendWho();
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SendChat, SendOption.None);
            messageWriter.Write(chatText);
            messageWriter.EndMessage();
            __result = true;
            return false;
        }
    }
}