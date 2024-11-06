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

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class RoleIntroPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            if (Main.GameStarted) return;
            new LateTask(() => CustomGamemode.Instance.OnShowRole(__instance), 0.0001f, "Show Role");
        }
    }
}