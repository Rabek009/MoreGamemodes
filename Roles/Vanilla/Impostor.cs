namespace MoreGamemodes
{
    public class Impostor : CustomRole
    {
        public Impostor(PlayerControl player)
        {
            Role = CustomRoles.Impostor;
            BaseRole = BaseRoles.Impostor;
            Player = player;
            Color = Palette.ImpostorRed;
            RoleName = "Impostor";
            RoleDescription = "Kill and sabotage";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Impostor];
            AbilityUses = -1f;
        }
    }
}