using HarmonyLib;
using AmongUs.GameOptions;

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
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
            {
                GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 0);
                GameManager.Instance.LogicOptions.SyncOptions();
            }
        }
    }
}