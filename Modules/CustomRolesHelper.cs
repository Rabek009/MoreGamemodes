using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    static class CustomRolesHelper
    {
        public static Dictionary<string, CustomRoles> RoleNames = new() 
        {
            { "crewmate", CustomRoles.Crewmate },
            { "scientist", CustomRoles.Scientist },
            { "engineer", CustomRoles.Engineer },
            { "noisemaker", CustomRoles.Noisemaker },
            { "tracker", CustomRoles.Tracker },
            { "investigator", CustomRoles.Investigator },
            { "sheriff", CustomRoles.Sheriff },
            { "immortal", CustomRoles.Immortal },
            { "securityguard", CustomRoles.SecurityGuard },
            { "impostor", CustomRoles.Impostor },
            { "shapeshifter", CustomRoles.Shapeshifter },
            { "phantom", CustomRoles.Phantom },
            { "timefreezer", CustomRoles.TimeFreezer },
            { "evilguesser", CustomRoles.EvilGuesser },
            { "trapster", CustomRoles.Trapster},
            { "opportunist", CustomRoles.Opportunist },
            { "jester", CustomRoles.Jester },
            { "serialkiller", CustomRoles.SerialKiller}
        };

        public static void SetCustomRole(this PlayerControl player, CustomRoles role)
        {
            if (ClassicGamemode.instance == null) return;
            ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = null;
            switch (role)
            {
                case CustomRoles.Crewmate:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Crewmate(player);
                    break;
                case CustomRoles.Scientist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Scientist(player);
                    break;
                case CustomRoles.Engineer:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Engineer(player);
                    break;
                case CustomRoles.Noisemaker:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Noisemaker(player);
                    break;
                case CustomRoles.Tracker:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Tracker(player);
                    break;
                case CustomRoles.Investigator:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Investigator(player);
                    break;
                case CustomRoles.Sheriff:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Sheriff(player);
                    break;
                case CustomRoles.Immortal:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Immortal(player);
                    break;
                case CustomRoles.SecurityGuard:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SecurityGuard(player);
                    break;
                case CustomRoles.Impostor:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Impostor(player);
                    break;
                case CustomRoles.Shapeshifter:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Shapeshifter(player);
                    break;
                case CustomRoles.Phantom:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Phantom(player);
                    break;
                case CustomRoles.TimeFreezer:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new TimeFreezer(player);
                    break;
                case CustomRoles.EvilGuesser:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new EvilGuesser(player);
                    break;
                case CustomRoles.Trapster:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Trapster(player);
                    break;
                case CustomRoles.Jester:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Jester(player);
                    break;
                case CustomRoles.Opportunist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Opportunist(player);
                    break;
                case CustomRoles.SerialKiller:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SerialKiller(player);
                    break;
            }
            if (player.GetRole().ForceKillButton())
                player.Data.Role.CanUseKillButton = true;
            if (AmongUsClient.Instance.AmHost)
            {
                if (player.GetRole().IsNeutral())
                    Main.NameColors[(player.PlayerId, player.PlayerId)] = player.GetRole().Color;
                else if (player.GetRole().IsImpostor() && !player.GetRole().CanUseKillButton())
                    Main.NameColors[(player.PlayerId, player.PlayerId)] = Palette.ImpostorRed;
                else if (player.GetRole().IsCrewmate() && player.GetRole().CanUseKillButton())
                    Main.NameColors[(player.PlayerId, player.PlayerId)] = Color.white;
            }
        }

        public static bool IsCrewmate(CustomRoles role)
        {
            return !IsImpostor(role) && !IsNeutral(role);
        }

        public static bool IsImpostor(CustomRoles role)
        {
            return role is CustomRoles.Impostor or
            CustomRoles.Shapeshifter or
            CustomRoles.Phantom or
            CustomRoles.TimeFreezer or
            CustomRoles.EvilGuesser or
            CustomRoles.Trapster;
        }

        public static bool IsNeutral(CustomRoles role)
        {
            return IsNeutralBenign(role) || IsNeutralEvil(role) || IsNeutralKilling(role);
        }

        public static bool IsNeutralBenign(CustomRoles role)
        {
            return role is CustomRoles.Opportunist;
        }

        public static bool IsNeutralEvil(CustomRoles role)
        {
            return role is CustomRoles.Jester;
        }

        public static bool IsNeutralKilling(CustomRoles role)
        {
            return role is CustomRoles.SerialKiller;
        }

        public static bool IsCrewmateKilling(CustomRoles role)
        {
            return role is CustomRoles.Sheriff;
        }

        public static bool IsVanilla(CustomRoles role)
        {
            return role is CustomRoles.Crewmate or
            CustomRoles.Scientist or
            CustomRoles.Engineer or
            CustomRoles.Noisemaker or
            CustomRoles.Tracker or
            CustomRoles.Impostor or
            CustomRoles.Shapeshifter or
            CustomRoles.Phantom;
        }

        public static int GetRoleChance(CustomRoles role)
        {
            if (role is CustomRoles.Crewmate or CustomRoles.Impostor) return 0;
            if (IsVanilla(role))
            {
                switch (role)
                {
                    case CustomRoles.Scientist:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Scientist);
                    case CustomRoles.Engineer:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Engineer);
                    case CustomRoles.Noisemaker:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Noisemaker);
                    case CustomRoles.Tracker:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Tracker);
                    case CustomRoles.Shapeshifter:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter);
                    case CustomRoles.Phantom:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(RoleTypes.Phantom);
                    default:
                        return 0;
                }
            }
            return Options.RolesChance.ContainsKey(role) ? Options.RolesChance[role].GetInt() : 0;
        }

        public static int GetRoleCount(CustomRoles role)
        {
            if (role is CustomRoles.Crewmate or CustomRoles.Impostor) return 0;
            if (IsVanilla(role))
            {
                switch (role)
                {
                    case CustomRoles.Scientist:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Scientist);
                    case CustomRoles.Engineer:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Engineer);
                    case CustomRoles.Noisemaker:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Noisemaker);
                    case CustomRoles.Tracker:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Tracker);
                    case CustomRoles.Shapeshifter:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter);
                    case CustomRoles.Phantom:
                        return GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(RoleTypes.Phantom);
                    default:
                        return 0;
                }
            }
            return Options.RolesCount.ContainsKey(role) ? Options.RolesCount[role].GetInt() : 0;
        }

        public static bool IsNeutralKillerEnabled()
        {
            foreach (var role in Enum.GetValues<CustomRoles>())
            {
                if (GetRoleChance(role) > 0 && IsNeutralKilling(role))
                    return true;
            }
            return false;
        }
    }
}

public enum CustomRoles
{
    // Crewmate
    Crewmate,
    Scientist,
    Engineer,
    Noisemaker,
    Tracker,
    Investigator,
    Sheriff,
    Immortal,
    SecurityGuard,
    // Impostor
    Impostor,
    Shapeshifter,
    Phantom,
    TimeFreezer,
    EvilGuesser,
    Trapster,
    //Neutral
    Opportunist,
    Jester,
    SerialKiller,
}

public enum CustomWinners
{
    None,
    Terminated,
    NoOne,
    Crewmates,
    Impostors,
    Jester,
    SerialKiller,
}

public enum AdditionalWinners
{
    Opportunist,
}