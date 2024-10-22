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
            { "serialkiller", CustomRoles.SerialKiller},
        };

        public static Dictionary<CustomRoles, string> RoleDescriptions = new() 
        {
            {CustomRoles.Crewmate, "Crewmate (Crewmate): Regular crewmate without any ability."},
            {CustomRoles.Scientist, "Scientist (Crewmate): You can use vitals from anywhere, but you have limited battery. Complete task to recharge your battery. When you complete all tasks, your battery will recharge automatically."},
            {CustomRoles.Engineer, "Engineer (Crewmate): You can vent like impostor, but you have venting cooldown and you can stay in vent for limited time."},
            {CustomRoles.Noisemaker, "Noisemaker (Crewmate): When you get killed, you send alert. That alert informs crewmates that you died and shows direction to your body. Depending on options killers get alert too."},
            {CustomRoles.Tracker, "Tracker (Crewmate): You can track other player too see that player on your map. Player position updates every few seconds. You can track player for limited amount of time. After tracking cooldown is over, you can track another player."},
            {CustomRoles.Investigator, "Investigator (Crewmate): You can use pet button to switch between task and investigate mode. In task mode you can do tasks. In investigate mode you have kill button. If players is good, his name will become green. But if player is evil then his name will turn red. But some roles that are good can show as evil, also sometimes evil roles show as good. You have limited ability uses, but you can do tasks to increase it.\n\nIf you have mod installed, you don't have task and investigate mode. You can do tasks and investigate at the same time."},
            {CustomRoles.Sheriff, "Sheriff (Crewmate): You can use pet button to switch between task and kill mode. In task mode you can do tasks. In kill mode you have kill button. You can kill impostors and depending on options neutrals. If you try to kill someone you can't, you die from misfire. Depending on options your target dies on misfire too.\n\nIf you have mod installed, you don't have task and kill mode. You can do tasks and kill at the same time."},
            {CustomRoles.Immortal, "Immortal (Crewmate): After completing all tasks you can survive few kill attempts. In addition after you complete task, you get temporarily protection. If impostor try to kill you, his cooldown will reset to 50%. You will know that someone tried to kill you when meeting is called. After completing all tasks you can't be guessed."},
            {CustomRoles.SecurityGuard, "Security Guard (Crewmate): You can use pet button near vent to block it pernamently. Blocked vent can't be used by anyone. When you're looking at cameras/doorlog/binoculars you get alerted when someone die. Depending on options cameras don't blink when you're using it."},
            {CustomRoles.Impostor, "Impostor (Impostor): Regular impostor without any ability."},
            {CustomRoles.Shapeshifter, "Shapeshifter (Impostor): You can shapeshift into other players. You can stay in shapeshifted form for limited time. When shapeshifting there is animation and depending on options you leave shapeshift evidence."},
            {CustomRoles.Phantom, "Phantom (Impostor): You can turn invisible for limited amount of time. When disappearing or appearing there is animation. While invisible, you can't kill, vent, repair sabotages, use platform and zipline. Other impostors can see you, when you're invisible. While invisible you're 10% faster."},
            {CustomRoles.TimeFreezer, "Time Freezer (Impostor): Click vanish button to stop time for short period of time. When time is frozen other players can't move and are blind. They can't use their abilities, kill, report bodies, call meeting, sabotage, vent. You can kill during freeze, just make sure no one realize that you moved."},
            {CustomRoles.EvilGuesser, "Evil Guesser (Impostor): You can guess roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 2 is sheriff, you should type <i>/guess 2 sheriff</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead. During rounds you can kill like regular impostor."},
            {CustomRoles.Trapster, "Trapster (Impostor): After killing someone, you place trap on dead body. Next player, who tries to report that body (or interact with it in any way) will be trapped on it unable to move and use abilities. Dead body can trap only 1 person at the time. If someone gets trapped on your body, your kill cooldown will decrease and you will get alerted."},
            {CustomRoles.Opportunist, "Opportunist (Neutral): Survive to the end to win with winning team. If you die, you lose."},
            {CustomRoles.Jester, "Jester (Neutral): Get voted out to win alone. Act suspicious to make people think you're impostor."},
            {CustomRoles.SerialKiller, "Serial Killer (Neutral): Your goal is to kill everyone. You have lower kill cooldown, so you can kill faster than impostors. You can vent and have impostor vision."},
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