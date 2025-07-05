using UnityEngine;

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
            player.GetRole().OnRevive();
            foreach (var addOn in player.GetAddOns())
                addOn.OnRevive();
            Revived = player.PlayerId;
            Killer = ClassicGamemode.instance.PlayerKiller[player.PlayerId];
            ClassicGamemode.instance.PlayerKiller[player.PlayerId] = byte.MaxValue;
            var killer = Utils.GetPlayerById(Killer);
            player.RpcReactorFlash(0.2f, Color);
            if (killer != null && killer != player && !killer.Data.IsDead)
                killer.RpcReactorFlash(0.2f, Color);
            return false;
        }

        public override void OnMeeting()
        {
            Revived = byte.MaxValue;
            Killer = byte.MaxValue;
        }

        public override void OnFixedUpdate()
        {
            if (Revived == byte.MaxValue || Killer == byte.MaxValue) return;
            var revived = Utils.GetPlayerById(Revived);
            var killer = Utils.GetPlayerById(Killer);
            if (revived != null && killer != null && revived != killer && !revived.Data.IsDead && !killer.Data.IsDead)
                ClassicGamemode.instance.NameSymbols[(Killer, Killer)][CustomRoles.Altruist] = ("★" + Utils.GetArrow(killer.transform.position, revived.transform.position), Color);
        }

        public override string GetNamePostfix()
        {
            if (SeeArrowToNearestBody.GetBool() && !Player.Data.IsDead && Player.GetClosestDeadBody() != null)
                return Utils.ColorString(Color, "\n" + Utils.GetArrow(Player.transform.position, Player.GetClosestDeadBody().transform.position));
            return "";
        }

        public override bool ShouldContinueGame()
        {
            return Object.FindObjectOfType<DeadBody>() != null;
        }

        public Altruist(PlayerControl player)
        {
            Role = CustomRoles.Altruist;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Revived = byte.MaxValue;
            Killer = byte.MaxValue;
        }

        public byte Revived;
        public byte Killer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem SeeArrowToNearestBody;
        public static OptionItem KillerSeeArrowToRevived;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(300300, CustomRoles.Altruist, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(300301, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            SeeArrowToNearestBody = BooleanOptionItem.Create(300302, "See arrow to nearest body", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            KillerSeeArrowToRevived = BooleanOptionItem.Create(300303, "Killer see arrow to revived", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Altruist] = Chance;
            Options.RolesCount[CustomRoles.Altruist] = Count;
        }
    }
}