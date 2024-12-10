using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    class EndGamePatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            if (!__instance.AmHost) return;
            List<byte> winners = new();
		    GameOverReason gameOverReason = endGameResult.GameOverReason;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
		    for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		    {
			    NetworkedPlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			    if (playerInfo != null && playerInfo.Role.DidWin(gameOverReason))
			    {
                    if (!((CustomGamemode.Instance.Gamemode is Gamemodes.Classic or Gamemodes.BombTag or Gamemodes.BattleRoyale or Gamemodes.Speedrun or Gamemodes.PaintBattle or Gamemodes.KillOrDie or Gamemodes.Zombies or Gamemodes.Jailbreak or Gamemodes.BaseWars or Gamemodes.ColorWars) && playerInfo.Disconnected))
                    {
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(playerInfo));
                        winners.Add(playerInfo.PlayerId);
                    }    
			    }
		    }
            string lastResult = "";
            for (int i = 0; i < GameData.Instance.PlayerCount; ++i)
            {
                NetworkedPlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (playerInfo == null) continue;
                if (!Main.StandardRoles.ContainsKey(playerInfo.PlayerId)) continue;
                if (!winners.Contains(playerInfo.PlayerId)) continue;
                if (Main.StandardColors[playerInfo.PlayerId] < 0 || Main.StandardColors[playerInfo.PlayerId] >= 18) Main.StandardColors[playerInfo.PlayerId] = 0;
                switch (CustomGamemode.Instance.Gamemode)
                {
                    case Gamemodes.Classic:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        foreach (var addOn in ClassicGamemode.instance.AllPlayersAddOns[playerInfo.PlayerId])
                            lastResult += Utils.ColorString(addOn.Color, "(" + addOn.AddOnName + ")") + " ";
                        lastResult += Utils.ColorString(ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].Color, ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].RoleName);
                        lastResult += Utils.ColorString(ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].Color, ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].GetProgressText()) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.HideAndSeek:
                    case Gamemodes.ShiftAndSeek:
                    case Gamemodes.RandomItems:
                    case Gamemodes.Deathrun:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        lastResult += Utils.ColorString(Main.StandardRoles[playerInfo.PlayerId].IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue, Utils.RoleToString(Main.StandardRoles[playerInfo.PlayerId], CustomGamemode.Instance.Gamemode)) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.BombTag:
                    case Gamemodes.KillOrDie:
                        lastResult += "★" + Main.StandardNames[playerInfo.PlayerId] + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.BattleRoyale:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.Speedrun:
                    case Gamemodes.PaintBattle:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + "";
                        break;
                    case Gamemodes.Zombies:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (ZombiesGamemode.instance.ZombieType[playerInfo.PlayerId] != ZombieTypes.None)
                            lastResult += Utils.ColorString(Palette.PlayerColors[2], "Zombie") + " (";
                        else if (Main.StandardRoles[playerInfo.PlayerId] == RoleTypes.Impostor)
                            lastResult += Utils.ColorString(Palette.ImpostorRed, "Impostor") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.CrewmateBlue, "Crewmate") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.Jailbreak:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (JailbreakGamemode.instance.PlayerType[playerInfo.PlayerId] == JailbreakPlayerTypes.Guard)
                            lastResult += Utils.ColorString(Color.blue, "Guard") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.Orange, "Prisoner") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.BaseWars:
                        if (BaseWarsGamemode.instance.PlayerTeam[playerInfo.PlayerId] == BaseWarsTeams.Red)
                            lastResult += Utils.ColorString(Color.red, "★" + Main.StandardNames[playerInfo.PlayerId]) + " - (";
                        else if (BaseWarsGamemode.instance.PlayerTeam[playerInfo.PlayerId] == BaseWarsTeams.Blue)
                            lastResult += Utils.ColorString(Color.blue, "★" + Main.StandardNames[playerInfo.PlayerId]) + " - (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.FreezeTag:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        lastResult += Utils.ColorString(Main.StandardRoles[playerInfo.PlayerId].IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue, Utils.RoleToString(Main.StandardRoles[playerInfo.PlayerId], CustomGamemode.Instance.Gamemode)) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.ColorWars:
                        if (ColorWarsGamemode.instance.Team[playerInfo.PlayerId] == byte.MaxValue)
                            lastResult += Utils.ColorString(Color.gray, "★" + Main.StandardNames[playerInfo.PlayerId]) + " (";
                        else
                            lastResult += Utils.ColorString(Palette.PlayerColors[ColorWarsGamemode.instance.Team[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                }
                if (CustomGamemode.Instance.Gamemode is Gamemodes.HideAndSeek or Gamemodes.ShiftAndSeek or Gamemodes.RandomItems or Gamemodes.Speedrun or Gamemodes.Zombies or Gamemodes.FreezeTag)
                {
                    if (!Main.StandardRoles[playerInfo.PlayerId].IsImpostor())
                    {
                        int completedTasks = 0;
                        int totalTasks = 0;
                        foreach (var task in playerInfo.Tasks)
                        {
                            ++totalTasks;
                            if (task.Complete)
                                ++completedTasks;
                        }
                        lastResult += Utils.ColorString(Color.yellow, " (" + completedTasks + "/" + totalTasks + ")");
                    }
                }
                if (Main.PlayerKills[playerInfo.PlayerId] > 0)
                    lastResult += Utils.ColorString(Color.red, " Kills: " + Main.PlayerKills[playerInfo.PlayerId]);
                lastResult += "\n";
            }
            for (int i = 0; i < GameData.Instance.PlayerCount; ++i)
            {
                NetworkedPlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (playerInfo == null) continue;
                if (!Main.StandardRoles.ContainsKey(playerInfo.PlayerId)) continue;
                if (winners.Contains(playerInfo.PlayerId)) continue;
                if (Main.StandardColors[playerInfo.PlayerId] < 0 || Main.StandardColors[playerInfo.PlayerId] >= 18) Main.StandardColors[playerInfo.PlayerId] = 0;
                switch (CustomGamemode.Instance.Gamemode)
                {
                    case Gamemodes.Classic:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        foreach (var addOn in ClassicGamemode.instance.AllPlayersAddOns[playerInfo.PlayerId])
                            lastResult += Utils.ColorString(addOn.Color, "(" + addOn.AddOnName + ")") + " ";
                        lastResult += Utils.ColorString(ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].Color, ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].RoleName);
                        lastResult += Utils.ColorString(ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].Color, ClassicGamemode.instance.AllPlayersRole[playerInfo.PlayerId].GetProgressText()) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.HideAndSeek:
                    case Gamemodes.ShiftAndSeek:
                    case Gamemodes.RandomItems:
                    case Gamemodes.Deathrun:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        lastResult += Utils.ColorString(Main.StandardRoles[playerInfo.PlayerId].IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue, Utils.RoleToString(playerInfo.Role.Role)) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.BombTag:
                    case Gamemodes.KillOrDie:
                        lastResult += Main.StandardNames[playerInfo.PlayerId] + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.BattleRoyale:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.Speedrun:
                    case Gamemodes.PaintBattle:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]);
                        break;
                    case Gamemodes.Zombies:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (ZombiesGamemode.instance.ZombieType[playerInfo.PlayerId] != ZombieTypes.None)
                            lastResult += Utils.ColorString(Palette.PlayerColors[2], "Zombie") + " (";
                        else if (Main.StandardRoles[playerInfo.PlayerId] == RoleTypes.Impostor)
                            lastResult += Utils.ColorString(Palette.ImpostorRed, "Impostor") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.CrewmateBlue, "Crewmate") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.Jailbreak:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (JailbreakGamemode.instance.PlayerType[playerInfo.PlayerId] == JailbreakPlayerTypes.Guard)
                            lastResult += Utils.ColorString(Color.blue, "Guard") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.Orange, "Prisoner") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.BaseWars:
                        if (BaseWarsGamemode.instance.PlayerTeam[playerInfo.PlayerId] == BaseWarsTeams.Red)
                            lastResult += Utils.ColorString(Color.red, Main.StandardNames[playerInfo.PlayerId]) + " - (";
                        else if (BaseWarsGamemode.instance.PlayerTeam[playerInfo.PlayerId] == BaseWarsTeams.Blue)
                            lastResult += Utils.ColorString(Color.blue, Main.StandardNames[playerInfo.PlayerId]) + " - (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.FreezeTag:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        lastResult += Utils.ColorString(Main.StandardRoles[playerInfo.PlayerId].IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue, Utils.RoleToString(Main.StandardRoles[playerInfo.PlayerId], CustomGamemode.Instance.Gamemode)) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                    case Gamemodes.ColorWars:
                        if (ColorWarsGamemode.instance.Team[playerInfo.PlayerId] == byte.MaxValue)
                            lastResult += Utils.ColorString(Color.gray, Main.StandardNames[playerInfo.PlayerId]) + " (";
                        else
                            lastResult += Utils.ColorString(Palette.PlayerColors[ColorWarsGamemode.instance.Team[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")";
                        break;
                }
                if (CustomGamemode.Instance.Gamemode is Gamemodes.HideAndSeek or Gamemodes.ShiftAndSeek or Gamemodes.RandomItems or Gamemodes.Speedrun or Gamemodes.Zombies or Gamemodes.FreezeTag)
                {
                    if (!Main.StandardRoles[playerInfo.PlayerId].IsImpostor())
                    {
                        int completedTasks = 0;
                        int totalTasks = 0;
                        foreach (var task in playerInfo.Tasks)
                        {
                            ++totalTasks;
                            if (task.Complete)
                                ++completedTasks;
                        }
                        lastResult += Utils.ColorString(Color.yellow, " (" + completedTasks + "/" + totalTasks + ")");
                    }
                }
                if (Main.PlayerKills[playerInfo.PlayerId] > 0)
                    lastResult += Utils.ColorString(Color.red, " Kills: " + Main.PlayerKills[playerInfo.PlayerId]);
                lastResult += "\n";
            }
            Main.LastResult = lastResult;
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    class SetEverythingUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            Main.GameStarted = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                var WinnerTextObject = Object.Instantiate(__instance.WinText.gameObject);
                WinnerTextObject.transform.position = new(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
                WinnerTextObject.transform.localScale = new(0.6f, 0.6f, 0.6f);
                var WinnerText = WinnerTextObject.GetComponent<TMPro.TextMeshPro>();
                WinnerText.fontSizeMin = 3f;
                WinnerText.color = Color.white;
                WinnerText.text = "";
                switch (ClassicGamemode.instance.Winner)
                {
                    case CustomWinners.Terminated:
                        WinnerText.text += Utils.ColorString(Color.gray, "Host terminated game");
                        __instance.BackgroundBar.material.color = Color.gray;
                        break;
                    case CustomWinners.NoOne:
                        WinnerText.text += Utils.ColorString(Color.red, "Everyone died");
                        __instance.BackgroundBar.material.color = Color.red;
                        break;
                    case CustomWinners.Crewmates:
                        WinnerText.text += Utils.ColorString(Palette.CrewmateBlue, "Crewmates ");
                        __instance.BackgroundBar.material.color = Palette.CrewmateBlue;
                        break;
                    case CustomWinners.Impostors:
                        WinnerText.text += Utils.ColorString(Palette.ImpostorRed, "Impostors ");
                        __instance.BackgroundBar.material.color = Palette.ImpostorRed;
                        break;
                    case CustomWinners.Jester:
                        WinnerText.text += Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Jester], "Jester ");
                        __instance.BackgroundBar.material.color = CustomRolesHelper.RoleColors[CustomRoles.Jester];
                        break;
                    case CustomWinners.SerialKiller:
                        WinnerText.text += Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.SerialKiller], "Serial Killer ");
                        __instance.BackgroundBar.material.color = CustomRolesHelper.RoleColors[CustomRoles.SerialKiller];
                        break;
                    case CustomWinners.Executioner:
                        WinnerText.text += Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Executioner], "Executioner ");
                        __instance.BackgroundBar.material.color = CustomRolesHelper.RoleColors[CustomRoles.Executioner];
                        break;
                    case CustomWinners.Pelican:
                        WinnerText.text += Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Pelican], "Pelican ");
                        __instance.BackgroundBar.material.color = CustomRolesHelper.RoleColors[CustomRoles.Pelican];
                        break;
                }
                if (!(ClassicGamemode.instance.Winner is CustomWinners.None or CustomWinners.Terminated or CustomWinners.NoOne))
                {
                    foreach (var winner in ClassicGamemode.instance.AdditionalWinners)
                    {
                        WinnerText.text += "& ";
                        switch (winner)
                        {
                            case AdditionalWinners.Opportunist:
                                ColorUtility.TryParseHtmlString("#1dde16", out Color opportunistColor);
                                WinnerText.text += Utils.ColorString(opportunistColor, "Opportunist ");
                                break;
                        }
                    }
                    WinnerText.text += "win!";
                }
            }

            CustomGamemode.Instance = null;
            ClassicGamemode.instance = null;
            UnmoddedGamemode.instance = null;
            HideAndSeekGamemode.instance = null;
            ShiftAndSeekGamemode.instance = null;
            BombTagGamemode.instance = null;
            RandomItemsGamemode.instance = null;
            BattleRoyaleGamemode.instance = null;
            SpeedrunGamemode.instance = null;
            PaintBattleGamemode.instance = null;
            KillOrDieGamemode.instance = null;
            ZombiesGamemode.instance = null;
            JailbreakGamemode.instance = null;
            DeathrunGamemode.instance = null;
            BaseWarsGamemode.instance = null;
            FreezeTagGamemode.instance = null;
            
            if (!AmongUsClient.Instance.AmHost) return;
            Main.AllShapeshifts = new Dictionary<byte, byte>();  
            Main.RealOptions.Restore(GameOptionsManager.Instance.CurrentGameOptions);
            Main.RealOptions = null;
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.MessagesToSend = new List<(string, byte, string)>();
            Main.StandardRoles = new Dictionary<byte, RoleTypes>();
            Main.DesyncRoles = new Dictionary<(byte, byte), RoleTypes>();
            Main.NameMessages = new Dictionary<byte, List<(string, float)>>();
            Main.NameColors = new Dictionary<(byte, byte), Color>();
            Main.IsModded = new Dictionary<byte, bool>();
            CustomNetObject.CustomObjects = new List<CustomNetObject>();
            CustomNetObject.MaxId = -1;
            RpcSetRolePatch.RoleAssigned = new Dictionary<byte, bool>();
            Main.RoleFakePlayer = new Dictionary<byte, uint>();
            Main.PlayerKills = new Dictionary<byte, int>();
            Main.KillCooldowns = new Dictionary<byte, float>();
            Main.OptionKillCooldowns = new Dictionary<byte, float>();
            Main.ProtectCooldowns = new Dictionary<byte, float>();
            Main.OptionProtectCooldowns = new Dictionary<byte, float>();
            Main.TimeSinceLastPet = new Dictionary<byte, float>();
            CreateOptionsPickerPatch.SetDleks = GameOptionsManager.Instance.CurrentGameOptions.MapId == 3;
            CoEnterVentPatch.PlayersToKick = new List<byte>();
            AntiBlackout.Reset();
            if (Options.CurrentGamemode == Gamemodes.Speedrun)
            {
                var hours = (int)Main.Timer / 3600;
                Main.Timer -= hours * 3600;
                var minutes = (int)Main.Timer / 60;
                Main.Timer -= minutes * 60;
                var seconds = (int)Main.Timer;
                Main.Timer -= seconds;
                var miliseconds = (int)(Main.Timer * 1000);
                var TimeTextObject = Object.Instantiate(__instance.WinText.gameObject);
                TimeTextObject.transform.position = new(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
                TimeTextObject.transform.localScale = new(0.6f, 0.6f, 0.6f);
                var TimeText = TimeTextObject.GetComponent<TMPro.TextMeshPro>();
                TimeText.fontSizeMin = 3f;
                TimeText.text = "Speedrun finished in ";
                TimeText.text += hours + ":";
                if (minutes < 10)
                    TimeText.text += "0";
                TimeText.text += minutes + ":";
                if (seconds < 10)
                    TimeText.text += "0";
                TimeText.text += seconds + ".";
                if (miliseconds < 10)
                    TimeText.text += "00";
                else if (miliseconds < 100)
                    TimeText.text += "0";
                TimeText.text += miliseconds;
                TimeText.color = Color.yellow;
            }
            Main.Timer = 0f;
        }
    }
}