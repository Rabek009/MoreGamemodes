using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class SerialKiller : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            base.OnHudUpate(__instance);
        }
        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            if (exiled != null && exiled.Object != null && exiled.Object == Player)
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

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.KillCooldown, KillCooldown.GetFloat());
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
                if (!pc.Data.IsDead)
                    ++playerCount;
            }
            if (!isKillerAlive && playerCount <= 2 && isPlayerAlive)
            {
                List<byte> winners = new();
                winners.Add(Player.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorByKill, winners, CustomWinners.SerialKiller);
                return true;
            }
            return false;
        }

        public SerialKiller(PlayerControl player)
        {
            Role = CustomRoles.SerialKiller;
            BaseRole = BaseRoles.DesyncImpostor;
            Player = player;
            ColorUtility.TryParseHtmlString("#63188f", out Color);
            RoleName = "Serial Killer";
            RoleDescription = "Kill everyone";
            RoleDescriptionLong = CustomRolesHelper.RoleDescriptions[CustomRoles.SerialKiller];
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem KillCooldown;
        public static void SetupOptionItem()
        {
            ColorUtility.TryParseHtmlString("#63188f", out Color c);
            Chance = IntegerOptionItem.Create(1000100, "Serial killer", new(0, 100, 5), 0, TabGroup.NeutralRoles, false)
                .SetColor(c)
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(1000101, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            KillCooldown = FloatOptionItem.Create(1000102, "Kill cooldown", new(1f, 30f, 0.5f), 15f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            Options.RolesChance[CustomRoles.SerialKiller] = Chance;
            Options.RolesCount[CustomRoles.SerialKiller] = Count;
        }
    }
}