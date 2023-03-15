using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    class EndGamePatch
    {
        public static void Postfix()
        {
            Main.GameStarted = false;
            if (!AmongUsClient.Instance.AmHost) return;
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.Impostors = new List<byte>();
            Main.StandardRoles = new Dictionary<byte, RoleTypes>();
            Main.HasBomb = new Dictionary<byte, bool>();
            Main.AllPlayersItems = new Dictionary<byte, Items>();
            Main.CanGameEnd = true;
            Main.IsMeeting = false;
            Main.Timer = 0f;
            Main.FlashTimer = 0f;
            Main.HackTimer = 0f;
            Main.CamouflageTimer = 0f;
            Main.AllKills = new Dictionary<byte, byte>();
            Main.ShieldTimer = new Dictionary<byte, float>();
            GameOptionsManager.Instance.CurrentGameOptions = Main.RealOptions.DeepCopy();
            Main.RealOptions = null;
            Main.Lives = new Dictionary<byte, int>();
        }
    }
}