namespace MoreGamemodes
{
    public class Noisemaker : CustomRole
    {
        public Noisemaker(PlayerControl player)
        {
            Role = CustomRoles.Noisemaker;
            BaseRole = BaseRoles.Noisemaker;
            Player = player;
            Color = Palette.CrewmateBlue;
            RoleName = "Noisemaker";
            RoleDescription = "Send out an alert when killed";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Noisemaker];
            AbilityUses = -1f;
        }
    }
}