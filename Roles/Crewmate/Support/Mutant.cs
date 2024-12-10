using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Mutant : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            __instance.ReportButton.OverrideText(TranslationController.Instance.GetString(StringNames.ReportButton));
            __instance.PetButton.OverrideText("Repair");
            if (AbilityUses < 1f || !Utils.IsSabotage() || Utils.IsActive(SystemTypes.MushroomMixupSabotage))
                __instance.PetButton.SetDisabled();
        }

        public override void OnPet()
        {
            if (!Utils.IsSabotage() || AbilityUses < 1f) return;
            if (Utils.IsActive(SystemTypes.MushroomMixupSabotage) && !Player.AmOwner && !Main.IsModded[Player.PlayerId])
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) You can't fix mushroom mixup (!)"));
                return;
            }
            Player.RpcSetAbilityUses(AbilityUses - 1f);
            if (Utils.IsActive(SystemTypes.Electrical))
            {
                var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
                if (switchSystem != null)
                {
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    switchSystem.IsDirty = true;
                }
            }
            if (Utils.IsActive(SystemTypes.Reactor))
            {
                ShipStatus.Instance.UpdateSystem(SystemTypes.Reactor, Player, 16);
            }
            if (Utils.IsActive(SystemTypes.Laboratory))
            {
                ShipStatus.Instance.UpdateSystem(SystemTypes.Laboratory, Player, 16);
            }
            if (Utils.IsActive(SystemTypes.LifeSupp))
            {
                ShipStatus.Instance.UpdateSystem(SystemTypes.LifeSupp, Player, 16);
            }
            if (Utils.IsActive(SystemTypes.HeliSabotage))
            {
                ShipStatus.Instance.UpdateSystem(SystemTypes.HeliSabotage, Player, 16);
                ShipStatus.Instance.UpdateSystem(SystemTypes.HeliSabotage, Player, 17);
            }
            if (Utils.IsActive(SystemTypes.Comms))
            {
                ShipStatus.Instance.UpdateSystem(SystemTypes.Comms, Player, 16);
                if (Main.RealOptions.GetByte(ByteOptionNames.MapId) is 1 or 5)
                    ShipStatus.Instance.UpdateSystem(SystemTypes.Comms, Player, 17);
            }
        }

        public override void OnCompleteTask()
        {
            if (AbilityUseGainWithEachTaskCompleted.GetFloat() <= 0f) return;
            Player.RpcSetAbilityUses(AbilityUses + AbilityUseGainWithEachTaskCompleted.GetFloat());
        }

        public Mutant(PlayerControl player)
        {
            Role = CustomRoles.Mutant;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = InitialAbilityUseLimit.GetInt();
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem InitialAbilityUseLimit;
        public static OptionItem AbilityUseGainWithEachTaskCompleted;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(400200, "Mutant", new(0, 100, 5), 0, TabGroup.CrewmateRoles, false)
                .SetColor(CustomRolesHelper.RoleColors[CustomRoles.Mutant])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(400201, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            InitialAbilityUseLimit = FloatOptionItem.Create(400202, "Initial ability use limit", new(0f, 99f, 1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(400203, "Ability use gain with each task completed", new(0f, 5f, 0.1f), 0.5f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Mutant] = Chance;
            Options.RolesCount[CustomRoles.Mutant] = Count;
        }
    }
}