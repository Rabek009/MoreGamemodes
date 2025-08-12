using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using System;

namespace MoreGamemodes
{
    public class Medic : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = ShieldCooldown.GetFloat();
        }

        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Shield");
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
                Player.RpcSetKillTimer(Math.Max(Cooldown, 0.001f));
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
            if (ShieldedPlayer != byte.MaxValue || AbilityUses < 1f) return false;
            ShieldedPlayer = target.PlayerId;
            Player.RpcSetAbilityUses(0f);
            Player.RpcSetKillTimer(ShieldCooldown.GetFloat());
            Cooldown = ShieldCooldown.GetFloat();
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
            if (Player.Data.IsDead)
            {
                ShieldedPlayer = byte.MaxValue;
                if (AbilityUses > -1f)
                    Player.RpcSetAbilityUses(-1f);
                return;
            }
            if (Cooldown > 0f)
                Cooldown -= Time.fixedDeltaTime;
            if (Cooldown < 0f)
                Cooldown = 0f;
            if (ShieldedPlayer == byte.MaxValue) return;
            var player = Utils.GetPlayerById(ShieldedPlayer);
            if (player == null || player.Data.IsDead)
            {
                ShieldedPlayer = byte.MaxValue;
                Player.RpcSetAbilityUses(1f);
                Player.RpcSetKillTimer(ShieldCooldown.GetFloat());
                Cooldown = ShieldCooldown.GetFloat();
                Player.Notify(Utils.ColorString(Color.red, "Shielded player died!"));
            }
            if (ShieldedPlayer == byte.MaxValue) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || (pc == player && ShieldedCanSeeShield.GetBool()) || pc.Data.IsDead)
                    ClassicGamemode.instance.NameSymbols[(ShieldedPlayer, pc.PlayerId)][CustomRoles.Medic] = ("✚", Color);
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
            opt.SetFloat(FloatOptionNames.KillCooldown, ShieldCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Main.IsModded[Player.PlayerId]) return "";
            if (BaseRole == BaseRoles.Crewmate)
            {
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Task\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")\n</size>") +
                    Utils.ColorString(Color.red, "<size=1.8>Shield cooldown: " + (int)(Cooldown + 0.99f) + "s</size>");
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Shield\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")</size>");
            }
            return "";
        }

        public override void OnRevive()
        {
            Cooldown = 10f;
            ShieldedPlayer = byte.MaxValue;
            Player.RpcSetAbilityUses(1f);
        }

        public bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (target.PlayerId == ShieldedPlayer)
            {
                Player.RpcReactorFlash(0.3f, Color);
                Player.Notify(Utils.ColorString(Color, "Someone attacked shielded player!"));
                return false;
            }
            return true;
        }

        public static bool OnGlobalCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (target == null) return true;
            bool result = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Medic && !pc.Data.IsDead)
                {
                    Medic medicRole = pc.GetRole() as Medic;
                    if (medicRole == null) continue;
                    if (!medicRole.OnCheckMurder(killer, target))
                        result = false;
                }
            }
            if (!result && ResetKillCooldown.GetBool())
                killer.RpcGuardAndKill(target);
            else if (!result)
                killer.RpcSetKillTimer(1f);
            return result;
        }

        public Medic(PlayerControl player)
        {
            Role = CustomRoles.Medic;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = 1f;
            Cooldown = 10f;
            ShieldedPlayer = byte.MaxValue;
        }

        public float Cooldown;
        public byte ShieldedPlayer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem ShieldCooldown;
        public static OptionItem ResetKillCooldown;
        public static OptionItem ShieldedCanSeeShield;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(300200, CustomRoles.Medic, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(300201, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            ShieldCooldown = FloatOptionItem.Create(300202, "Shield cooldown", new(5f, 60f, 2.5f), 15f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            ResetKillCooldown = BooleanOptionItem.Create(300203, "Reset kill cooldown", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            ShieldedCanSeeShield = BooleanOptionItem.Create(300204, "Shielded can see shield", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Medic] = Chance;
            Options.RolesCount[CustomRoles.Medic] = Count;
        }
    }
}