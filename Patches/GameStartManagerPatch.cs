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
            __result = GameOptionsManager.Instance.currentGameOptions.GetInt(Int32OptionNames.NumImpostors);
            return false;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    class BeginGamePatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            if (Options.RandomMap.GetBool())
            {
                List<byte> maps = new();
                if (Options.AddTheSkeld.GetBool())
                    maps.Add(0);
                if (Options.AddMiraHQ.GetBool())
                    maps.Add(1);
                if (Options.AddPolus.GetBool())
                    maps.Add(2);
                if (Options.AddTheAirship.GetBool())
                    maps.Add(4);
                if (Options.AddTheFungle.GetBool())
                    maps.Add(5);
                if (maps.Count == 0)
                {
                    maps.Add(0);
                    maps.Add(1);
                    maps.Add(2);
                    maps.Add(4);
                    maps.Add(5);
                }
                var rand = new Random();
                byte map = maps[rand.Next(0, maps.Count)];
                GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, map);
            }
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
                GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            GameManager.Instance.RpcSyncCustomOptions();
            Utils.SyncSettings(GameOptionsManager.Instance.currentGameOptions);
        }
    }
}