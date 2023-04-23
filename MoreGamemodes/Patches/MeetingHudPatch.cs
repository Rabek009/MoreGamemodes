using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static void Prefix(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.RpcSetOutfit(Main.StandardColors[pc.PlayerId], Main.StandardNames[pc.PlayerId], Main.StandardHats[pc.PlayerId], Main.StandardSkins[pc.PlayerId], Main.StandardPets[pc.PlayerId], Main.StandardVisors[pc.PlayerId]);
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    pc.RpcSetNamePrivate(Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)], ar, true);
                }    
            }
        }
        public static void Postfix(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            foreach (var pva in __instance.playerStates)
            {
                if (pva == null) continue;
                PlayerControl seer = PlayerControl.LocalPlayer;
                PlayerControl target = Utils.GetPlayerById(pva.TargetPlayerId);
                if (target == null) continue;

                pva.NameText.text = Main.LastNotifyNames[(target.PlayerId, seer.PlayerId)];
            }      
            Main.IsMeeting = true;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
    class MeetingHudOnDestroyPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Main.SkipMeeting = !Main.SkipMeeting;
            if (Main.SkipMeeting)
            {
                PlayerControl.LocalPlayer.ReportDeadBody(PlayerControl.LocalPlayer.Data);
                MeetingHud.Instance.RpcClose();
            }
            Main.CamouflageTimer = 0f;
            if (Options.RandomSpawn.GetBool() && Options.TeleportAfterMeeting.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
            Main.IsMeeting = false;
        }
    }
}
