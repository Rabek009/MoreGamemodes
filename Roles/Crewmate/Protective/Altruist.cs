namespace MoreGamemodes
{
    public class Altruist : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            __instance.ReportButton.OverrideText("Revive");
        }

        public override bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            if (target == null || target.Object == null || target.Disconnected) return true;
            var player = target.Object;
            player.RpcRevive();
            Player.RpcSetDeathReason(DeathReasons.Suicide);
            Player.RpcExileV2();
            return false;
        }

        public override string GetNamePostfix()
        {
            if (SeeArrowToNearestBody.GetBool() && !Player.Data.IsDead && Player.GetClosestDeadBody() != null)
                return Utils.ColorString(Color, "\n" + Utils.GetArrow(Player.GetRealPosition(), Player.GetClosestDeadBody().transform.position));
            return "";
        }

        public Altruist(PlayerControl player)
        {
            Role = CustomRoles.Altruist;
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
            Chance = RoleOptionItem.Create(300300, CustomRoles.Altruist, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(300301, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            SeeArrowToNearestBody = BooleanOptionItem.Create(300302, "See arrow to nearest body", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Altruist] = Chance;
            Options.RolesCount[CustomRoles.Altruist] = Count;
        }
    }
}