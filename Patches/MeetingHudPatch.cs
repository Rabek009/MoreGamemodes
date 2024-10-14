using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
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
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
    class MeetingHudOnDestroyPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AntiBlackout.SetIsDead();
            if (RandomItemsGamemode.instance != null)
                RandomItemsGamemode.instance.CamouflageTimer = -1f;
            if (Options.EnableRandomSpawn.GetBool() && Options.TeleportAfterMeeting.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    class VotingCompletePatch
    {
        public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MeetingHud.VoterState[] states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var netObject in CustomNetObject.CustomObjects)
                netObject.RpcTeleport(netObject.Position);
            CustomGamemode.Instance.OnVotingComplete(__instance, states, exiled, tie);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
    class CastVotePatch
    {
        public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId, [HarmonyArgument(1)] byte suspectPlayerId)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            bool canceled = !CustomGamemode.Instance.OnCastVote(__instance, srcPlayerId, suspectPlayerId);
            if (canceled)
            {
                var voter = Utils.GetPlayerById(srcPlayerId);
                __instance.RpcClearVote(voter.GetClientId());
            }
            return !canceled;
        }
    }
}
