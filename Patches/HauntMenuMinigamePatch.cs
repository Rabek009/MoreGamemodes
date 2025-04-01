using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
    public static class HauntMenuMinigameSetFilterTextPatch
    {
        public static void Postfix(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget == null) return;
            CustomGamemode.Instance.OnSetFilterText(__instance);
        }
    }
}