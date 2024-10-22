namespace MoreGamemodes
{
    public class Shapeshifter : CustomRole
    {
        public Shapeshifter(PlayerControl player)
        {
            Role = CustomRoles.Shapeshifter;
            BaseRole = BaseRoles.Shapeshifter;
            Player = player;
            Color = Palette.ImpostorRed;
            RoleName = "Shapeshifter";
            RoleDescription = "Disguise yourself";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Shapeshifter];
            AbilityUses = -1f;
        }
    }
}