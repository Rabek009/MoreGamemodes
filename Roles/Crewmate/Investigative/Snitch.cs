namespace MoreGamemodes
{
    public class Snitch : CustomRole
    {
        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            if (IsRevealed())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == Player || pc.GetRole().IsImpostor() || (pc.GetRole().IsNeutralKilling() && CanFindNeutralKillers.GetBool()) || pc.Data.IsDead)
                        ClassicGamemode.instance.NameSymbols[(Player.PlayerId, pc.PlayerId)][CustomRoles.Snitch] = ("★", Color);
                    if ((pc.GetRole().IsImpostor() || (pc.GetRole().IsNeutralKilling() && CanFindNeutralKillers.GetBool())) && !pc.Data.IsDead)
                    {
                        if (!ClassicGamemode.instance.NameSymbols[(pc.PlayerId, pc.PlayerId)].ContainsKey(CustomRoles.Snitch))
                            ClassicGamemode.instance.NameSymbols[(pc.PlayerId, pc.PlayerId)][CustomRoles.Snitch] = ("★", Color);
                        ClassicGamemode.instance.NameSymbols[(pc.PlayerId, pc.PlayerId)][CustomRoles.Snitch] = (ClassicGamemode.instance.NameSymbols[(pc.PlayerId, pc.PlayerId)][CustomRoles.Snitch].Item1 + Utils.GetArrow(pc.GetRealPosition(), Player.transform.position), Color);
                    }
                }
            }
            if (Player.AllTasksCompleted() && !Player.Data.IsDead)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsImpostor() || (pc.GetRole().IsNeutralKilling() && CanFindNeutralKillers.GetBool()))
                        Main.NameColors[(pc.PlayerId, Player.PlayerId)] = pc.GetRole().Color;
                }
            }
        }

        public override string GetNamePostfix()
        {
            string postfix = "";
            if (SeeArrowToImpostors.GetBool() && !Player.Data.IsDead && Player.AllTasksCompleted())
            {
                postfix += "\n";
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsImpostor() || (pc.GetRole().IsNeutralKilling() && CanFindNeutralKillers.GetBool()))
                        postfix += Utils.ColorString(pc.GetRole().Color, Utils.GetArrow(Player.GetRealPosition(), pc.transform.position));
                }
            }
            return postfix;
        }

        public override bool CanGetGuessed(PlayerControl guesser, CustomRoles? role)
        {
            return !IsRevealed() || (!guesser.GetRole().IsImpostor() && (!guesser.GetRole().IsNeutralKilling() || !CanFindNeutralKillers.GetBool()));
        }

        public bool IsRevealed()
        {
            if (Player.Data.IsDead) return false;
            int completedTasks = 0;
            int totalTasks = 0;
            foreach (var task in Player.Data.Tasks)
            {
                ++totalTasks;
                if (task.Complete)
                    ++completedTasks;
            }
            return completedTasks >= totalTasks - TasksRemainingWhenRevealed.GetInt();
        }

        public Snitch(PlayerControl player)
        {
            Role = CustomRoles.Snitch;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CanFindNeutralKillers;
        public static OptionItem SeeArrowToImpostors;
        public static OptionItem TasksRemainingWhenRevealed;
        public static OptionItem AdditionalShortTasks;
        public static OptionItem AdditionalLongTasks;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(100300, CustomRoles.Snitch, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(100301, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanFindNeutralKillers = BooleanOptionItem.Create(100302, "Can find neutral killers", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            SeeArrowToImpostors = BooleanOptionItem.Create(100303, "See arrow to impostors", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            TasksRemainingWhenRevealed = IntegerOptionItem.Create(100304, "Tasks remaining when revealed", new(1, 10, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AdditionalShortTasks = IntegerOptionItem.Create(100305, "Additional short tasks", new(0, 5, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AdditionalLongTasks = IntegerOptionItem.Create(100306, "Additional long tasks", new(0, 5, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Snitch] = Chance;
            Options.RolesCount[CustomRoles.Snitch] = Count;
        }
    }
}