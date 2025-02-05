using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class Arsonist : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            if (__instance.KillButton.currentTarget != null && DouseState[__instance.KillButton.currentTarget.PlayerId] == DouseStates.Doused)
                __instance.KillButton.OverrideText("Ignite");
            else
                __instance.KillButton.OverrideText("Douse");
            if (__instance.KillButton.currentTarget != null && DouseState[__instance.KillButton.currentTarget.PlayerId] == DouseStates.Ignited)
                __instance.KillButton.SetTarget(null);
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            if (!CanUseVents.GetBool())
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
            }
        }

        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            if (exiled != null && exiled.Object != null && exiled.Object == Player && BaseRole == BaseRoles.DesyncImpostor)
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
                Player.Data.RpcSetTasks(new byte[0]);
                Player.SyncPlayerSettings();
            }
        }

        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.Data.RpcSetTasks(new byte[0]);
                Player.SyncPlayerSettings();
            }
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead && BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.Data.RpcSetTasks(new byte[0]);
                Player.SyncPlayerSettings();
            }
        }

        public override bool OnEnterVent(int id)
        {
            return CanUseVents.GetBool();
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.KillCooldown, DouseCooldown.GetFloat());
            return opt;
        }

        public override bool CheckEndCriteria()
        {
            int playerCount = 0;
            bool isPlayerAlive = !Player.Data.IsDead;
            bool isKillerAlive = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if ((pc.GetRole().IsImpostor() || pc.GetRole().IsNeutralKilling()) && !pc.Data.IsDead && pc != Player)
                    isKillerAlive = true;
                if (pc.GetRole().IsCounted() && !pc.Data.IsDead)
                    ++playerCount;
            }
            if (!isKillerAlive && playerCount <= 2 && isPlayerAlive)
            {
                List<byte> winners = new();
                winners.Add(Player.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorByKill, winners, CustomWinners.Arsonist);
                return true;
            }
            return false;
        }

        public Arsonist(PlayerControl player)
        {
            Role = CustomRoles.Arsonist;
            BaseRole = BaseRoles.DesyncImpostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            DouseState = new Dictionary<byte, DouseStates>();
            IgniteTimer = new Dictionary<byte, float>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                DouseState[pc.PlayerId] = DouseStates.NotDoused;
                IgniteTimer[pc.PlayerId] = 0f;
            }
        }

        public Dictionary<byte, DouseStates> DouseState;
        public Dictionary<byte, float> IgniteTimer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem DouseCooldown;
        public static OptionItem IgniteCooldown;
        public static OptionItem IgniteDuration;
        public static OptionItem IgniteRadius;
        public static OptionItem CanUseVents;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(1000300, CustomRoles.Arsonist, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(1000301, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            DouseCooldown = FloatOptionItem.Create(1000302, "Douse cooldown", new(2.5f, 60f, 2.5f), 10f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            IgniteCooldown = FloatOptionItem.Create(1000303, "Ignite cooldown", new(5f, 60f, 2.5f), 20f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            IgniteDuration = FloatOptionItem.Create(1000304, "Ignite duration", new(1f, 30f, 0.5f), 10f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            IgniteRadius = FloatOptionItem.Create(1000305, "Ignite radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Multiplier);
            CanUseVents = BooleanOptionItem.Create(1000306, "Can use vents", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Arsonist] = Chance;
            Options.RolesCount[CustomRoles.Arsonist] = Count;
        }
    }

    public enum DouseStates
    {
        NotDoused,
        Doused,
        Ignited,
    }
}