namespace MoreGamemodes
{
    public class Crewmate : CustomRole
    {
        public Crewmate(PlayerControl player)
        {
            Role = CustomRoles.Crewmate;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}