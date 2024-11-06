namespace MoreGamemodes
{
    public class Tracker : CustomRole
    {
        public Tracker(PlayerControl player)
        {
            Role = CustomRoles.Tracker;
            BaseRole = BaseRoles.Tracker;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}