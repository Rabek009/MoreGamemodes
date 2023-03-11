using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && Options.ImpostorsAreVisible.GetBool()))
            {
                var Team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                Team.Add(PlayerControl.LocalPlayer);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != PlayerControl.LocalPlayer && !pc.Data.Role.IsImpostor)
                        Team.Add(pc);
                }
                teamToDisplay = Team;
            }
            else
            {
                var Team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                Team.Add(PlayerControl.LocalPlayer);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != PlayerControl.LocalPlayer)
                        Team.Add(pc);
                }
                teamToDisplay = Team;
            }
        }
        public static void Postfix(IntroCutscene __instance)
        {
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
                __instance.TeamTitle.text = "Hider";
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class BeginImpostorPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
                __instance.TeamTitle.text = "Seeker";
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                __instance.TeamTitle.text = "Bomb Tag";
                __instance.TeamTitle.color = Color.green;
                __instance.BackgroundBar.material.color = Color.green;
            }
        }
    }
    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__35), nameof(IntroCutscene._ShowRole_d__35.MoveNext))]
    class RoleIntroPatch
    {
        public static void Postfix(IntroCutscene._ShowRole_d__35 __instance)
        {
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
            {
                if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Crewmate)
                {
                    __instance.__4__this.RoleText.text = "Hider";
                    __instance.__4__this.RoleBlurbText.text = "Do your tasks and survive";
                }
                else if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Impostor)
                {
                    __instance.__4__this.RoleText.text = "Seeker";
                    __instance.__4__this.RoleBlurbText.text = "Kill all hiders";
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    __instance.__4__this.RoleText.text = "Shifter";
                    __instance.__4__this.RoleBlurbText.text = "Shift into your victim";
                }
                else
                {
                    __instance.__4__this.RoleText.text = "Hider";
                    __instance.__4__this.RoleBlurbText.text = "Hide in vents and do tasks";
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                if (PlayerControl.LocalPlayer.HasBomb())
                {
                    __instance.__4__this.RoleText.text = "You have bomb";
                    __instance.__4__this.RoleText.color = Color.gray;
                    __instance.__4__this.RoleBlurbText.text = "Give your bomb away";
                    __instance.__4__this.RoleBlurbText.color = Color.gray;
                    __instance.__4__this.YouAreText.color = Color.clear;
                }
                else
                {
                    __instance.__4__this.RoleText.text = "You haven't bomb";
                    __instance.__4__this.RoleText.color = Color.green;
                    __instance.__4__this.RoleBlurbText.text = "Don't get bomb";
                    __instance.__4__this.RoleBlurbText.color = Color.green;
                    __instance.__4__this.YouAreText.color = Color.clear;
                }
            }
        }
    }
}