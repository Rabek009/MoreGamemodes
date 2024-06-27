using HarmonyLib;
using UnityEngine;
using TMPro;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingShowerPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.alignment = TextAlignmentOptions.Right;
            var offset_x = 2.5f;
            var offset_y = 5.8f;
            if (AmongUsClient.Instance.IsGameStarted)
            {
                if (DestroyableSingleton<HudManager>.Instance && !HudManager.Instance.Chat.isActiveAndEnabled)
                    offset_x += 0.7f;
                else
                    offset_x += 0.1f;
                __instance.aspectPosition.DistanceFromEdge = new Vector3(offset_x, offset_y, 0f);
            }
            else
                __instance.aspectPosition.DistanceFromEdge = new Vector3(offset_x, offset_y, 0f);
            __instance.text.text += Utils.ColorString(Color.green, "\nMore Gamemodes v" + Main.CurrentVersion);
        }
    }
}