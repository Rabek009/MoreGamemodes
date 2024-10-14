using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Phantom : CustomRole
    {
        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            if (Player.shouldAppearInvisible || Player.invisibilityAlpha < 1f)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, opt.GetFloat(FloatOptionNames.PlayerSpeedMod) * 1.2f);
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
            RoleDescriptionLong = "Phantom (Impostor): You can turn invisible for limited amount of time. When disappearing or appearing there is animation. While invisible, you can't kill, vent, repair sabotages, use platform and zipline. Other impostors can see you, when you're invisible. While invisible you're 20% faster.";
            AbilityUses = -1f;
        }
    }
}