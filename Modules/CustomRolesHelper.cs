using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    static class CustomRolesHelper
    {
        public static Dictionary<string, CustomRoles> CommandRoleNames = new() 
        {
            {"crewmate", CustomRoles.Crewmate},
            {"scientist", CustomRoles.Scientist},
            {"engineer", CustomRoles.Engineer},
            {"noisemaker", CustomRoles.Noisemaker},
            {"tracker", CustomRoles.Tracker},
            {"investigator", CustomRoles.Investigator},
            {"mortician", CustomRoles.Mortician},
            {"sheriff", CustomRoles.Sheriff},
            {"niceguesser", CustomRoles.NiceGuesser},
            {"immortal", CustomRoles.Immortal},
            {"medic", CustomRoles.Medic},
            {"securityguard", CustomRoles.SecurityGuard},
            {"mutant", CustomRoles.Mutant},
            {"impostor", CustomRoles.Impostor},
            {"shapeshifter", CustomRoles.Shapeshifter},
            {"phantom", CustomRoles.Phantom},
            {"timefreezer", CustomRoles.TimeFreezer},
            {"escapist", CustomRoles.Escapist},
            {"evilguesser", CustomRoles.EvilGuesser},
            {"hitman", CustomRoles.Hitman},
            {"trapster", CustomRoles.Trapster},
            {"parasite", CustomRoles.Parasite},
            {"opportunist", CustomRoles.Opportunist},
            {"amnesiac", CustomRoles.Amnesiac},
            {"jester", CustomRoles.Jester},
            {"executioner", CustomRoles.Executioner},
            {"serialkiller", CustomRoles.SerialKiller},
            {"pelican", CustomRoles.Pelican},
        };

        public static Dictionary<CustomRoles, string> RoleNames = new() 
        {
            {CustomRoles.Crewmate, "Crewmate"},
            {CustomRoles.Scientist, "Scientist"},
            {CustomRoles.Engineer, "Engineer"},
            {CustomRoles.Noisemaker, "Noisemaker"},
            {CustomRoles.Tracker, "Tracker"},
            {CustomRoles.Investigator, "Investigator"},
            {CustomRoles.Mortician, "Mortician"},
            {CustomRoles.Sheriff, "Sheriff"},
            {CustomRoles.NiceGuesser, "Nice Guesser"},
            {CustomRoles.Immortal, "Immortal"},
            {CustomRoles.Medic, "Medic"},
            {CustomRoles.SecurityGuard, "Security Guard"},
            {CustomRoles.Mutant, "Mutant"},
            {CustomRoles.Impostor, "Impostor"},
            {CustomRoles.Shapeshifter, "Shapeshifter"},
            {CustomRoles.Phantom, "Phantom"},
            {CustomRoles.TimeFreezer, "Time Freezer"},
            {CustomRoles.Escapist, "Escapist"},
            {CustomRoles.EvilGuesser, "Evil Guesser"},
            {CustomRoles.Hitman, "Hitman"},
            {CustomRoles.Trapster, "Trapster"},
            {CustomRoles.Parasite, "Parasite"},
            {CustomRoles.Opportunist, "Opportunist"},
            {CustomRoles.Amnesiac, "Amnesiac"},
            {CustomRoles.Jester, "Jester"},
            {CustomRoles.Executioner, "Executioner"},
            {CustomRoles.SerialKiller, "Serial Killer"},
            {CustomRoles.Pelican, "Pelican"},
        };

        public static Dictionary<CustomRoles, string> RoleDescriptions = new() 
        {
            {CustomRoles.Crewmate, "Do your tasks"},
            {CustomRoles.Scientist, "Access vitals at any time"},
            {CustomRoles.Engineer, "Can use the vents"},
            {CustomRoles.Noisemaker, "Send out an alert when killed"},
            {CustomRoles.Tracker, "Track a crewmate with your map"},
            {CustomRoles.Investigator, "See if someone is good or evil"},
            {CustomRoles.Mortician, "Get info about bodies"},
            {CustomRoles.Sheriff, "Shoot impostors"},
            {CustomRoles.NiceGuesser, "Guess evil roles during meeting"},
            {CustomRoles.Immortal, "Complete tasks to get protection"},
            {CustomRoles.Medic, "Give shield to crewmate"},
            {CustomRoles.SecurityGuard, "Block vents and gain extra info from security"},
            {CustomRoles.Mutant, "Fix sabotages from anywhere"},
            {CustomRoles.Impostor, "Kill and sabotage"},
            {CustomRoles.Shapeshifter, "Disguise yourself"},
            {CustomRoles.Phantom, "Turn invisible"},
            {CustomRoles.TimeFreezer, "Freeze time to make sneaky kills"},
            {CustomRoles.Escapist, "Mark location and teleport to it"},
            {CustomRoles.EvilGuesser, "Guess roles during meeting"},
            {CustomRoles.Hitman, "Kill your targets with no cooldown"},
            {CustomRoles.Trapster, "Trap players on dead bodies"},
            {CustomRoles.Parasite, "Sacrifice to make someone impostor"},
            {CustomRoles.Opportunist, "Survive to win"},
            {CustomRoles.Amnesiac, "Remember role from dead body"},
            {CustomRoles.Jester, "Get voted out"},
            {CustomRoles.Executioner, "Vote your target out"},
            {CustomRoles.SerialKiller, "Kill everyone"},
            {CustomRoles.Pelican, "Eat every player"},
        };

        public static Dictionary<CustomRoles, string> RoleDescriptionsLong = new() 
        {
            {CustomRoles.Crewmate, "Crewmate (Crewmate): Regular crewmate without any ability.<size=0>"},
            {CustomRoles.Scientist, "Scientist (Crewmate): You can use vitals from anywhere, but you have limited battery. Complete task to recharge your battery. When you complete all tasks, your battery will recharge automatically."},
            {CustomRoles.Engineer, "Engineer (Crewmate): You can vent like impostor, but you have venting cooldown and you can stay in vent for limited time."},
            {CustomRoles.Noisemaker, "Noisemaker (Crewmate): When you get killed, you send alert. That alert informs crewmates that you died and shows direction to your body. Depending on options killers get alert too."},
            {CustomRoles.Tracker, "Tracker (Crewmate): You can track other player too see that player on your map. Player position updates every few seconds. You can track player for limited amount of time. After tracking cooldown is over, you can track another player."},
            {CustomRoles.Investigator, "Investigator (Crewmate): You can use pet button to switch between task and investigate mode. In task mode you can do tasks. In investigate mode you have kill button. Use kill button to investigate player. If players is good, his name will become green. But if player is evil then his name will turn red. But some roles that are good can show as evil, also sometimes evil roles show as good. You have limited ability uses, but you can do tasks to increase it.\n\nIf you have mod installed, you don't have task and investigate mode. You can do tasks and investigate at the same time."},
            {CustomRoles.Mortician, "Mortician (Crewmate): When you report dead body, you know target's role, killer's role and how old is body. During meeting you see death reasons. Depending on options you have arrow pointing to nearest dead body."},
            {CustomRoles.Sheriff, "Sheriff (Crewmate): You can use pet button to switch between task and kill mode. In task mode you can do tasks. In kill mode you have kill button. You can kill impostors and depending on options neutrals. If you try to kill someone you can't, you die from misfire. Depending on options your target dies on misfire too.\n\nIf you have mod installed, you don't have task and kill mode. You can do tasks and kill at the same time."},
            {CustomRoles.NiceGuesser, "Nice Guesser (Crewmate): You can guess evil roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 6 is trapster, you should type <i>/guess 6 trapster</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead."},
            {CustomRoles.Immortal, "Immortal (Crewmate): After completing all tasks you can survive few kill attempts. In addition after you complete task, you get temporarily protection. If impostor try to kill you, his cooldown will reset to 50%. You will know that someone tried to kill you when meeting is called."},
            {CustomRoles.Medic, "Medic (Crewmate): You can use pet button to switch between task and shield mode. In task mode you can do tasks. In shield mode you have kill button. Use kill button to give shield to crewmate. Player with shield can't be killed. If player with shield die, you can give shield to another player. You get alerted when someone try to kill protected player and depending on options killer's cooldown reset to 50%. When you die, shield disappear.\n\nIf you have mod installed, you don't have task and shield mode. You can do tasks and give shield at the same time."},
            {CustomRoles.SecurityGuard, "Security Guard (Crewmate): You can use pet button near vent to block it permanently. Blocked vent can't be used by anyone. When you're looking at cameras/doorlog/binoculars you get alerted when someone die. Depending on options cameras don't blink when you're using it."},
            {CustomRoles.Mutant, "Mutant (Crewmate): You can use pet button during sabotage to instantly fix it from anywhere. You can't fix mushroom mixup sabotage."},
            {CustomRoles.Impostor, "Impostor (Impostor): Regular impostor without any ability.<size=0>"},
            {CustomRoles.Shapeshifter, "Shapeshifter (Impostor): You can shapeshift into other players. You can stay in shapeshifted form for limited time. When shapeshifting there is animation and depending on options you leave shapeshift evidence."},
            {CustomRoles.Phantom, "Phantom (Impostor): You can turn invisible for limited amount of time. When disappearing or appearing there is animation. While invisible, you can't kill, vent, repair sabotages, use platform and zipline. Other impostors can see you, when you're invisible. While invisible you're 10% faster."},
            {CustomRoles.TimeFreezer, "Time Freezer (Impostor): Click vanish button to freeze time for short period of time. When time is frozen other players can't move and are blind. They can't use their abilities, kill, report bodies, call meeting, sabotage, vent. You can kill during freeze, just make sure no one realize that you moved."},
            {CustomRoles.Escapist, "Escapist (Impostor): You can mark position with your vanish button. You can teleport to that position by using vanish button again. After teleporting or after meeting your marked position reset and you have to mark again."},
            {CustomRoles.EvilGuesser, "Evil Guesser (Impostor): You can guess roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 2 is sheriff, you should type <i>/guess 2 sheriff</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead. During rounds you can kill like regular impostor."},
            {CustomRoles.Hitman, "Hitman (Impostor): Your target is random non impostor and you're only allowed to kill that target. After killing you have 1 second kill cooldown and your target changes. Target changes when your current target dies, after meeting or when your shift timer goes to 0. Depending on options you have arrow pointing to your target. You can see your target name at any time and you see target's name in black."},
            {CustomRoles.Trapster, "Trapster (Impostor): After killing someone, you place trap on dead body. Next player, who tries to report that body (or interact with it in any way) will be trapped on it unable to move and use abilities. Dead body can trap only 1 person at the time. If someone gets trapped on your body, your kill cooldown will decrease and you will get alerted."},
            {CustomRoles.Parasite, "Parasite (Impostor): You can use pet button to turn nearby player into impostor, but you die after doing it. Depending on options that person becomes regular impostor, parasite or random impostor role. You can kill like regular impostor, but after kill your ability cooldown reset. You can use ability when you're suspicious to turn someone less suspicious into impostor."},
            {CustomRoles.Opportunist, "Opportunist (Neutral): Survive to the end to win with winning team. If you die, you lose."},
            {CustomRoles.Amnesiac, "Amnesiac (Neutral): Use report button on dead body to steal role of dead player. Depending on options you have arrow pointing to nearest dead body. You don't have win condition, you have to steal role from dead body and then win."},
            {CustomRoles.Jester, "Jester (Neutral): Get voted out to win alone. Act suspicious to make people think you're impostor."},
            {CustomRoles.Executioner, "Executioner (Neutral): You have random target that is crewmate. If your target is ejected, you win alone. You see your target name in black. If your target dies, you either become amnesiac, opportunist, jester or crewmate depending on options."},
            {CustomRoles.SerialKiller, "Serial Killer (Neutral): Your goal is to kill everyone. You have lower kill cooldown, so you can kill faster than impostors."},
            {CustomRoles.Pelican, "Pelican (Neutral): You can eat players by using your kill button. It means you can kill players without leaving bodies. Eat everyone to win alone."},
        };

        public static Dictionary<CustomRoles, Color> RoleColors = new() 
        {
            {CustomRoles.Crewmate, Palette.CrewmateBlue},
            {CustomRoles.Scientist, Palette.CrewmateBlue},
            {CustomRoles.Engineer, Palette.CrewmateBlue},
            {CustomRoles.Noisemaker, Palette.CrewmateBlue},
            {CustomRoles.Tracker, Palette.CrewmateBlue},
            {CustomRoles.Investigator, Utils.HexToColor("#118385")},
            {CustomRoles.Mortician, Utils.HexToColor("#4d5254")},
            {CustomRoles.Sheriff, Utils.HexToColor("#e8e11e")},
            {CustomRoles.NiceGuesser, Utils.HexToColor("#f5f17a")},
            {CustomRoles.Immortal, Utils.HexToColor("#5e2a10")},
            {CustomRoles.Medic, Utils.HexToColor("#144015")},
            {CustomRoles.SecurityGuard, Utils.HexToColor("#96944e")},
            {CustomRoles.Mutant, Utils.HexToColor("#4b0c4d")},
            {CustomRoles.Impostor, Palette.ImpostorRed},
            {CustomRoles.Shapeshifter, Palette.ImpostorRed},
            {CustomRoles.Phantom, Palette.ImpostorRed},
            {CustomRoles.TimeFreezer, Palette.ImpostorRed},
            {CustomRoles.Escapist, Palette.ImpostorRed},
            {CustomRoles.EvilGuesser, Palette.ImpostorRed},
            {CustomRoles.Hitman, Palette.ImpostorRed},
            {CustomRoles.Trapster, Palette.ImpostorRed},
            {CustomRoles.Parasite, Palette.ImpostorRed},
            {CustomRoles.Opportunist, Utils.HexToColor("#1dde16")},
            {CustomRoles.Amnesiac, Utils.HexToColor("#0fbcbf")},
            {CustomRoles.Jester, Utils.HexToColor("#db72e0")},
            {CustomRoles.Executioner, Utils.HexToColor("#c9ba95")},
            {CustomRoles.SerialKiller, Utils.HexToColor("#63188f")},
            {CustomRoles.Pelican, Utils.HexToColor("#35ab15")},
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
                case CustomRoles.Mortician:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Mortician(player);
                    break;
                case CustomRoles.Sheriff:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Sheriff(player);
                    break;
                case CustomRoles.NiceGuesser:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new NiceGuesser(player);
                    break;
                case CustomRoles.Immortal:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Immortal(player);
                    break;
                case CustomRoles.Medic:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Medic(player);
                    break;
                case CustomRoles.SecurityGuard:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SecurityGuard(player);
                    break;
                case CustomRoles.Mutant:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Mutant(player);
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
                case CustomRoles.Escapist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Escapist(player);
                    break;
                case CustomRoles.EvilGuesser:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new EvilGuesser(player);
                    break;
                case CustomRoles.Hitman:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Hitman(player);
                    break;
                case CustomRoles.Trapster:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Trapster(player);
                    break;
                case CustomRoles.Parasite:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Parasite(player);
                    break;
                case CustomRoles.Opportunist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Opportunist(player);
                    break;
                case CustomRoles.Amnesiac:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Amnesiac(player);
                    break;
                case CustomRoles.Jester:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Jester(player);
                    break;
                case CustomRoles.Executioner:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Executioner(player);
                    break;
                case CustomRoles.SerialKiller:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SerialKiller(player);
                    break;
                case CustomRoles.Pelican:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Pelican(player);
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
                else
                    Main.NameColors[(player.PlayerId, player.PlayerId)] = Color.clear;
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
            CustomRoles.Escapist or
            CustomRoles.EvilGuesser or
            CustomRoles.Hitman or
            CustomRoles.Trapster or
            CustomRoles.Parasite;
        }

        public static bool IsNeutral(CustomRoles role)
        {
            return IsNeutralBenign(role) || IsNeutralEvil(role) || IsNeutralKilling(role);
        }

        public static bool IsNeutralBenign(CustomRoles role)
        {
            return role is CustomRoles.Opportunist or
            CustomRoles.Amnesiac;
        }

        public static bool IsNeutralEvil(CustomRoles role)
        {
            return role is CustomRoles.Jester or
            CustomRoles.Executioner;
        }

        public static bool IsNeutralKilling(CustomRoles role)
        {
            return role is CustomRoles.SerialKiller or
            CustomRoles.Pelican;
        }

        public static bool IsCrewmateKilling(CustomRoles role)
        {
            return role is CustomRoles.Sheriff or
            CustomRoles.NiceGuesser;
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

        public static bool HasTasks(CustomRoles role)
        {
            return IsCrewmate(role) ||
            (role == CustomRoles.Executioner && (Executioner.CurrentRoleAfterTargetDeath is RolesAfterTargetDeath.Amnesiac or RolesAfterTargetDeath.Crewmate)) ||
            role is CustomRoles.Amnesiac;
        }

        public static int GetRoleChance(CustomRoles role)
        {
            if (role is CustomRoles.Crewmate or CustomRoles.Impostor) return 0;
            if (role is CustomRoles.Engineer or CustomRoles.SecurityGuard && GameManager.Instance.LogicOptions.MapId == 3) return 0;
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
    Mortician,
    Sheriff,
    NiceGuesser,
    Immortal,
    Medic,
    SecurityGuard,
    Mutant,
    // Impostor
    Impostor,
    Shapeshifter,
    Phantom,
    TimeFreezer,
    Escapist,
    EvilGuesser,
    Hitman,
    Trapster,
    Parasite,
    //Neutral
    Opportunist,
    Amnesiac,
    Jester,
    Executioner,
    SerialKiller,
    Pelican,
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
    Executioner,
    Pelican,
}

public enum AdditionalWinners
{
    Opportunist,
}