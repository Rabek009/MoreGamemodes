using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient._CoStartGameHost_d__32), nameof(AmongUsClient._CoStartGameHost_d__32.MoveNext))]
    public static class DleksPatch
    {
        private static bool Prefix(AmongUsClient._CoStartGameHost_d__32 __instance, ref bool __result)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
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

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    class AllMapIconsPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            MapIconByName DleksIncon = Object.Instantiate(__instance, __instance.gameObject.transform).AllMapIcons[0];
            DleksIncon.Name = MapNames.Dleks;
            __instance.AllMapIcons.Add(DleksIncon);
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Start))]
    class AutoSelectDleksPatch
    {
        public static void Postfix(StringOption __instance)
        {
            if (__instance.Title == StringNames.GameMapName)
            {
                __instance.Value = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            }
        }
    }
}