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
            RoleDescriptionLong = "Tracker (Crewmate): You can track other player too see that player on your map. Player position updates every few seconds. You can track player for limited amount of time. After tracking cooldown is over, you can track another player.";
            AbilityUses = -1f;
        }
    }
}