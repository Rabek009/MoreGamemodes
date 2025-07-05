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
            {"snitch", CustomRoles.Snitch},
            {"sniffer", CustomRoles.Sniffer},
            {"sheriff", CustomRoles.Sheriff},
            {"niceguesser", CustomRoles.NiceGuesser},
            {"shaman", CustomRoles.Shaman},
            {"immortal", CustomRoles.Immortal},
            {"medic", CustomRoles.Medic},
            {"altruist", CustomRoles.Altruist},
            {"securityguard", CustomRoles.SecurityGuard},
            {"mutant", CustomRoles.Mutant},
            {"judge", CustomRoles.Judge},
            {"mayor", CustomRoles.Mayor},
            {"impostor", CustomRoles.Impostor},
            {"shapeshifter", CustomRoles.Shapeshifter},
            {"phantom", CustomRoles.Phantom},
            {"timefreezer", CustomRoles.TimeFreezer},
            {"escapist", CustomRoles.Escapist},
            {"droner", CustomRoles.Droner},
            {"evilguesser", CustomRoles.EvilGuesser},
            {"hitman", CustomRoles.Hitman},
            {"archer", CustomRoles.Archer},
            {"trapster", CustomRoles.Trapster},
            {"parasite", CustomRoles.Parasite},
            {"undertaker", CustomRoles.Undertaker},
            {"opportunist", CustomRoles.Opportunist},
            {"amnesiac", CustomRoles.Amnesiac},
            {"romantic", CustomRoles.Romantic},
            {"jester", CustomRoles.Jester},
            {"executioner", CustomRoles.Executioner},
            {"soulcollector", CustomRoles.SoulCollector},
            {"serialkiller", CustomRoles.SerialKiller},
            {"pelican", CustomRoles.Pelican},
            {"arsonist", CustomRoles.Arsonist},
            {"ninja", CustomRoles.Ninja},
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
            {CustomRoles.Snitch, "Snitch"},
            {CustomRoles.Sniffer, "Sniffer"},
            { CustomRoles.Sheriff, "Sheriff"},
            {CustomRoles.NiceGuesser, "Nice Guesser"},
            {CustomRoles.Shaman, "Shaman"},
            {CustomRoles.Immortal, "Immortal"},
            {CustomRoles.Medic, "Medic"},
            {CustomRoles.Altruist, "Altruist"},
            {CustomRoles.SecurityGuard, "Security Guard"},
            {CustomRoles.Mutant, "Mutant"},
            {CustomRoles.Judge, "Judge"},
            {CustomRoles.Mayor, "Mayor"},
            {CustomRoles.Impostor, "Impostor"},
            {CustomRoles.Shapeshifter, "Shapeshifter"},
            {CustomRoles.Phantom, "Phantom"},
            {CustomRoles.TimeFreezer, "Time Freezer"},
            {CustomRoles.Escapist, "Escapist"},
            {CustomRoles.Droner, "Droner"},
            {CustomRoles.EvilGuesser, "Evil Guesser"},
            {CustomRoles.Hitman, "Hitman"},
            {CustomRoles.Archer, "Archer"},
            {CustomRoles.Trapster, "Trapster"},
            {CustomRoles.Parasite, "Parasite"},
            {CustomRoles.Undertaker, "Undertaker"},
            {CustomRoles.Opportunist, "Opportunist"},
            {CustomRoles.Amnesiac, "Amnesiac"},
            {CustomRoles.Romantic, "Romantic"},
            {CustomRoles.Jester, "Jester"},
            {CustomRoles.Executioner, "Executioner"},
            {CustomRoles.SoulCollector, "Soul Collector"},
            {CustomRoles.SerialKiller, "Serial Killer"},
            {CustomRoles.Pelican, "Pelican"},
            {CustomRoles.Arsonist, "Arsonist"},
            {CustomRoles.Ninja, "Ninja"},
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
            {CustomRoles.Snitch, "Complete tasks to find killers"},
            {CustomRoles.Sniffer, "Sniff who was near who"},
            {CustomRoles.Sheriff, "Shoot impostors"},
            {CustomRoles.NiceGuesser, "Guess evil roles during meeting"},
            {CustomRoles.Shaman, "Curse killers and make them die"},
            {CustomRoles.Immortal, "Complete tasks to get protection"},
            {CustomRoles.Medic, "Give shield to crewmate"},
            {CustomRoles.Altruist, "Sacrifice yourself to revive someone"},
            {CustomRoles.SecurityGuard, "Block vents and gain extra info from security"},
            {CustomRoles.Mutant, "Fix sabotages from anywhere"},
            {CustomRoles.Judge, "Exile who you want"},
            {CustomRoles.Mayor, "Your vote is more important"},
            {CustomRoles.Impostor, "Kill and sabotage"},
            {CustomRoles.Shapeshifter, "Disguise yourself"},
            {CustomRoles.Phantom, "Turn invisible"},
            {CustomRoles.TimeFreezer, "Freeze time to make sneaky kills"},
            {CustomRoles.Escapist, "Mark location and teleport to it"},
            {CustomRoles.Droner, "Use drones to kill players"},
            {CustomRoles.EvilGuesser, "Guess roles during meeting"},
            {CustomRoles.Hitman, "Kill your targets with no cooldown"},
            {CustomRoles.Archer, "Shoot players from far away"},
            {CustomRoles.Trapster, "Trap players on dead bodies"},
            {CustomRoles.Parasite, "Sacrifice to make someone impostor"},
            {CustomRoles.Undertaker, "Help teammates hide dead bodies"},
            {CustomRoles.Opportunist, "Survive to win"},
            {CustomRoles.Amnesiac, "Remember role from dead body"},
            {CustomRoles.Jester, "Get voted out"},
            {CustomRoles.Romantic, "Choose your lover and help him win"},
            {CustomRoles.Executioner, "Vote your target out"},
            {CustomRoles.SoulCollector, "Predict players death"},
            {CustomRoles.SerialKiller, "Kill everyone"},
            {CustomRoles.Pelican, "Eat every player"},
            {CustomRoles.Arsonist, "Douse and ignite everyone"},
            {CustomRoles.Ninja, "Become fast and invisible"},
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
            {CustomRoles.Snitch, "Snitch (Crewmate): After completing all tasks you see who is impostor and depending on options neutral killer. Depending on options you can also see arrows to them. But if you're close to complete tasks, killers will know you and see arrow to you. Depending on options you might have more tasks than other crewmates. Finish tasks fast and don't get killed!"},
            {CustomRoles.Sniffer, "Sniffer (Crewmate): You can use pet button to switch between task and sniff mode. In task mode you can do tasks. In sniff mode you have kill button. Use kill button to sniff a player once per round. During meeting you can see who was near that player after you sniffed him. These players are given in random order. Invisible players don't count and shapeshifter can cause fake result.\n\nIf you have mod installed, you don't have task and sniff mode. You can do tasks and sniff at the same time."},
            {CustomRoles.Sheriff, "Sheriff (Crewmate): You can use pet button to switch between task and kill mode. In task mode you can do tasks. In kill mode you have kill button. You can kill impostors and depending on options neutrals. If you try to kill someone you can't, you die from misfire. Depending on options your target dies on misfire too.\n\nIf you have mod installed, you don't have task and kill mode. You can do tasks and kill at the same time."},
            {CustomRoles.NiceGuesser, "Nice Guesser (Crewmate): You can guess evil roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 6 is trapster, you should type <i>/guess 6 trapster</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead."},
            {CustomRoles.Shaman, "Shaman (Crewmate): During meeting you can curse someone. If that player is killer he has to kill someone next round, or he will die and is informed about it. If that player can't kill, nothing will happen. You can curse max 1 person per meeting. You can't call meeting, but you can still report dead body. You have limited ability uses, but you can do tasks to increase it.\n\nIf you have mod installed you can use curse button to curse someone."},
            {CustomRoles.Immortal, "Immortal (Crewmate): After completing all tasks you can survive few kill attempts. In addition after you complete task, you get temporarily protection. If impostor try to kill you, his cooldown will reset to 50%."},
            {CustomRoles.Medic, "Medic (Crewmate): You can use pet button to switch between task and shield mode. In task mode you can do tasks. In shield mode you have kill button. Use kill button to give shield to crewmate. Player with shield can't be killed. If player with shield die, you can give shield to another player. You get alerted when someone try to kill protected player and depending on options killer's cooldown reset to 50%. When you die, shield disappear.\n\nIf you have mod installed, you don't have task and shield mode. You can do tasks and give shield at the same time."},
            {CustomRoles.Altruist, "Altruist (Crewmate): You can use report button to revive someone. After reviving someone you die. Revived player can still use their abilities, chat and do everything else. Depending on options you have arrow pointing to nearest dead body."},
            {CustomRoles.SecurityGuard, "Security Guard (Crewmate): You can use pet button near vent to block it permanently. Blocked vent can't be used by anyone. When you're looking at cameras/doorlog/binoculars you get alerted when someone die. Depending on options cameras don't blink when you're using it."},
            {CustomRoles.Mutant, "Mutant (Crewmate): You can use pet button during sabotage to instantly fix it from anywhere. You can't fix mushroom mixup sabotage."},
            {CustomRoles.Judge, "Judge (Crewmate): During meeting you can eject anyone you want one time. To do this open kick menu (open chat and click red button), select player who you want to exile and click \"kick\". After that meeting will instantly end and that player will be ejected. Depending on options you might die after using ability.\n\nIf you have mod installed you can use judge button to exile player."},
            {CustomRoles.Mayor, "Mayor (Crewmate): Your vote counts as multiple votes. Use it to eject impostors easier."},
            {CustomRoles.Impostor, "Impostor (Impostor): Regular impostor without any ability.<size=0>"},
            {CustomRoles.Shapeshifter, "Shapeshifter (Impostor): You can shapeshift into other players. You can stay in shapeshifted form for limited time. When shapeshifting there is animation and depending on options you leave shapeshift evidence."},
            {CustomRoles.Phantom, "Phantom (Impostor): You can turn invisible for limited amount of time. When disappearing or appearing there is animation. While invisible, you can't kill, vent, repair sabotages, use platform and zipline. Other impostors can see you, when you're invisible. While invisible you're 10% faster."},
            {CustomRoles.TimeFreezer, "Time Freezer (Impostor): Click vanish button to freeze time for short period of time. When time is frozen other players can't move and are blind. They can't use their abilities, kill, report bodies, call meeting, sabotage, vent. You can't freeze time during sabotage."},
            {CustomRoles.Escapist, "Escapist (Impostor): You can mark position with your vanish button. You can teleport to that position by using vanish button again. After teleporting or after meeting your marked position reset and you have to mark again."},
            {CustomRoles.Droner, "Droner (Impostor): You can use vanish button to launch the drone from your position. You can control this drone and everyone can see it. Drone can stay on map for limited amount of time. When controlling drone you can kill people using it. Use pet button to stop using it or it will stop automatically when ability duration is over. After that you have cooldown before you can use your ability again. When you're using a drone, your real character is not moving (other people see you standing still). People can still interact with you when you're using a drone. When using drone you can't vent, report, call meetings and interact with objects."},
            {CustomRoles.EvilGuesser, "Evil Guesser (Impostor): You can guess roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 2 is sheriff, you should type <i>/guess 2 sheriff</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead. During rounds you can kill like regular impostor."},
            {CustomRoles.Hitman, "Hitman (Impostor): Your target is random non impostor and you're only allowed to kill that target. After killing you have 1 second kill cooldown and your target changes. Target changes when your current target dies, after meeting or when your shift timer goes to 0. Depending on options you have arrow pointing to your target. You can see your target name at any time and you see target's name in black."},
            {CustomRoles.Archer, "Archer (Impostor): You can use pet button to kill nearest player without teleporting to them, but it uses arrow. Kill range is limited, but you can kill through walls. You can kill normally without using it. You have limited arrows, but you can get more by killing. You can only shoot, when your cooldown is 0."},
            {CustomRoles.Trapster, "Trapster (Impostor): After killing someone, you place trap on dead body. Next player, who tries to report that body (or interact with it in any way) will be trapped on it unable to move and use abilities. Dead body can trap only 1 person at the time. If someone gets trapped on your body, your kill cooldown will decrease and you will get alerted."},
            {CustomRoles.Parasite, "Parasite (Impostor): You can use pet button to turn nearby player into impostor, but you die after doing it. Depending on options that person becomes regular impostor, parasite or random impostor role. You can kill like regular impostor, but after kill your ability cooldown reset. You can use ability when you're suspicious to turn someone less suspicious into impostor."},
            {CustomRoles.Undertaker, "Undertaker (Impostor): You can use shift button to select a target (it must be another impostor). That impostor is target until either he kills someone, dies or after some time passes. If selected impostor kills someone, dead body is teleported to you. When you're last impostor you can't use your ability, but depending on options you might get kill cooldown reduction."},
            {CustomRoles.Opportunist, "Opportunist (Neutral): Survive to the end to win with winning team. If you die, you lose."},
            {CustomRoles.Amnesiac, "Amnesiac (Neutral): Use report button on dead body to steal role of dead player. Depending on options you have arrow pointing to nearest dead body. You don't have win condition, you have to steal role from dead body and then win."},
            {CustomRoles.Jester, "Jester (Neutral): Get voted out to win alone. Act suspicious to make people think you're impostor."},
            {CustomRoles.Romantic, "Romantic (Neutral): Use kill button to choose your lover. Your goal is to protect your lover and help him win. If your lover wins, you win. If your lover loses, you lose. If one of you die, the other one dies too. Your role/team can't change and other romantics can't select you as lover. Depending on options after choosing target, you can use vanish button to protect both of you for some time. You might know lover's role and be able to chat with him privately during round by typing /lc MESSAGE"},
            {CustomRoles.Executioner, "Executioner (Neutral): You have random target that is crewmate. If your target is ejected, you win alone. You see your target name in black. If your target dies, you either become amnesiac, opportunist, jester or crewmate depending on options."},
            {CustomRoles.SoulCollector, "Soul Collector (Neutral): Use kill button to predict that selected player will die. When your target dies in any way, you collect their soul and you can choose new target. Depending on options you can change target anytime. When you collect enough souls, you instantly win alone."},
            {CustomRoles.SerialKiller, "Serial Killer (Neutral): Your goal is to kill everyone. You have lower kill cooldown, so you can kill faster than impostors."},
            {CustomRoles.Pelican, "Pelican (Neutral): You can eat players by using your kill button. It means you can kill players without leaving bodies. Eat everyone to win alone."},
            {CustomRoles.Arsonist, "Arsonist (Neutral): Use kill button on not doused player to douse him. If you click kill button on doused player, you ignite him. Ignited player will ignite every doused player that he touches and die after some time. Doused players have black name, ignited have orange name. Igniting bypasses all protections. Kill everyone to win alone."},
            {CustomRoles.Ninja, "Ninja (Neutral): You can use vanish button to become invisible - there is no animation. During invisibility your speed is greatly increased and you don't teleport when killing. Depending on options you can vent when you're not invisible. Kill everyone to win alone."},
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
            {CustomRoles.Snitch, Utils.HexToColor("#7c9923")},
            {CustomRoles.Sniffer, Utils.HexToColor("#269669")},
            {CustomRoles.Sheriff, Utils.HexToColor("#e8e11e")},
            {CustomRoles.NiceGuesser, Utils.HexToColor("#f5f17a")},
            {CustomRoles.Shaman, Utils.HexToColor("#dce7e8")},
            {CustomRoles.Immortal, Utils.HexToColor("#5e2a10")},
            {CustomRoles.Medic, Utils.HexToColor("#144015")},
            {CustomRoles.Altruist, Utils.HexToColor("#3d040e")},
            {CustomRoles.SecurityGuard, Utils.HexToColor("#96944e")},
            {CustomRoles.Mutant, Utils.HexToColor("#4b0c4d")},
            {CustomRoles.Judge, Utils.HexToColor("#294361")},
            {CustomRoles.Mayor, Utils.HexToColor("#332880")},
            {CustomRoles.Impostor, Palette.ImpostorRed},
            {CustomRoles.Shapeshifter, Palette.ImpostorRed},
            {CustomRoles.Phantom, Palette.ImpostorRed},
            {CustomRoles.TimeFreezer, Palette.ImpostorRed},
            {CustomRoles.Escapist, Palette.ImpostorRed},
            {CustomRoles.Droner, Palette.ImpostorRed},
            {CustomRoles.EvilGuesser, Palette.ImpostorRed},
            {CustomRoles.Hitman, Palette.ImpostorRed},
            {CustomRoles.Archer, Palette.ImpostorRed},
            {CustomRoles.Trapster, Palette.ImpostorRed},
            {CustomRoles.Parasite, Palette.ImpostorRed},
            {CustomRoles.Undertaker, Palette.ImpostorRed},
            {CustomRoles.Opportunist, Utils.HexToColor("#1dde16")},
            {CustomRoles.Amnesiac, Utils.HexToColor("#0fbcbf")},
            {CustomRoles.Romantic, Utils.HexToColor("#bf0d96")},
            {CustomRoles.Jester, Utils.HexToColor("#db72e0")},
            {CustomRoles.Executioner, Utils.HexToColor("#c9ba95")},
            {CustomRoles.SoulCollector, Utils.HexToColor("#9c86b3")},
            {CustomRoles.SerialKiller, Utils.HexToColor("#63188f")},
            {CustomRoles.Pelican, Utils.HexToColor("#35ab15")},
            {CustomRoles.Arsonist, Utils.HexToColor("#d67d24")},
            {CustomRoles.Ninja, Utils.HexToColor("#802051")},
        };

        public static Dictionary<CustomRoles, CustomRoleTypes> CRoleTypes = new() 
        {
            {CustomRoles.Crewmate, CustomRoleTypes.CrewmateVanilla},
            {CustomRoles.Scientist, CustomRoleTypes.CrewmateVanilla},
            {CustomRoles.Engineer, CustomRoleTypes.CrewmateVanilla},
            {CustomRoles.Noisemaker, CustomRoleTypes.CrewmateVanilla},
            {CustomRoles.Tracker, CustomRoleTypes.CrewmateVanilla},
            {CustomRoles.Investigator, CustomRoleTypes.CrewmateInvestigative},
            {CustomRoles.Mortician, CustomRoleTypes.CrewmateInvestigative},
            {CustomRoles.Snitch, CustomRoleTypes.CrewmateInvestigative},
            {CustomRoles.Sniffer, CustomRoleTypes.CrewmateInvestigative},
            {CustomRoles.Sheriff, CustomRoleTypes.CrewmateKilling},
            {CustomRoles.NiceGuesser, CustomRoleTypes.CrewmateKilling},
            {CustomRoles.Shaman, CustomRoleTypes.CrewmateKilling},
            {CustomRoles.Immortal, CustomRoleTypes.CrewmateProtective},
            {CustomRoles.Medic, CustomRoleTypes.CrewmateProtective},
            {CustomRoles.Altruist, CustomRoleTypes.CrewmateProtective},
            {CustomRoles.SecurityGuard, CustomRoleTypes.CrewmateSupport},
            {CustomRoles.Mutant, CustomRoleTypes.CrewmateSupport},
            {CustomRoles.Judge, CustomRoleTypes.CrewmateSupport},
            {CustomRoles.Mayor, CustomRoleTypes.CrewmateSupport},
            {CustomRoles.Impostor, CustomRoleTypes.ImpostorVanilla},
            {CustomRoles.Shapeshifter, CustomRoleTypes.ImpostorVanilla},
            {CustomRoles.Phantom, CustomRoleTypes.ImpostorVanilla},
            {CustomRoles.TimeFreezer, CustomRoleTypes.ImpostorConcealing},
            {CustomRoles.Escapist, CustomRoleTypes.ImpostorConcealing},
            {CustomRoles.Droner, CustomRoleTypes.ImpostorConcealing},
            {CustomRoles.EvilGuesser, CustomRoleTypes.ImpostorKilling},
            {CustomRoles.Hitman, CustomRoleTypes.ImpostorKilling},
            {CustomRoles.Archer, CustomRoleTypes.ImpostorKilling},
            {CustomRoles.Trapster, CustomRoleTypes.ImpostorSupport},
            {CustomRoles.Parasite, CustomRoleTypes.ImpostorSupport},
            {CustomRoles.Undertaker, CustomRoleTypes.ImpostorSupport},
            {CustomRoles.Opportunist, CustomRoleTypes.NeutralBenign},
            {CustomRoles.Amnesiac, CustomRoleTypes.NeutralBenign},
            {CustomRoles.Romantic, CustomRoleTypes.NeutralBenign},
            {CustomRoles.Jester, CustomRoleTypes.NeutralEvil},
            {CustomRoles.Executioner, CustomRoleTypes.NeutralEvil},
            {CustomRoles.SoulCollector, CustomRoleTypes.NeutralEvil},
            {CustomRoles.SerialKiller, CustomRoleTypes.NeutralKilling},
            {CustomRoles.Pelican, CustomRoleTypes.NeutralKilling},
            {CustomRoles.Arsonist, CustomRoleTypes.NeutralKilling},
            {CustomRoles.Ninja, CustomRoleTypes.NeutralKilling},
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
                case CustomRoles.Snitch:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Snitch(player);
                    break;
                case CustomRoles.Sniffer:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Sniffer(player);
                    break;
                case CustomRoles.Sheriff:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Sheriff(player);
                    break;
                case CustomRoles.NiceGuesser:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new NiceGuesser(player);
                    break;
                case CustomRoles.Shaman:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Shaman(player);
                    break;
                case CustomRoles.Immortal:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Immortal(player);
                    break;
                case CustomRoles.Medic:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Medic(player);
                    break;
                case CustomRoles.Altruist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Altruist(player);
                    break;
                case CustomRoles.SecurityGuard:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SecurityGuard(player);
                    break;
                case CustomRoles.Mutant:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Mutant(player);
                    break;
                case CustomRoles.Judge:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Judge(player);
                    break;
                case CustomRoles.Mayor:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Mayor(player);
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
                case CustomRoles.Droner:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Droner(player);
                    break;
                case CustomRoles.EvilGuesser:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new EvilGuesser(player);
                    break;
                case CustomRoles.Hitman:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Hitman(player);
                    break;
                case CustomRoles.Archer:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Archer(player);
                    break;
                case CustomRoles.Trapster:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Trapster(player);
                    break;
                case CustomRoles.Parasite:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Parasite(player);
                    break;
                case CustomRoles.Undertaker:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Undertaker(player);
                    break;
                case CustomRoles.Opportunist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Opportunist(player);
                    break;
                case CustomRoles.Amnesiac:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Amnesiac(player);
                    break;
                case CustomRoles.Romantic:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Romantic(player);
                    break;
                case CustomRoles.Jester:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Jester(player);
                    break;
                case CustomRoles.Executioner:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Executioner(player);
                    break;
                case CustomRoles.SoulCollector:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SoulCollector(player);
                    break;
                case CustomRoles.SerialKiller:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new SerialKiller(player);
                    break;
                case CustomRoles.Pelican:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Pelican(player);
                    break;
                case CustomRoles.Arsonist:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Arsonist(player);
                    break;
                case CustomRoles.Ninja:
                    ClassicGamemode.instance.AllPlayersRole[player.PlayerId] = new Ninja(player);
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
            return CRoleTypes[role] is CustomRoleTypes.CrewmateVanilla or
            CustomRoleTypes.CrewmateInvestigative or
            CustomRoleTypes.CrewmateKilling or
            CustomRoleTypes.CrewmateProtective or
            CustomRoleTypes.CrewmateSupport;
        }

        public static bool IsImpostor(CustomRoles role)
        {
            return CRoleTypes[role] is CustomRoleTypes.ImpostorVanilla or
            CustomRoleTypes.ImpostorConcealing or
            CustomRoleTypes.ImpostorKilling or
            CustomRoleTypes.ImpostorSupport;
        }

        public static bool IsNeutral(CustomRoles role)
        {
            return IsNeutralBenign(role) || IsNeutralEvil(role) || IsNeutralKilling(role);
        }

        public static bool IsNeutralBenign(CustomRoles role)
        {
            return CRoleTypes[role] == CustomRoleTypes.NeutralBenign;
        }

        public static bool IsNeutralEvil(CustomRoles role)
        {
            return CRoleTypes[role] == CustomRoleTypes.NeutralEvil;
        }

        public static bool IsNeutralKilling(CustomRoles role)
        {
            return CRoleTypes[role] == CustomRoleTypes.NeutralKilling;
        }

        public static bool IsCrewmateKilling(CustomRoles role)
        {
            return CRoleTypes[role] == CustomRoleTypes.CrewmateKilling;
        }

        public static bool IsVanilla(CustomRoles role)
        {
            return CRoleTypes[role] is CustomRoleTypes.CrewmateVanilla or
            CustomRoleTypes.ImpostorVanilla;
        }

        public static bool HasTasks(CustomRoles role)
        {
            return IsCrewmate(role) ||
            (role == CustomRoles.Executioner && (Executioner.CurrentRoleAfterTargetDeath is RolesAfterTargetDeath.Amnesiac or RolesAfterTargetDeath.Crewmate)) ||
            role == CustomRoles.Amnesiac;
        }

        public static int GetRoleChance(CustomRoles role)
        {
            if (role is CustomRoles.Crewmate or CustomRoles.Impostor) return 0;
            if (role == CustomRoles.Undertaker && (Main.RealOptions != null ? Main.RealOptions.GetInt(Int32OptionNames.NumImpostors) : GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors)) < 2) return 0;
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
            if (role == CustomRoles.Undertaker && (Main.RealOptions != null ? Main.RealOptions.GetInt(Int32OptionNames.NumImpostors) : GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors)) < 2) return 0;
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
    Snitch,
    Sniffer,
    Sheriff,
    NiceGuesser,
    Shaman,
    Immortal,
    Medic,
    Altruist,
    SecurityGuard,
    Mutant,
    Judge,
    Mayor,
    // Impostor
    Impostor,
    Shapeshifter,
    Phantom,
    TimeFreezer,
    Escapist,
    Droner,
    EvilGuesser,
    Hitman,
    Archer,
    Trapster,
    Parasite,
    Undertaker,
    //Neutral
    Opportunist,
    Amnesiac,
    Romantic,
    Jester,
    Executioner,
    SoulCollector,
    SerialKiller,
    Pelican,
    Arsonist,
    Ninja,
}

public enum BaseRoles
{
    Crewmate,
    Scientist,
    Engineer,
    Noisemaker,
    Tracker,
    Impostor,
    Shapeshifter,
    Phantom,
    DesyncImpostor,
    DesyncShapeshifter,
    DesyncPhantom,
}

public enum CustomRoleTypes
{
    CrewmateVanilla,
    CrewmateInvestigative,
    CrewmateKilling,
    CrewmateProtective,
    CrewmateSupport,
    ImpostorVanilla,
    ImpostorConcealing,
    ImpostorKilling,
    ImpostorSupport,
    NeutralBenign,
    NeutralEvil,
    NeutralKilling,
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
    Arsonist,
    SoulCollector,
    Ninja,
}

public enum AdditionalWinners
{
    Opportunist,
    Romantic,
}