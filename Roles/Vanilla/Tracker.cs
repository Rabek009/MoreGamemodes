namespace MoreGamemodes
{
    public class Tracker : CustomRole
    {
        public Tracker(PlayerControl player)
        {
            Role = CustomRoles.Tracker;
            BaseRole = BaseRoles.Tracker;
            Player = player;
            Color = Palette.CrewmateBlue;
            RoleName = "Tracker";
            RoleDescription = "Track a crewmate with your map";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Tracker];
            AbilityUses = -1f;
        }
    }
}