using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    public class Undertaker : CustomRole
    {
        public override void OnHudUpdate(HudManager __instance)
        {
            base.OnHudUpdate(__instance);
            __instance.AbilityButton.OverrideText("Select Target");
            if (IsLastImpostor())
                __instance.AbilityButton.SetDisabled();
        }

        public override void OnMeeting()
        {
            Target = byte.MaxValue;
        }

        public override bool OnCheckShapeshift(PlayerControl target)
        {
            if (Target != byte.MaxValue || target.Data.IsDead || IsTarget(target)) return false;
            if (!target.GetRole().IsImpostor())
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) You can only select impostors (!)"));
                return false;
            }
            Target = target.PlayerId;
            AbilityDuration = TargetTimeLimit.GetFloat();
            Player.RpcSetAbilityCooldown(TargetTimeLimit.GetFloat());
            target.Notify(Utils.ColorString(Color, "Undertaker chose you! Your victim's body will be teleported to undertaker!"));
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead || Target == byte.MaxValue) return;
            if (IsLastImpostor() != LastImpostor)
            {
                LastImpostor = IsLastImpostor();
                Player.SyncPlayerSettings();
            }
            var target = Utils.GetPlayerById(Target);
            AbilityDuration -= Time.fixedDeltaTime;
            if (target == null || target.Data.IsDead || !target.GetRole().IsImpostor() || AbilityDuration <= 0f)
            {
                Target = byte.MaxValue;
                AbilityDuration = 0f;
                Player.RpcResetAbilityCooldown();
            }
            if (Target == byte.MaxValue) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc.GetRole().IsImpostor() || pc.Data.IsDead)
                    ClassicGamemode.instance.NameSymbols[(Target, pc.PlayerId)][CustomRoles.Undertaker] = ("◌", Color);
            }
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, SelectTargetCooldown.GetFloat());
            if (IsLastImpostor())
                opt.SetFloat(FloatOptionNames.KillCooldown, opt.GetFloat(FloatOptionNames.KillCooldown) * ((100f - KillCooldownDecreaseWhenLastImpostor.GetInt()) / 100f));
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Target != byte.MaxValue)
                return Utils.ColorString(Color, "\n<size=1.8>Target: " + Main.StandardNames[Target] + "</size>");
            return Utils.ColorString(Color, "<size=1.8>Target: <b>None</b></size>");
        }

        public bool IsLastImpostor()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc != Player && pc.GetRole().IsImpostor() && !pc.Data.IsDead)
                    return false;
            }
            return true;
        }

        public bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (killer.GetRole().IsImpostor() && killer.PlayerId == Target)
            {
                if (killer.GetRole().Role != CustomRoles.Droner || (killer.GetRole() as Droner)?.ControlledDrone == null)
                    killer.RpcTeleport(target.transform.position);
                target.RpcTeleport(Player.transform.position);
                ClassicGamemode.instance.PlayerKiller[target.PlayerId] = killer.PlayerId;
                ++Main.PlayerKills[killer.PlayerId];
                target.RpcMurderPlayer(target, true);
                killer.RpcSetKillTimer(Main.OptionKillCooldowns[killer.PlayerId]);
                Target = byte.MaxValue;
                AbilityDuration = 0f;
                Player.RpcResetAbilityCooldown();
                return false;
            }
            return true;
        }

        public static bool OnGlobalCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (target == null) return true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Undertaker && !pc.Data.IsDead)
                {
                    Undertaker undertakerRole = pc.GetRole() as Undertaker;
                    if (undertakerRole == null) continue;
                    if (!undertakerRole.OnCheckMurder(killer, target))
                        return false;
                }
            }
            return true;
        }

        public static bool IsTarget(PlayerControl player)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Undertaker && !pc.Data.IsDead)
                {
                    Undertaker undertakerRole = pc.GetRole() as Undertaker;
                    if (undertakerRole == null) continue;
                    if (undertakerRole.Target == player.PlayerId)
                        return true;
                }
            }
            return false;
        }

        public Undertaker(PlayerControl player)
        {
            Role = CustomRoles.Undertaker;
            BaseRole = BaseRoles.Shapeshifter;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Target = byte.MaxValue;
            AbilityDuration = 0f;
            LastImpostor = false;
        }

        public byte Target;
        public float AbilityDuration;
        public bool LastImpostor;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem SelectTargetCooldown;
        public static OptionItem TargetTimeLimit;
        public static OptionItem KillCooldownDecreaseWhenLastImpostor;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(700300, CustomRoles.Undertaker, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(700301, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            SelectTargetCooldown = FloatOptionItem.Create(700302, "Select target cooldown", new(5f, 90f, 5f), 20f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            TargetTimeLimit = FloatOptionItem.Create(700303, "Target time limit", new(5f, 90f, 5f), 15f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            KillCooldownDecreaseWhenLastImpostor = IntegerOptionItem.Create(700304, "Kill cooldown decrease when last impostor", new(0, 50, 5), 20, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Percent);
            Options.RolesChance[CustomRoles.Undertaker] = Chance;
            Options.RolesCount[CustomRoles.Undertaker] = Count;
        }
    }
}