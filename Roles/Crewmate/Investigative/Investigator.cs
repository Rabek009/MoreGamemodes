using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Investigator : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = InvestigateCooldown.GetFloat();
        }

        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Investigate");
            if (AbilityUses < 1f)
                __instance.KillButton.SetDisabled();
        }

        public override void OnIntroDestroy()
        {
            Cooldown = 10f;
        }

        public override void OnPet()
        {
            if (Main.IsModded[Player.PlayerId]) return;
            if (BaseRole == BaseRoles.Crewmate)
            {
                BaseRole = BaseRoles.DesyncImpostor;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                }
                Player.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.white;
                new LateTask(() => Player.RpcSetKillTimer(Cooldown > 0.001f ? Cooldown : 0.001f), 0.5f);
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                Player.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.clear;
            }
        }

        public override bool OnCheckMurder(PlayerControl target)
        {
            if (!Main.IsModded[Player.PlayerId] && Cooldown > 0f) return false;
            if (AbilityUses < 1f) return false;
            Main.NameColors[(target.PlayerId, Player.PlayerId)] = ShowRed(target) ? Palette.ImpostorRed : Palette.AcceptedGreen;
            Player.RpcSetAbilityUses(AbilityUses - 1f);
            Player.RpcSetKillTimer(InvestigateCooldown.GetFloat());
            Cooldown = InvestigateCooldown.GetFloat();
            return false;
        }

        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                Player.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.clear;
            }
        }

        public override void OnMeeting()
        {
            if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                Player.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.clear;
            }
        }

        public override void OnFixedUpdate()
        {
            if (Cooldown > 0f)
                Cooldown -= Time.fixedDeltaTime;
            if (Cooldown < 0f)
                Cooldown = 0f;
        }

        public override bool OnEnterVent(int id)
        {
            return false;
        }

        public override void OnCompleteTask()
        {
            if (AbilityUseGainWithEachTaskCompleted.GetFloat() <= 0f) return;
            Player.RpcSetAbilityUses(AbilityUses + AbilityUseGainWithEachTaskCompleted.GetFloat());
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.KillCooldown, InvestigateCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Main.IsModded[Player.PlayerId]) return "";
            if (BaseRole == BaseRoles.Crewmate)
            {
                return Utils.ColorString(Color, "\nMode: Task\n<size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")</size>\n") +
                    Utils.ColorString(Color.red, "Investigate cooldown: " + (int)(Cooldown + 0.99f) + "s");
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                return Utils.ColorString(Color, "\nMode: Investigate\n<size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")</size>");
            }
            return "";
        }

        public bool ShowRed(PlayerControl target)
        {
            return target.GetRole().IsImpostor() || (target.GetRole().IsNeutralKilling() && NeutralKillingShowRed.GetBool()) || (target.GetRole().IsNeutralEvil() && NeutralEvilShowRed.GetBool()) || 
                (target.GetRole().IsNeutralBenign() && NeutralBenignShowRed.GetBool()) || (target.GetRole().IsCrewmateKilling() && CrewmateKillingShowRed.GetBool());
        }

        public Investigator(PlayerControl player)
        {
            Role = CustomRoles.Investigator;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            ColorUtility.TryParseHtmlString("#118385", out Color);
            RoleName = "Investigator";
            RoleDescription = "See if someone is good or evil";
            RoleDescriptionLong = "Investigator (Crewmate): You can use pet button to switch between task and investigate mode. In task mode you can do tasks. In investigate mode you have kill button. If players is good, his name will become green. But if player is evil then his name will turn red. But some roles that are good can show as evil, also sometimes evil roles show as good. You have limited ability uses, but you can do tasks to increase it.\n\nIf you have mod installed, you don't have task and investigate mode. You can do tasks and investigate at the same time.";
            AbilityUses = InitialAbilityUseLimit.GetInt();
            Cooldown = 10f;
        }

        public float Cooldown;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem InvestigateCooldown;
        public static OptionItem NeutralKillingShowRed;
        public static OptionItem NeutralEvilShowRed;
        public static OptionItem NeutralBenignShowRed;
        public static OptionItem CrewmateKillingShowRed;
        public static OptionItem InitialAbilityUseLimit;
        public static OptionItem AbilityUseGainWithEachTaskCompleted;
        public static void SetupOptionItem()
        {
            ColorUtility.TryParseHtmlString("#118385", out Color c);
            Chance = IntegerOptionItem.Create(100100, "Investigator", new(0, 100, 5), 0, TabGroup.CrewmateRoles, false)
                .SetColor(c)
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(100101, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            InvestigateCooldown = FloatOptionItem.Create(100102, "Investigate cooldown", new(10f, 60f, 2.5f), 30f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            NeutralKillingShowRed = BooleanOptionItem.Create(100103, "Neutral killing show red", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            NeutralEvilShowRed = BooleanOptionItem.Create(100104, "Neutral evil show red", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            NeutralBenignShowRed = BooleanOptionItem.Create(100105, "Neutral benign show red", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CrewmateKillingShowRed = BooleanOptionItem.Create(100106, "Crewmate killing show red", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            InitialAbilityUseLimit = FloatOptionItem.Create(100107, "Initial ability use limit", new(1f, 15f, 1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(100108, "Ability use gain with each task completed", new(0f, 2f, 0.1f), 0.5f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Investigator] = Chance;
            Options.RolesCount[CustomRoles.Investigator] = Count;
        }
    }
}