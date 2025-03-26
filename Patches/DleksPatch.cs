using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/SupportAU_2025.3.25/Patches/DleksPatch.cs
    [HarmonyPatch(typeof(AmongUsClient._CoStartGameHost_d__37), nameof(AmongUsClient._CoStartGameHost_d__37.MoveNext))]
    public static class DleksPatch
    {
        private static bool Prefix(AmongUsClient._CoStartGameHost_d__37 __instance, ref bool __result)
        {
            if (__instance.__1__state != 0)
            {
                return true;
            }
            __instance.__1__state = -1;
            if (LobbyBehaviour.Instance)
            {
                LobbyBehaviour.Instance.Despawn();
            }
            if (ShipStatus.Instance)
            {
                __instance.__2__current = null;
                __instance.__1__state = 2;
                __result = true;
                return false;
            }

            var num2 = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.MapId, 0, Constants.MapNames.Length - 1);
            __instance.__2__current = __instance.__4__this.ShipLoadingAsyncHandle = __instance.__4__this.ShipPrefabs[num2].InstantiateAsync();
            __instance.__1__state = 1;
            __result = true;
            return false;
    }
    }
}