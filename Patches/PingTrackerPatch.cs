using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingShowerPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.text += Utils.ColorString(Color.green, "\nMore Gamemodes v" + Main.CurrentVersion);
        }
    }
}