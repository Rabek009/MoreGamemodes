using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static void Prefix(MeetingHud __instance)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)], ar, true);
            }
        }
        public static void Postfix(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                HudManager.Instance.Chat.SetVisible(true);
                if (!PlayerControl.LocalPlayer.Data.IsDead)
                    MeetingHud.Instance.SkipVoteButton.SetEnabled();
                return;
            }

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
            if (!AmongUsClient.Instance.AmHost)
            {
                MeetingHud.Instance.SkipVoteButton.SetDisabled();
                HudManager.Instance.Chat.SetVisible(PlayerControl.LocalPlayer.Data.IsDead);
                return;
            }
            if (Main.CamouflageTimer > 0f)
            {
                Main.CamouflageTimer = 0f;
                Utils.RevertCamouflage();
                PlayerControl.LocalPlayer.ReportDeadBody(PlayerControl.LocalPlayer.Data);
                MeetingHud.Instance.RpcClose();
            }
            if (Options.RandomSpawn.GetBool() && Options.TeleportAfterMeeting.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
            Main.IsMeeting = false;
        }
    }
}
