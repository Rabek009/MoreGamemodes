namespace MoreGamemodes
{
    public class Mayor : CustomRole
    {
        public override int AdditionalVotes()
        {
            return AdditionalVoteCount.GetInt();
        }

        public override int AdditionalVisualVotes()
        {
            return HideAdditionalVotes.GetBool() ? 0 : AdditionalVoteCount.GetInt();
        }

        public override int GetPlayerCount()
        {
            return 1 + AdditionalVoteCount.GetInt();
        }

        public Mayor(PlayerControl player)
        {
            Role = CustomRoles.Mayor;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem AdditionalVoteCount;
        public static OptionItem HideAdditionalVotes;
        public static OptionItem CanBeGuessed;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(400400, CustomRoles.Mayor, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(400401, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AdditionalVoteCount = IntegerOptionItem.Create(400402, "Additional vote count", new(1, 14, 1), 2, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            HideAdditionalVotes = BooleanOptionItem.Create(400403, "Hide additional votes", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanBeGuessed = BooleanOptionItem.Create(400404, "Can be guessed", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Mayor] = Chance;
            Options.RolesCount[CustomRoles.Mayor] = Count;
        }
    }
}