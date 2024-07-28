using UnityEngine;
using HarmonyLib;

namespace MoreGamemodes;

// Thanks: https://github.com/0xDrMoe/TownofHost-Enhanced/blob/main/Patches/DleksPatch.cs
[HarmonyPatch(typeof(AmongUsClient._CoStartGameHost_d__32), nameof(AmongUsClient._CoStartGameHost_d__32.MoveNext))]
public static class DleksPatch
{
    private static bool Prefix(AmongUsClient._CoStartGameHost_d__32 __instance, ref bool __result)
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
[HarmonyPatch(typeof(GameStartManager))]
class AllMapIconsPatch
{
    [HarmonyPatch(nameof(GameStartManager.Start)), HarmonyPostfix]
    public static void Postfix_AllMapIcons(GameStartManager __instance)
    {
        if (__instance == null) return;

        __instance.UpdateMapImage(MapNames.Skeld);
        if (!Options.EnableRandomMap.GetBool())
            CreateOptionsPickerPatch.SetDleks = true;

        MapIconByName DleksIncon = Object.Instantiate(__instance, __instance.gameObject.transform).AllMapIcons[0];
        DleksIncon.Name = MapNames.Dleks;
        DleksIncon.MapImage = Utils.LoadSprite("MoreGamemodes.Resources.DleksBanner.png", 100f);
        DleksIncon.NameImage = Utils.LoadSprite("MoreGamemodes.Resources.DleksBanner-Wordart.png", 100f);

        __instance.AllMapIcons.Add(DleksIncon);
    }
}
[HarmonyPatch(typeof(StringOption), nameof(StringOption.Start))]
class AutoSelectDleksPatch
{
    private static void Postfix(StringOption __instance)
    {
        if (__instance.Title == StringNames.GameMapName)
        {
            __instance.Value = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        }
    }
}