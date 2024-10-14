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
            RoleDescriptionLong = "Scientist (Crewmate): You can use vitals from anywhere, but you have limited battery. Complete task to recharge your battery. When you complete all tasks, your battery will recharge automatically.";
            AbilityUses = -1f;
        }
    }
}