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
            else if (Options.CurrentGamemode == Gamemodes.Speedrun)
            {
                __instance.TeamTitle.text = "Speedrun";
                __instance.TeamTitle.color = Color.yellow;
                __instance.BackgroundBar.material.color = Color.yellow;
            }
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
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
                __instance.TeamTitle.text = "Battle Royale";
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class RoleIntroPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            new LateTask(() =>
            {
                if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
                {
                    if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Crewmate)
                    {
                        __instance.RoleText.text = "Hider";
                        __instance.RoleBlurbText.text = "Do your tasks and survive";
                    }
                    else if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Impostor)
                    {
                        __instance.RoleText.text = "Seeker";
                        __instance.RoleBlurbText.text = "Kill all hiders";
                    }
                }
                else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
                {
                    if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                    {
                        __instance.RoleText.text = "Shifter";
                        __instance.RoleBlurbText.text = "Shift into your victim";
                    }
                    else
                    {
                        __instance.RoleText.text = "Hider";
                        __instance.RoleBlurbText.text = "Hide in vents and do tasks";
                    }
                }
                else if (Options.CurrentGamemode == Gamemodes.BombTag)
                {
                    if (PlayerControl.LocalPlayer.HasBomb())
                    {
                        __instance.RoleText.text = "You have bomb";
                        __instance.RoleText.color = Color.gray;
                        __instance.RoleBlurbText.text = "Give your bomb away";
                        __instance.RoleBlurbText.color = Color.gray;
                        __instance.YouAreText.color = Color.clear;
                    }
                    else
                    {
                        __instance.RoleText.text = "You haven't bomb";
                        __instance.RoleText.color = Color.green;
                        __instance.RoleBlurbText.text = "Don't get bomb";
                        __instance.RoleBlurbText.color = Color.green;
                        __instance.YouAreText.color = Color.clear;
                    }
                }
                else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
                {
                    __instance.RoleText.text = "Player";
                    __instance.RoleBlurbText.text = "Kill everyone and survive";
                    __instance.YouAreText.color = Color.clear;
                }
                else if (Options.CurrentGamemode == Gamemodes.Speedrun)
                {
                    __instance.RoleText.text = "Speedrunner";
                    __instance.RoleText.color = Color.yellow;
                    __instance.RoleBlurbText.text = "Finish tasks as fast as you can";
                    __instance.RoleBlurbText.color = Color.yellow;
                    __instance.YouAreText.color = Color.yellow;
                }
            }, 0.01f, "Show Role");
        }
    }
}