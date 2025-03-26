using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class SoulCollector : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (TargetId == byte.MaxValue || exiled == null) return;
            if (exiled.PlayerId == Player.PlayerId)
            {
                TargetId = byte.MaxValue;
                Player.RpcSetSoulCollectorTarget(TargetId);
            }
            if (exiled.PlayerId == TargetId && GameManager.Instance.ShouldCheckForGameEnd)
            {
                GainSoul();
                TargetId = byte.MaxValue;
                Player.RpcSetSoulCollectorTarget(TargetId);
            }
        }

        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Predict");
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            if (TargetId != byte.MaxValue && !CanChangeTarget.GetBool())
            {
                __instance.KillButton.SetDisabled();
                __instance.KillButton.SetTarget(null);
            }
            if (TargetId != byte.MaxValue && CanChangeTarget.GetBool() && __instance.KillButton.currentTarget != null && __instance.KillButton.currentTarget.PlayerId == TargetId)
                __instance.KillButton.SetTarget(null);
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
            if (TargetId != byte.MaxValue && !CanChangeTarget.GetBool()) return false;
            if (target.PlayerId == TargetId) return false;
            TargetId = target.PlayerId;
            Player.RpcSetSoulCollectorTarget(TargetId);
            Player.RpcSetKillTimer(PredictCooldown.GetFloat());
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
            TargetId = byte.MaxValue;
            Player.RpcSetSoulCollectorTarget(TargetId);
        }

        public override void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (Player.Data.IsDead || TargetId == byte.MaxValue) return;
            var player = Utils.GetPlayerById(TargetId);
            if (player != null && target == player && GameManager.Instance.ShouldCheckForGameEnd)
            {
                GainSoul();
                TargetId = byte.MaxValue;
                Player.RpcSetSoulCollectorTarget(TargetId);
                Player.RpcSetKillTimer(PredictCooldown.GetFloat());
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
            if (TargetId == byte.MaxValue || Player.Data.IsDead) return;
            var player = Utils.GetPlayerById(TargetId);
            if ((player == null || player.Data.IsDead) && GameManager.Instance.ShouldCheckForGameEnd)
            {
                GainSoul();
                TargetId = byte.MaxValue;
                Player.RpcSetSoulCollectorTarget(TargetId);
                Player.RpcSetKillTimer(PredictCooldown.GetFloat());
            }
            if (TargetId == byte.MaxValue) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc.Data.IsDead)
                    ClassicGamemode.instance.NameSymbols[(TargetId, pc.PlayerId)][CustomRoles.SoulCollector] = ("▼", Color);
            }
        }

        public override bool OnEnterVent(int id)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.KillCooldown, PredictCooldown.GetFloat());
            return opt;
        }

        public override string GetProgressText(bool gameEnded)
        {
            return " (" + Souls + "/" + SoulsRequiredToWin.GetInt() + ")";
        }

        public override bool PreventGameEnd()
        {
            return Souls >= SoulsRequiredToWin.GetInt();
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

        public void GainSoul()
        {
            if (Player.Data.IsDead || !GameManager.Instance.ShouldCheckForGameEnd) return;
            ++Souls;
            new LateTask(() => {
                if (Player.Data.IsDead)
                {
                    --Souls;
                }
                else if (Souls >= SoulsRequiredToWin.GetInt() && GameManager.Instance.ShouldCheckForGameEnd && !Options.NoGameEnd.GetBool())
                {
                    List<byte> winners = new();
                    winners.Add(Player.PlayerId);
                    CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.CrewmatesByTask, winners, CustomWinners.SoulCollector);
                }
            }, 0f);
        }

        public SoulCollector(PlayerControl player)
        {
            Role = CustomRoles.SoulCollector;
            BaseRole = BaseRoles.DesyncImpostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = 1f;
            TargetId = byte.MaxValue;
            Souls = 0;
        }

        public byte TargetId;
        public int Souls;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem PredictCooldown;
        public static OptionItem CanChangeTarget;
        public static OptionItem SoulsRequiredToWin;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(900300, CustomRoles.SoulCollector, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(900301, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            PredictCooldown = FloatOptionItem.Create(900302, "Predict cooldown", new(5f, 60f, 2.5f), 15f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CanChangeTarget = BooleanOptionItem.Create(900303, "Can change target", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            SoulsRequiredToWin = IntegerOptionItem.Create(900304, "Souls required to win", new(1, 10, 1), 3, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.SoulCollector] = Chance;
            Options.RolesCount[CustomRoles.SoulCollector] = Count;
        }
    }
}