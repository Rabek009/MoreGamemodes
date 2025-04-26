using AmongUs.GameOptions;
using Hazel;
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
            if (Utils.IsSabotage())
                __instance.AbilityButton.SetDisabled();
            if (!CanUseVents.GetBool())
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
            }
        }

        public override bool OnCheckMurder(PlayerControl target)
        {
            if (AbilityDuration > 0f && !CanKillDuringFreeze.GetBool()) return false;
            return true;
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            if (AbilityDuration > -1f)
            {
                AbilityDuration -= Time.fixedDeltaTime;
            }
            if (AbilityDuration <= 0f && AbilityDuration > -1f)
            {
                AbilityDuration = -1f;
                Player.RpcResetAbilityCooldown();
            }
            if (!LastIsSabotage && Utils.IsSabotage())
            {
                Player.RpcSetAbilityCooldown(255f);
                LastIsSabotage = true;
            }
            if (LastIsSabotage && !Utils.IsSabotage())
            {
                Player.RpcSetAbilityCooldown(CooldownAfterSabotage.GetFloat());
                LastIsSabotage = false;
            }
        }

        public override bool OnCheckVanish()
        {
            if (Utils.IsSabotage())
            {
                new LateTask(() => Player.RpcSetAbilityCooldown(0.001f), 0.2f);
                return false;
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player) continue;
                ClassicGamemode.instance.SetFreezeTimer(pc, FreezeDuration.GetFloat());
                ClassicGamemode.instance.SetBlindTimer(pc, FreezeDuration.GetFloat());
                ClassicGamemode.instance.SetRoleblockTimer(pc, FreezeDuration.GetFloat());
                pc.SyncPlayerSettings();
            }
            Utils.SetAllVentInteractions();
            AbilityDuration = FreezeDuration.GetFloat();
            new LateTask(() => Player.RpcSetAbilityCooldown(FreezeDuration.GetFloat()), 0.2f);
            var sabotageSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>();
            if (sabotageSystem != null && sabotageSystem.Timer < CooldownAfterSabotage.GetFloat() + FreezeDuration.GetFloat())
            {
                sabotageSystem.Timer = CooldownAfterSabotage.GetFloat() + FreezeDuration.GetFloat();
                sabotageSystem.IsDirty = true;
            }
            return false;
        }

        public override bool OnEnterVent(int id)
        {
            return CanUseVents.GetBool();
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage && AbilityDuration > 0f) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.PhantomCooldown, FreezeCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (AbilityDuration > 0f)
                return Utils.ColorString(Color, "\n<size=1.8>[TIME FROZEN]</size>");
            return "";
        }

        public override bool IsCompatible(AddOns addOn)
        {
            return addOn != AddOns.Lurker || CanUseVents.GetBool();
        }

        public override void OnRevive()
        {
            AbilityDuration = -1f;
            if (Utils.IsSabotage())
                Player.RpcSetAbilityCooldown(255f);
            LastIsSabotage = Utils.IsSabotage();
        }

        public TimeFreezer(PlayerControl player)
        {
            Role = CustomRoles.TimeFreezer;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
            LastIsSabotage = Utils.IsSabotage();
        }

        public float AbilityDuration;
        public bool LastIsSabotage;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem FreezeCooldown;
        public static OptionItem FreezeDuration;
        public static OptionItem CanUseVents;
        public static OptionItem CanKillDuringFreeze;
        public static OptionItem CooldownAfterSabotage;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(500100, CustomRoles.TimeFreezer, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(500101, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            FreezeCooldown = FloatOptionItem.Create(500102, "Freeze cooldown", new(10f, 60f, 5f), 30f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            FreezeDuration = FloatOptionItem.Create(500103, "Freeze duration", new(1f, 20f, 1f), 5f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CanUseVents = BooleanOptionItem.Create(500104, "Can use vents", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanKillDuringFreeze = BooleanOptionItem.Create(500105, "Can kill during freeze", true, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CooldownAfterSabotage = FloatOptionItem.Create(500106, "Cooldown after sabotage", new(1f, 45f, 1f), 10f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            Options.RolesChance[CustomRoles.TimeFreezer] = Chance;
            Options.RolesCount[CustomRoles.TimeFreezer] = Count;
        }
    }
}