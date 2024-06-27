using HarmonyLib;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManagerUpdatePatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    class UnrestrictedNumImpostorsPatch
    {
        public static bool Prefix(ref int __result)
        {
            __result = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors);
            return false;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    class BeginGamePatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Options.EnableRandomMap.GetBool())
            {
                List<byte> maps = new();
                if (Options.AddTheSkeld.GetBool())
                    maps.Add(0);
                if (Options.AddMiraHQ.GetBool())
                    maps.Add(1);
                if (Options.AddPolus.GetBool())
                    maps.Add(2);
                if (Options.AddDleksEht.GetBool())
                    maps.Add(3);
                if (Options.AddTheAirship.GetBool())
                    maps.Add(4);
                if (Options.AddTheFungle.GetBool())
                    maps.Add(5);
                if (maps.Count == 0)
                {
                    maps.Add(0);
                    maps.Add(1);
                    maps.Add(2);
                    maps.Add(3);
                    maps.Add(4);
                    maps.Add(5);
                }
                var rand = new Random();
                byte map = maps[rand.Next(0, maps.Count)];
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, map);
            }
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            if (Options.CurrentGamemode == Gamemodes.Jailbreak && GameOptionsManager.Instance.CurrentGameOptions.MapId != 0 && GameOptionsManager.Instance.CurrentGameOptions.MapId != 3)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		    GameManager.Instance.LogicOptions.SyncOptions();
        }
    }
}