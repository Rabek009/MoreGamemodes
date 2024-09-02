using HarmonyLib;

namespace MoreGamemodes
{
    class ExileControllerWrapUpPatch
    {
        public static NetworkedPlayerInfo AntiBlackout_LastExiled;

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                try
                {
                    WrapUpPostfix(__instance.initData.networkedPlayer);
                }
                finally
                {
                    WrapUpFinalizer(__instance.initData.networkedPlayer);
                }
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                try
                {
                    WrapUpPostfix(__instance.initData.networkedPlayer);
                }
                finally
                {
                    WrapUpFinalizer(__instance.initData.networkedPlayer);
                }
            }
        }

        static void WrapUpPostfix(NetworkedPlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (AntiBlackout.OverrideExiledPlayer)
            {
                exiled = AntiBlackout_LastExiled;
            }
            AntiBlackout.RestoreIsDead(doSend: false);
            CustomGamemode.Instance.OnExile(exiled);
            if (exiled != null)
                exiled.IsDead = true;
                
            if (exiled == null || exiled.Object == null) return;
            if (exiled.Object.GetDeathReason() == DeathReasons.Alive)
                exiled.Object.RpcSetDeathReason(DeathReasons.Exiled);
        }

        static void WrapUpFinalizer(NetworkedPlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            new LateTask(() =>
            {
                exiled = AntiBlackout_LastExiled;
                Utils.SendGameData();
                if (AntiBlackout.OverrideExiledPlayer &&
                    exiled != null &&
                    exiled.Object != null)
                {
                    exiled.Object.RpcExileV2();
                }
            }, 0.5f, "Restore IsDead Task");
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

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    class ReEnableGameplayPatch
    {
        public static void Postfix()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                Main.KillCooldowns[pc.PlayerId] = Main.OptionKillCooldowns[pc.PlayerId];
        }
    }
}