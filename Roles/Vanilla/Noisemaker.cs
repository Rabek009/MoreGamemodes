namespace MoreGamemodes
{
    public class Noisemaker : CustomRole
    {
        public Noisemaker(PlayerControl player)
        {
            Role = CustomRoles.Noisemaker;
            BaseRole = BaseRoles.Noisemaker;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}