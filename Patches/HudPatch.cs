using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerPatch
    {
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) return;
            if (!Main.GameStarted) return;
            CustomGamemode.Instance.OnHudUpate(__instance);
        }
    }

    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
    class SetTaskTextPatch
    {
        public static void Postfix(TaskPanelBehaviour __instance, [HarmonyArgument(0)] string str)
        {
            if (!Main.GameStarted) return;
            CustomGamemode.Instance.OnSetTaskText(__instance, str);
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    class ShowNormalMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            CustomGamemode.Instance.OnShowNormalMap(__instance);
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class ShowSabotageMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            CustomGamemode.Instance.OnShowSabotageMap(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
    class ToggleHighlightPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            CustomGamemode.Instance.OnToggleHighlight(__instance);
        }
    }
}