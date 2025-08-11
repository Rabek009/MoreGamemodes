using HarmonyLib;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using System;
using Assets.CoreScripts;
using System.Text;
using AmongUs.Data;
using InnerNet;
using AmongUs.InnerNet.GameDataMessages;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    class SendChatPatch
    {
        public static bool Prefix(ChatController __instance)
        {
            var text = __instance.freeChatField.Text;
            string[] args = text.Split(' ');
            string subArgs = "";
            if (args[0] == "/name" && (text.Contains("<") || text.Contains(">")))
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChatWarning("You can't use text formatting in /name command.");
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText("");
                return false;
            }
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Main.ModdedProtocol.Value || Main.GameStarted)
                __instance.timeSinceLastMessage = 3f;
            if (__instance.quickChatField.Visible)
            {
                return true;
            }
            if (__instance.freeChatField.textArea.text == "")
            {
                return false;
            }
            var canceled = false;
            if (Main.GameStarted && !MeetingHud.Instance && CustomGamemode.Instance.Gamemode == Gamemodes.Classic && args[0] == "/lc" && Romantic.CanChatWithLover.GetBool() && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (Utils.IsActive(SystemTypes.Comms) && Romantic.DisableChatDuringCommsSabotage.GetBool())
                {
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText("");
                    return false;
                }
                var message = "";
                for (int i = 1; i <= args.Length; ++i)
                {
                    subArgs = args.Length < i + 1 ? "" : " " + args[i];
                    message += subArgs;
                }
                if (PlayerControl.LocalPlayer.GetRole().Role == CustomRoles.Romantic)
                {
                    Romantic romanticRole = PlayerControl.LocalPlayer.GetRole() as Romantic;
                    if (romanticRole != null && romanticRole.LoverId != byte.MaxValue)
                    {
                        var lover = Utils.GetPlayerById(romanticRole.LoverId);
                        if (lover != null)
                            lover.Notify(Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Romantic], "[Lover] " + Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] + ": " + message));
                    }
                }
                else
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.GetRole().Role == CustomRoles.Romantic)
                        {
                            Romantic romanticRole = pc.GetRole() as Romantic;
                            if (romanticRole != null && romanticRole.LoverId == PlayerControl.LocalPlayer.PlayerId)
                                pc.Notify(Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Romantic], "[Lover] " + Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] + ": " + message));
                        }
                    }
                }
                Utils.SendSpam(true);
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText("");
                return false;
            }
            if (!Options.EnableMidGameChat.GetBool() && Main.GameStarted && !MeetingHud.Instance && CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText("");
                return false;
            }
            if (Options.EnableMidGameChat.GetBool() && Main.GameStarted && !MeetingHud.Instance && CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle)
            {
                if (Utils.IsActive(SystemTypes.Comms) && Options.DisableDuringCommsSabotage.GetBool())
                {
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText("");
                    return false;
                }
                if ((args[0] == "/radio" || args[0] == "/rd") && Options.EnableMidGameChat.GetBool() && Options.ProximityChat.GetBool() && Options.ImpostorRadio.GetBool() && Main.StandardRoles[PlayerControl.LocalPlayer.PlayerId].IsImpostor() && (CustomGamemode.Instance.Gamemode == Gamemodes.Classic || CustomGamemode.Instance.Gamemode == Gamemodes.HideAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.ShiftAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems || CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun  || CustomGamemode.Instance.Gamemode == Gamemodes.FreezeTag) && !PlayerControl.LocalPlayer.Data.IsDead)
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
                            pc.Notify(Utils.ColorString(Palette.ImpostorRed, "[Radio] " + Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] + ": " + message));
                    }
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText("");
                    return false;
                }
                if (Options.EnableMidGameChat.GetBool() && Options.ProximityChat.GetBool())
                {
                    var message = "";
                    for (int i = 0; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        message += subArgs;
                    }
                    string appearance = Main.StandardNames[PlayerControl.LocalPlayer.PlayerId];
                    if (Options.FakeShapeshiftAppearance.GetBool()) appearance = Main.StandardNames[Main.AllShapeshifts[PlayerControl.LocalPlayer.PlayerId]];
                    if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems)
                    {
                        if (RandomItemsGamemode.instance.CamouflageTimer > -1f) 
                            appearance = "???";
                    }
                    PlayerControl.LocalPlayer.SendProximityMessage(appearance, message);
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
                    subArgs = subArgs.ToLower();
                    switch (subArgs)
                    {
                        case "map":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "theskeld":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
                                    break;
                                case "mirahq":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 1);
                                    break;
                                case "polus":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 2);
                                    break;
                                case "dlekseht":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 3);
                                    break;
                                case "airship":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 4);
                                    break;
                                case "thefungle":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 5);
                                    break;
                                case "custom":
                                    subArgs = args.Length < 4 ? "" : args[3];
                                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, byte.Parse(subArgs));
                                    break;
                            }
                            break;
                        case "impostors":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.NumImpostors, int.Parse(subArgs));
                            break;
                        case "players":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.MaxPlayers, int.Parse(subArgs));
                            break;
                        case "recommended":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.IsDefaults, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.IsDefaults, false);
                                    break;
                            }
                            break;
                        case "confirmejects":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ConfirmImpostor, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ConfirmImpostor, false);
                                    break;
                            }
                            break;
                        case "emergencymeetings":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, int.Parse(subArgs));
                            break;
                        case "anonymousvotes":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.AnonymousVotes, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.AnonymousVotes, false);
                                    break;
                            }
                            break;
                        case "emergencycooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.EmergencyCooldown, int.Parse(subArgs));
                            break;
                        case "discussiontime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.DiscussionTime, int.Parse(subArgs));
                            break;
                        case "votingtime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.VotingTime, int.Parse(subArgs));
                            break;
                        case "playerspeed":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, float.Parse(subArgs));
                            break;
                        case "crewmatevision":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.CrewLightMod, float.Parse(subArgs));
                            break;
                        case "impostorvision":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, float.Parse(subArgs));
                            break;
                        case "killcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.KillCooldown, float.Parse(subArgs));
                            break;
                        case "killdistance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "short":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.KillDistance, 0);
                                    break;
                                case "medium":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.KillDistance, 1);
                                    break;
                                case "long":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.KillDistance, 2);
                                    break;
                                case "custom":
                                    subArgs = args.Length < 4 ? "" : args[3];
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.KillDistance, int.Parse(subArgs));
                                    break;
                            }
                            break;
                        case "taskbarupdates":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "always":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)AmongUs.GameOptions.TaskBarMode.Normal);
                                    break;
                                case "meetings":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)AmongUs.GameOptions.TaskBarMode.MeetingOnly);
                                    break;
                                case "never":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)AmongUs.GameOptions.TaskBarMode.Invisible);
                                    break;
                                case "custom":
                                    subArgs = args.Length < 4 ? "" : args[3];
                                    GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.TaskBarMode, int.Parse(subArgs));
                                    break;
                            }
                            break;
                        case "visualtasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.VisualTasks, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.VisualTasks, false);
                                    break;
                            }
                            break;
                        case "commontasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.NumCommonTasks, int.Parse(subArgs));
                            break;
                        case "longtasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.NumLongTasks, int.Parse(subArgs));
                            break;
                        case "shorttasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.NumShortTasks, int.Parse(subArgs));
                            break;
                        case "scientistcount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Scientist));
                            break;
                        case "scientistchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Scientist), int.Parse(subArgs));
                            break;
                        case "vitalsdisplaycooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, float.Parse(subArgs));
                            break;
                        case "batteryduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ScientistBatteryCharge, float.Parse(subArgs));
                            break;
                        case "engineercount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Engineer));
                            break;
                        case "engineerchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Engineer), int.Parse(subArgs));
                            break;
                        case "ventusecooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, float.Parse(subArgs));
                            break;
                        case "maxtimeinvents":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.EngineerInVentMaxTime, float.Parse(subArgs));
                            break;
                        case "guardianangelcount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel));
                            break;
                        case "guardianangelchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel), int.Parse(subArgs));
                            break;
                        case "protectcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.GuardianAngelCooldown, float.Parse(subArgs));
                            break;
                        case "protectionduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ProtectionDurationSeconds, float.Parse(subArgs));
                            break;
                        case "protectvisibletoimpostors":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
                                    break;
                            }
                            break;
                        case "shapeshiftercount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter));
                            break;
                        case "shapeshifterchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter), int.Parse(subArgs));
                            break;
                        case "shapeshiftduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ShapeshifterDuration, float.Parse(subArgs));
                            break;
                        case "shapeshiftcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, float.Parse(subArgs));
                            break;
                        case "leaveshapeshiftevidence":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
                                    break;
                            }
                            break;
                        case "trackercount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Tracker, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter));
                            break;
                        case "trackerchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Tracker, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter), int.Parse(subArgs));
                            break;
                        case "trackingcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.TrackerCooldown, float.Parse(subArgs));
                            break;
                        case "trackingdelay":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.TrackerDelay, float.Parse(subArgs));
                            break;
                        case "trackingduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.TrackerDuration, float.Parse(subArgs));
                            break;
                        case "noisemakercount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter));
                            break;
                        case "noisemakerchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter), int.Parse(subArgs));
                            break;
                        case "impostorsgetalert":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.NoisemakerImpostorAlert, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.NoisemakerImpostorAlert, false);
                                    break;
                            }
                            break;
                        case "alertduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.NoisemakerAlertDuration, float.Parse(subArgs));
                            break;
                        case "phantomcount":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Phantom, int.Parse(subArgs), GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter));
                            break;
                        case "phantomchance":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Phantom, GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter), int.Parse(subArgs));
                            break;
                        case "vanishduration":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.PhantomDuration, float.Parse(subArgs));
                            break;
                        case "vanishcooldown":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.PhantomCooldown, float.Parse(subArgs));
                            break;
                        case "ghostdotasks":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.GhostsDoTasks, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.GhostsDoTasks, false);
                                    break;
                            }
                            break;
                        case "hidingtime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.EscapeTime, float.Parse(subArgs));
                            break;
                        case "finalhidetime":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.FinalEscapeTime, float.Parse(subArgs));
                            break;
                        case "maxventuses":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.CrewmateVentUses, int.Parse(subArgs));
                            break;
                        case "maxtimeinvent":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.CrewmateTimeInVent, float.Parse(subArgs));
                            break;
                        case "flashlightmode":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.UseFlashlight, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.UseFlashlight, false);
                                    break;
                            }
                            break;
                        case "crewmateflashlightsize":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.CrewmateFlashlightSize, float.Parse(subArgs));
                            break;
                        case "impostorflashlightsize":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.ImpostorFlashlightSize, float.Parse(subArgs));
                            break;
                        case "finalhideimpostorspeed":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.SeekerFinalSpeed, float.Parse(subArgs));
                            break;
                        case "finalhideseekmap":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.SeekerFinalMap, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.SeekerFinalMap, false);
                                    break;
                            }
                            break;
                        case "finalhidepings":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.SeekerPings, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.SeekerPings, false);
                                    break;
                            }
                            break;
                        case "pinginterval":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.MaxPingTime, float.Parse(subArgs));
                            break;
                        case "shownames":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "on":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ShowCrewmateNames, true);
                                    break;
                                case "off":
                                    GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.ShowCrewmateNames, false);
                                    break;
                            }
                            break;
                        case "impostor":
                            subArgs = args.Length < 3 ? "" : args[2];
                            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.ImpostorPlayerID, int.Parse(subArgs));
                            break;
                        case "preset":
                            subArgs = args.Length < 3 ? "" : args[2];
                            subArgs = subArgs.ToLower();
                            switch (subArgs)
                            {
                                case "coresettings":
                                case "pitchdark":
                                    GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.RulePreset, (int)RulesPresets.Standard);
                                    break;
                                case "rolesgalore":
                                    GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.RulePreset, (int)RulesPresets.StandardRoles);
                                    break;
                                case "flashlights":
                                    GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.RulePreset, (int)RulesPresets.Flashlight);
                                    break;
                                case "custom":
                                    subArgs = args.Length < 4 ? "" : args[3];
                                    if (subArgs == "")
                                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.RulePreset, (int)RulesPresets.Custom);
                                    else
                                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.RulePreset, int.Parse(subArgs));
                                    break;
                            }
                            break;
                        default:
                            PlayerControl.LocalPlayer.RpcSendMessage("Invalid setting. Please provide existing setting.\nUsage: /cs killcooldown 0,001.", "Warning");
                            break;
                    }
                    GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		            GameManager.Instance.LogicOptions.SyncOptions();
                    break;
                case "/gm":
                case "/gamemode":
                    canceled = true;
                    if (Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /gamemode during game.", "Warning");
                        break;
                    }
                    var gamemode = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        gamemode += subArgs;
                    }
                    gamemode = gamemode.ToLower().Replace(" ", "");
                    switch (gamemode)
                    {
                        case "classic":
                            Options.Gamemode.SetValue(0);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is classic", "ModesChanger");
                            break;
                        case "hideandseek":
                            Options.Gamemode.SetValue(1);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is hide and seek", "ModesChanger");
                            break;
                        case "shiftandseek":
                            Options.Gamemode.SetValue(2);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is shift and seek", "ModesChanger");
                            break;
                        case "bombtag":
                            Options.Gamemode.SetValue(3);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is bomb tag", "ModesChanger");
                            break;
                        case "randomitems":
                            Options.Gamemode.SetValue(4);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is random items", "ModesChanger");
                            break;
                        case "battleroyale":
                            Options.Gamemode.SetValue(5);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is battle royale", "ModesChanger");
                            break;
                        case "speedrun":
                            Options.Gamemode.SetValue(6);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is speedrun", "ModesChanger");
                            break;
                        case "paintbattle":
                            Options.Gamemode.SetValue(7);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is paint battle", "ModesChanger");
                            break;
                        case "killordie":
                            Options.Gamemode.SetValue(8);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is kill or die", "ModesChanger");
                            break;
                        case "zombies":
                            Options.Gamemode.SetValue(9);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is zombies", "ModesChanger");
                            break;
                        case "jailbreak":
                            Options.Gamemode.SetValue(10);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is jailbreak", "ModesChanger");
                            break;
                        case "deathrun":
                            Options.Gamemode.SetValue(11);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is deathrun", "ModesChanger");
                            break;
                        case "basewars":
                            Options.Gamemode.SetValue(12);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is base wars", "ModesChanger");
                            break;
                        case "freezetag":
                            Options.Gamemode.SetValue(13);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is freeze tag", "ModesChanger");
                            break;
                        case "colorwars":
                            Options.Gamemode.SetValue(14);
                            PlayerControl.LocalPlayer.RpcSendMessage("Now gamemode is color wars", "ModesChanger");
                            break;
                        default:
                            PlayerControl.LocalPlayer.RpcSendMessage("Invalid gamemode. Please provide existing gamemode.\nUsage: /gamemode hideandseek", "Warning");
                            break;
                    }
                    GameManager.Instance.RpcSyncCustomOptions();
                    break;       
                case "/color":
                case "/colour":
                    canceled = true;
                    if (Main.GameStarted && CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /color during game.", "Warning");
                        break;
                    }
                    var color = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        color += subArgs;
                    }
                    color = color.ToLower().Replace(" ", "");
                    switch (color)
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
                            PlayerControl.LocalPlayer.RpcSetColor(byte.Parse(color));  
                            break;
                    }        
                    break;
                case "/name":
                    canceled = true;
                    if (Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /name during game.", "Warning");
                        break;
                    }
                    var name = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        name += subArgs;
                    }
                    name = name[1..];
                    PlayerControl.LocalPlayer.RpcSetName(name);
                    break;
                case "/h":
                case "/help":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    subArgs = subArgs.ToLower();
                    switch (subArgs)
                    {
                        case "gm":
                        case "gamemode":
                            var gamemode2 = "";
                            for (int i = 2; i <= args.Length; ++i)
                            {
                                subArgs = args.Length < i + 1 ? "" : " " + args[i];
                                gamemode2 += subArgs;
                            }
                            gamemode2 = gamemode2.ToLower().Replace(" ", "");
                            switch (gamemode2)
                            {
                                case "classic":
                                    Utils.SendChat("Classic: Standard among us game with extra roles. During meeting you can type /m to see your role description. Type /r to see role list and /r ROLE to see what specific role does.", "Gamemodes");
                                    break;
                                case "hideandseek":
                                    Utils.SendChat("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                    break;
                                case "shiftandseek":
                                    Utils.SendChat("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes");
                                    break;
                                case "bombtag":
                                    Utils.SendChat("Bomb Tag: Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports, meetings, sabotages or venting. Click kill button to give bomb away. Depending on options players with bomb see arrow to nearest non bombed. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "randomitems":
                                    Utils.SendChat("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick. During meeting you can type /h i to see your current item description and /h i ITEM to see what specific item does.", "Gamemodes");
                                    break;
                                case "battleroyale":
                                    Utils.SendChat("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "speedrun":
                                    Utils.SendChat("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                    break;
                                case "paintbattle":
                                    Utils.SendChat("Paint Battle: Type /color COLOR command to change paint color. Click shift button to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                    break;
                                case "killordie":
                                    Utils.SendChat("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Depending on options killer gets arrow to nearest survivor. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "zombies":
                                    Utils.SendChat("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Depending on options you see arrow pointing to zombie(s). Impostors and zombies can vent if option is turned on.", "Gamemodes");
                                    break;
                                case "jailbreak":
                                    Utils.SendChat("Jailbreak: There are prisoners and guards. Prisoner win, if he escape. Guards win together, if less than half of prisoners escape. Prisoners can only vent, if they have screwdriver, but guards can vent anytime. As prisoner you can use pet button to switch current recipe. Use shift button to craft item in current recipe or destroy wall in reactor. You use resources to craft. Also use kill button to attack someone. If you beat up prisoner, you steal all his items. If you beat up guard, you get 100 resources and steal his weapon. When your health go down to 0, you get eliminated and respawn after some time. Guards can only attack wanted prisoners. Prisoner will become wanted, when he do something illegal near guard. Guards can use kill button on not wanted players to check them.", "Gamemodes");
                                    Utils.SendChat("If that player has illegal (red) item, he becomes wanted. When player is beaten up, he is no longer wanted. As guard you can buy things with money like prisoners craft. You can also repair wall in reactor by using shift button. If guard beat up prisoner, then prisoner lose all illegal items. Depending on options prisoners can help other after escaping.\nItems:\nResources - prisoners use it to craft items.\nScrewdriver (illegal) - gives prisoner ability to vent\nWeapon (illegal) - has 10 levels. Increase damage depending on level.\nSpaceship part (illegal) - used to craft spaceship.\nSpaceship (illegal) - used to escape.\nBreathing mask - used to escape.\nPickaxe (illegal) - has 10 levels and gives you ability to destroy wall in reactor. Destroying speed depends on level.", "Gamemodes");
                                    Utils.SendChat("Guard outfit (illegal) - gives you ability to disguise into guard. While disguised as guard, guards can use kill button on you to check uf you're fake guard.\nMoney - used to buy items by guards.\nEnergy drink - increase your speed temporarily.\n\nIllegal actions:\nAttacking\nVenting\nBeing in forbidden room\nDisguising as guard\nHaving illegal itemAll prisoners are orange and all guards are blue.\nDo /help jailbreak to see how to play in actual map.", "Gamemodes");
                                    break;
                                case "deathrun":
                                    Utils.SendChat("Deathrun: Deathrun is normal among us, but there are no cooldowns. There is no kill cooldown and ability cooldown for roles. There is only cooldown at the start of every round. Crewmates have only 1 short tasks (it can't be download data). Depending on options impostors can vent. Meetings can be disabled by host in options.", "Gamemodes");
                                    break;
                                case "basewars":
                                    Utils.SendChat("Base Wars: Players are divided into two teams, Red and Blue, with the objective of destroying the opposing team's base while defending their own. Each team has two turrets - Red's in Upper and Lower Engine, and Blue's in Shields and Weapons - that can be attacked by players using the shift button. Depending on the options set by the host, turrets can also slow down enemy players, adding an extra layer of defense. These turrets automatically defend the base and do not require a teammate to be present to activate. Players attack enemy players using the kill button and can earn experience points (EXP) by eliminating opponents and controlling key areas - Storage and Cafeteria. Gaining EXP allows players to level up, enhancing their abilities.", "Gamemodes");
                                    Utils.SendChat("Health can be regenerated quickly at the team's base, and depending on options players can also teleport back to their base when needed. The game is won when one team successfully destroys the opposing team's base, securing victory for their side.", "Gamemodes");
                                    break;
                                case "freezetag":
                                    Utils.SendChat("Freeze Tag: Crewmates are green, impostors are red and frozen crewmates are cyan. Impostors can use kill button to freeze crewmates. When all crewmates are frozen, impostors win. Crewmates can unfreeze others by standing near them. Crewmates win by completing all tasks. Reporting, sabotages and meetings are disabled. When crewmate is frozen his tasks will slowly complete automatically. Frozen crewmates can't move, but can see and do task, if there is nearby. Most roles work like in classic, but noisemaker sends alert when frozen.", "Gamemodes");
                                    break;
                                case "colorwars":
                                    Utils.SendChat("Color Wars: At start there are few leaders with their own color, everyone else is gray. Gray people are slow and have low vision. Leaders can use kill button on gray person to recruit this player to their team. Use kill button to enemy to kill this player. Leaders have multiple lives and other players have only 1. The goal is to protect leader and attack enemies. If leader dies, entire team die and lose. Player can respawn after few seconds, if his leader is alive. Depending on options players see arrow to their leader and nearest enemy leader. Last remaining team wins!", "Gamemodes");
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
                                case "disablezipline":
                                    Utils.SendChat("Disable Zipline: Players can't use zipline on the fungle.", "Gamemodes");
                                    break;
                                default:
                                    switch (Options.CurrentGamemode)
                                    {
                                        case Gamemodes.Classic:
                                            Utils.SendChat("Classic: Standard among us game with extra roles. During meeting you can type /m to see your role description. Type /r to see role list and /r ROLE to see what specific role does.", "Gamemodes");
                                            break;
                                        case Gamemodes.HideAndSeek:
                                            Utils.SendChat("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                            break;
                                        case Gamemodes.ShiftAndSeek:
                                            Utils.SendChat("Shift And Seek: Everyone is engineer or shapeshifter.Impostors are visible to other players.Crewmates wins by finishing their tasks, impostors by killing every single crewmate.Impostor must shapeshift into person he want kill.", "Gamemodes");
                                            break;
                                        case Gamemodes.BombTag:
                                            Utils.SendChat("Bomb Tag: Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports, meetings, sabotages or venting. Click kill button to give bomb away. Depending on options players with bomb see arrow to nearest non bombed. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.RandomItems:
                                            Utils.SendChat("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick. During meeting you can type /h i to see your current item description and /h i ITEM to see what specific item does.", "Gamemodes");
                                            break;
                                        case Gamemodes.BattleRoyale:
                                            Utils.SendChat("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Speedrun:
                                            Utils.SendChat("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                            break;
                                        case Gamemodes.PaintBattle:
                                            Utils.SendChat("Paint Battle: Type /color COLOR command to change paint color. Click shift button to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                            break;
                                        case Gamemodes.KillOrDie:
                                            Utils.SendChat("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Depending on options killer gets arrow to nearest survivor. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Zombies:
                                            Utils.SendChat("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Depending on options you see arrow pointing to zombie(s). Impostors and zombies can vent if option is turned on.", "Gamemodes");
                                            break;
                                        case Gamemodes.Jailbreak:
                                            Utils.SendChat("Jailbreak: There are prisoners and guards. Prisoner win, if he escape. Guards win together, if less than half of prisoners escape. Prisoners can only vent, if they have screwdriver, but guards can vent anytime. As prisoner you can use pet button to switch current recipe. Use shift button to craft item in current recipe or destroy wall in reactor. You use resources to craft. Also use kill button to attack someone. If you beat up prisoner, you steal all his items. If you beat up guard, you get 100 resources and steal his weapon. When your health go down to 0, you get eliminated and respawn after some time. Guards can only attack wanted prisoners. Prisoner will become wanted, when he do something illegal near guard. Guards can use kill button on not wanted players to check them.", "Gamemodes");
                                            Utils.SendChat("If that player has illegal (red) item, he becomes wanted. When player is beaten up, he is no longer wanted. As guard you can buy things with money like prisoners craft. You can also repair wall in reactor by using shift button. If guard beat up prisoner, then prisoner lose all illegal items. Depending on options prisoners can help other after escaping.\nItems:\nResources - prisoners use it to craft items.\nScrewdriver (illegal) - gives prisoner ability to vent\nWeapon (illegal) - has 10 levels. Increase damage depending on level.\nSpaceship part (illegal) - used to craft spaceship.\nSpaceship (illegal) - used to escape.\nBreathing mask - used to escape.\nPickaxe (illegal) - has 10 levels and gives you ability to destroy wall in reactor. Destroying speed depends on level.", "Gamemodes");
                                            Utils.SendChat("Guard outfit (illegal) - gives you ability to disguise into guard. While disguised as guard, guards can use kill button on you to check uf you're fake guard.\nMoney - used to buy items by guards.\nEnergy drink - increase your speed temporarily.\n\nIllegal actions:\nAttacking\nVenting\nBeing in forbidden room\nDisguising as guard\nHaving illegal itemAll prisoners are orange and all guards are blue.\nDo /help jailbreak to see how to play in actual map.", "Gamemodes");
                                            break;
                                        case Gamemodes.Deathrun:
                                            Utils.SendChat("Deathrun: Deathrun is normal among us, but there are no cooldowns. There is no kill cooldown and ability cooldown for roles. There is only cooldown at the start of every round. Crewmates have only 1 short tasks (it can't be download data). Depending on options impostors can vent. Meetings can be disabled by host in options.", "Gamemodes");
                                            break;
                                        case Gamemodes.BaseWars:
                                            Utils.SendChat("Base Wars: Players are divided into two teams, Red and Blue, with the objective of destroying the opposing team's base while defending their own. Each team has two turrets - Red's in Upper and Lower Engine, and Blue's in Shields and Weapons - that can be attacked by players using the shift button. Depending on the options set by the host, turrets can also slow down enemy players, adding an extra layer of defense. These turrets automatically defend the base and do not require a teammate to be present to activate. Players attack enemy players using the kill button and can earn experience points (EXP) by eliminating opponents and controlling key areas - Storage and Cafeteria. Gaining EXP allows players to level up, enhancing their abilities.", "Gamemodes");
                                            Utils.SendChat("Health can be regenerated quickly at the team's base, and depending on options players can also teleport back to their base when needed. The game is won when one team successfully destroys the opposing team's base, securing victory for their side.", "Gamemodes");
                                            break;
                                        case Gamemodes.FreezeTag:
                                            Utils.SendChat("Freeze Tag: Crewmates are green, impostors are red and frozen crewmates are cyan. Impostors can use kill button to freeze crewmates. When all crewmates are frozen, impostors win. Crewmates can unfreeze others by standing near them. Crewmates win by completing all tasks. Reporting, sabotages and meetings are disabled. When crewmate is frozen his tasks will slowly complete automatically. Frozen crewmates can't move, but can see and do task, if there is nearby. Most roles work like in classic, but noisemaker sends alert when frozen.", "Gamemodes");
                                            break;
                                        case Gamemodes.ColorWars:
                                            Utils.SendChat("Color Wars: At start there are few leaders with their own color, everyone else is gray. Gray people are slow and have low vision. Leaders can use kill button on gray person to recruit this player to their team. Use kill button to enemy to kill this player. Leaders have multiple lives and other players have only 1. The goal is to protect leader and attack enemies. If leader dies, entire team die and lose. Player can respawn after few seconds, if his leader is alive. Depending on options players see arrow to their leader and nearest enemy leader. Last remaining team wins!", "Gamemodes");
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "item":
                        case "i":
                            var item = "";
                            for (int i = 2; i <= args.Length; ++i)
                            {
                                subArgs = args.Length < i + 1 ? "" : " " + args[i];
                                item += subArgs;
                            }
                            item = item.ToLower().Replace(" ", "");
                            switch (item)
                            {
                                case "timeslower":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.TimeSlower), "Items");
                                    break;
                                case "knowledge":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Knowledge), "Items");
                                    break;
                                case "gun":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Gun), "Items");
                                    break;
                                case "shield":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Shield), "Items");
                                    break;
                                case "illusion":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Illusion), "Items");
                                    break;
                                case "radar":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Radar), "Items");
                                    break;
                                case "swap":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Swap), "Items");
                                    break;
                                case "medicine":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Medicine), "Items");
                                    break;
                                case "timespeeder":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.TimeSpeeder), "Items");
                                    break;
                                case "flash":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Flash), "Items");
                                    break;
                                case "hack":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Hack), "Items");
                                    break;
                                case "camouflage":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Camouflage), "Items");
                                    break;
                                case "multiteleport":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.MultiTeleport), "Items");
                                    break;
                                case "bomb":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Bomb), "Items");
                                    break;
                                case "trap":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Trap), "Items");
                                    break;
                                case "teamchanger":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.TeamChanger), "Items");
                                    break;
                                case "teleport":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Teleport), "Items");
                                    break;
                                case "button":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Button), "Items");
                                    break;
                                case "finder":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Finder), "Items");
                                    break;
                                case "rope":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Rope), "Items");
                                    break;
                                case "stop":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Stop), "Items");
                                    break;
                                case "newsletter":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Newsletter), "Items");
                                    break;
                                case "compass":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Compass), "Items");
                                    break;
                                case "booster":
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Booster), "Items");
                                    break;
                                default:
                                    if (RandomItemsGamemode.instance == null) break;
                                    if (RandomItemsGamemode.instance.GetItem(PlayerControl.LocalPlayer) == Items.None)
                                    {
                                        PlayerControl.LocalPlayer.RpcSendMessage("You don't have any item. Do your task or kill someone to get item!", "Items");
                                        break;
                                    }
                                    PlayerControl.LocalPlayer.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(RandomItemsGamemode.instance.GetItem(PlayerControl.LocalPlayer)), "Items");
                                    break;
                            }
                            break;
                        case "jailbreak":
                        case "j":
                            var map = "";
                            for (int i = 2; i <= args.Length; ++i)
                            {
                                subArgs = args.Length < i + 1 ? "" : " " + args[i];
                                map += subArgs;
                            }
                            map = map.ToLower().Replace(" ", "");
                            switch (map)
                            {
                                case "theskeld":
                                case "dlekseht":
                                    Utils.SendChat("In the skeld there are 5 forbidden areas, where is illegal for prisoners to be in. These are:\nReactor\nSecurity\nAdmin\nStorage\nNavigation\n\nYour health is regenerating faster in medbay, while not fighting. You can fill spaceship with fuel in lower or upper engine. You can fill your breathing mask with oxygen in o2. In electrical prisoners get 2 resources per second. In storage prisoners get 5 resources per secons. Guards always get 2 dollars per second. There are 3 ways to escape:\n1. Craft spaceship parts with resources. Then craft spaceship with them. Then fill it with fuel, go to storage and you escaped!\n2. Craft breathing mask and pickaxe. Fill your breathing mask with oxygen. Then go to reactor and destroy a wall with your pickaxe. If you do it, you're free!", "Jailbreak");
                                    Utils.SendChat("3. Prison takeover - prisoners have to work together to beat up every guard fast. Then go to navigation to change ship direction. Changing direction only works when all guards are beaten up. If guard come to navigation, the entire progress of changing direction resets. If you success, every prisoner win!", "Jailbreak");
                                    break;
                                case "mirahq":
                                case "polus":
                                case "theairship":
                                case "thefungle":
                                    Utils.SendChat("Jailbreak doesn't work in this map for now. Compatibility will be added in next updates.", "Jailbreak");
                                    break;
                                default:
                                    switch (GameOptionsManager.Instance.CurrentGameOptions.MapId)
                                    {
                                        case 0:
                                        case 3:
                                            Utils.SendChat("In the skeld there are 5 forbidden areas, where is illegal for prisoners to be in. These are:\nReactor\nSecurity\nAdmin\nStorage\nNavigation\n\nYour health is regenerating faster in medbay, while not fighting. You can fill spaceship with fuel in lower or upper engine. You can fill your breathing mask with oxygen in o2. In electrical prisoners get 2 resources per second. In storage prisoners get 5 resources per secons. Guards always get 2 dollars per second. There are 3 ways to escape:\n1. Craft spaceship parts with resources. Then craft spaceship with them. Then fill it with fuel, go to storage and you escaped!\n2. Craft breathing mask and pickaxe. Fill your breathing mask with oxygen. Then go to reactor and destroy a wall with your pickaxe. If you do it, you're free!\n3. Prison takeover - prisoners have to work together to beat up every guard fast. Then go to navigation to change ship direction. Changing direction only works when all guards are beaten up. If guard come to navigation, the entire progress of changing direction resets. If you success, every prisoner win!", "Jailbreak");
                                            break;
                                        default:
                                            Utils.SendChat("Jailbreak doesn't work in this map for now. Compatibility will be added in next updates.", "Jailbreak");
                                            break;
                                    }
                                    break;
                            }
                            break;
                        default:
                            PlayerControl.LocalPlayer.RpcSendMessage("Invalid use of command.\nUsage: /h gm, /h i, /h j", "Warning");
                            break;
                    }
                    break;
                case "/stop":
                    canceled = true;
                    if (!Main.GameStarted || CustomGamemode.Instance.Gamemode != Gamemodes.RandomItems || PlayerControl.LocalPlayer.Data.IsDead || !MeetingHud.Instance || !(MeetingHud.Instance.state is MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Voted))
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /stop now.", "Warning");
                        break;
                    }
                    if (RandomItemsGamemode.instance.GetItem(PlayerControl.LocalPlayer) == Items.Stop)
                    {
                        MeetingHud.Instance.RpcVotingComplete(new MeetingHud.VoterState[0], null, false);    
                        RandomItemsGamemode.instance.SendRPC(PlayerControl.LocalPlayer, Items.None);
                        Utils.SendSpam(true);
                    }
                    break;
                case "/n":
                case "/now":
                    canceled = true;
                    Utils.SendGameOptionsMessage();
                    break;
                case "/id":
                    canceled = true;
                    var type = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        type += subArgs;
                    }
                    type = type.ToLower().Replace(" ", "");
                    switch (type)
                    {
                        case "players":
                            var player_ids = "";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (AntiCheat.BannedPlayers.Contains(pc.NetId)) continue;
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
                        default:
                            PlayerControl.LocalPlayer.RpcSendMessage("Invalid use of command.\nUsage: /id players, /id colors", "Warning");
                            break;
                    }
                    break;
                case "/commands":
                case "/cm":
                    canceled = true;
                    PlayerControl.LocalPlayer.RpcSendMessage("Commands:\n/color COLOR - changes your color\n/name NAME - changes your name\n/h gm - show gamemode description\n" +
                        "/n - show active settings\n/id (players, colors) - show ids\n/h i - show item description\n/cm - show list of commands\n/cs SETTING VALUE - changes setting value\n" +
                        "/gm GAMEMODE - changes gamemode\n/kick PLAYER_ID - kick player\n/ban PLAYER_ID - ban player\n/ac MESSAGE - send message\n/l - show last game result\n" +
                        "/tpout - teleports you outside lobby ship\n/tpin - teleports you into lobby ship\n/tagcolor - changes color of your tag (not host tag)\n/hostcolor - changes color of host tag\n" +
                        "/m - shows your role description\n/guess PLAYER_ID ROLE - you guess player as nice/evil guesser\n/r - shows roles percentage and amount\n/r ROLE - shows role description\n" +
                        "/kc - shows alive killers amount in classic", "Command");
                    break;
                case "/kick":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    if (Utils.GetPlayerById(byte.Parse(subArgs)).GetClientId() == AmongUsClient.Instance.ClientId)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't kick yourself!", "Warning");
                        break;
                    }
                    AmongUsClient.Instance.KickPlayer(Utils.GetPlayerById(byte.Parse(subArgs)).GetClientId(), false);
                    break;
                case "/ban":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    if (Utils.GetPlayerById(byte.Parse(subArgs)).GetClientId() == AmongUsClient.Instance.ClientId)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't ban yourself!", "Warning");
                        break;
                    }
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
                    canceled = true;
                    if (!Main.GameStarted || CustomGamemode.Instance.Gamemode != Gamemodes.RandomItems || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /info now.", "Warning");
                        break;
                    }
                    if (RandomItemsGamemode.instance.GetItem(PlayerControl.LocalPlayer) == Items.Newsletter)
                    {
                        int crewmates = 0;
                        int scientists = 0 ;
                        int engineers = 0;       
                        int impostors = 0;
                        int shapeshifters = 0;
                        int noisemakers = 0;
                        int phantoms = 0;
                        int trackers = 0;
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
                            if (pc.Data.IsDead) continue;
                            switch (pc.Data.Role.Role)
                            {
                                case RoleTypes.Crewmate: ++crewmates; break;
                                case RoleTypes.Scientist: ++scientists; break;
                                case RoleTypes.Engineer: ++engineers; break;
                                case RoleTypes.Impostor: ++impostors; break;
                                case RoleTypes.Shapeshifter: ++shapeshifters; break;
                                case RoleTypes.Noisemaker: ++noisemakers; break;
                                case RoleTypes.Phantom: ++phantoms; break;
                                case RoleTypes.Tracker: ++trackers; break;
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
                        msg += noisemakers + " noisemakers\n";
                        msg += trackers + " trackers\n";
                        msg += impostors + " impostors\n";
                        msg += shapeshifters + " shapeshifters\n";
                        msg += phantoms + " phantoms\n\n";
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
                        RandomItemsGamemode.instance.SendRPC(PlayerControl.LocalPlayer, Items.None);
                    }
                    break;
                case "/tpout":
                    canceled = true;
                    if (Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /tpout during game.", "Warning");
                        break;
                    }
                    PlayerControl.LocalPlayer.RpcTeleport(new Vector2(0.1f, 3.8f));
                    break;
                case "/tpin":
                    canceled = true;
                    if (Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /tpin during game.", "Warning");
                        break;
                    }
                    PlayerControl.LocalPlayer.RpcTeleport(new Vector2(-0.2f, 1.3f));
                    break;
                case "/tagcolor":
                    canceled = true;
                    if (args.Length < 2 || !Utils.IsValidHexCode(args[1]))
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("Invalid color code. Please provide a valid hex color code.\nUsage: /tagcolor FF0000", "Warning");
                        break;
                    }
                    var hexColor = args[1];
                    string playerName = Main.StandardNames[PlayerControl.LocalPlayer.PlayerId];
                    string friendCode = PlayerControl.LocalPlayer.Data.FriendCode;
                    PlayerTagManager.UpdateNameAndTag(playerName, friendCode, hexColor, false);
                    break;
                case "/hostcolor":
                    canceled = true;
                    if (args.Length < 2 || !Utils.IsValidHexCode(args[1]))
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("Invalid color code. Please provide a valid hex color code.\nUsage: /hostcolor FF0000", "Warning");
                        break;
                    }
                    var hexColor2 = args[1];
                    string playerName2 = Main.StandardNames[PlayerControl.LocalPlayer.PlayerId];
                    string friendCode2 = PlayerControl.LocalPlayer.Data.FriendCode;
                    PlayerTagManager.UpdateNameAndTag(playerName2, friendCode2, hexColor2, true);
                    break;
                case "/myrole":
                case "/m":
                    canceled = true;
                    if (!Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /myrole in lobby.", "Warning");
                        break;
                    }
                    if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can only use /myrole in classic gamemode.", "Warning");
                        break;
                    }
                    PlayerControl.LocalPlayer.RpcSendMessage(PlayerControl.LocalPlayer.GetRole().RoleDescriptionLong + "\n\n<u>Role options:</u>\n" + Utils.GetRoleOptions(PlayerControl.LocalPlayer.GetRole().Role), "RoleInfo");
                    foreach (var addOn in PlayerControl.LocalPlayer.GetAddOns())
                        PlayerControl.LocalPlayer.RpcSendMessage(addOn.AddOnDescriptionLong + "\n\n<u>Add on options:</u>\n" + Utils.GetAddOnOptions(addOn.Type), "RoleInfo");
                    break;
                case "/guess":
                case "/shoot":
                case "/bet":
                case "/bt":
                case "/gs":
                    canceled = true;
                    if (!Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /guess in lobby.", "Warning");
                        break;
                    }
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can only use /guess when you're alive.", "Warning");
                        break;
                    }
                    if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can only use /guess in classic gamemode.", "Warning");
                        break;
                    }
                    subArgs = args.Length < 2 ? "" : args[1];
                    byte playerId = byte.Parse(subArgs);
                    var role = "";
                    for (int i = 2; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        role += subArgs;
                    }
                    role = role.ToLower().Replace(" ", "");
                    if (!CustomRolesHelper.CommandRoleNames.ContainsKey(role) && !AddOnsHelper.CommandAddOnNames.ContainsKey(role))
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("This role doesn't exist!", "Warning");
                        break;
                    }
                    var target = Utils.GetPlayerById(playerId);
                    if (target == null)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("This player doesn't exist!", "Warning");
                        break;
                    }
                    if (target.Data.IsDead)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("This player is already dead!", "Warning");
                        break;
                    }
                    if (CustomRolesHelper.CommandRoleNames.ContainsKey(role))
                    {
                        if (!PlayerControl.LocalPlayer.GetRole().CanGuess(target, CustomRolesHelper.CommandRoleNames[role]) ||
                            !target.GetRole().CanGetGuessed(PlayerControl.LocalPlayer, CustomRolesHelper.CommandRoleNames[role]) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.Immortal && !Immortal.CanBeGuessed.GetBool()) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.SecurityGuard && !SecurityGuard.CanBeGuessed.GetBool()) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.Mortician && !Mortician.CanBeGuessed.GetBool()) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.Mayor && !Mayor.CanBeGuessed.GetBool()))
                        {
                            PlayerControl.LocalPlayer.RpcSendMessage("You can't guess this player!", "Warning");
                            break;
                        }
                        if (target.GetRole().Role == CustomRolesHelper.CommandRoleNames[role])
                        {
                            target.RpcSetDeathReason(DeathReasons.Guessed);
                            target.RpcExileV2();
                            target.RpcGuessPlayer();
                            ++Main.PlayerKills[PlayerControl.LocalPlayer.PlayerId];
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[playerId] + " was guessed", "Guesser"), 1f);
                        }
                        else
                        {
                            PlayerControl.LocalPlayer.RpcSetDeathReason(DeathReasons.Guessed);
                            PlayerControl.LocalPlayer.RpcExileV2();
                            PlayerControl.LocalPlayer.RpcGuessPlayer();
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] + " was guessed", "Guesser"), 1f);
                        }
                    }
                    else
                    {
                        if (!PlayerControl.LocalPlayer.GetRole().CanGuess(target, AddOnsHelper.CommandAddOnNames[role]) ||
                            (AddOnsHelper.CommandAddOnNames[role] == AddOns.Bait && !Bait.CanBeGuessed.GetBool()))
                        {
                            PlayerControl.LocalPlayer.RpcSendMessage("You can't guess this player!", "Warning");
                            break;
                        }
                        if (target.HasAddOn(AddOnsHelper.CommandAddOnNames[role]))
                        {
                            target.RpcSetDeathReason(DeathReasons.Guessed);
                            target.RpcExileV2();
                            target.RpcGuessPlayer();
                            ++Main.PlayerKills[PlayerControl.LocalPlayer.PlayerId];
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[playerId] + " was guessed", "Guesser"), 1f);
                        }
                        else
                        {
                            PlayerControl.LocalPlayer.RpcSetDeathReason(DeathReasons.Guessed);
                            PlayerControl.LocalPlayer.RpcExileV2();
                            PlayerControl.LocalPlayer.RpcGuessPlayer();
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[PlayerControl.LocalPlayer.PlayerId] + " was guessed", "Guesser"), 1f);
                        }
                    }
                    break;
                case "/roles":
                case "/role":
                case "/r":
                    canceled = true;
                    if (Options.CurrentGamemode != Gamemodes.Classic)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("Roles are only in classic gamemode", "Warning");
                        break;
                    }
                    var role2 = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        role2 += subArgs;
                    }
                    role = role2.ToLower().Replace(" ", "");
                    string crewmateRoles = "";
                    string impostorRoles = "";
                    string neutralRoles = "";
                    string addOns = "";
                    if (CustomRolesHelper.CommandRoleNames.ContainsKey(role))
                        PlayerControl.LocalPlayer.RpcSendMessage(CustomRolesHelper.RoleDescriptionsLong[CustomRolesHelper.CommandRoleNames[role]] + "\n\n<u>Role options:</u>\n" + Utils.GetRoleOptions(CustomRolesHelper.CommandRoleNames[role]), "RoleInfo");
                    else if (AddOnsHelper.CommandAddOnNames.ContainsKey(role))
                        PlayerControl.LocalPlayer.RpcSendMessage(AddOnsHelper.AddOnDescriptionsLong[AddOnsHelper.CommandAddOnNames[role]] + "\n\n<u>Add on options:</u>\n" + Utils.GetAddOnOptions(AddOnsHelper.CommandAddOnNames[role]), "RoleInfo");
                    else
                    {
                        foreach (var roleType in Enum.GetValues<CustomRoles>())
                        {
                            if (CustomRolesHelper.IsVanilla(roleType)) continue;
                            string chance = CustomRolesHelper.GetRoleChance(roleType) <= 100 ? CustomRolesHelper.GetRoleChance(roleType).ToString() + "%" : "Always";
                            string count = CustomRolesHelper.GetRoleCount(roleType).ToString();
                            if (CustomRolesHelper.IsCrewmate(roleType) && CustomRolesHelper.GetRoleChance(roleType) > 0)
                                crewmateRoles += Options.RolesChance[roleType].GetName() + ": " + chance + " x" + count + "\n";
                            if (CustomRolesHelper.IsImpostor(roleType) && CustomRolesHelper.GetRoleChance(roleType) > 0)
                                impostorRoles += Options.RolesChance[roleType].GetName() + ": " + chance + " x" + count + "\n";
                            if (CustomRolesHelper.IsNeutral(roleType) && CustomRolesHelper.GetRoleChance(roleType) > 0)
                                neutralRoles += Options.RolesChance[roleType].GetName() + ": " + chance + " x" + count + "\n";
                        }
                        if (crewmateRoles != "")
                            PlayerControl.LocalPlayer.RpcSendMessage(crewmateRoles, "Crewmates");
                        if (impostorRoles != "")
                            PlayerControl.LocalPlayer.RpcSendMessage(impostorRoles, "Impostors");
                        if (neutralRoles != "")
                            PlayerControl.LocalPlayer.RpcSendMessage(neutralRoles, "Neutrals");
                        foreach (var addOn in Enum.GetValues<AddOns>())
                        {
                            if (AddOnsHelper.GetAddOnChance(addOn) > 0)
                                addOns += Options.AddOnsChance[addOn].GetName() + ": " + AddOnsHelper.GetAddOnChance(addOn) + "% x" + AddOnsHelper.GetAddOnCount(addOn) + "\n";
                        }
                        PlayerControl.LocalPlayer.RpcSendMessage(addOns, "AddOns");
                    }
                    break;
                case "/kcount":
                case "/kc":
                    canceled = true;
                    if (!Main.GameStarted)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can't use /kcount in lobby", "Warning");
                        break;
                    }
                    if (Options.CurrentGamemode != Gamemodes.Classic)
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("You can use /kcount only in classic gamemode", "Warning");
                        break;
                    }
                    if (!Options.CanUseKcountCommand.GetBool())
                    {
                        PlayerControl.LocalPlayer.RpcSendMessage("Host disabled usage of this command.", "Warning");
                        break;
                    }
                    int impostorCount = 0;
                    int neutralKillerCount = 0;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data.IsDead || pc.Data.Disconnected) continue;
                        if (pc.GetRole().IsImpostor())
                            ++impostorCount;
                        if (pc.GetRole().IsNeutralKilling())
                            ++neutralKillerCount;
                    }
                    var text2 = impostorCount + Utils.ColorString(Palette.ImpostorRed, impostorCount == 1 ? " impostor" : " impostors") + "\n";
                    text2 += neutralKillerCount + Utils.ColorString(Color.gray, neutralKillerCount == 1 ? " neutral killer" : " neutral killers");
                    PlayerControl.LocalPlayer.RpcSendMessage(text2, "KillCount");
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
            if (Main.GameStarted && !MeetingHud.Instance && CustomGamemode.Instance.Gamemode == Gamemodes.Classic && args[0] == "/lc" && Romantic.CanChatWithLover.GetBool() && !player.Data.IsDead)
            {
                if (Utils.IsActive(SystemTypes.Comms) && Romantic.DisableChatDuringCommsSabotage.GetBool())
                {
                    Utils.SendSpam(true);
                    return false;
                }
                var message = "";
                for (int i = 1; i <= args.Length; ++i)
                {
                    subArgs = args.Length < i + 1 ? "" : " " + args[i];
                    message += subArgs;
                }
                if (player.GetRole().Role == CustomRoles.Romantic)
                {
                    Romantic romanticRole = player.GetRole() as Romantic;
                    if (romanticRole != null && romanticRole.LoverId != byte.MaxValue)
                    {
                        var lover = Utils.GetPlayerById(romanticRole.LoverId);
                        if (lover != null)
                            lover.Notify(Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Romantic], "[Lover] " + Main.StandardNames[player.PlayerId] + ": " + message));
                    }
                }
                else
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.GetRole().Role == CustomRoles.Romantic)
                        {
                            Romantic romanticRole = pc.GetRole() as Romantic;
                            if (romanticRole != null && romanticRole.LoverId == player.PlayerId)
                                pc.Notify(Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Romantic], "[Lover] " + Main.StandardNames[player.PlayerId] + ": " + message));
                        }
                    }
                }
                Utils.SendSpam(true);
                return false;
            }
            if (!Options.EnableMidGameChat.GetBool() && Main.GameStarted && !MeetingHud.Instance && CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle && !player.Data.IsDead)
            {
                Utils.SendSpam(true);
                return false;
            }
            if (Options.EnableMidGameChat.GetBool() && Main.GameStarted && !MeetingHud.Instance && CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle)
            {
                if (Utils.IsActive(SystemTypes.Comms) && Options.DisableDuringCommsSabotage.GetBool())
                {
                    if (!Options.ProximityChat.GetBool())
                        Utils.SendSpam(true);
                    return false;
                }
                if ((args[0] == "/radio" || args[0] == "/rd") && Options.ProximityChat.GetBool() && Options.ImpostorRadio.GetBool() && Main.StandardRoles[player.PlayerId].IsImpostor() && (CustomGamemode.Instance.Gamemode == Gamemodes.Classic || CustomGamemode.Instance.Gamemode == Gamemodes.HideAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.ShiftAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems || CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun  || CustomGamemode.Instance.Gamemode == Gamemodes.FreezeTag) && !player.Data.IsDead)
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
                            pc.Notify(Utils.ColorString(Palette.ImpostorRed, "[Radio] " + Main.StandardNames[player.PlayerId] + ": " + message));
                    }
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
                    if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems)
                    {
                        if (RandomItemsGamemode.instance.CamouflageTimer > -1f) 
                            appearance = "???";
                    }
                    player.SendProximityMessage(appearance, message);
                    return false;
                }
                return true;
            }
            switch (args[0])
            {
                case "/color":
                case "/colour":
                    canceled = true;
                    if (!Options.CanUseColorCommand.GetBool() && (!Main.GameStarted || CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle))
                    {
                        player.RpcSendMessage("Host disabled usage of this command.", "Warning");
                        break;
                    }
                    if (Main.GameStarted && CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle)
                    {
                        player.RpcSendMessage("You can't use /color during game.", "Warning");
                        break;
                    }
                    var color = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        color += subArgs;
                    }
                    color = color.ToLower().Replace(" ", "");
                    switch (color)
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
                            if (Options.EnableFortegreen.GetBool() || (Main.GameStarted && CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle))
                                player.RpcSetColor(18);
                            break;
                        default:
                            if (byte.Parse(color) < 18 || Options.EnableFortegreen.GetBool() || (Main.GameStarted && CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle))
                                player.RpcSetColor(byte.Parse(color));  
                            break;
                    }        
                    break;
                case "/name":
                    canceled = true;
                    if (!Options.CanUseNameCommand.GetBool())
                    {
                        player.RpcSendMessage("Host disabled usage of this command.", "Warning");
                        break;
                    }
                    if (Main.GameStarted)
                    {
                        player.RpcSendMessage("You can't use /name during game.", "Warning");
                        break;
                    }
                    var name = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        name += subArgs;
                    }
                    name = name[1..];
                    if (name.Length > Options.MaximumNameLength.GetInt()) break;
                    if (Options.EnableNameRepeating.GetBool())
                        player.RpcSetName(name);
                    else
                        player.CheckName(name);
                    break;
                case "/h":
                case "/help":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    subArgs = subArgs.ToLower();
                    switch (subArgs)
                    {
                        case "gm":
                        case "gamemode":
                            var gamemode = "";
                            for (int i = 2; i <= args.Length; ++i)
                            {
                                subArgs = args.Length < i + 1 ? "" : " " + args[i];
                                gamemode += subArgs;
                            }
                            gamemode = gamemode.ToLower().Replace(" ", "");
                            switch (gamemode)
                            {
                                case "classic":
                                    player.RpcSendMessage("Classic: Standard among us game with extra roles. During meeting you can type /m to see your role description. Type /r to see role list and /r ROLE to see what specific role does.", "Gamemodes");
                                    break;
                                case "hideandseek":
                                    player.RpcSendMessage("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                    break;
                                case "shiftandseek":
                                    player.RpcSendMessage("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes");
                                    break;
                                case "bombtag":
                                    player.RpcSendMessage("Bomb Tag: Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports, meetings, sabotages or venting. Click kill button to give bomb away. Depending on options players with bomb see arrow to nearest non bombed. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "randomitems":
                                    player.RpcSendMessage("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick. During meeting you can type /h i to see your current item description and /h i ITEM to see what specific item does.", "Gamemodes");
                                    break;
                                case "battleroyale":
                                    player.RpcSendMessage("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "speedrun":
                                    player.RpcSendMessage("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                    break;
                                case "paintbattle":
                                    player.RpcSendMessage("Paint Battle: Type /color COLOR command to change paint color. Click shift button to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                    break;
                                case "killordie":
                                    player.RpcSendMessage("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Depending on options killer gets arrow to nearest survivor. Last standing alive wins!", "Gamemodes");
                                    break;
                                case "zombies":
                                    player.RpcSendMessage("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Depending on options you see arrow pointing to zombie(s). Impostors and zombies can vent if option is turned on.", "Gamemodes");
                                    break;
                                case "jailbreak":
                                    player.RpcSendMessage("Jailbreak: There are prisoners and guards. Prisoner win, if he escape. Guards win together, if less than half of prisoners escape. Prisoners can only vent, if they have screwdriver, but guards can vent anytime. As prisoner you can use pet button to switch current recipe. Use shift button to craft item in current recipe or destroy wall in reactor. You use resources to craft. Also use kill button to attack someone. If you beat up prisoner, you steal all his items. If you beat up guard, you get 100 resources and steal his weapon. When your health go down to 0, you get eliminated and respawn after some time. Guards can only attack wanted prisoners. Prisoner will become wanted, when he do something illegal near guard. Guards can use kill button on not wanted players to check them.", "Gamemodes");
                                    player.RpcSendMessage("If that player has illegal (red) item, he becomes wanted. When player is beaten up, he is no longer wanted. As guard you can buy things with money like prisoners craft. You can also repair wall in reactor by using shift button. If guard beat up prisoner, then prisoner lose all illegal items. Depending on options prisoners can help other after escaping.\nItems:\nResources - prisoners use it to craft items.\nScrewdriver (illegal) - gives prisoner ability to vent\nWeapon (illegal) - has 10 levels. Increase damage depending on level.\nSpaceship part (illegal) - used to craft spaceship.\nSpaceship (illegal) - used to escape.\nBreathing mask - used to escape.\nPickaxe (illegal) - has 10 levels and gives you ability to destroy wall in reactor. Destroying speed depends on level.", "Gamemodes");
                                    player.RpcSendMessage("Guard outfit (illegal) - gives you ability to disguise into guard. While disguised as guard, guards can use kill button on you to check uf you're fake guard.\nMoney - used to buy items by guards.\nEnergy drink - increase your speed temporarily.\n\nIllegal actions:\nAttacking\nVenting\nBeing in forbidden room\nDisguising as guard\nHaving illegal itemAll prisoners are orange and all guards are blue.\nDo /help jailbreak to see how to play in actual map.", "Gamemodes");
                                    break;
                                case "deathrun":
                                    player.RpcSendMessage("Deathrun: Deathrun is normal among us, but there are no cooldowns. There is no kill cooldown and ability cooldown for roles. There is only cooldown at the start of every round. Crewmates have only 1 short tasks (it can't be download data). Depending on options impostors can vent. Meetings can be disabled by host in options.", "Gamemodes");
                                    break;
                                case "basewars":
                                    player.RpcSendMessage("Base Wars: Players are divided into two teams, Red and Blue, with the objective of destroying the opposing team's base while defending their own. Each team has two turrets - Red's in Upper and Lower Engine, and Blue's in Shields and Weapons - that can be attacked by players using the shift button. Depending on the options set by the host, turrets can also slow down enemy players, adding an extra layer of defense. These turrets automatically defend the base and do not require a teammate to be present to activate. Players attack enemy players using the kill button and can earn experience points (EXP) by eliminating opponents and controlling key areas - Storage and Cafeteria. Gaining EXP allows players to level up, enhancing their abilities.", "Gamemodes");
                                    player.RpcSendMessage("Health can be regenerated quickly at the team's base, and depending on options players can also teleport back to their base when needed. The game is won when one team successfully destroys the opposing team's base, securing victory for their side.", "Gamemodes");
                                    break;
                                case "freezetag":
                                    player.RpcSendMessage("Freeze Tag: Crewmates are green, impostors are red and frozen crewmates are cyan. Impostors can use kill button to freeze crewmates. When all crewmates are frozen, impostors win. Crewmates can unfreeze others by standing near them. Crewmates win by completing all tasks. Reporting, sabotages and meetings are disabled. When crewmate is frozen his tasks will slowly complete automatically. Frozen crewmates can't move, but can see and do task, if there is nearby. Most roles work like in classic, but noisemaker sends alert when frozen.", "Gamemodes");
                                    break;
                                case "colorwars":
                                    player.RpcSendMessage("Color Wars: At start there are few leaders with their own color, everyone else is gray. Gray people are slow and have low vision. Leaders can use kill button on gray person to recruit this player to their team. Use kill button to enemy to kill this player. Leaders have multiple lives and other players have only 1. The goal is to protect leader and attack enemies. If leader dies, entire team die and lose. Player can respawn after few seconds, if his leader is alive. Depending on options players see arrow to their leader and nearest enemy leader. Last remaining team wins!", "Gamemodes");
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
                                case "disablezipline":
                                    player.RpcSendMessage("Disable Zipline: Players can't use zipline on the fungle.", "Gamemodes");
                                    break;
                                default:
                                    switch (Options.CurrentGamemode)
                                    {
                                        case Gamemodes.Classic:
                                            player.RpcSendMessage("Classic: Standard among us game with extra roles. During meeting you can type /m to see your role description. Type /r to see role list and /r ROLE to see what specific role does.", "Gamemodes");
                                            break;
                                        case Gamemodes.HideAndSeek:
                                            player.RpcSendMessage("Hide And Seek: All crewmates are blue and impostors are red. Crewmates wins by finishing their tasks, impostors by killing every single crewmate.", "Gamemodes");
                                            break;
                                        case Gamemodes.ShiftAndSeek:
                                            player.RpcSendMessage("Shift And Seek: Everyone is engineer or shapeshifter. Impostors are visible to other players. Crewmates wins by finishing their tasks, impostors by killing every single crewmate. Impostor must shapeshift into person he want kill.", "Gamemodes");
                                            break;
                                        case Gamemodes.BombTag:
                                            player.RpcSendMessage("Bomb Tag: Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports, meetings, sabotages or venting. Click kill button to give bomb away. Depending on options players with bomb see arrow to nearest non bombed. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.RandomItems:
                                            player.RpcSendMessage("Random Items: When you do task or kill someone you get item. Use this item by petting your pet. Every item is single use. If you have item and get new, your old item will be removed! Item and description is under your nick. During meeting you can type /h i to see your current item description and /h i ITEM to see what specific item does.", "Gamemodes");
                                            break;
                                        case Gamemodes.BattleRoyale:
                                            player.RpcSendMessage("Battle Royale: Click kill button to attack. When player is attacked, he loses 1 live. If he still have lives left, he survives. If not, he die. Depending on options players have arrow to nearest player. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Speedrun:
                                            player.RpcSendMessage("Speedrun: Finish your tasks first to win! No impostors. meetings and sabotages. You can play alone - just do your tasks as fast as you can!", "Gamemodes");
                                            break;
                                        case Gamemodes.PaintBattle:
                                            player.RpcSendMessage("Paint Battle: Type /color COLOR command to change paint color. Click shift button to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.", "Gamemodes");
                                            break;
                                        case Gamemodes.KillOrDie:
                                            player.RpcSendMessage("Kill Or Die: Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Depending on options killer gets arrow to nearest survivor. Last standing alive wins!", "Gamemodes");
                                            break;
                                        case Gamemodes.Zombies:
                                            player.RpcSendMessage("Zombies: Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Depending on options you see arrow pointing to zombie(s). Impostors and zombies can vent if option is turned on.", "Gamemodes");
                                            break;
                                        case Gamemodes.Jailbreak:
                                            player.RpcSendMessage("Jailbreak: There are prisoners and guards. Prisoner win, if he escape. Guards win together, if less than half of prisoners escape. Prisoners can only vent, if they have screwdriver, but guards can vent anytime. As prisoner you can use pet button to switch current recipe. Use shift button to craft item in current recipe or destroy wall in reactor. You use resources to craft. Also use kill button to attack someone. If you beat up prisoner, you steal all his items. If you beat up guard, you get 100 resources and steal his weapon. When your health go down to 0, you get eliminated and respawn after some time. Guards can only attack wanted prisoners. Prisoner will become wanted, when he do something illegal near guard. Guards can use kill button on not wanted players to check them.", "Gamemodes");
                                            player.RpcSendMessage("If that player has illegal (red) item, he becomes wanted. When player is beaten up, he is no longer wanted. As guard you can buy things with money like prisoners craft. You can also repair wall in reactor by using shift button. If guard beat up prisoner, then prisoner lose all illegal items. Depending on options prisoners can help other after escaping.\nItems:\nResources - prisoners use it to craft items.\nScrewdriver (illegal) - gives prisoner ability to vent\nWeapon (illegal) - has 10 levels. Increase damage depending on level.\nSpaceship part (illegal) - used to craft spaceship.\nSpaceship (illegal) - used to escape.\nBreathing mask - used to escape.\nPickaxe (illegal) - has 10 levels and gives you ability to destroy wall in reactor. Destroying speed depends on level.", "Gamemodes");
                                            player.RpcSendMessage("Guard outfit (illegal) - gives you ability to disguise into guard. While disguised as guard, guards can use kill button on you to check uf you're fake guard.\nMoney - used to buy items by guards.\nEnergy drink - increase your speed temporarily.\n\nIllegal actions:\nAttacking\nVenting\nBeing in forbidden room\nDisguising as guard\nHaving illegal itemAll prisoners are orange and all guards are blue.\nDo /help jailbreak to see how to play in actual map.", "Gamemodes");
                                            break;
                                        case Gamemodes.Deathrun:
                                            player.RpcSendMessage("Deathrun: Deathrun is normal among us, but there are no cooldowns. There is no kill cooldown and ability cooldown for roles. There is only cooldown at the start of every round. Crewmates have only 1 short tasks (it can't be download data). Depending on options impostors can vent. Meetings can be disabled by host in options.", "Gamemodes");
                                            break;
                                        case Gamemodes.BaseWars:
                                            player.RpcSendMessage("Base Wars: Players are divided into two teams, Red and Blue, with the objective of destroying the opposing team's base while defending their own. Each team has two turrets - Red's in Upper and Lower Engine, and Blue's in Shields and Weapons - that can be attacked by players using the shift button. Depending on the options set by the host, turrets can also slow down enemy players, adding an extra layer of defense. These turrets automatically defend the base and do not require a teammate to be present to activate. Players attack enemy players using the kill button and can earn experience points (EXP) by eliminating opponents and controlling key areas - Storage and Cafeteria. Gaining EXP allows players to level up, enhancing their abilities.", "Gamemodes");
                                            player.RpcSendMessage("Health can be regenerated quickly at the team's base, and depending on options players can also teleport back to their base when needed. The game is won when one team successfully destroys the opposing team's base, securing victory for their side.", "Gamemodes");
                                            break;
                                        case Gamemodes.FreezeTag:
                                            player.RpcSendMessage("Freeze Tag: Crewmates are green, impostors are red and frozen crewmates are cyan. Impostors can use kill button to freeze crewmates. When all crewmates are frozen, impostors win. Crewmates can unfreeze others by standing near them. Crewmates win by completing all tasks. Reporting, sabotages and meetings are disabled. When crewmate is frozen his tasks will slowly complete automatically. Frozen crewmates can't move, but can see and do task, if there is nearby. Most roles work like in classic, but noisemaker sends alert when frozen.", "Gamemodes");
                                            break;
                                        case Gamemodes.ColorWars:
                                            player.RpcSendMessage("Color Wars: At start there are few leaders with their own color, everyone else is gray. Gray people are slow and have low vision. Leaders can use kill button on gray person to recruit this player to their team. Use kill button to enemy to kill this player. Leaders have multiple lives and other players have only 1. The goal is to protect leader and attack enemies. If leader dies, entire team die and lose. Player can respawn after few seconds, if his leader is alive. Depending on options players see arrow to their leader and nearest enemy leader. Last remaining team wins!", "Gamemodes");
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case "item":
                        case "i":
                            var item = "";
                            for (int i = 2; i <= args.Length; ++i)
                            {
                                subArgs = args.Length < i + 1 ? "" : " " + args[i];
                                item += subArgs;
                            }
                            item = item.ToLower().Replace(" ", "");
                            switch (item)
                            {
                                case "timeslower":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.TimeSlower), "Items");
                                    break;
                                case "knowledge":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Knowledge), "Items");
                                    break;
                                case "gun":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Gun), "Items");
                                    break;
                                case "shield":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Shield), "Items");
                                    break;
                                case "illusion":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Illusion), "Items");
                                    break;
                                case "radar":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Radar), "Items");
                                    break;
                                case "swap":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Swap), "Items");
                                    break;
                                case "medicine":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Medicine), "Items");
                                    break;
                                case "timespeeder":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.TimeSpeeder), "Items");
                                    break;
                                case "flash":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Flash), "Items");
                                    break;
                                case "hack":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Hack), "Items");
                                    break;
                                case "camouflage":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Camouflage), "Items");
                                    break;
                                case "multiteleport":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.MultiTeleport), "Items");
                                    break;
                                case "bomb":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Bomb), "Items");
                                    break;
                                case "trap":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Trap), "Items");
                                    break;
                                case "teamchanger":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.TeamChanger), "Items");
                                    break;
                                case "teleport":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Teleport), "Items");
                                    break;
                                case "button":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Button), "Items");
                                    break;
                                case "finder":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Finder), "Items");
                                    break;
                                case "rope":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Rope), "Items");
                                    break;
                                case "stop":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Stop), "Items");
                                    break;
                                case "newsletter":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Newsletter), "Items");
                                    break;
                                case "compass":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Compass), "Items");
                                    break;
                                case "booster":
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(Items.Booster), "Items");
                                    break;
                                default:
                                    if (RandomItemsGamemode.instance == null) break;
                                    if (RandomItemsGamemode.instance.GetItem(player) == Items.None)
                                    {
                                        player.RpcSendMessage("You don't have any item. Do your task or kill someone to get item!", "Items");
                                        break;
                                    }
                                    player.RpcSendMessage(RandomItemsGamemode.ItemDescriptionLong(RandomItemsGamemode.instance.GetItem(player)), "Items");
                                    break;
                            }
                            break;
                        case "jailbreak":
                        case "j":
                            var map = "";
                            for (int i = 2; i <= args.Length; ++i)
                            {
                                subArgs = args.Length < i + 1 ? "" : " " + args[i];
                                map += subArgs;
                            }
                            map = map.ToLower().Replace(" ", "");
                            switch (map)
                            {
                                case "theskeld":
                                case "dlekseht":
                                    player.RpcSendMessage("In the skeld there are 5 forbidden areas, where is illegal for prisoners to be in. These are:\nReactor\nSecurity\nAdmin\nStorage\nNavigation\n\nYour health is regenerating faster in medbay, while not fighting. You can fill spaceship with fuel in lower or upper engine. You can fill your breathing mask with oxygen in o2. In electrical prisoners get 2 resources per second. In storage prisoners get 5 resources per secons. Guards always get 2 dollars per second. There are 3 ways to escape:\n1. Craft spaceship parts with resources. Then craft spaceship with them. Then fill it with fuel, go to storage and you escaped!\n2. Craft breathing mask and pickaxe. Fill your breathing mask with oxygen. Then go to reactor and destroy a wall with your pickaxe. If you do it, you're free!", "Jailbreak");
                                    player.RpcSendMessage("3. Prison takeover - prisoners have to work together to beat up every guard fast. Then go to navigation to change ship direction. Changing direction only works when all guards are beaten up. If guard come to navigation, the entire progress of changing direction resets. If you success, every prisoner win!", "Jailbreak");
                                    break;
                                case "mirahq":
                                case "polus":
                                case "theairship":
                                case "thefungle":
                                    player.RpcSendMessage("Jailbreak doesn't work in this map for now. Compatibility will be added in next updates.", "Jailbreak");
                                    break;
                                default:
                                    switch (GameOptionsManager.Instance.CurrentGameOptions.MapId)
                                    {
                                        case 0:
                                        case 3:
                                            player.RpcSendMessage("In the skeld there are 5 forbidden areas, where is illegal for prisoners to be in. These are:\nReactor\nSecurity\nAdmin\nStorage\nNavigation\n\nYour health is regenerating faster in medbay, while not fighting. You can fill spaceship with fuel in lower or upper engine. You can fill your breathing mask with oxygen in o2. In electrical prisoners get 2 resources per second. In storage prisoners get 5 resources per secons. Guards always get 2 dollars per second. There are 3 ways to escape:\n1. Craft spaceship parts with resources. Then craft spaceship with them. Then fill it with fuel, go to storage and you escaped!\n2. Craft breathing mask and pickaxe. Fill your breathing mask with oxygen. Then go to reactor and destroy a wall with your pickaxe. If you do it, you're free!\n3. Prison takeover - prisoners have to work together to beat up every guard fast. Then go to navigation to change ship direction. Changing direction only works when all guards are beaten up. If guard come to navigation, the entire progress of changing direction resets. If you success, every prisoner win!", "Jailbreak");
                                            break;
                                        default:
                                            player.RpcSendMessage("Jailbreak doesn't work in this map for now. Compatibility will be added in next updates.", "Jailbreak");
                                            break;
                                    }
                                    break;
                            }
                            break;
                        default:
                            player.RpcSendMessage("Invalid use of command.\nUsage: /h gm, /h i, /h j", "Warning");
                            break;
                    }
                    break;
                case "/stop":
                    canceled = true;
                    if (!Main.GameStarted || CustomGamemode.Instance.Gamemode != Gamemodes.RandomItems || player.Data.IsDead || !MeetingHud.Instance || !(MeetingHud.Instance.state is MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Voted))
                    {
                        player.RpcSendMessage("You can't use /stop now.", "Warning");
                        break;
                    }
                    if (RandomItemsGamemode.instance.GetItem(player) == Items.Stop)
                    {
                        MeetingHud.Instance.RpcVotingComplete(new MeetingHud.VoterState[0], null, false);  
                        RandomItemsGamemode.instance.SendRPC(player, Items.None);
                        Utils.SendSpam(true);
                    }
                    break;
                case "/n":
                case "/now":
                    canceled = true;
                    Utils.SendGameOptionsMessage(player);
                    break;
                case "/id":
                    canceled = true;
                    var type = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        type += subArgs;
                    }
                    type = type.ToLower().Replace(" ", "");
                    switch (type)
                    {
                        case "players":
                            var player_ids = "";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (AntiCheat.BannedPlayers.Contains(pc.NetId)) continue;
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
                        default:
                            player.RpcSendMessage("Invalid use of command.\nUsage: /id players, /id colors", "Warning");
                            break;
                    }
                    break;
                case "/commands":
                case "/cm":
                    canceled = true;
                    player.RpcSendMessage("Commands:\n/color COLOR - changes your color\n/name NAME - changes your name\n/h gm - show gamemode description\n/n - show active settings\n" +
                        "/id (players, colors) - show ids\n/h i - show item description\n/cm - show list of commands\n/l - show last game result\n/tpout - teleports you outside lobby ship\n" +
                        "/tpin - teleports you into lobby ship\n/tagcolor - changes color of your tag\n/m - shows your role description\n/guess PLAYER_ID ROLE - you guess player as nice/evil guesser\n" +
                        "/r - shows roles percentage and amount\n/r ROLE - shows role description\n/kc - shows alive killers amount in classic", "Commands");
                    break;
                case "/lastresult":
                case "/l":
                    canceled = true;
                    if (Main.LastResult != "")
                        player.RpcSendMessage(Main.LastResult, "LastResult");
                    break;
                case "/info":
                    canceled = true;
                    if (!Main.GameStarted || CustomGamemode.Instance.Gamemode != Gamemodes.RandomItems || player.Data.IsDead)
                    {
                        player.RpcSendMessage("You can't use /info now.", "Warning");
                        break;
                    }
                    if (RandomItemsGamemode.instance.GetItem(player) == Items.Newsletter)
                    {
                        int crewmates = 0;
                        int scientists = 0 ;
                        int engineers = 0;       
                        int impostors = 0;
                        int shapeshifters = 0;
                        int noisemakers = 0;
                        int phantoms = 0;
                        int trackers = 0;
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
                            if (pc.Data.IsDead) continue;
                            switch (pc.Data.Role.Role)
                            {
                                case RoleTypes.Crewmate: ++crewmates; break;
                                case RoleTypes.Scientist: ++scientists; break;
                                case RoleTypes.Engineer: ++engineers; break;
                                case RoleTypes.Impostor: ++impostors; break;
                                case RoleTypes.Shapeshifter: ++shapeshifters; break;
                                case RoleTypes.Noisemaker: ++noisemakers; break;
                                case RoleTypes.Phantom: ++phantoms; break;
                                case RoleTypes.Tracker: ++trackers; break;
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
                        msg += noisemakers + " noisemakers\n";
                        msg += trackers + " trackers\n";
                        msg += impostors + " impostors\n";
                        msg += shapeshifters + " shapeshifters\n";
                        msg += phantoms + " phantoms\n\n";
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
                        RandomItemsGamemode.instance.SendRPC(player, Items.None);
                    }
                    break;
                case "/tpout":
                    canceled = true;
                    if (Main.GameStarted)
                    {
                        player.RpcSendMessage("You can't use /tpout during game.", "Warning");
                        break;
                    }
                    if (!Options.CanUseTpoutCommand.GetBool() || !Main.ModdedProtocol.Value)
                    {
                        player.RpcSendMessage("Host disabled usage of this command.", "Warning");
                        break;
                    }
                    player.RpcTeleport(new Vector2(0.1f, 3.8f));
                    break;
                case "/tpin":
                    canceled = true;
                    if (Main.GameStarted)
                    {
                        player.RpcSendMessage("You can't use /tpin during game.", "Warning");
                        break;
                    }
                    if (!Options.CanUseTpoutCommand.GetBool() || !Main.ModdedProtocol.Value)
                    {
                        player.RpcSendMessage("Host disabled usage of this command.", "Warning");
                        break;
                    }
                    player.RpcTeleport(new Vector2(-0.2f, 1.3f));
                    break;
                case "/tagcolor":
                    canceled = true;
                    if (args.Length < 2 || !Utils.IsValidHexCode(args[1]))
                    {
                        player.RpcSendMessage("Invalid color code. Please provide a valid hex color code.\nUsage: /tagcolor FF0000", "Warning");
                        break;
                    }
                    var hexColor = args[1];
                    string playerName = Main.StandardNames[player.PlayerId];
                    string friendCode = player.Data.FriendCode;
                    PlayerTagManager.UpdateNameAndTag(playerName, friendCode, hexColor, false);
                    break;
                case "/myrole":
                case "/m":
                    canceled = true;
                    if (!Main.GameStarted)
                    {
                        player.RpcSendMessage("You can't use /myrole in lobby.", "Warning");
                        break;
                    }
                    if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic)
                    {
                        player.RpcSendMessage("You can only use /myrole in classic gamemode.", "Warning");
                        break;
                    }
                    player.RpcSendMessage(player.GetRole().RoleDescriptionLong + "\n\n<u>Role options:</u>\n" + Utils.GetRoleOptions(player.GetRole().Role), "RoleInfo");
                    foreach (var addOn in player.GetAddOns())
                        player.RpcSendMessage(addOn.AddOnDescriptionLong + "\n\n<u>Add on options:</u>\n" + Utils.GetAddOnOptions(addOn.Type), "RoleInfo");
                    break;
                case "/guess":
                case "/shoot":
                case "/bet":
                case "/bt":
                case "/gs":
                    canceled = true;
                    if (!Main.GameStarted)
                    {
                        player.RpcSendMessage("You can't use /guess in lobby.", "Warning");
                        break;
                    }
                    if (player.Data.IsDead)
                    {
                        player.RpcSendMessage("You can only use /guess when you're alive.", "Warning");
                        break;
                    }
                    if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic)
                    {
                        player.RpcSendMessage("You can only use /guess in classic gamemode.", "Warning");
                        break;
                    }
                    subArgs = args.Length < 2 ? "" : args[1];
                    byte playerId = byte.Parse(subArgs);
                    var role = "";
                    for (int i = 2; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        role += subArgs;
                    }
                    role = role.ToLower().Replace(" ", "");
                    if (!CustomRolesHelper.CommandRoleNames.ContainsKey(role) && !AddOnsHelper.CommandAddOnNames.ContainsKey(role))
                    {
                        player.RpcSendMessage("This role doesn't exist!", "Warning");
                        break;
                    }
                    var target = Utils.GetPlayerById(playerId);
                    if (target == null)
                    {
                        player.RpcSendMessage("This player doesn't exist!", "Warning");
                        break;
                    }
                    if (target.Data.IsDead)
                    {
                        player.RpcSendMessage("This player is already dead!", "Warning");
                        break;
                    }
                    if (CustomRolesHelper.CommandRoleNames.ContainsKey(role))
                    {
                        if (!player.GetRole().CanGuess(target, CustomRolesHelper.CommandRoleNames[role]) ||
                            !target.GetRole().CanGetGuessed(player, CustomRolesHelper.CommandRoleNames[role]) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.Immortal && !Immortal.CanBeGuessed.GetBool()) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.SecurityGuard && !SecurityGuard.CanBeGuessed.GetBool()) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.Mortician && !Mortician.CanBeGuessed.GetBool()) ||
                            (CustomRolesHelper.CommandRoleNames[role] == CustomRoles.Mayor && !Mayor.CanBeGuessed.GetBool()))
                        {
                            player.RpcSendMessage("You can't guess this player!", "Warning");
                            break;
                        }
                        if (target.GetRole().Role == CustomRolesHelper.CommandRoleNames[role])
                        {
                            target.RpcSetDeathReason(DeathReasons.Guessed);
                            target.RpcExileV2();
                            target.RpcGuessPlayer();
                            ++Main.PlayerKills[player.PlayerId];
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[playerId] + " was guessed", "Guesser"), 1f);
                        }
                        else
                        {
                            player.RpcSetDeathReason(DeathReasons.Guessed);
                            player.RpcExileV2();
                            player.RpcGuessPlayer();
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[player.PlayerId] + " was guessed", "Guesser"), 1f);
                        }
                    }
                    else
                    {
                        if (!player.GetRole().CanGuess(target, AddOnsHelper.CommandAddOnNames[role]) ||
                            (AddOnsHelper.CommandAddOnNames[role] == AddOns.Bait && !Bait.CanBeGuessed.GetBool()))
                        {
                            player.RpcSendMessage("You can't guess this player!", "Warning");
                            break;
                        }
                        if (target.HasAddOn(AddOnsHelper.CommandAddOnNames[role]))
                        {
                            target.RpcSetDeathReason(DeathReasons.Guessed);
                            target.RpcExileV2();
                            target.RpcGuessPlayer();
                            ++Main.PlayerKills[player.PlayerId];
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[playerId] + " was guessed", "Guesser"), 1f);
                        }
                        else
                        {
                            player.RpcSetDeathReason(DeathReasons.Guessed);
                            player.RpcExileV2();
                            player.RpcGuessPlayer();
                            Utils.SendSpam(true);
                            new LateTask(() => Utils.SendChat(Main.StandardNames[player.PlayerId] + " was guessed", "Guesser"), 1f);
                        }
                    }
                    break;
                case "/roles":
                case "/role":
                case "/r":
                    canceled = true;
                    if (Options.CurrentGamemode != Gamemodes.Classic)
                    {
                        player.RpcSendMessage("Roles are only in classic gamemode", "RoleInfo");
                        break;
                    }
                    var role2 = "";
                    for (int i = 1; i <= args.Length; ++i)
                    {
                        subArgs = args.Length < i + 1 ? "" : " " + args[i];
                        role2 += subArgs;
                    }
                    role = role2.ToLower().Replace(" ", "");
                    string crewmateRoles = "";
                    string impostorRoles = "";
                    string neutralRoles = "";
                    string addOns = "";
                    if (CustomRolesHelper.CommandRoleNames.ContainsKey(role))
                        player.RpcSendMessage(CustomRolesHelper.RoleDescriptionsLong[CustomRolesHelper.CommandRoleNames[role]] + "\n\n<u>Role options:</u>\n" + Utils.GetRoleOptions(CustomRolesHelper.CommandRoleNames[role]), "RoleInfo");
                    else if (AddOnsHelper.CommandAddOnNames.ContainsKey(role))
                        player.RpcSendMessage(AddOnsHelper.AddOnDescriptionsLong[AddOnsHelper.CommandAddOnNames[role]] + "\n\n<u>Add on options:</u>\n" + Utils.GetAddOnOptions(AddOnsHelper.CommandAddOnNames[role]), "RoleInfo");
                    else
                    {
                        foreach (var roleType in Enum.GetValues<CustomRoles>())
                        {
                            if (CustomRolesHelper.IsVanilla(roleType)) continue;
                            string chance = CustomRolesHelper.GetRoleChance(roleType) <= 100 ? CustomRolesHelper.GetRoleChance(roleType).ToString() + "%" : "Always";
                            string count = CustomRolesHelper.GetRoleCount(roleType).ToString();
                            if (CustomRolesHelper.IsCrewmate(roleType) && CustomRolesHelper.GetRoleChance(roleType) > 0)
                                crewmateRoles += Options.RolesChance[roleType].GetName() + ": " + chance + " x" + count + "\n";
                            if (CustomRolesHelper.IsImpostor(roleType) && CustomRolesHelper.GetRoleChance(roleType) > 0)
                                impostorRoles += Options.RolesChance[roleType].GetName() + ": " + chance + " x" + count + "\n";
                            if (CustomRolesHelper.IsNeutral(roleType) && CustomRolesHelper.GetRoleChance(roleType) > 0)
                                neutralRoles += Options.RolesChance[roleType].GetName() + ": " + chance + " x" + count + "\n";
                        }
                        if (crewmateRoles != "")
                            player.RpcSendMessage(crewmateRoles, "Crewmates");
                        if (impostorRoles != "")
                            player.RpcSendMessage(impostorRoles, "Impostors");
                        if (neutralRoles != "")
                            player.RpcSendMessage(neutralRoles, "Neutrals");
                        foreach (var addOn in Enum.GetValues<AddOns>())
                        {
                            if (AddOnsHelper.GetAddOnChance(addOn) > 0)
                                addOns += Options.AddOnsChance[addOn].GetName() + ": " + AddOnsHelper.GetAddOnChance(addOn) + "% x" + AddOnsHelper.GetAddOnCount(addOn) + "\n";
                        }
                        player.RpcSendMessage(addOns, "AddOns");
                    }
                    break;
                case "/kcount":
                case "/kc":
                    canceled = true;
                    if (!Main.GameStarted)
                    {
                        player.RpcSendMessage("You can't use /kcount in lobby", "Warning");
                        break;
                    }
                    if (Options.CurrentGamemode != Gamemodes.Classic)
                    {
                        player.RpcSendMessage("You can use /kcount only in classic gamemode", "Warning");
                        break;
                    }
                    if (!Options.CanUseKcountCommand.GetBool())
                    {
                        player.RpcSendMessage("Host disabled usage of this command.", "Warning");
                        break;
                    }
                    int impostorCount = 0;
                    int neutralKillerCount = 0;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data.IsDead || pc.Data.Disconnected) continue;
                        if (pc.GetRole().IsImpostor())
                            ++impostorCount;
                        if (pc.GetRole().IsNeutralKilling())
                            ++neutralKillerCount;
                    }
                    var text2 = impostorCount + Utils.ColorString(Palette.ImpostorRed, impostorCount == 1 ? " impostor" : " impostors") + "\n";
                    text2 += neutralKillerCount + Utils.ColorString(Color.gray, neutralKillerCount == 1 ? " neutral killer" : " neutral killers");
                    player.RpcSendMessage(text2, "KillCount");
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
        public static bool SendingSystemMessage = false;
        public static void Prefix()
        {
            if (AmongUsClient.Instance.AmHost && DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly)
                DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
        public static void Postfix(ChatController __instance)
        {
            // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Patches/ChatControlPatch.cs#L18
            if (Main.DarkTheme.Value)
            {
                __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                __instance.freeChatField.textArea.compoText.Color(Color.white);
                __instance.freeChatField.textArea.outputText.color = Color.white;

                __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                __instance.quickChatField.text.color = Color.white;
            }
            else
            {
                __instance.freeChatField.textArea.outputText.color = Color.black;
            }

            if (!AmongUsClient.Instance.AmHost || Main.MessagesToSend.Count < 1 || (Main.MessagesToSend[0].Item2 == byte.MaxValue && 1f > __instance.timeSinceLastMessage)) return;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            (string msg, byte sendTo, string title) = Main.MessagesToSend[0];
            Main.MessagesToSend.RemoveAt(0);
            int clientId = sendTo == byte.MaxValue ? -1 : Utils.GetPlayerById(sendTo).GetClientId();
            var name = player.Data.PlayerName;
            if (!Main.ModdedProtocol.Value && !Main.GameStarted)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    var clientId2 = pc.GetClientId();
                    if (clientId == clientId2 || clientId == -1)
                    {
                        if (pc.AmOwner)
                        {
                            player.SetName(Utils.ColorString(Color.blue, "MGM.SystemMessage." + title));
                            SendingSystemMessage = true;
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg);
                            SendingSystemMessage = false;
                            player.SetName(name);
                        }
                        else
                        {
                            PlayerControl playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
                            playerControl.PlayerId = player.PlayerId;
                            playerControl.isNew = false;
                            playerControl.notRealPlayer = true;
                            playerControl.NetTransform.SnapTo(new Vector2(50f, 50f));
                            AmongUsClient.Instance.NetIdCnt += 1U;
                            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                            MessageWriter writer = sender.stream;
                            sender.StartMessage(clientId);
                            writer.StartMessage(4);
                            SpawnGameDataMessage item = AmongUsClient.Instance.CreateSpawnMessage(playerControl, -2, SpawnFlags.None);
                            item.SerializeValues(writer);
                            writer.EndMessage();
			                for (uint i = 1; i <= 3; ++i)
                            {
                                writer.StartMessage(4);
                                writer.WritePacked(2U);
                                writer.WritePacked(-2);
                                writer.Write((byte)SpawnFlags.None);
                                writer.WritePacked(1);
                                writer.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
                                writer.StartMessage(1);
                                writer.EndMessage();
                                writer.EndMessage();
                            }
                            if (PlayerControl.AllPlayerControls.Contains(playerControl))
                                PlayerControl.AllPlayerControls.Remove(playerControl);
                            sender.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                                .Write(player.Data.NetId)
                                .Write(Utils.ColorString(Color.blue, "MGM.SystemMessage." + title))
                                .EndRpc();
                            sender.StartRpc(playerControl.NetId, (byte)RpcCalls.SendChat)
                                .Write(msg)
                                .EndRpc();
                            sender.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                                .Write(player.Data.NetId)
                                .Write(name)
                                .EndRpc();
                            writer.StartMessage(5);
			                writer.WritePacked(playerControl.NetId);
			                writer.EndMessage();
                            sender.EndMessage();
                            sender.SendMessage();
                            AmongUsClient.Instance.RemoveNetObject(playerControl);
                            playerControl.DespawnOnDestroy = false;
                            Object.Destroy(playerControl.gameObject);
                        }
                    }
                }
                return;
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var clientId2 = pc.GetClientId();
                if (clientId == clientId2 || clientId == -1)
                {
                    if (pc.AmOwner)
                    {
                        player.SetName(Utils.ColorString(Color.blue, "MGM.SystemMessage." + title));
                        SendingSystemMessage = true;
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg);
                        SendingSystemMessage = false;
                        player.SetName(name);
                    }
                    else
                    {
                        CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                        MessageWriter writer = sender.stream;
                        sender.StartMessage(clientId2);
                        sender.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                            .Write(player.Data.NetId)
                            .Write(Utils.ColorString(Color.blue, "MGM.SystemMessage." + title))
                            .EndRpc();
                        sender.StartRpc(player.NetId, (byte)RpcCalls.SendChat)
                            .Write(msg)
                            .EndRpc();
                        sender.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                            .Write(player.Data.NetId)
                            .Write(Main.GameStarted ? Main.LastNotifyNames[(player.PlayerId, pc.PlayerId)] : name)
                            .EndRpc();
                        sender.EndMessage();
                        sender.SendMessage();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
    class UpdateCharCountPatch
    {
        public static void Postfix(FreeChatInputField __instance)
        {
            int length = __instance.textArea.text.Length;
		    __instance.charCountText.text = length + "/" + __instance.textArea.characterLimit;
            if (Main.ModdedProtocol.Value || Main.GameStarted)
            {
                if (length < (AmongUsClient.Instance.AmHost ? 750 : 225))
		        {
		    	    __instance.charCountText.color = Color.black;
		    	    return;
		        }
		        if (length < (AmongUsClient.Instance.AmHost ? 1000 : 300))
		        {
		    	    __instance.charCountText.color = new Color(1f, 1f, 0f, 1f);
		    	    return;
		        }
		        __instance.charCountText.color = Color.red;
            }
		    else
            {
                if (length < 90)
		        {
		    	    __instance.charCountText.color = Color.black;
		    	    return;
		        }
		        if (length < 118)
		        {
		    	    __instance.charCountText.color = new Color(1f, 1f, 0f, 1f);
		    	    return;
		        }
		        __instance.charCountText.color = Color.red;
            }
		    __instance.charCountText.color = Color.red;
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
            if (!Main.GameStarted && Main.ModdedProtocol.Value)
            {
                chatText = chatText.Replace("[", "【");
                chatText = chatText.Replace("]", "】");
                chatText = chatText.Replace("<", " ");
                chatText = chatText.Replace(">", " ");
            }
            if (chatText[0] == '/' && !AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SendChat, SendOption.Reliable, AmongUsClient.Instance.HostId);
                writer.Write(chatText);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                __result = true;
                return false;
            }
            if (!Main.GameStarted && Main.ModdedProtocol.Value)
            {
                int return_count = PlayerControl.LocalPlayer.Data.PlayerName.Count(x => x == '\n');
                chatText = new StringBuilder(chatText).Insert(0, "<size=1.5>\n</size>", return_count).ToString();
            }
            else if (!Main.GameStarted)
            {
                int return_count = PlayerControl.LocalPlayer.Data.PlayerName.Count(x => x == '\n');
                chatText = new StringBuilder(chatText).Insert(0, "\n", return_count).ToString();
            }
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
            if (chatText.Contains("who", StringComparison.OrdinalIgnoreCase))
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SendChat, SendOption.Reliable, -1);
            messageWriter.Write(chatText);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
    class IsCharAllowedPatch
    {
        public static void Postfix(TextBoxTMP __instance, [HarmonyArgument(0)] char i, ref bool __result)
        {
            if (TextBoxTMP.SymbolChars.Contains(i) || TextBoxTMP.EmailChars.Contains(i) || i == '`' || i == '$' || i == '*' || i == '[' ||
                i == ']' || i == '{' || i == '}' || i == '|' || i == '"' || (i == '<' && __instance.AllowSymbols) || 
                (i == '>' && __instance.AllowSymbols) || i > 127)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatControllerUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (Main.ModdedProtocol.Value || Main.GameStarted)
                __instance.freeChatField.textArea.characterLimit = AmongUsClient.Instance.AmHost ? 1000 : 300;
            else
                __instance.freeChatField.textArea.characterLimit = 118;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
                ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
            {
                if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
                {
                    string copiedText = GUIUtility.systemCopyBuffer;
                    if ((__instance.freeChatField.textArea.text + copiedText).Length < __instance.freeChatField.textArea.characterLimit)
                        __instance.freeChatField.textArea.SetText(__instance.freeChatField.textArea.text + copiedText);
                    else
                    {
                        int remainingLength = __instance.freeChatField.textArea.characterLimit - __instance.freeChatField.textArea.text.Length;
                        if (remainingLength > 0)
                        {
                            string text = copiedText[..remainingLength];
                            __instance.freeChatField.textArea.SetText(__instance.freeChatField.textArea.text + text);
                        }
                    }
                }
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
            {
                ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
                __instance.freeChatField.textArea.SetText("");
            }
        }
    }

    [HarmonyPatch(typeof(UrlFinder), nameof(UrlFinder.TryFindUrl))]
    class TryFindUrlPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
    class SetRightPatch
    {
        public static bool Prefix(ChatBubble __instance)
        {
            if (ChatUpdatePatch.SendingSystemMessage)
            {
                __instance.SetLeft();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    class ChatBubbleSetNamePatch
    {
        public static void Postfix(ChatBubble __instance, [HarmonyArgument(1)] bool isDead, [HarmonyArgument(2)] bool voted)
        {
            // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Patches/ChatBubblePatch.cs#L35
            if (Main.DarkTheme.Value)
            {
                if (isDead)
                    __instance.Background.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
                else
                    __instance.Background.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                __instance.TextArea.color = Color.white;
            }
        }
    }
}