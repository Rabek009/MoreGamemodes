using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class Hitman : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.AbilityButton.OverrideText("Target Change");
            if (__instance.KillButton.currentTarget != null && __instance.KillButton.currentTarget.PlayerId != Target)
                __instance.KillButton.SetTarget(null);
        }

        public override bool OnCheckMurder(PlayerControl target)
        {
            if (target.PlayerId != Target) return false;
            return true;
        }

        public override void OnMurderPlayer(PlayerControl target)
        {
            if (target.PlayerId == Target)
            {
                new LateTask(() => Player.RpcSetKillTimer(0.5f), 0.5f);
                ChangeTarget();
            }
        }

        public override bool OnCheckShapeshift(PlayerControl target)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            Timer += Time.fixedDeltaTime;
            PlayerControl targetPlayer = Utils.GetPlayerById(Target);
            if (Target == byte.MaxValue || targetPlayer == null || targetPlayer.Data.IsDead || targetPlayer.GetRole().IsImpostor() || Timer >= TargetChangeTime.GetFloat())
                ChangeTarget();
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, TargetChangeTime.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Target != byte.MaxValue)
            {
                if (ShowArrowToTarget.GetBool() && Utils.GetPlayerById(Target) != null)
                    return Utils.ColorString(Color, "\n<size=1.8>Target: " + Main.StandardNames[Target] + "</size>\n" + Utils.GetArrow(Player.GetRealPosition(), Utils.GetPlayerById(Target).transform.position));
                return Utils.ColorString(Color, "\n<size=1.8>Target: " + Main.StandardNames[Target] + "</size>");
            }
            return Utils.ColorString(Color, "\n<size=1.8>Target: None</size>");
        }

        public override void OnRevive()
        {
            ChangeTarget();
        }

        public void SendRPC()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpc(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable);
            writer.Write(Target);
            writer.EndMessage();
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            Target = reader.ReadByte();
        }

        public void ChangeTarget()
        {
            if (Target != byte.MaxValue)
                Main.NameColors[(Target, Player.PlayerId)] = Color.clear;
            Target = byte.MaxValue;
            List<NetworkedPlayerInfo> PotentialTargets = new();
            foreach (var player in GameData.Instance.AllPlayers)
            {
                if (player != null && player.Object != null && !player.IsDead && !player.Disconnected && !player.GetRole().IsImpostor())
                {
                    if (player.GetRole().Role == CustomRoles.Romantic)
                    {
                        Romantic romanticRole = player.GetRole() as Romantic;
                        if (romanticRole != null && romanticRole.LoverId == Player.PlayerId)
                            break;
                    }
                    PotentialTargets.Add(player);
                }
            }
            if (PotentialTargets.Any())
            {
                var rand = new System.Random();
                Target = PotentialTargets[rand.Next(0, PotentialTargets.Count)].PlayerId;
                Main.NameColors[(Target, Player.PlayerId)] = Color.black;
            }
            SendRPC();
            Timer = 0f;
            Player.RpcResetAbilityCooldown();
        }

        public Hitman(PlayerControl player)
        {
            Role = CustomRoles.Hitman;
            BaseRole = BaseRoles.Shapeshifter;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Target = byte.MaxValue;
            Timer = 0f;
        }

        public byte Target;
        public float Timer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem TargetChangeTime;
        public static OptionItem ShowArrowToTarget;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(600200, CustomRoles.Hitman, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(600201, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            TargetChangeTime = FloatOptionItem.Create(600202, "Target change time", new(5f, 180f, 5f), 45f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            ShowArrowToTarget = BooleanOptionItem.Create(600203, "Show arrow to target", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Hitman] = Chance;
            Options.RolesCount[CustomRoles.Hitman] = Count;
        }
    }
}