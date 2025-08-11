namespace MoreGamemodes
{
    public class Noisemaker : CustomRole
    {
        public override bool IsCompatible(AddOns addOn)
        {
            return addOn != AddOns.Bait;
        }

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