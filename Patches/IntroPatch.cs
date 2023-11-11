using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            if (Main.GameStarted) return;
            CustomGamemode.Instance.OnBeginCrewmatePrefix(__instance);
        }
        public static void Postfix(IntroCutscene __instance)
        {
            if (Main.GameStarted) return;
            CustomGamemode.Instance.OnBeginCrewmatePostfix(__instance);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class BeginImpostorPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            if (Main.GameStarted) return;
            CustomGamemode.Instance.OnBeginImpostor(__instance);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class RoleIntroPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            if (Main.GameStarted) return;
            new LateTask(() => CustomGamemode.Instance.OnShowRole(__instance), 0.01f, "Show Role");
        }
    }
}