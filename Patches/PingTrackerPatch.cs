using HarmonyLib;
using UnityEngine;
using TMPro;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingShowerPatch
    {
        private static float FPSUpdateDelay = 0f;
        private static int FPSGame = 60;
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.alignment = TextAlignmentOptions.Right;
            var settingButtonTransformPosition = DestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition;
            var offset_x = settingButtonTransformPosition.x - 1.58f;
            var offset_y = settingButtonTransformPosition.y + 3.2f;
            if (AmongUsClient.Instance.IsGameStarted)
            {
                if (DestroyableSingleton<HudManager>.Instance && !HudManager.Instance.Chat.isActiveAndEnabled)
                {
                    offset_x += 0.7f;
                }
                else
                {
                    offset_x += 0.1f;
                }
                __instance.aspectPosition.DistanceFromEdge = new Vector3(offset_x, offset_y, 0f);
            }
            else
            {
                __instance.aspectPosition.DistanceFromEdge = new Vector3(offset_x, offset_y, 0f);
            }
            if (Main.ShowFPS.Value)
            {
                FPSUpdateDelay += Time.deltaTime;
                if (FPSUpdateDelay > 1f)
                {
                    FPSGame = (int)(1.0f / Time.deltaTime);
                    FPSUpdateDelay -= 1f;
                }
                __instance.text.text += Utils.ColorString(Color.yellow, "\nFPS: " + FPSGame);
            }
            __instance.text.text += Utils.ColorString(Color.green, "\nMore Gamemodes v" + Main.CurrentVersion);
        }
    }
}