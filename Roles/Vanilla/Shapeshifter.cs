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
            RoleDescriptionLong = "Shapeshifter (Impostor): You can shapeshift into other players. You can stay in shapeshifted form for limited time. When shapeshifting there is animation and depending on options you leave shapeshift evidence.";
            AbilityUses = -1f;
        }
    }
}