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
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
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