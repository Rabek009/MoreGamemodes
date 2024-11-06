namespace MoreGamemodes
{
    public class Engineer : CustomRole
    {
        public Engineer(PlayerControl player)
        {
            Role = CustomRoles.Engineer;
            BaseRole = BaseRoles.Engineer;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}