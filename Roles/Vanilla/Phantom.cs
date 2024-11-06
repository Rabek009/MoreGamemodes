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
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }
    }
}