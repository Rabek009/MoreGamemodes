using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using System.Collections.Generic;

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
            if (AbilityDuration > 0f && KilledPlayers.Contains(target.PlayerId)) return false;
            if (AbilityDuration > 0f)
            {
                KilledPlayers.Add(target.PlayerId);
                Player.RpcTeleport(target.transform.position);
                Player.RpcSetKillTimer(Main.OptionKillCooldowns[Player.PlayerId]);
                return false;
            }
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
                foreach (var playerId in KilledPlayers)
                {
                    var player = Utils.GetPlayerById(playerId);
                    if (player != null && !player.Data.IsDead)
                    {
                        ClassicGamemode.instance.PlayerKiller[player.PlayerId] = Player.PlayerId;
                        ++Main.PlayerKills[Player.PlayerId];
                        player.RpcMurderPlayer(player, true);
                    }
                }
                KilledPlayers = new List<byte>();
                AbilityDuration = -1f;
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
            KilledPlayers = new List<byte>();
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
            KilledPlayers = new List<byte>();
        }

        public TimeFreezer(PlayerControl player)
        {
            Role = CustomRoles.TimeFreezer;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
            KilledPlayers = new List<byte>();
        }

        public float AbilityDuration;
        public List<byte> KilledPlayers;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem FreezeCooldown;
        public static OptionItem FreezeDuration;
        public static OptionItem CanUseVents;
        public static OptionItem CanKillDuringFreeze;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(500100, CustomRoles.TimeFreezer, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(500101, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            FreezeCooldown = FloatOptionItem.Create(500102, "Freeze cooldown", new(10f, 60f, 5f), 25f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            FreezeDuration = FloatOptionItem.Create(500103, "Freeze duration", new(1f, 20f, 1f), 4f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CanUseVents = BooleanOptionItem.Create(500104, "Can use vents", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CanKillDuringFreeze = BooleanOptionItem.Create(500105, "Can kill during freeze", true, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.TimeFreezer] = Chance;
            Options.RolesCount[CustomRoles.TimeFreezer] = Count;
        }
    }
}