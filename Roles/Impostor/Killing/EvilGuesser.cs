namespace MoreGamemodes
{
    public class EvilGuesser : CustomRole
    {
        public override bool CanGuess(PlayerControl target, CustomRoles role)
        {
            if (target.GetRole().IsImpostor()) return false; 
            if (role == CustomRoles.Crewmate) return CanGuessCrewmateRole.GetBool();
            return CustomRolesHelper.IsCrewmate(role) || (CustomRolesHelper.IsNeutralKilling(role) && CanGuessNeutralKilling.GetBool()) || (CustomRolesHelper.IsNeutralEvil(role) && CanGuessNeutralEvil.GetBool()) ||
            (CustomRolesHelper.IsNeutralBenign(role) && CanGuessNeutralBenign.GetBool());
        }

        public EvilGuesser(PlayerControl player)
        {
            Role = CustomRoles.EvilGuesser;
            BaseRole = BaseRoles.Impostor;
            Player = player;
            Color = Palette.ImpostorRed;
            RoleName = "Evil Guesser";
            RoleDescription = "Guess roles during meeting";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.EvilGuesser];
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CanGuessNeutralKilling;
        public static OptionItem CanGuessNeutralEvil;
        public static OptionItem CanGuessNeutralBenign;
        public static OptionItem CanGuessCrewmateRole;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(600100, "Evil guesser", new(0, 100, 5), 0, TabGroup.ImpostorRoles, false)
                .SetColor(Palette.ImpostorRed)
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(600101, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanGuessNeutralKilling = BooleanOptionItem.Create(600102, "Can guess neutral killing", true, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanGuessNeutralEvil = BooleanOptionItem.Create(600103, "Can guess neutral evil", true, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanGuessNeutralBenign = BooleanOptionItem.Create(600104, "Can guess neutral benign", true, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanGuessCrewmateRole = BooleanOptionItem.Create(200107, "Can guess \"Crewmate\" role", true, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.EvilGuesser] = Chance;
            Options.RolesCount[CustomRoles.EvilGuesser] = Count;
        }
    }
}