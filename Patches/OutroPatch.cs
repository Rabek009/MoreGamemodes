using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.Data;
using Assets.CoreScripts;
using Il2CppSystem;
using AmongUs.GameOptions;
using System.Linq;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    class EndGamePatch
    {
        public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
        {
            if (!__instance.AmHost) return true;
            List<byte> winners = new();
            StatsManager.Instance.BanPoints -= 1.5f;
		    StatsManager.Instance.LastGameStarted = DateTime.MinValue;
		    GameOverReason gameOverReason = endGameResult.GameOverReason;
		    bool showAd = endGameResult.ShowAd;
		    __instance.DisconnectHandlers.Clear();
		    if (Minigame.Instance)
		    {
				Minigame.Instance.Close();
				Minigame.Instance.Close();
		    }
		    float durationInSeconds = Time.realtimeSinceStartup - TempData.TimeGameStarted;
		    DestroyableSingleton<DebugAnalytics>.Instance.Analytics.EndGame(durationInSeconds, gameOverReason, GameData.Instance.AllPlayers);
			DestroyableSingleton<UnityTelemetry>.Instance.EndGame(gameOverReason);
		    TempData.EndReason = gameOverReason;
		    TempData.showAd = showAd;
		    GameManager.Instance.DidHumansWin(gameOverReason);
		    TempData.OnGameEnd();
		    ProgressionManager.XpGrantResult xpGrantedResult = endGameResult.XpGrantResult ?? ProgressionManager.XpGrantResult.Default();
		    ProgressionManager.CurrencyGrantResult currencyGrantResult = endGameResult.BeansGrantResult ?? ProgressionManager.CurrencyGrantResult.Default();
		    ProgressionManager.CurrencyGrantResult currencyGrantResult2 = endGameResult.PodsGrantResult ?? ProgressionManager.CurrencyGrantResult.Default();
		    TempData.XpGrantedResult = xpGrantedResult;
		    TempData.BeansGrantResult = currencyGrantResult;
		    TempData.PodsGrantResult = currencyGrantResult2;
		    if (endGameResult.XpGrantResult != null)
		    {
			    DataManager.Player.Stats.Xp = endGameResult.XpGrantResult.OldXpAmount + endGameResult.XpGrantResult.GrantedXp;
			    if (endGameResult.XpGrantResult.LevelledUp)
			    {
			    	DataManager.Player.Stats.Xp = endGameResult.XpGrantResult.OldXpAmount + endGameResult.XpGrantResult.GrantedXp - endGameResult.XpGrantResult.XpRequiredToLevelUp;
			    	DataManager.Player.Stats.Level = endGameResult.XpGrantResult.NewLevel;
			    	DataManager.Player.Stats.XpForNextLevel = endGameResult.XpGrantResult.XpRequiredToLevelUpNextLevel;
			    }
			    else
			    {
				    DataManager.Player.Stats.Xp = endGameResult.XpGrantResult.OldXpAmount + endGameResult.XpGrantResult.GrantedXp;
				    DataManager.Player.Stats.Level = endGameResult.XpGrantResult.OldLevel;
				    DataManager.Player.Stats.XpForNextLevel = endGameResult.XpGrantResult.XpRequiredToLevelUp;
			    }
			    DataManager.Player.Save();
		    }
		    DestroyableSingleton<InventoryManager>.Instance.ChangePodCount(currencyGrantResult2.PodId, (int)currencyGrantResult2.GrantedPodsWithMultiplierApplied);
		    DestroyableSingleton<InventoryManager>.Instance.UnusedBeans += (int)currencyGrantResult.GrantedPodsWithMultiplierApplied;
		    for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		    {
			    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			    if (playerInfo != null && playerInfo.Role.DidWin(gameOverReason))
			    {
                    if (!((Options.CurrentGamemode == Gamemodes.BombTag || Options.CurrentGamemode == Gamemodes.BattleRoyale || Options.CurrentGamemode == Gamemodes.Speedrun || Options.CurrentGamemode == Gamemodes.PaintBattle || Options.CurrentGamemode == Gamemodes.KillOrDie || Options.CurrentGamemode == Gamemodes.Zombies) && playerInfo.Disconnected))
                    {
                        TempData.winners.Add(new WinningPlayerData(playerInfo));
                        winners.Add(playerInfo.PlayerId);
                    }    
			    }
		    }
            string lastResult = "";
            for (int i = 0; i < GameData.Instance.PlayerCount; ++i)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (playerInfo == null) continue;
                if (!Main.StandardRoles.ContainsKey(playerInfo.PlayerId)) continue;
                if (!winners.Contains(playerInfo.PlayerId)) continue;
                if (Main.StandardColors[playerInfo.PlayerId] < 0 || Main.StandardColors[playerInfo.PlayerId] >= 18) Main.StandardColors[playerInfo.PlayerId] = 0;
                switch (Options.CurrentGamemode)
                {
                    case Gamemodes.Classic:
                    case Gamemodes.HideAndSeek:
                    case Gamemodes.ShiftAndSeek:
                    case Gamemodes.RandomItems:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        lastResult += Utils.ColorString(Main.StandardRoles[playerInfo.PlayerId].IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue, Utils.RoleToString(Main.StandardRoles[playerInfo.PlayerId], Options.CurrentGamemode)) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.BombTag:
                    case Gamemodes.KillOrDie:
                        lastResult += "★" + Main.StandardNames[playerInfo.PlayerId] + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.BattleRoyale:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.Speedrun:
                        int completedTasks = 0;
                        int totalTasks = 0;
                        foreach (var task in playerInfo.Tasks)
                        {
                            ++totalTasks;
                            if (task.Complete)
                                ++completedTasks;
                        }
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Color.yellow, completedTasks + "/" + totalTasks) + ")\n";
                        break;
                    case Gamemodes.PaintBattle:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + "\n";
                        break;
                    case Gamemodes.Zombies:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (ZombiesGamemode.instance.ZombieType[playerInfo.PlayerId] != ZombieTypes.None)
                            lastResult += Utils.ColorString(Palette.PlayerColors[2], "Zombie") + " (";
                        else if (Main.StandardRoles[playerInfo.PlayerId] == RoleTypes.Impostor)
                            lastResult += Utils.ColorString(Palette.ImpostorRed, "Impostor") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.CrewmateBlue, "Crewmate") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.Jailbreak:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], "★" + Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (JailbreakGamemode.instance.PlayerType[playerInfo.PlayerId] == JailbreakPlayerTypes.Guard)
                            lastResult += Utils.ColorString(Color.blue, "Guard") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.Orange, "Prisoner") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                }
            }
            for (int i = 0; i < GameData.Instance.PlayerCount; ++i)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (playerInfo == null) continue;
                if (!Main.StandardRoles.ContainsKey(playerInfo.PlayerId)) continue;
                if (winners.Contains(playerInfo.PlayerId)) continue;
                if (Main.StandardColors[playerInfo.PlayerId] < 0 || Main.StandardColors[playerInfo.PlayerId] >= 18) Main.StandardColors[playerInfo.PlayerId] = 0;
                switch (Options.CurrentGamemode)
                {
                    case Gamemodes.Classic:
                    case Gamemodes.HideAndSeek:
                    case Gamemodes.ShiftAndSeek:
                    case Gamemodes.RandomItems:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        lastResult += Utils.ColorString(Main.StandardRoles[playerInfo.PlayerId].IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue, Utils.RoleToString(playerInfo.Role.Role)) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.BombTag:
                    case Gamemodes.KillOrDie:
                        lastResult += Main.StandardNames[playerInfo.PlayerId] + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.BattleRoyale:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.Speedrun:
                        int completedTasks = 0;
                        int totalTasks = 0;
                        foreach (var task in playerInfo.Tasks)
                        {
                            ++totalTasks;
                            if (task.Complete)
                                ++completedTasks;
                        }
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " (";
                        lastResult += Utils.ColorString(Color.yellow, completedTasks + "/" + totalTasks) + ")\n";
                        break;
                    case Gamemodes.PaintBattle:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + "\n";
                        break;
                    case Gamemodes.Zombies:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (ZombiesGamemode.instance.ZombieType[playerInfo.PlayerId] != ZombieTypes.None)
                            lastResult += Utils.ColorString(Palette.PlayerColors[2], "Zombie") + " (";
                        else if (Main.StandardRoles[playerInfo.PlayerId] == RoleTypes.Impostor)
                            lastResult += Utils.ColorString(Palette.ImpostorRed, "Impostor") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.CrewmateBlue, "Crewmate") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                    case Gamemodes.Jailbreak:
                        lastResult += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[playerInfo.PlayerId]], Main.StandardNames[playerInfo.PlayerId]) + " - ";
                        if (JailbreakGamemode.instance.PlayerType[playerInfo.PlayerId] == JailbreakPlayerTypes.Guard)
                            lastResult += Utils.ColorString(Color.blue, "Guard") + " (";
                        else
                            lastResult += Utils.ColorString(Palette.Orange, "Prisoner") + " (";
                        lastResult += Utils.ColorString(Main.AllPlayersDeathReason[playerInfo.PlayerId] == DeathReasons.Alive ? Color.green : Color.red, Utils.DeathReasonToString(Main.AllPlayersDeathReason[playerInfo.PlayerId])) + ")\n";
                        break;
                }
            }
            Main.LastResult = lastResult;
            GameDebugCommands.RemoveCommands();
		    __instance.StartCoroutine(__instance.CoEndGame());
            return false;
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    class SetEverythingUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            Main.GameStarted = false;
            CustomGamemode.Instance = null;
            ClassicGamemode.instance = null;
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
            if (!AmongUsClient.Instance.AmHost) return;
            Main.AllShapeshifts = new Dictionary<byte, byte>();  
            Main.RealOptions.Restore(GameOptionsManager.Instance.currentGameOptions);
            Main.RealOptions = null;
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.MessagesToSend = new List<(string, byte, string)>();
            Main.StandardRoles = new Dictionary<byte, RoleTypes>();
            CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
            CheckProtectPatch.TimeSinceLastProtect = new Dictionary<byte, float>();
            Main.ProximityMessages = new Dictionary<byte, List<(string, float)>>();
            Main.NameColors = new Dictionary<(byte, byte), Color>();
            Main.IsModded = new Dictionary<byte, bool>();
            Main.Disconnected = new Dictionary<byte, bool>();
            CustomNetObject.CustomObjects = new List<CustomNetObject>();
            CustomNetObject.MaxId = -1;
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
                var TimeTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
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