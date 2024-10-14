using UnityEngine;

namespace MoreGamemodes
{
    public class Opportunist : CustomRole
    {
        public Opportunist(PlayerControl player)
        {
            Role = CustomRoles.Opportunist;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            ColorUtility.TryParseHtmlString("#1dde16", out Color);
            RoleName = "Opportunist";
            RoleDescription = "Survive to win";
            RoleDescriptionLong = "Opportunist (Neutral): Survive to the end to win with winning team. If you die, you lose.";
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static void SetupOptionItem()
        {
            ColorUtility.TryParseHtmlString("#1dde16", out Color c);
            Chance = IntegerOptionItem.Create(800100, "Opportunist", new(0, 100, 5), 0, TabGroup.NeutralRoles, false)
                .SetColor(c)
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(800101, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Opportunist] = Chance;
            Options.RolesCount[CustomRoles.Opportunist] = Count;
        }
    }
}