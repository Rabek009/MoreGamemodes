using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    class SetEverythingUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            Main.GameStarted = false;
            if (!AmongUsClient.Instance.AmHost) return;
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.Impostors = new List<byte>();
            Main.HasBomb = new Dictionary<byte, bool>();
            Main.AllPlayersItems = new Dictionary<byte, Items>();
            Main.IsMeeting = false;    
            Main.FlashTimer = 0f;
            Main.HackTimer = 0f;
            Main.CamouflageTimer = 0f;
            Main.ShieldTimer = new Dictionary<byte, float>();
            GameOptionsManager.Instance.CurrentGameOptions = Main.RealOptions;
            Main.Lives = new Dictionary<byte, int>();
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.NoBombTimer = 0f;
            Main.SkipMeeting = false;
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