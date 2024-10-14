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
            RoleDescriptionLong = "Engineer (Crewmate): You can vent like impostor, but you have venting cooldown and you can stay in vent for limited time.";
            AbilityUses = -1f;
        }
    }
}