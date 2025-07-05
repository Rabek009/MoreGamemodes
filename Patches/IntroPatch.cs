using HarmonyLib;
using Il2CppSystem.Collections.Generic;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static bool Prefix(IntroCutscene __instance, ref List<PlayerControl> teamToDisplay)
        {
            if (Main.GameStarted) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                if (!PlayerControl.LocalPlayer.GetRole().IsCrewmate())
                {
                    teamToDisplay = new List<PlayerControl>();
                    teamToDisplay.Add(PlayerControl.LocalPlayer);
                    if (PlayerControl.LocalPlayer.GetRole().IsImpostor())
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.AmOwner && pc.GetRole().IsImpostor())
                                teamToDisplay.Add(pc);
                        }
                    }
                    __instance.BeginImpostor(teamToDisplay);
                    __instance.overlayHandle.color = Palette.ImpostorRed;
                    return false;
                }
                return true;
            }
            teamToDisplay = CustomGamemode.Instance.OnBeginCrewmatePrefix(__instance);
            return true;
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
        public static bool Prefix(IntroCutscene __instance, ref List<PlayerControl> yourTeam)
        {
            if (Main.GameStarted) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                if (PlayerControl.LocalPlayer.GetRole().IsCrewmate())
                {
                    yourTeam = new List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.AmOwner && pc.GetRole().IsImpostor())
                            yourTeam.Add(pc);
                    }
                    __instance.BeginCrewmate(yourTeam);
                    __instance.overlayHandle.color = Palette.CrewmateBlue;
                    return false;
                }
                return true;
            }
            yourTeam = CustomGamemode.Instance.OnBeginImpostorPrefix(__instance);
            return true;
        }
        public static void Postfix(IntroCutscene __instance)
        {
            if (Main.GameStarted) return;
            CustomGamemode.Instance.OnBeginImpostorPostfix(__instance);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    class IntroCutsceneCoBeginPatch
    {
        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            var patcher = new CoroutinPatcher(__result);
            patcher.AddPrefix(typeof(IntroCutscene._ShowRole_d__41), () => ShowRolePatch.Postfix(__instance));
            __result = patcher.EnumerateWithPatch();
        }
    }
    class ShowRolePatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            if (Main.GameStarted) return;
            new LateTask(() => CustomGamemode.Instance.OnShowRole(__instance), 0f, "Show Role");
        }
    }
}