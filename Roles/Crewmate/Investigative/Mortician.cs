namespace MoreGamemodes
{
    public class Mortician : CustomRole
    {
        public override bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            if (target == null) return true;
            var name = Utils.ColorString(Palette.PlayerColors[Main.StandardColors[target.PlayerId]], Main.StandardNames[target.PlayerId]);
            var role = target.GetRole();
            var killerRole = GameData.Instance.GetPlayerById(ClassicGamemode.instance.PlayerKiller[target.PlayerId]).GetRole();
            Player.RpcSendMessage("You reported " + name + "'s body!\n" + name + " was " + Utils.ColorString(role.Color, role.RoleName) + ".\nKiller's role is " + Utils.ColorString(killerRole.Color, killerRole.RoleName) + ".\n" + name + " died " + (int)(ClassicGamemode.instance.TimeSinceDeath[target.PlayerId] + 0.99f) + " second ago.", "Mortician");
            return true;
        }

        public override string GetNamePostfix()
        {
            if (SeeArrowToNearestBody.GetBool() && !Player.Data.IsDead && Player.GetClosestDeadBody() != null)
                return Utils.ColorString(Color, "\n" + Utils.GetArrow(Player.GetRealPosition(), Player.GetClosestDeadBody().transform.position));
            return "";
        }

        public override bool IsCompatible(AddOns addOn)
        {
            return addOn != AddOns.Oblivious;
        }

        public Mortician(PlayerControl player)
        {
            Role = CustomRoles.Mortician;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem SeeArrowToNearestBody;
        public static OptionItem CanBeGuessed;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(100200, CustomRoles.Mortician, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(100201, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            SeeArrowToNearestBody = BooleanOptionItem.Create(100202, "See arrow to nearest body", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanBeGuessed = BooleanOptionItem.Create(100203, "Can be guessed", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Mortician] = Chance;
            Options.RolesCount[CustomRoles.Mortician] = Count;
        }
    }
}