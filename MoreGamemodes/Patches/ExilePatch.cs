using HarmonyLib;
using System.Collections.Generic;

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
            if (Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                exiled.Object.RpcSetItem(Items.None);
                Main.NoBombTimer = 11f;
            }      
            if (exiled.Object.GetDeathReason() == DeathReasons.Alive)
                exiled.Object.RpcSetDeathReason(DeathReasons.Exiled);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.RpcResetAbilityCooldown();
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)], ar, true);
            }
        }
    }
}