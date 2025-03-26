using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldFlipSkeld))]
    class ShouldFlipSkeldPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = GameOptionsManager.Instance.CurrentGameOptions.MapId == 3;
            return false;
        }
    }
}