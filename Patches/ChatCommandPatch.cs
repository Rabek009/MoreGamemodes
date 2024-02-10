﻿using HarmonyLib;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using System;
using Assets.CoreScripts;
using System.Text;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    class SendChatPatch
    {
        public static bool Prefix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (__instance.quickChatField.Visible)
            {
                return true;
            }
            if (__instance.freeChatField.textArea.text == "")
            {
                return false;
            }
            __instance.timeSinceLastMessage = 3f;
            var text = __instance.freeChatField.Text;
            string[] args = text.Split(' ');
            string subArgs = "";
            var canceled = false;
            if (Options.MidGameChat.GetBool() && Main.GameStarted && !Main.IsMeeting && Options.CurrentGamemode != Gamemodes.PaintBattle)
            {
                if (Utils.IsActive(SystemTypes.Comms) && Options.DisableDuringCommsSabotage.GetBool())
                {
                    Utils.SendSpam("Someone tried to send message during comms sabotage");
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText("");
                    return false;
                }
                if ((args[0] == "/radio" || args[0] == "/rd") && Options.ProximityChat.GetBool() && Options.ImpostorRadio.GetBool() && PlayerControl.LocalPlayer.Data.Role.IsImpostor && (Options.CurrentGamemode == Gamemodes.Classic || Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek || Options.CurrentGamemode == Gamemodes.RandomItems) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var message = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        message += subArgs;
                    }
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (PlayerControl.LocalPlayer != pc && pc.Data.Role.IsImpostor)
                            Main.ProximityMessages[pc.PlayerId].Add(("[Radio] " + Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] + ": " + message, 0f));
                    }
                    Utils.SendSpam("Someone sent proximity message");
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText("");
                    return false;
                }
                if (Options.ProximityChat.GetBool())
                {
                    var message = "";
                    for (int i = 0; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        message += subArgs;
                    }
                    string appearance = Main.StandardNames[PlayerControl.LocalPlayer.PlayerId];
                    if (Options.FakeShapeshiftAppearance.GetBool()) appearance = Main.StandardNames[Main.AllShapeshifts[PlayerControl.LocalPlayer.PlayerId]];
                    if (Options.CurrentGamemode == Gamemodes.RandomItems)
                    {
                        if (RandomItemsGamemode.instance.CamouflageTimer > 0f) 
                            appearance = "???";
                    }
                    PlayerControl.LocalPlayer.SendProximityMessage(appearance, message);
                    Utils.SendSpam("Someone sent proximity message");
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText("");
                    return false;
                }
                return true;
            }
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
                                case "thefungle":
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
                            GameOptionsManager.Instance.currentNormalGameOptions.SetInt(Int32OptionNames.NumImpostors, int.Parse(subArgs));
                            break;
                        case "players":
                            subArgs = args.Length < 3 ? "" : args[2];
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
                                case "none":
                                    GameOptionsManager.Instance.currentGameMode = GameModes.None;
                                    break;
                                case "classic":
                                    GameOptionsManager.Instance.SwitchGameMode(GameModes.Normal);
                                    break;
                                case "hidenseek":
                                    GameOptionsManager.Instance.SwitchGameMode(GameModes.HideNSeek);
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
                    Utils.SyncSettings(GameOptionsManager.Instance.currentGameOptions);
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
                        case "paintbattle":
                            Options.Gamemode.SetValue(7);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is paint battle", "GamemodesChanger");
                            break;
                        case "killordie":
                            Options.Gamemode.SetValue(8);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is kill or die", "GamemodesChanger");
                            break;
                        case "zombies":
                            Options.Gamemode.SetValue(9);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is zombies", "GamemodesChanger");
                            break;
                    }
                    break;
               
                case "/color":
                case "/colour":
                    canceled = true;
                    if (Main.GameStarted && Options.CurrentGamemode != Gamemodes.PaintBattle) break;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "red":
                            PlayerControl.LocalPlayer.RpcSetColor(0);
                            break;
                        case "blue":
                            PlayerControl.LocalPlayer.RpcSetColor(1);
                            break;
                        case "green":
                            PlayerControl.LocalPlayer.RpcSetColor(2);
                            break;
                        case "pink":
                            PlayerControl.LocalPlayer.RpcSetColor(3);
                            break;
                        case "orange":
                            PlayerControl.LocalPlayer.RpcSetColor(4);
                            break;
                        case "yellow":
                            PlayerControl.LocalPlayer.RpcSetColor(5);
                            break;
                        case "black":
                            PlayerControl.LocalPlayer.RpcSetColor(6);
                            break;
                        case "white":
                            PlayerControl.LocalPlayer.RpcSetColor(7);
                            break;
                        case "purple":
                            PlayerControl.LocalPlayer.RpcSetColor(8);
                            break;
                        case "brown":
                            PlayerControl.LocalPlayer.RpcSetColor(9);
                            break;
                        case "cyan":
                            PlayerControl.LocalPlayer.RpcSetColor(10);
                            break;
                        case "lime":
                            PlayerControl.LocalPlayer.RpcSetColor(11);
                            break;
                        case "maroon":
                            PlayerControl.LocalPlayer.RpcSetColor(12);
                            break;
                        case "rose":
                            PlayerControl.LocalPlayer.RpcSetColor(13);
                            break;
                        case "banana":
                            PlayerControl.LocalPlayer.RpcSetColor(14);
                            break;
                        case "gray":
                        case "grey":
                            PlayerControl.LocalPlayer.RpcSetColor(15);
                            break;
                        case "tan":
                            PlayerControl.LocalPlayer.RpcSetColor(16);
                            break;
                        case "coral":
                            PlayerControl.LocalPlayer.RpcSetColor(17);
                            break;
                        case "fortegreen":
                            PlayerControl.LocalPlayer.RpcSetColor(18);
                            break;
                        default:
                            PlayerControl.LocalPlayer.RpcSetColor(byte.Parse(subArgs));  
                            break;
                    }        
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
                    PlayerControl.LocalPlayer.RpcSetName(name);
                    break;
                case "/tpout":
                    canceled = true;
                    if (Main.GameStarted) break;
                    PlayerControl.LocalPlayer.RpcTeleport(new Vector2(0.1f, 3.8f));
                    break;
                case "/tpin":
                    canceled = true;
                    if (Main.GameStarted) break;
                    PlayerControl.LocalPlayer.RpcTeleport(new Vector2(-0.2f, 1.3f));
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
                                    Utils.SendChat("Classic: Standard among us game.", "Gamemodes");
                                    break;
                                case "hideandseek":
                                    Utils.SendChat("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                    break;
                                case "shiftandseek":
                                    Utils.SendChat("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes");
                                    break;
                                case "bombtag":
                                    Utils.SendChat("Bomb Tag: Game lasts for few round. Random player/players gets bomb at start of the round. Player with bomb need to give away a bomb. After the round players with bombs die. Click kill to give bomb away. Black players have bomb. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "randomitems":
                                    Utils.SendChat("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick.", "Gamemodes");
                                    break;
                                case "battleroyale":
                                    Utils.SendChat("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "speedrun":
                                    Utils.SendChat("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                    break;
                                case "paintbattle":
                                    Utils.SendChat("Paint Battle: Type (/color COLOR) command to change paint color. Pet your pet to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                    break;
                                case "killordie":
                                    Utils.SendChat("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "zombies":
                                    Utils.SendChat("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Zombies show up as dead during meetings. Special roles and sabotages are disabled. Impostors can't vent. Depending on options you see arrow pointing to zombie(s).", "Gamemodes");
                                    break;
                                case "randomspawn":
                                    Utils.SendChat("Random Spawn: At start teleports everyone to random vent. Depending on options it teleports after meeting too.", "Gamemodes");
                                    break;
                                case "randommap":
                                    Utils.SendChat("Random Map: Map is randomly chosen before game starts.", "Gamemodes");
                                    break;
                                case "disablegapplatform":
                                    Utils.SendChat("Disable Gap Platform: Players can't use gap platform on airship.", "Gamemodes");
                                    break;
                                case "midgamechat":
                                    Utils.SendChat("Mid Game Chat: You can chat during rounds. If proximity chat is on only nearby players see you messages. Depending on options impostors can communicate via radio by typing /radio MESSAGE.", "Gamemodes");
                                    break;
                                default:
                                    switch (Options.CurrentGamemode)
                                    {
                                        case Gamemodes.Classic:
                                            Utils.SendChat("Classic: Standard among us game.", "Gamemodes");
                                            break;
                                        case Gamemodes.HideAndSeek:
                                            Utils.SendChat("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                            break;
                                        case Gamemodes.ShiftAndSeek:
                                            Utils.SendChat("Shift And Seek: Everyone is engineer or shapeshifter.Impostors are visible to other players.Crewmates wins by finishing their tasks, impostors by killing every single crewmate.Impostor must shapeshift into person he want kill.", "Gamemodes");
                                            break;
                                        case Gamemodes.BombTag:
                                            Utils.SendChat("Bomb Tag: Game lasts for few round. Random player/players gets bomb at start of the round. Player with bomb need to give away a bomb. After the round players with bombs die. Click kill to give bomb away. Black players have bomb. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.RandomItems:
                                            Utils.SendChat("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick.", "Gamemodes");
                                            break;
                                        case Gamemodes.BattleRoyale:
                                            Utils.SendChat("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Speedrun:
                                            Utils.SendChat("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                            break;
                                        case Gamemodes.PaintBattle:
                                            Utils.SendChat("Paint Battle: Type (/color COLOR) command to change paint color. Pet your pet to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                            break;
                                        case Gamemodes.KillOrDie:
                                            Utils.SendChat("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Zombies:
                                            Utils.SendChat("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Zombies show up as dead during meetings. Special roles and sabotages are disabled. Impostors can't vent. Depending on options you see arrow pointing to zombie(s).", "Gamemodes");
                                            break;
                                    }
                                    if (Options.RandomSpawn.GetBool())
                                        Utils.SendChat("Random Spawn: At start teleports everyone to random vent. Depending on options it teleports after meeting too.", "Gamemodes");
                                    if (Options.RandomMap.GetBool())
                                        Utils.SendChat("Random Map: Map is randomly chosen before game starts.", "Gamemodes");
                                    if (Options.DisableGapPlatform.GetBool())
                                        Utils.SendChat("Disable Gap Platform: Players can't use gap platform on airship.", "Gamemodes");
                                    if (Options.MidGameChat.GetBool())
                                        Utils.SendChat("Mid Game Chat: You can chat during rounds. If proximity chat is on only nearby players see you messages. Depending on options impostors can communicate via radio by typing /radio MESSAGE.", "Gamemodes");
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
                                case "swap":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Swap), "Items");
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
                                case "trap":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Trap), "Items");
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
                                case "compass":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Compass), "Items");
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
                    if (PlayerControl.LocalPlayer.GetItem() == Items.Stop)
                    {
                        MeetingHud.Instance.RpcClose();
                        VotingCompletePatch.Postfix();  
                        PlayerControl.LocalPlayer.RpcSetItem(Items.None);
                        Utils.SendSpam("Someone used /stop command!");
                    }
                    break;
                case "/n":
                case "/now":
                    canceled = true;
                    var message = "";
                    message += "No game end: "; message += Options.NoGameEnd.GetBool() ? "ON\n" : "OFF\n";
                    message += "Can use /color command: "; message += Options.CanUseColorCommand.GetBool() ? "ON\n" : "OFF\n";
                    if (Options.CanUseColorCommand.GetBool())
                    {
                        message += "\nEnable fortegreen: "; message += Options.EnableFortegreen.GetBool() ? "ON" : "OFF";
                    }   
                    message += "\nCan use /name command: "; message += Options.CanUseNameCommand.GetBool() ? "ON" : "OFF";
                    if (Options.CanUseNameCommand.GetBool())
                    {
                        message += "\nEnable name repeating: "; message += Options.EnableNameRepeating.GetBool() ? "ON" : "OFF";
                        message += "\nMaximum name length: " + Options.MaximumNameLength.GetInt();
                    }

                    message += "\nCan use /tpout command: "; message += Options.CanUseTpoutCommand.GetBool() ? "ON" : "OFF";
                    Utils.SendChat(message, "Options");

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
                            message += "\nSwap: "; message += Options.EnableSwap.GetBool() ? "ON\n" : "OFF\n";
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
                                message += "Bomb radius: " + Options.BombRadius.GetFloat() + "x\n";
                                message += "Can kill impostors: "; message += Options.CanKillImpostors.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nTrap: "; message += Options.EnableTrap.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableTrap.GetBool())
                            {
                                message += "Trap wait time: " + Options.TrapWaitTime.GetFloat() + "s\n";
                                message += "Trap radius: " + Options.BombRadius.GetFloat() + "x\n";
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
                            message += "\nCompass: "; message += Options.EnableCompass.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableCompass.GetBool())
                            {
                                message += "Compass duration: " + Options.CompassDuration.GetFloat() + "s\n";
                            }
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
                        case Gamemodes.PaintBattle:
                            message = "Gamemode: Paint Battle\n\n";
                            message += "Painting Time: " + Options.PaintingTime.GetInt() + "s\n";
                            message += "Voting Time: " + Options.VotingTime.GetInt() + "s\n";
                            break;
                        case Gamemodes.KillOrDie:
                            message = "Gamemode: Kill or die\n\n";
                            message += "Teleport after round: "; message += Options.TeleportAfterRound.GetBool() ? "ON\n" : "OFF\n";
                            message += "Killer blind time: " + Options.KillerBlindTime.GetFloat() + "s\n";
                            message += "Time to kill: " + Options.TimeToKill.GetInt() + "%\n";
                            break;
                        case Gamemodes.Zombies:
                            message = "Gamemode: Zombies\n\n";
                            message += "Zombie Kills Turn Into Zombie: "; message += Options.ZombieKillsTurnIntoZombie.GetBool() ? "ON\n" : "OFF\n";
                            message += "Zombie Speed: " + Options.ZombieSpeed.GetFloat() + "x\n";
                            message += "Zombie Vision: " + Options.ZombieVision.GetFloat() + "x\n";
                            message += "Can Kill Zombies After Tasks: "; message += Options.CanKillZombiesAfterTasks.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.CanKillZombiesAfterTasks.GetBool())
                            {
                                message += "Number Of Kills: " + Options.NumberOfKills.GetInt() + "\n";
                            }
                            message += "Zombie Blind Time: " + Options.ZombieBlindTime.GetFloat() + "s\n";
                            message += "Tracking Zombies Mode: " + Utils.TrackingZombiesModeString(Options.CurrentTrackingZombiesMode);
                            break;
                    }
                    Utils.SendChat(message, "Options");

                    message = "Additional Gamemodes:\n";
                    if (Options.RandomSpawn.GetBool())
                    {
                        message += "\nRandom Spawn\n";
                        message += "Teleport After Meeting: "; message += Options.TeleportAfterMeeting.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (Options.RandomMap.GetBool())
                    {
                        message += "\nRandom Map\n";
                        message += "Add The Skeld: "; message += Options.AddTheSkeld.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add Mira HQ: "; message += Options.AddMiraHQ.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add Polus: "; message += Options.AddPolus.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add The Airship: "; message += Options.AddTheAirship.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add The Fungle: "; message += Options.AddTheFungle.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (Options.DisableGapPlatform.GetBool())
                        message += "\nDisable Gap Platform\n";
                    if (Options.MidGameChat.GetBool())
                    {
                        message += "\nMid Game Chat\n";
                        message += "Proximity Chat: "; message += Options.ProximityChat.GetBool() ? "ON\n" : "OFF\n";
                        if (Options.ProximityChat.GetBool())
                        {
                            message += "Messages Radius: " + Options.MessagesRadius.GetFloat() + "x\n";
                            message += "Impostor Radio: "; message += Options.ImpostorRadio.GetBool() ? "ON\n" : "OFF\n";
                            message += "Fake Shapeshift Appearance: "; message += Options.FakeShapeshiftAppearance.GetBool() ? "ON\n" : "OFF\n";
                        }
                        message += "Disable During Comms Sabotage: "; message += Options.DisableDuringCommsSabotage.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (message != "Additional Gamemodes:\n")
                        Utils.SendChat(message, "Options");
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
                    PlayerControl.LocalPlayer.RpcSendMessage("Commands:\n/color COLOR - changes your color\n/name NAME - changes your name\n/help gamemode - show gamemode description\n" +
                        "/tpout - teleport outside the ship\n/tpin - teleport inside the ship\n" +
                        "/now - show active settings\n/id (players, colors) - show ids\n/help item - show item description\n/commands - show list of commands\n/changesetting SETTING VALUE - changes setting value\n" +
                        "/gamemode GAMEMODE - changes gamemode\n/kick PLAYER_ID - kick player\n/ban PLAYER_ID - ban player\n/announce MESSAGE - send message\n/lastresult - show last game result", "Command");
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
                    Utils.SendChat(announce, "HostMessage");
                    break;
                case "/lastresult":
                case "/l":
                    canceled = true;
                    if (Main.LastResult != "")
                        PlayerControl.LocalPlayer.RpcSendMessage(Main.LastResult, "LastResult");
                    break;
                case "/info":
                    if (Options.CurrentGamemode != Gamemodes.RandomItems || !Main.GameStarted || PlayerControl.LocalPlayer.Data.IsDead) break;
                    canceled = true;
                    if (PlayerControl.LocalPlayer.GetItem() == Items.Newsletter)
                    {
                        int crewmates = 0;
                        int scientists = 0 ;
                        int engineers = 0;       
                        int impostors = 0;
                        int shapeshifters = 0;
                        int alivePlayers = 0;
                        int deadPlayers = 0;
                        int killedPlayers = 0;
                        int exiledPlayers = 0;
                        int disconnectedPlayers = 0;
                        int misfiredPlayers = 0;
                        int bombedPlayers = 0;
                        int suicidePlayers = 0;
                        int trappedPlayers = 0;
                        string msg = "Roles in game:\n";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            switch (pc.Data.Role.Role)
                            {
                                case RoleTypes.Crewmate: ++crewmates; break;
                                case RoleTypes.Scientist: ++scientists; break;
                                case RoleTypes.Engineer: ++engineers; break;
                                case RoleTypes.Impostor: ++impostors; break;
                                case RoleTypes.Shapeshifter: ++shapeshifters; break;
                            }
                        }
                        for (byte i = 0; i <= 14; ++i)
                        {
                            if (Main.AllPlayersDeathReason.ContainsKey(i))
                            {
                                if (Main.AllPlayersDeathReason[i] == DeathReasons.Alive)
                                    ++alivePlayers;
                                else
                                {
                                    ++deadPlayers;
                                    switch (Main.AllPlayersDeathReason[i])
                                    {
                                        case DeathReasons.Killed: ++killedPlayers; break;
                                        case DeathReasons.Exiled: ++exiledPlayers; break;
                                        case DeathReasons.Disconnected: ++disconnectedPlayers; break;
                                        case DeathReasons.Misfire: ++misfiredPlayers; break;
                                        case DeathReasons.Bombed: ++bombedPlayers; break;
                                        case DeathReasons.Suicide: ++suicidePlayers; break;
                                        case DeathReasons.Trapped: ++trappedPlayers; break;
                                    }
                                }
                            }
                        }
                        msg += crewmates + " crewamtes\n";
                        msg += scientists + " scientists\n";
                        msg += engineers + " engineers\n";
                        msg += impostors + " impostors\n";
                        msg += shapeshifters + " shapeshifters\n\n";
                        msg += alivePlayers + " players are alive\n";
                        msg += deadPlayers + " players died:\n";
                        msg += killedPlayers + " by getting killed\n";
                        msg += exiledPlayers + " by voting\n";
                        msg += disconnectedPlayers + " players disconnected\n";
                        msg += misfiredPlayers + " misfired on crewmate\n";
                        msg += bombedPlayers + " players got bombed\n";
                        msg += suicidePlayers + " commited suicide\n";  
                        msg += trappedPlayers + " players trapped\n";      
                        PlayerControl.LocalPlayer.RpcSendMessage(msg, "Newsletter");
                        PlayerControl.LocalPlayer.RpcSetItem(Items.None);
                    }
                    break;
                case "1":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 1, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "2":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 2, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "3":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 3, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "4":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 4, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "5":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 5, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "6":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 6, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "7":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 7, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "8":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 8, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "9":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 9, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                case "10":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 10, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[PlayerControl.LocalPlayer.PlayerId] = true;
                    break;
                default:
                    break;
            }
            if (canceled)
            {
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText("");
            }
            return !canceled;
        }
        public static bool OnReceiveChat(PlayerControl player, string text)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var canceled = false;
            string[] args = text.Split(' ');
            string subArgs = "";   
            if (Options.MidGameChat.GetBool() && Main.GameStarted && !Main.IsMeeting && Options.CurrentGamemode != Gamemodes.PaintBattle)
            {
                if (Utils.IsActive(SystemTypes.Comms) && Options.DisableDuringCommsSabotage.GetBool())
                {
                    Utils.SendSpam("Someone tried to send message during comms sabotage");
                    return false;
                }
                if ((args[0] == "/radio" || args[0] == "/rd") && Options.ProximityChat.GetBool() && Options.ImpostorRadio.GetBool() && player.Data.Role.IsImpostor && (Options.CurrentGamemode == Gamemodes.Classic || Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek || Options.CurrentGamemode == Gamemodes.RandomItems) && !player.Data.IsDead)
                {
                    var message = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        message += subArgs;
                    }
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (player != pc && pc.Data.Role.IsImpostor)
                            Main.ProximityMessages[pc.PlayerId].Add(("[Radio] " + Main.StandardNames[player.PlayerId] + ": " + message, 0f));
                    }
                    Utils.SendSpam("Someone sent proximity message");
                    return false;
                }
                if (Options.ProximityChat.GetBool())
                {
                    var message = "";
                    for (int i = 0; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        message += subArgs;
                    }
                    string appearance = Main.StandardNames[player.PlayerId];
                    if (Options.FakeShapeshiftAppearance.GetBool()) appearance = Main.StandardNames[Main.AllShapeshifts[player.PlayerId]];
                    if (Options.CurrentGamemode == Gamemodes.RandomItems)
                    {
                        if (RandomItemsGamemode.instance.CamouflageTimer > 0f) 
                            appearance = "???";
                    }
                    player.SendProximityMessage(appearance, message);
                    Utils.SendSpam("Someone sent proximity message");
                    return false;
                }
                return true;
            }
            switch (args[0])
            {
                case "/color":
                case "/colour":
                    canceled = true;
                    if (!Options.CanUseColorCommand.GetBool() && Options.CurrentGamemode != Gamemodes.PaintBattle) break;
                    if (Main.GameStarted && Options.CurrentGamemode != Gamemodes.PaintBattle) break;
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "red":
                            player.RpcSetColor(0);
                            break;
                        case "blue":
                            player.RpcSetColor(1);
                            break;
                        case "green":
                            player.RpcSetColor(2);
                            break;
                        case "pink":
                            player.RpcSetColor(3);
                            break;
                        case "orange":
                            player.RpcSetColor(4);
                            break;
                        case "yellow":
                            player.RpcSetColor(5);
                            break;
                        case "black":
                            player.RpcSetColor(6);
                            break;
                        case "white":
                            player.RpcSetColor(7);
                            break;
                        case "purple":
                            player.RpcSetColor(8);
                            break;
                        case "brown":
                            player.RpcSetColor(9);
                            break;
                        case "cyan":
                            player.RpcSetColor(10);
                            break;
                        case "lime":
                            player.RpcSetColor(11);
                            break;
                        case "maroon":
                            player.RpcSetColor(12);
                            break;
                        case "rose":
                            player.RpcSetColor(13);
                            break;
                        case "banana":
                            player.RpcSetColor(14);
                            break;
                        case "gray":
                        case "grey":
                            player.RpcSetColor(15);
                            break;
                        case "tan":
                            player.RpcSetColor(16);
                            break;
                        case "coral":
                            player.RpcSetColor(17);
                            break;
                        case "fortegreen":
                            if (Options.EnableFortegreen.GetBool() || (Options.CurrentGamemode == Gamemodes.PaintBattle && Main.GameStarted))
                                player.RpcSetColor(18);
                            break;
                        default:
                            if (byte.Parse(subArgs) < 18 || Options.EnableFortegreen.GetBool() || (Options.CurrentGamemode == Gamemodes.PaintBattle && Main.GameStarted))
                                player.RpcSetColor(byte.Parse(subArgs));  
                            break;
                    }        
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
                    if (name.Length > Options.MaximumNameLength.GetInt()) break;
                    if (Options.EnableNameRepeating.GetBool())
                        player.RpcSetName(name);
                    else
                        player.CheckName(name);
                    break;
                case "/tpout":
                    if (!Options.CanUseTpoutCommand.GetBool()) break;
                    canceled = true;
                    if (Main.GameStarted) break;
                    player.RpcTeleport(new Vector2(0.1f, 3.8f));
                    break;
                case "/tpin":
                    canceled = true;
                    if (Main.GameStarted) break;
                    player.RpcTeleport(new Vector2(-0.2f, 1.3f));
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
                                case "speedrun":
                                    player.RpcSendMessage("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                    break;
                                case "paintbattle":
                                    player.RpcSendMessage("Paint Battle: Type (/color COLOR) command to change paint color. Pet your pet to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                    break;
                                case "killordie":
                                    player.RpcSendMessage("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "zombies":
                                    player.RpcSendMessage("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Zombies show up as dead during meetings. Special roles and sabotages are disabled. Impostors can't vent. Depending on options you see arrow pointing to zombie(s).", "Gamemodes");
                                    break;
                                case "randomspawn":
                                    player.RpcSendMessage("Random Spawn: At start teleports everyone to random vent. Depending on options it teleports after meeting too.", "Gamemodes");
                                    break;
                                case "randommap":
                                    player.RpcSendMessage("Random Map: Map is randomly chosen before game starts.", "Gamemodes");
                                    break;
                                case "disablegapplatform":
                                    player.RpcSendMessage("Disable Gap Platform: Players can't use gap platform on airship.", "Gamemodes");
                                    break;
                                case "midgamechat":
                                    player.RpcSendMessage("Mid Game Chat: You can chat during rounds. If proximity chat is on only nearby players see you messages. Depending on options impostors can communicate via radio by typing /radio MESSAGE.", "Gamemodes");
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
                                        case Gamemodes.Speedrun:
                                            player.RpcSendMessage("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                            break;
                                        case Gamemodes.PaintBattle:
                                            player.RpcSendMessage("Paint Battle: Type (/color COLOR) command to change paint color. Pet your pet to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                            break;
                                        case Gamemodes.KillOrDie:
                                            player.RpcSendMessage("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Zombies:
                                            player.RpcSendMessage("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Zombies show up as dead during meetings. Special roles and sabotages are disabled. Impostors can't vent. Depending on options you see arrow pointing to zombie(s).", "Gamemodes");
                                            break;
                                    }
                                    if (Options.RandomSpawn.GetBool())
                                        player.RpcSendMessage("Random Spawn: At start teleports everyone to random vent. Depending on options it teleports after meeting too.", "Gamemodes");
                                    if (Options.RandomMap.GetBool())
                                        player.RpcSendMessage("Random Map: Map is randomly chosen before game starts.", "Gamemodes");
                                    if (Options.DisableGapPlatform.GetBool())
                                        player.RpcSendMessage("Disable Gap Platform: Players can't use gap platform on airship.", "Gamemodes");
                                    if (Options.MidGameChat.GetBool())
                                        player.RpcSendMessage("Mid Game Chat: You can chat during rounds. If proximity chat is on only nearby players see you messages. Depending on options impostors can communicate via radio by typing /radio MESSAGE.", "Gamemodes");
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
                                case "swap":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Swap), "Items");
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
                                case "trap":
                                    PlayerControl.LocalPlayer.RpcSendMessage(Utils.ItemDescriptionLong(Items.Trap), "Items");
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
                                case "compass":
                                    player.RpcSendMessage(Utils.ItemDescriptionLong(Items.Compass), "Items");
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
                    if (player.GetItem() == Items.Stop)
                    {
                        MeetingHud.Instance.RpcClose();
                        VotingCompletePatch.Postfix();  
                        player.RpcSetItem(Items.None);
                        Utils.SendSpam("Someone used /stop command!");
                    }
                    break;
                case "/n":
                case "/now":
                    canceled = true;
                    var message = "";
                    message += "No game end: "; message += Options.NoGameEnd.GetBool() ? "ON" : "OFF";
                    message += "\nCan use /color command: "; message += Options.CanUseColorCommand.GetBool() ? "ON" : "OFF";
                    if (Options.CanUseColorCommand.GetBool())
                    {
                        message += "\nEnable fortegreen: "; message += Options.EnableFortegreen.GetBool() ? "ON" : "OFF";
                    }   
                    message += "\nCan use /name command: "; message += Options.CanUseNameCommand.GetBool() ? "ON" : "OFF";
                    if (Options.CanUseNameCommand.GetBool())
                    {
                        message += "\nEnable name repeating: "; message += Options.EnableNameRepeating.GetBool() ? "ON" : "OFF";
                        message += "\nMaximum name length:" + Options.MaximumNameLength.GetInt();
                    }
                    message += "\nCan use /tpout command: "; message += Options.CanUseTpoutCommand.GetBool() ? "ON" : "OFF";
                    player.RpcSendMessage(message, "Options");

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
                            message += "\nSwap: "; message += Options.EnableSwap.GetBool() ? "ON\n" : "OFF\n";
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
                                message += "Bomb radius: " + Options.BombRadius.GetFloat() + "x\n";
                                message += "Can kill impostors: "; message += Options.CanKillImpostors.GetBool() ? "ON\n" : "OFF\n";
                            }
                            message += "\nTrap: "; message += Options.EnableTrap.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableTrap.GetBool())
                            {
                                message += "Trap wait time: " + Options.TrapWaitTime.GetFloat() + "s\n";
                                message += "Trap radius: " + Options.BombRadius.GetFloat() + "x\n";
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
                            message += "\nCompass: "; message += Options.EnableCompass.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.EnableCompass.GetBool())
                            {
                                message += "Compass duration: " + Options.CompassDuration.GetFloat() + "s\n";
                            }
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
                        case Gamemodes.PaintBattle:
                            message = "Gamemode: Paint Battle\n\n";
                            message += "Painting Time: " + Options.PaintingTime.GetInt() + "s\n";
                            message += "Voting Time: " + Options.VotingTime.GetInt() + "s\n";
                            break;
                        case Gamemodes.KillOrDie:
                            message = "Gamemode: Kill or die\n\n";
                            message += "Teleport After Round: "; message += Options.TeleportAfterRound.GetBool() ? "ON\n" : "OFF\n";
                            message += "Killer Blind Time: " + Options.KillerBlindTime.GetFloat() + "s\n";
                            message += "Time To Kill: " + Options.TimeToKill.GetInt() + "%\n";
                            break;
                        case Gamemodes.Zombies:
                            message = "Gamemode: Zombies\n\n";
                            message += "Zombie Kills Turn Into Zombie: "; message += Options.ZombieKillsTurnIntoZombie.GetBool() ? "ON\n" : "OFF\n";
                            message += "Zombie Speed: " + Options.ZombieSpeed.GetFloat() + "x\n";
                            message += "Zombie Vision: " + Options.ZombieVision.GetFloat() + "x\n";
                            message += "Can Kill Zombies After Tasks: "; message += Options.CanKillZombiesAfterTasks.GetBool() ? "ON\n" : "OFF\n";
                            if (Options.CanKillZombiesAfterTasks.GetBool())
                            {
                                message += "Number Of Kills: " + Options.NumberOfKills.GetInt() + "\n";
                            }
                            message += "Zombie Blind Time: " + Options.ZombieBlindTime.GetFloat() + "s\n";
                            message += "Tracking Zombies Mode: " + Utils.TrackingZombiesModeString(Options.CurrentTrackingZombiesMode);
                            break;
                    }
                    player.RpcSendMessage(message, "Options");

                    message = "Additional Gamemodes:\n";
                    if (Options.RandomSpawn.GetBool())
                    {
                        message += "\nRandom Spawn\n";
                        message += "Teleport After Meeting: "; message += Options.TeleportAfterMeeting.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (Options.RandomMap.GetBool())
                    {
                        message += "\nRandom Map\n";
                        message += "Add The Skeld: "; message += Options.AddTheSkeld.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add Mira HQ: "; message += Options.AddMiraHQ.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add Polus: "; message += Options.AddPolus.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add The Airship: "; message += Options.AddTheAirship.GetBool() ? "ON\n" : "OFF\n";
                        message += "Add The Fungle: "; message += Options.AddTheFungle.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (Options.DisableGapPlatform.GetBool())
                        message += "\nDisable Gap Platform\n";
                    if (Options.MidGameChat.GetBool())
                    {
                        message += "\nMid Game Chat\n";
                        message += "Proximity Chat: "; message += Options.ProximityChat.GetBool() ? "ON\n" : "OFF\n";
                        if (Options.ProximityChat.GetBool())
                        {
                            message += "Messages Radius: " + Options.MessagesRadius.GetFloat() + "x\n";
                            message += "Impostor Radio: "; message += Options.ImpostorRadio.GetBool() ? "ON\n" : "OFF\n";
                            message += "Fake Shapeshift Appearance: "; message += Options.FakeShapeshiftAppearance.GetBool() ? "ON\n" : "OFF\n";
                        }
                        message += "Disable During Comms Sabotage: "; message += Options.DisableDuringCommsSabotage.GetBool() ? "ON\n" : "OFF\n";
                    }
                    if (message != "Additional Gamemodes:\n")
                        player.RpcSendMessage(message, "Options");
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
                    player.RpcSendMessage("Commands:\n/color COLOR - changes your color\n/name NAME - changes your name\n/help gamemode - show gamemode description\n" +
                        "/tpout - teleport outside the ship\n/tpin - teleport inside the ship\n/now - show active settings\n" +
                        "/id (players, colors) - show ids\n/help item - show item description\n/commands - show list of commands\n/lastresult - show last game result", "Commands");
                    break;
                case "/lastresult":
                case "/l":
                    canceled = true;
                    if (Main.LastResult != "")
                        player.RpcSendMessage(Main.LastResult, "LastResult");
                    break;
                case "/info":
                    if (Options.CurrentGamemode != Gamemodes.RandomItems || !Main.GameStarted || player.Data.IsDead) break;
                    canceled = true;
                    if (player.GetItem() == Items.Newsletter)
                    {
                        int crewmates = 0;
                        int scientists = 0 ;
                        int engineers = 0;       
                        int impostors = 0;
                        int shapeshifters = 0;
                        int alivePlayers = 0;
                        int deadPlayers = 0;
                        int killedPlayers = 0;
                        int exiledPlayers = 0;
                        int disconnectedPlayers = 0;
                        int misfiredPlayers = 0;
                        int bombedPlayers = 0;
                        int suicidePlayers = 0;
                        int trappedPlayers = 0;
                        string msg = "Roles in game:\n";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            switch (pc.Data.Role.Role)
                            {
                                case RoleTypes.Crewmate: ++crewmates; break;
                                case RoleTypes.Scientist: ++scientists; break;
                                case RoleTypes.Engineer: ++engineers; break;
                                case RoleTypes.Impostor: ++impostors; break;
                                case RoleTypes.Shapeshifter: ++shapeshifters; break;
                            }
                        }
                        for (byte i = 0; i <= 14; ++i)
                        {
                            if (Main.AllPlayersDeathReason.ContainsKey(i))
                            {
                                if (Main.AllPlayersDeathReason[i] == DeathReasons.Alive)
                                    ++alivePlayers;
                                else
                                {
                                    ++deadPlayers;
                                    switch (Main.AllPlayersDeathReason[i])
                                    {
                                        case DeathReasons.Killed: ++killedPlayers; break;
                                        case DeathReasons.Exiled: ++exiledPlayers; break;
                                        case DeathReasons.Disconnected: ++disconnectedPlayers; break;
                                        case DeathReasons.Misfire: ++misfiredPlayers; break;
                                        case DeathReasons.Bombed: ++bombedPlayers; break;
                                        case DeathReasons.Suicide: ++suicidePlayers; break;
                                        case DeathReasons.Trapped: ++trappedPlayers; break;
                                    }
                                }
                            }
                        }
                        msg += crewmates + " crewamtes\n";
                        msg += scientists + " scientists\n";
                        msg += engineers + " engineers\n";
                        msg += impostors + " impostors\n";
                        msg += shapeshifters + " shapeshifters\n\n";
                        msg += alivePlayers + " players are alive\n";
                        msg += deadPlayers + " players died:\n";
                        msg += killedPlayers + " by getting killed\n";
                        msg += exiledPlayers + " by voting\n";
                        msg += disconnectedPlayers + " players disconnected\n";
                        msg += misfiredPlayers + " misfired on crewmate\n";
                        msg += bombedPlayers + " players got bombed\n";
                        msg += suicidePlayers + " commited suicide\n";  
                        msg += trappedPlayers + " players trapped\n";      
                        player.RpcSendMessage(msg, "Newsletter");
                        player.RpcSetItem(Items.None);
                    }
                    break;
                case "1":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 1, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "2":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 2, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "3":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 3, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "4":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 4, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "5":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 5, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "6":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 6, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "7":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 7, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "8":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 8, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "9":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 9, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                case "10":
                    if (PaintBattleGamemode.instance == null) break;
                    if (PaintBattleGamemode.instance.HasVoted[player.PlayerId]) break;
                    PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId] = (PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item1 + 10, PaintBattleGamemode.instance.PlayerVotes[PaintBattleGamemode.instance.VotingPlayerId].Item2 + 1);
                    PaintBattleGamemode.instance.HasVoted[player.PlayerId] = true;
                    break;
                default:
                    break;
            }
            return !canceled;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost || Main.MessagesToSend.Count < 1 || (Main.MessagesToSend[0].Item2 == byte.MaxValue && 1f > __instance.timeSinceLastMessage)) return;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            (string msg, byte sendTo, string title) = Main.MessagesToSend[0];
            Main.MessagesToSend.RemoveAt(0);
            int clientId = sendTo == byte.MaxValue ? -1 : Utils.GetPlayerById(sendTo).GetClientId();
            var name = player.Data.PlayerName;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var clientId2 = pc.GetClientId();
                if (clientId == clientId2 || clientId == -1)
                {
                    if (pc.AmOwner)
                    {
                        player.SetName(Utils.ColorString(Color.blue, "MG.SystemMessage." + title));
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg);
                        player.SetName(name);
                        if (Main.GameStarted)
                            player.RpcSetNamePrivate(Main.LastNotifyNames[(player.PlayerId, pc.PlayerId)], pc, true);
                    }
                    else
                    {
                        new LateTask(() =>
                        {
                            var writer = CustomRpcSender.Create("MessagesToSend", SendOption.None);
                            writer.StartMessage(clientId2);
                            writer.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                                .Write(Utils.ColorString(Color.blue, "MG.SystemMessage." + title))
                                .EndRpc();
                            writer.StartRpc(player.NetId, (byte)RpcCalls.SendChat)
                                .Write(msg)
                                .EndRpc();
                            writer.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                                .Write(Main.GameStarted ? Main.LastNotifyNames[(player.PlayerId, pc.PlayerId)] : name)
                                .EndRpc();
                            writer.EndMessage();
                            writer.SendMessage();
                        }, 0f, "Send Message");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    class RpcSendChatPatch
    {
        public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
        {
            if (string.IsNullOrWhiteSpace(chatText))
            {
                __result = false;
                return false;
            }
            int return_count = PlayerControl.LocalPlayer.name.Count(x => x == '\n');
            chatText = new StringBuilder(chatText).Insert(0, "\n", return_count).ToString();
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
            if (chatText.Contains("who", StringComparison.OrdinalIgnoreCase))
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SendChat, SendOption.None);
            messageWriter.Write(chatText);
            messageWriter.EndMessage();
            __result = true;
            return false;
        }
    }
}