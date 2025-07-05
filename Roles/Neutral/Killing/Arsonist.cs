using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;
using UnityEngine;
using InnerNet;

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

        public override bool OnCheckMurder(PlayerControl target)
        {
            if (DouseState[target.PlayerId] == DouseStates.NotDoused)
            {
                SendRPC(target, DouseStates.Doused);
                Player.RpcSetKillTimer(DouseIgniteCooldown.GetFloat());
                Main.NameColors[(target.PlayerId, Player.PlayerId)] = Color.black;
            }
            else if (DouseState[target.PlayerId] == DouseStates.Doused)
            {
                SendRPC(target, DouseStates.Ignited);
                IgniteTimer[target.PlayerId] = IgniteDuration.GetFloat();
                Player.RpcSetKillTimer(DouseIgniteCooldown.GetFloat());
                Main.NameColors[(target.PlayerId, Player.PlayerId)] = Palette.Orange;
            }
            return false;
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

        public void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (DouseState[pc.PlayerId] == DouseStates.Ignited && pc != Player && !pc.Data.IsDead)
                {
                    pc.RpcSetDeathReason(DeathReasons.Burned);
                    pc.RpcMurderPlayer(pc, true);
                    IgniteTimer[pc.PlayerId] = -1f;
                    ++Main.PlayerKills[Player.PlayerId];
                    ClassicGamemode.instance.PlayerKiller[pc.PlayerId] = Player.PlayerId;
                }
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
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (DouseState[pc.PlayerId] != DouseStates.Ignited || pc == Player || pc.Data.IsDead) continue;
                if (IgniteTimer[pc.PlayerId] <= -1f)
                {
                    SendRPC(pc, DouseStates.Doused);
                    continue;
                }
                if (IgniteTimer[pc.PlayerId] > 0f)
                {
                    IgniteTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                    foreach (var ar in PlayerControl.AllPlayerControls)
                    {
                        if (DouseState[ar.PlayerId] == DouseStates.Doused && Vector2.Distance(pc.transform.position, ar.transform.position) <= IgniteRadius.GetFloat())
                        {
                            SendRPC(ar, DouseStates.Ignited);
                            IgniteTimer[ar.PlayerId] = IgniteDuration.GetFloat();
                            Main.NameColors[(ar.PlayerId, Player.PlayerId)] = Palette.Orange;
                        }
                    }
                }
                if (IgniteTimer[pc.PlayerId] <= 0f && IgniteTimer[pc.PlayerId] > -1f)
                {
                    pc.RpcSetDeathReason(DeathReasons.Burned);
                    pc.RpcMurderPlayer(pc, true);
                    IgniteTimer[pc.PlayerId] = -1f;
                    ++Main.PlayerKills[Player.PlayerId];
                    ClassicGamemode.instance.PlayerKiller[pc.PlayerId] = Player.PlayerId;
                }
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
            opt.SetFloat(FloatOptionNames.KillCooldown, DouseIgniteCooldown.GetFloat());
            return opt;
        }

        public override bool CheckEndCriteria()
        {
            int playerCount = 0;
            bool isPlayerAlive = !Player.Data.IsDead;
            bool isKillerAlive = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if ((pc.GetRole().IsImpostor() || pc.GetRole().IsNeutralKilling() || pc.GetRole().ShouldContinueGame()) && !pc.Data.IsDead && pc != Player)
                    isKillerAlive = true;
                if (!pc.Data.IsDead)
                    playerCount += pc.GetRole().GetPlayerCount();
            }
            if (!isKillerAlive && playerCount <= 2 && isPlayerAlive)
            {
                List<byte> winners = new();
                winners.Add(Player.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorsByKill, winners, CustomWinners.Arsonist);
                return true;
            }
            return false;
        }

        public override void OnRevive()
        {
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
                new LateTask(() => Player.RpcSetKillTimer(9.5f), 0.5f);
            }
        }

        public void SendRPC(PlayerControl target, DouseStates douseState)
        {
            DouseState[target.PlayerId] = douseState;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable, -1);
            writer.WriteNetObject(target);
            writer.Write((int)douseState);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            PlayerControl target = reader.ReadNetObject<PlayerControl>();
            DouseStates douseState = (DouseStates)reader.ReadInt32();
            DouseState[target.PlayerId] = douseState;
        }

        public static void OnGlobalReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Arsonist)
                {
                    Arsonist arsonistRole = pc.GetRole() as Arsonist;
                    if (arsonistRole == null) continue;
                    arsonistRole.OnReportDeadBody(reporter, target);
                }
            }
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
                IgniteTimer[pc.PlayerId] = -1f;
            }
        }

        public Dictionary<byte, DouseStates> DouseState;
        public Dictionary<byte, float> IgniteTimer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem DouseIgniteCooldown;
        public static OptionItem IgniteDuration;
        public static OptionItem IgniteRadius;
        public static OptionItem CanUseVents;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(1000300, CustomRoles.Arsonist, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(1000301, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            DouseIgniteCooldown = FloatOptionItem.Create(1000302, "Douse/Ignite cooldown", new(2.5f, 60f, 2.5f), 12.5f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            IgniteDuration = FloatOptionItem.Create(1000303, "Ignite duration", new(1f, 30f, 0.5f), 10f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            IgniteRadius = FloatOptionItem.Create(1000304, "Ignite radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Multiplier);
            CanUseVents = BooleanOptionItem.Create(1000305, "Can use vents", true, TabGroup.NeutralRoles, false)
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