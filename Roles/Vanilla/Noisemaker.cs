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
            RoleDescriptionLong = "Noisemaker (Crewmate): When you get killed, you send alert. That alert informs crewmates that you died and shows direction to your body. Depending on options killers get alert too.";
            AbilityUses = -1f;
        }
    }
}