namespace MoreGamemodes
{
    public class Engineer : CustomRole
    {
        public Engineer(PlayerControl player)
        {
            Role = CustomRoles.Engineer;
            BaseRole = BaseRoles.Engineer;
            Player = player;
            Color = Palette.CrewmateBlue;
            RoleName = "Engineer";
            RoleDescription = "Can use the vents";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Engineer];
            AbilityUses = -1f;
        }
    }
}