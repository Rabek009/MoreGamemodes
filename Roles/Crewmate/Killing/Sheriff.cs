using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using System;

namespace MoreGamemodes
{
    public class Sheriff : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = KillCooldown.GetFloat();
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
            if (!CanKill(target))
            {
                Player.RpcSetDeathReason(DeathReasons.Misfire);
                Player.RpcMurderPlayer(Player, true);
                return MisfireKillsTarget.GetBool();
            }
            return true;
        }

        public override void OnMurderPlayer(PlayerControl target)
        {
            Cooldown = KillCooldown.GetFloat();
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
            if (Player.Data.IsDead) return;
            if (Cooldown > 0f)
                Cooldown -= Time.fixedDeltaTime;
            if (Cooldown < 0f)
                Cooldown = 0f;
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
            opt.SetFloat(FloatOptionNames.KillCooldown, KillCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Main.IsModded[Player.PlayerId]) return "";
            if (BaseRole == BaseRoles.Crewmate)
            {
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Task\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")\n</size>") +
                    Utils.ColorString(Color.red, "<size=1.8>Kill cooldown: " + (int)(Cooldown + 0.99f) + "s</size>");
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Kill\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")</size>");
            }
            return "";
        }

        public override void OnRevive()
        {
            Cooldown = 10f;
        }

        public override bool ShouldContinueGame()
        {
            return true;
        }

        public bool CanKill(PlayerControl target)
        {
            if (target.GetRole().Role == CustomRoles.Jester) return CanKillJester.GetBool();
            return target.GetRole().IsImpostor() || (target.GetRole().IsNeutralKilling() && CanKillNeutralKilling.GetBool()) ||
                (target.GetRole().IsNeutralEvil() && CanKillNeutralEvil.GetBool()) || (target.GetRole().IsNeutralBenign() && CanKillNeutralBenign.GetBool());
        }

        public Sheriff(PlayerControl player)
        {
            Role = CustomRoles.Sheriff;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Cooldown = 10f;
        }

        public float Cooldown;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem KillCooldown;
        public static OptionItem MisfireKillsTarget;
        public static OptionItem CanKillNeutralKilling;
        public static OptionItem CanKillNeutralEvil;
        public static OptionItem CanKillNeutralBenign;
        public static OptionItem CanKillJester;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(200100, CustomRoles.Sheriff, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(200101, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            KillCooldown = FloatOptionItem.Create(200102, "Kill cooldown", new(10f, 60f, 2.5f), 25f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            MisfireKillsTarget = BooleanOptionItem.Create(200103, "Misfire kills target", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanKillNeutralKilling = BooleanOptionItem.Create(200104, "Can kill neutral killing", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanKillNeutralEvil = BooleanOptionItem.Create(200105, "Can kill neutral evil", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanKillNeutralBenign = BooleanOptionItem.Create(200106, "Can kill neutral benign", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanKillJester = BooleanOptionItem.Create(200107, "Can kill jester", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Sheriff] = Chance;
            Options.RolesCount[CustomRoles.Sheriff] = Count;
        }
    }
}