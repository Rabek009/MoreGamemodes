namespace MoreGamemodes
{
    public class Crewmate : CustomRole
    {
        public Crewmate(PlayerControl player)
        {
            Role = CustomRoles.Crewmate;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Color = Palette.CrewmateBlue;
            RoleName = "Crewmate";
            RoleDescription = "Do your tasks";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Crewmate];
            AbilityUses = -1f;
        }
    }
}