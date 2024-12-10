using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    public class TimeFreezer : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.AbilityButton.OverrideText("Freeze Time");
            if ((Utils.IsActive(SystemTypes.Reactor) || Utils.IsActive(SystemTypes.LifeSupp) || Utils.IsActive(SystemTypes.Laboratory) || Utils.IsActive(SystemTypes.HeliSabotage)) && !CanFreezeDuringCriticalSabotage.GetBool())
                __instance.AbilityButton.SetDisabled();
            if (!CanUseVents.GetBool())
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
            }
        }

        public override void OnFixedUpdate()
        {
            if (AbilityDuration <= -1f)
            {
                TimeSinceAbilityUse += Time.fixedDeltaTime;
            }
            if (AbilityDuration > -1f)
            {
                AbilityDuration -= Time.fixedDeltaTime;
            }
            if (AbilityDuration <= 0f && AbilityDuration > -1f)
            {
                AbilityDuration = -1f;
                TimeSinceAbilityUse = 0f;
                Player.RpcResetAbilityCooldown();
            }
        }

        public override bool OnCheckVanish()
        {
            if (AbilityDuration > 0f) return false;
            if (TimeSinceAbilityUse < 1f) return false;
            if ((Utils.IsActive(SystemTypes.Reactor) || Utils.IsActive(SystemTypes.LifeSupp) || Utils.IsActive(SystemTypes.Laboratory) || Utils.IsActive(SystemTypes.HeliSabotage)) && !CanFreezeDuringCriticalSabotage.GetBool())
                return false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player) continue;
                ClassicGamemode.instance.SetFreezeTimer(pc, FreezeDuration.GetFloat());
                ClassicGamemode.instance.SetBlindTimer(pc, FreezeDuration.GetFloat());
                ClassicGamemode.instance.SetRoleblockTimer(pc, FreezeDuration.GetFloat());
            }
            Utils.SyncAllSettings();
            Utils.SetAllVentInteractions();
            AbilityDuration = FreezeDuration.GetFloat();
            new LateTask(() => Player.RpcSetAbilityCooldown(FreezeDuration.GetFloat()), 0.2f);
            return false;
        }

        public override bool OnEnterVent(int id)
        {
            return CanUseVents.GetBool();
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.PhantomCooldown, FreezeCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (AbilityDuration > 0f)
                return Utils.ColorString(Palette.ImpostorRed, "\n<size=1.8>[TIME FROZEN]</size>");
            return "";
        }

        public TimeFreezer(PlayerControl player)
        {
            Role = CustomRoles.TimeFreezer;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
            TimeSinceAbilityUse = 0f;
        }

        public float AbilityDuration;
        public float TimeSinceAbilityUse;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem FreezeCooldown;
        public static OptionItem FreezeDuration;
        public static OptionItem CanFreezeDuringCriticalSabotage;
        public static OptionItem CanUseVents;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(500100, "Time freezer", new(0, 100, 5), 0, TabGroup.ImpostorRoles, false)
                .SetColor(CustomRolesHelper.RoleColors[CustomRoles.TimeFreezer])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(500101, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            FreezeCooldown = FloatOptionItem.Create(500102, "Freeze cooldown", new(10f, 60f, 5f), 25f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            FreezeDuration = FloatOptionItem.Create(500103, "Freeze duration", new(1f, 20f, 1f), 5f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CanFreezeDuringCriticalSabotage = BooleanOptionItem.Create(500104, "Can freeze during critical sabotage", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanUseVents = BooleanOptionItem.Create(500105, "Can use vents", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.TimeFreezer] = Chance;
            Options.RolesCount[CustomRoles.TimeFreezer] = Count;
        }
    }
}