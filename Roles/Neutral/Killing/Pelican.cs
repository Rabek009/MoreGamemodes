using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class Pelican : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Eat");
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

        public override bool OnCheckMurderLate(PlayerControl target)
        {
            Player.RpcTeleport(target.transform.position);
            target.RpcSetDeathReason(DeathReasons.Eaten);
            target.RpcExileV2();
            target.RpcSetScanner(false);
            Player.RpcSetKillTimer(EatCooldown.GetFloat());
            ++Main.PlayerKills[Player.PlayerId];
            ClassicGamemode.instance.PlayerKiller[target.PlayerId] = Player.PlayerId;
            ClassicGamemode.instance.OnMurderPlayer(Player, target);
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
            opt.SetFloat(FloatOptionNames.KillCooldown, EatCooldown.GetFloat());
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
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorByKill, winners, CustomWinners.Pelican);
                return true;
            }
            return false;
        }

        public Pelican(PlayerControl player)
        {
            Role = CustomRoles.Pelican;
            BaseRole = BaseRoles.DesyncImpostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem EatCooldown;
        public static OptionItem CanUseVents;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(1000200, CustomRoles.Pelican, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(1000201, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            EatCooldown = FloatOptionItem.Create(1000202, "Eat cooldown", new(10f, 60f, 2.5f), 30f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CanUseVents = BooleanOptionItem.Create(1000203, "Can use vents", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Pelican] = Chance;
            Options.RolesCount[CustomRoles.Pelican] = Count;
        }
    }
}