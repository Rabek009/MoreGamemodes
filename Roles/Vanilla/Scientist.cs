namespace MoreGamemodes
{
    public class Scientist : CustomRole
    {
        public Scientist(PlayerControl player)
        {
            Role = CustomRoles.Scientist;
            BaseRole = BaseRoles.Scientist;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}