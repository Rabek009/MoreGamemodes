using HarmonyLib;

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
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    if (ar != PlayerControl.LocalPlayer)
                        pc.RpcSetNamePrivate(Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)], ar, true);
                }         
            }
            if (Main.CamouflageTimer > 1f)
            {
                PlayerControl.LocalPlayer.RpcSetColor((byte)Main.StandardColors[PlayerControl.LocalPlayer.PlayerId]);
                PlayerControl.LocalPlayer.RpcSetName(Main.StandardNames[PlayerControl.LocalPlayer.PlayerId]);
                PlayerControl.LocalPlayer.RpcSetHat(Main.StandardHats[PlayerControl.LocalPlayer.PlayerId]);
                PlayerControl.LocalPlayer.RpcSetSkin(Main.StandardSkins[PlayerControl.LocalPlayer.PlayerId]);
                PlayerControl.LocalPlayer.RpcSetPet("pet_clank");
                PlayerControl.LocalPlayer.RpcSetVisor(Main.StandardVisors[PlayerControl.LocalPlayer.PlayerId]);
            }
            Main.IsMeeting = true;
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
            Main.IsMeeting = false;
        }
    }
}
