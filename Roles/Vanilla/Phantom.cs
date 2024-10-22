using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Phantom : CustomRole
    {
        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            if (Player.shouldAppearInvisible || Player.invisibilityAlpha < 1f)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, opt.GetFloat(FloatOptionNames.PlayerSpeedMod) * 1.1f);
            return opt;
        }

        public Phantom(PlayerControl player)
        {
            Role = CustomRoles.Phantom;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Color = Palette.ImpostorRed;
            RoleName = "Phantom";
            RoleDescription = "Turn invisible";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.Phantom];
            AbilityUses = -1f;
        }
    }
}