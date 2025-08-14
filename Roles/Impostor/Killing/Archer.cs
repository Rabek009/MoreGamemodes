using UnityEngine;

namespace MoreGamemodes
{
    public class Archer : CustomRole
    {
        public override void OnHudUpdate(HudManager __instance)
        {
            __instance.ReportButton.OverrideText("Report");
            __instance.KillButton.OverrideText("Kill");
            __instance.PetButton.OverrideText("Shoot");
            if (ClassicGamemode.instance.IsOnPetAbilityCooldown[Player.PlayerId])
                __instance.PetButton.SetDisabled();
        }

        public override void OnPet()
        {
            if (Main.KillCooldowns[Player.PlayerId] > 0f || AbilityUses < 1f) return;
            PlayerControl target = Player.GetClosestPlayer(true);
            if (target == null || (target.GetRole().IsImpostor() && !CanKillImpostors.GetBool()) || Vector2.Distance(Player.transform.position, target.transform.position) > ArrowDistance.GetFloat() * 7f) return;
            ClassicGamemode.instance.PlayerKiller[target.PlayerId] = Player.PlayerId;
            ++Main.PlayerKills[Player.PlayerId];
            target.RpcSetDeathReason(DeathReasons.Shot);
            target.RpcMurderPlayer(target, true);
            Player.RpcSetKillTimer(Main.OptionKillCooldowns[Player.PlayerId]);
            Player.RpcSetAbilityUses(AbilityUses - 1f);
        }

        public override void OnMurderPlayer(PlayerControl target)
        {
            if (target.GetDeathReason() == DeathReasons.Shot) return;
            Player.RpcSetAbilityUses(AbilityUses + AbilityUseGainWithEachRegularKill.GetFloat());
        }

        public override void OnFixedUpdate()
        {
            if (MeetingHud.Instance || Player.Data.IsDead) return;
            if (ClassicGamemode.instance.IsOnPetAbilityCooldown[Player.PlayerId] && Main.KillCooldowns[Player.PlayerId] <= 0f)
                Player.RpcSetPetAbilityCooldown(false);
            if (!ClassicGamemode.instance.IsOnPetAbilityCooldown[Player.PlayerId] && Main.KillCooldowns[Player.PlayerId] > 0f)
                Player.RpcSetPetAbilityCooldown(true);
        }

        public Archer(PlayerControl player)
        {
            Role = CustomRoles.Archer;
            BaseRole = BaseRoles.Impostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = InitialAbilityUseLimit.GetFloat();
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CanKillImpostors;
        public static OptionItem ArrowDistance;
        public static OptionItem InitialAbilityUseLimit;
        public static OptionItem AbilityUseGainWithEachRegularKill;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(600300, CustomRoles.Archer, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(600301, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanKillImpostors = BooleanOptionItem.Create(600302, "Can kill impostors", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            ArrowDistance = FloatOptionItem.Create(600303, "Arrow distance", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Multiplier);
            InitialAbilityUseLimit = FloatOptionItem.Create(600304, "Initial ability use limit", new(0f, 15f, 1f), 1f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            AbilityUseGainWithEachRegularKill = FloatOptionItem.Create(600305, "Ability use gain with each kill", new(0f, 2f, 0.1f), 0.5f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Archer] = Chance;
            Options.RolesCount[CustomRoles.Archer] = Count;
        }
    }
}