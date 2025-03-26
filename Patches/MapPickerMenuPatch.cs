using System.Linq;
using HarmonyLib;
using Il2CppSystem;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.Initialize))]
    class GameOptionsMapPickerInitializePatch
    {
        public static void Prefix(GameOptionsMapPicker __instance)
        {
            MapIconByName dleksMap = new()
            {
                Name = MapNames.Dleks,
                MapIcon = __instance.AllMapIcons[0].MapIcon,
                MapImage = __instance.AllMapIcons[0].MapImage,
                NameImage = __instance.AllMapIcons[0].NameImage,
            };
            __instance.AllMapIcons.Insert(3, dleksMap);
        }
        public static void Postfix(GameOptionsMapPicker __instance)
        {
            for (int i = 0; i < __instance.mapButtons[3].MapIcon.Count; ++i)
                __instance.mapButtons[3].MapIcon[i].flipX = true;
        }
    }

    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SelectMap), typeof(int))]
    class IntSelectMapPatch
    {
        public static void Postfix(GameOptionsMapPicker __instance, [HarmonyArgument(0)] int mapId)
        {
		    __instance.MapName.flipX = mapId == 3;
        }
    }

    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SelectMap), typeof(MapIconByName))]
    class MapIconByNameSelectMapPatch
    {
        public static void Postfix(GameOptionsMapPicker __instance, [HarmonyArgument(0)] MapIconByName mapInfo)
        {
			__instance.MapName.flipX = mapInfo.Name == MapNames.Dleks;
        }
    }
}