using HarmonyLib;

namespace MoreGamemodes
{
    class ExileControllerWrapUpPatch
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
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
            if (exiled != null)
                exiled.IsDead = true;
            if (AntiBlackout.ShowDoubleAnimation && exiled != null)
            {
                Utils.ShowExileAnimation();
                return;
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
                    if (pc.Data != null && pc.Data.IsDead && !pc.Data.Disconnected)
                        pc.SetChatVisible(true);
                }
                if (Options.EnableMidGameChat.GetBool())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data != null && !pc.Data.IsDead && !pc.Data.Disconnected)
                            pc.SetChatVisible(true);
                    }
                }
            }, 0.9f);
            CustomGamemode.Instance.OnExile(exiled);
            
            if (exiled == null || exiled.Object == null) return;
            if (exiled.Object.GetDeathReason() == DeathReasons.Alive)
                exiled.Object.RpcSetDeathReason(DeathReasons.Exiled);
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