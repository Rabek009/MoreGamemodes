using HarmonyLib;

namespace MoreGamemodes
{
    class ExileControllerWrapUpPatch
    {
        public static NetworkedPlayerInfo AntiBlackout_LastExiled;

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class WrapUpPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.Animate))]
        class AirshipExileControllerAnimatePatch
        {
            public static void Postfix(AirshipExileController __instance, ref Il2CppSystem.Collections.IEnumerator __result)
            {
                var patcher = new CoroutinPatcher(__result);
                patcher.AddPrefix(typeof(AirshipExileController._WrapUpAndSpawn_d__11), () =>
                    WrapUpAndSpawnPatch.Postfix(__instance)
                );
                __result = patcher.EnumerateWithPatch();
            }
        }
        class WrapUpAndSpawnPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        static void WrapUpPostfix(NetworkedPlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (exiled != null && exiled.Object != null && exiled.Object.GetDeathReason() == DeathReasons.Alive)
                exiled.Object.RpcSetDeathReason(DeathReasons.Exiled);
            if (AntiBlackout.ShowDoubleAnimation && exiled != null)
            {
                GameManager.Instance.ShouldCheckForGameEnd = false;
                AntiBlackout_LastExiled = exiled;
                Utils.ShowExileAnimation();
                return;
            }
            if (AntiBlackout.ShowDoubleAnimation && exiled == null && AntiBlackout_LastExiled != null)
            {
                GameManager.Instance.ShouldCheckForGameEnd = true;
                exiled = AntiBlackout_LastExiled;
                AntiBlackout_LastExiled = null;
            }
            new LateTask(() => {
                if (!GameManager.Instance.ShouldCheckForGameEnd) return;
                Utils.SendGameData();
                AntiBlackout.IsCached = false;
            }, 0.7f);
            new LateTask(() => {
                if (!GameManager.Instance.ShouldCheckForGameEnd) return;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.IsDead)
                        pc.SetChatVisible(true);
                }
                if (Options.EnableMidGameChat.GetBool())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                            pc.SetChatVisible(true);
                    }
                }
            }, 0.9f);
            CustomGamemode.Instance.OnExile(exiled);
        }
    }

    [HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
    class PolusExileHatFixPatch
    {
        public static void Prefix(PbExileController __instance)
        {
            __instance.Player.cosmetics.hat.transform.localPosition = new(-0.2f, 0.6f, 1.1f);
        }
    }
}