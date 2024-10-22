namespace MoreGamemodes
{
    public class Scientist : CustomRole
    {
        public Scientist(PlayerControl player)
        {
            Role = CustomRoles.Scientist;
            BaseRole = BaseRoles.Scientist;
            Player = player;
            Color = Palette.CrewmateBlue;
            RoleName = "Scientist";
            RoleDescription = "Access vitals at any time";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Scientist];
            AbilityUses = -1f;
        }
    }
}