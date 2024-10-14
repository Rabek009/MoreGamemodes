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
            RoleDescriptionLong = "Crewmate (Crewmate): Regular crewmate without any ability.";
            AbilityUses = -1f;
        }
    }
}