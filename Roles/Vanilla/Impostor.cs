namespace MoreGamemodes
{
    public class Impostor : CustomRole
    {
        public Impostor(PlayerControl player)
        {
            Role = CustomRoles.Impostor;
            BaseRole = BaseRoles.Impostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}