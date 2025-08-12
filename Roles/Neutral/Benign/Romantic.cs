using Hazel;
using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    public class Romantic : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (LoverId == byte.MaxValue || exiled == null) return;
            var lover = Utils.GetPlayerById(LoverId);
            if (lover != null && lover.GetRole().Role != CustomRoles.Romantic)
            {
                if (exiled.PlayerId == Player.PlayerId && !lover.Data.IsDead)
                {
                    lover.RpcSetDeathReason(DeathReasons.Heartbroken);
                    lover.RpcExileV2();
                    return;
                }
                if (exiled.PlayerId == LoverId && !Player.Data.IsDead)
                {
                    Player.RpcSetDeathReason(DeathReasons.Heartbroken);
                    Player.RpcExileV2();
                    return;
                }
                if (CanChatWithLover.GetBool() && !Options.EnableMidGameChat.GetBool() && !Player.Data.IsDead && !lover.Data.IsDead)
                {
                    new LateTask(() => {
                        if (!GameManager.Instance.ShouldCheckForGameEnd) return;
                        if (Player == null || Player.Data == null || Player.Data.IsDead || Player.Data.Disconnected || lover == null || lover.Data == null || lover.Data.IsDead || lover.Data.Disconnected || lover.GetRole().Role == CustomRoles.Romantic) return;
                        Player.SetChatVisible(true);
                        lover.SetChatVisible(true);
                    }, 0.9f);
                }
            }
        }

        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Romance");
            __instance.AbilityButton.OverrideText("Protect");
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            if (LoverId == byte.MaxValue || AbilityUses < 1f)
            {
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
            }
            if (LoverId != byte.MaxValue)
            {
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
            }
        }

        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            if (exiled != null && exiled.Object != null && exiled.Object == Player && (BaseRole == BaseRoles.DesyncImpostor || BaseRole == BaseRoles.DesyncPhantom))
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
            if (LoverId != byte.MaxValue) return false;
            if (target.GetRole().Role == CustomRoles.Romantic)
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) Target can't be romanced (!)"));
                return false;
            }
            LoverId = target.PlayerId;
            SendRPC();
            Player.RpcSetAbilityCooldown(10f);
            Player.Notify(Utils.ColorString(Color, Main.StandardNames[target.PlayerId] + " is now your lover!"));
            target.Notify(Utils.ColorString(Color, Main.StandardNames[Player.PlayerId] + " is now your lover!"));
            if (CanChatWithLover.GetBool() && !Options.EnableMidGameChat.GetBool())
            {
                Player.SetChatVisible(true);
                target.SetChatVisible(true);
                Player.Notify(Utils.ColorString(Color, "Type /lc MESSAGE during round to chat with lover privately"));
                target.Notify(Utils.ColorString(Color, "Type /lc MESSAGE during round to chat with lover privately"));
            }
            return false;
        }

        public override bool OnCheckMurderAsTarget(PlayerControl killer)
        {
            if (LoverId == byte.MaxValue) return true;
            if (killer.PlayerId == LoverId) return false;
            if (ProtectionTimer > 0f)
            {
                killer.RpcGuardAndKill(Player);
                return false;
            }
            return true;
        }

        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (BaseRole == BaseRoles.DesyncImpostor || BaseRole == BaseRoles.DesyncPhantom)
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
            if (LoverId == byte.MaxValue) return;
            var lover = Utils.GetPlayerById(LoverId);
            if (lover != null && !lover.Data.IsDead && lover.GetRole().Role != CustomRoles.Romantic)
            {
                lover.RpcSetDeathReason(DeathReasons.Heartbroken);
                lover.RpcMurderPlayer(lover, true);
            }
        }

        public override void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (Player.Data.IsDead || LoverId == byte.MaxValue) return;
            var lover = Utils.GetPlayerById(LoverId);
            if (lover != null && lover.GetRole().Role != CustomRoles.Romantic && target == lover)
            {
                Player.RpcSetDeathReason(DeathReasons.Heartbroken);
                Player.RpcMurderPlayer(Player, true);
            }
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead && (BaseRole == BaseRoles.DesyncImpostor || BaseRole == BaseRoles.DesyncPhantom))
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
            if (LoverId == byte.MaxValue) return;
            var lover = Utils.GetPlayerById(LoverId);
            if (lover == null || lover.GetRole().Role == CustomRoles.Romantic)
            {
                LoverId = byte.MaxValue;
                SendRPC();
                if (!Player.Data.IsDead)
                {
                    Player.RpcSetKillTimer(RomanceCooldown.GetFloat());
                    if (!MeetingHud.Instance && !Options.EnableMidGameChat.GetBool())
                        Player.SetChatVisible(false);
                }
            }
            if (LoverId == byte.MaxValue) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc == lover || pc.Data.IsDead)
                {
                    ClassicGamemode.instance.NameSymbols[(Player.PlayerId, pc.PlayerId)][CustomRoles.Romantic] = ("♥", Color);
                    ClassicGamemode.instance.NameSymbols[(LoverId, pc.PlayerId)][CustomRoles.Romantic] = ("♥", Color);
                }
            }
            if (Player.Data.IsDead) return;
            if (ProtectionTimer > 0f)
                ProtectionTimer -= Time.fixedDeltaTime;
            if (ProtectionTimer <= 0f && ProtectionTimer > -1f)
            {
                ProtectionTimer = -1f;
                Player.RpcResetAbilityCooldown();
            }
        }

        public override bool OnEnterVent(int id)
        {
            return false;
        }

        public override bool OnCheckVanish()
        {
            if (AbilityUses < 1f || LoverId == byte.MaxValue)
            {
                new LateTask(() => Player.RpcSetAbilityCooldown(0.001f), 0.2f);
                return false;
            }
            ProtectionTimer = ProtectDuration.GetFloat();
            Player.RpcSetAbilityUses(AbilityUses - 1f);
            new LateTask(() => Player.RpcSetAbilityCooldown(ProtectDuration.GetFloat()), 0.2f);
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.KillCooldown, RomanceCooldown.GetFloat());
            opt.SetFloat(FloatOptionNames.PhantomCooldown, ProtectCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (LoverId == byte.MaxValue || !CanProtect.GetBool()) return "";
            if (ProtectionTimer > 0f)
                return Utils.ColorString(Color.cyan, "\n<size=1.8>[PROTECTED]</size>");
            return "";
        }

        public override bool SeePlayerRole(PlayerControl player)
        {
            return SeeLoverRole.GetBool() && player.PlayerId == LoverId;
        }

        public override bool IsRoleRevealed(PlayerControl seer)
        {
            return SeeLoverRole.GetBool() && seer.PlayerId == LoverId;
        }

        public override int GetPlayerCount()
        {
            if (LoverId == byte.MaxValue) return 1;
            var lover = Utils.GetPlayerById(LoverId);
            if (lover != null && !lover.Data.IsDead && (lover.GetRole().IsImpostor() || lover.GetRole().IsNeutralKilling()))
                return 0;
            return 1;
        }

        public override void OnRevive()
        {
            if (BaseRole == BaseRoles.Crewmate)
            {
                BaseRole = CanProtect.GetBool() ? BaseRoles.DesyncPhantom : BaseRoles.DesyncImpostor;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                }
                Player.RpcSetDesyncRole(CanProtect.GetBool() ? RoleTypes.Phantom : RoleTypes.Impostor, Player);
                Player.SyncPlayerSettings();
                Player.RpcSetKillTimer(10f);
            }
            ProtectionTimer = -1f;
        }

        public void SendRPC()
        {
            if (Player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable, -1);
            writer.Write(LoverId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            LoverId = reader.ReadByte();
            if (Player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
        }

        public bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (target.PlayerId == LoverId && ProtectionTimer > 0f)
                return false;
            return true;
        }

        public static bool OnGlobalCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (target == null) return true;
            bool result = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Romantic && !pc.Data.IsDead)
                {
                    Romantic romanticRole = pc.GetRole() as Romantic;
                    if (romanticRole == null) continue;
                    if (!romanticRole.OnCheckMurder(killer, target))
                        result = false;
                }
            }
            if (!result)
                killer.RpcGuardAndKill(target);
            return result;
        }

        public Romantic(PlayerControl player)
        {
            Role = CustomRoles.Romantic;
            BaseRole = CanProtect.GetBool() ? BaseRoles.DesyncPhantom : BaseRoles.DesyncImpostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = CanProtect.GetBool() ? AbilityUseLimit.GetFloat() : -1f;
            LoverId = byte.MaxValue;
            ProtectionTimer = -1f;
        }

        public byte LoverId;
        public float ProtectionTimer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem RomanceCooldown;
        public static OptionItem CanProtect;
        public static OptionItem ProtectCooldown;
        public static OptionItem ProtectDuration;
        public static OptionItem AbilityUseLimit;
        public static OptionItem SeeLoverRole;
        public static OptionItem CanChatWithLover;
        public static OptionItem DisableChatDuringCommsSabotage;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(800300, CustomRoles.Romantic, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(800301, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            RomanceCooldown = FloatOptionItem.Create(800302, "Romance cooldown", new(5f, 60f, 2.5f), 15f, TabGroup.NeutralRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CanProtect = BooleanOptionItem.Create(800303, "Can protect", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            ProtectCooldown = FloatOptionItem.Create(800304, "Protect cooldown", new(5f, 60f, 5f), 20f, TabGroup.NeutralRoles, false)
                .SetParent(CanProtect)
                .SetValueFormat(OptionFormat.Seconds);
            ProtectDuration = FloatOptionItem.Create(800305, "Protect duration", new(3f, 30f, 1f), 10f, TabGroup.NeutralRoles, false)
                .SetParent(CanProtect)
                .SetValueFormat(OptionFormat.Seconds);
            AbilityUseLimit = FloatOptionItem.Create(800306, "Ability use limit", new(1f, 99f, 1f), 3f, TabGroup.NeutralRoles, false)
                .SetParent(CanProtect);
            SeeLoverRole = BooleanOptionItem.Create(800307, "See lover role", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            CanChatWithLover = BooleanOptionItem.Create(800308, "Can chat with lover", false, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            DisableChatDuringCommsSabotage = BooleanOptionItem.Create(800309, "Disable chat during comms sabotage", false, TabGroup.NeutralRoles, false)
                .SetParent(CanChatWithLover);
            Options.RolesChance[CustomRoles.Romantic] = Chance;
            Options.RolesCount[CustomRoles.Romantic] = Count;
        }
    }
}