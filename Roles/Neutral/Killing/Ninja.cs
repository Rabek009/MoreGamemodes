using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;
using UnityEngine;

namespace MoreGamemodes
{
    public class Ninja : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            if (!CanUseVents.GetBool())
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
            }
        }

        public override bool OnCheckMurderLate(PlayerControl target)
        {
            if (AbilityDuration <= 0f) return true;
            ClassicGamemode.instance.PlayerKiller[target.PlayerId] = Player.PlayerId;
            ++Main.PlayerKills[Player.PlayerId];
            target.RpcMurderPlayer(target, true);
            Player.RpcSetKillTimer(Main.OptionKillCooldowns[Player.PlayerId]);
            return false;
        }

        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            if (exiled != null && exiled.Object != null && exiled.Object == Player && BaseRole == BaseRoles.DesyncPhantom)
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
            if (BaseRole == BaseRoles.DesyncPhantom)
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
            if (Player.Data.IsDead && BaseRole == BaseRoles.DesyncPhantom)
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
            if (Player.Data.IsDead) return;
            if (AbilityDuration > -1f)
            {
                AbilityDuration -= Time.fixedDeltaTime;
            }
            if (AbilityDuration <= 0f && AbilityDuration > -1f)
            {
                AbilityDuration = -1f;
                Player.RpcMakeVisible();
                Player.SyncPlayerSettings();
                Player.RpcSetVentInteraction();
                Player.RpcResetAbilityCooldown();
            }
        }

        public override bool OnCheckVanish()
        {
            if (Utils.IsSabotage())
            {
                new LateTask(() => Player.RpcSetAbilityCooldown(0.001f), 0.2f);
                return false;
            }
            Player.RpcMakeInvisible();
            Player.SyncPlayerSettings();
            Player.RpcSetVentInteraction();
            AbilityDuration = VanishDuration.GetFloat();
            new LateTask(() => Player.RpcSetAbilityCooldown(VanishDuration.GetFloat()), 0.2f);
            return false;
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
            opt.SetFloat(FloatOptionNames.KillCooldown, KillCooldown.GetFloat());
            opt.SetFloat(FloatOptionNames.PhantomCooldown, VanishCooldown.GetFloat());
            if (Player.shouldAppearInvisible || Player.invisibilityAlpha < 1f)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, opt.GetFloat(FloatOptionNames.PlayerSpeedMod) * (1f + (SpeedIncreaseWhileInvisible.GetInt() / 100f)));
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (AbilityDuration > 0f)
                return Utils.ColorString(Color, "\n<size=1.8>[INVISIBLE]</size>");
            return "";
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
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorsByKill, winners, CustomWinners.Ninja);
                return true;
            }
            return false;
        }

        public override void OnRevive()
        {
            AbilityDuration = -1f;
            if (BaseRole == BaseRoles.Crewmate)
            {
                BaseRole = BaseRoles.DesyncPhantom;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                }
                Player.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                Player.SyncPlayerSettings();
                Player.RpcSetKillTimer(10f);
            }
        }

        public Ninja(PlayerControl player)
        {
            Role = CustomRoles.Ninja;
            BaseRole = BaseRoles.DesyncPhantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
        }

        public float AbilityDuration;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem KillCooldown;
        public static OptionItem VanishCooldown;
        public static OptionItem VanishDuration;
        public static OptionItem SpeedIncreaseWhileInvisible;
        public static OptionItem CanUseVents;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(1000400, CustomRoles.Ninja, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(1000401, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            KillCooldown = FloatOptionItem.Create(1000402, "Kill cooldown", new(10f, 60f, 2.5f), 25f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            VanishCooldown = FloatOptionItem.Create(1000403, "Vanish cooldown", new(10f, 60f, 5f), 25f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            VanishDuration = FloatOptionItem.Create(1000404, "Vanish duration", new(1f, 15f, 1f), 3f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            SpeedIncreaseWhileInvisible = IntegerOptionItem.Create(1000405, "Speed increase while invisible", new(10, 300, 10), 100, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Percent);
            CanUseVents = BooleanOptionItem.Create(1000406, "Can use vents", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Ninja] = Chance;
            Options.RolesCount[CustomRoles.Ninja] = Count;
        }
    }
}