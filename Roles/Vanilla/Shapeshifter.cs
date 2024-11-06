namespace MoreGamemodes
{
    public class Shapeshifter : CustomRole
    {
        public Shapeshifter(PlayerControl player)
        {
            Role = CustomRoles.Shapeshifter;
            BaseRole = BaseRoles.Shapeshifter;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}