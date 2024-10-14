using System.Collections.Generic;
using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    public class Trapster : CustomRole
    {
        public override void OnMeeting()
        {
            TrappedPlayers = new Dictionary<byte, byte>();
        }

        public override void OnMurderPlayer(PlayerControl target)
        {
            TrappedPlayers.Add(target.PlayerId, byte.MaxValue);
        }

        public override void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            foreach (KeyValuePair<byte, byte> pair in TrappedPlayers)
            {
                if (pair.Value == target.PlayerId)
                {
                    ClassicGamemode.instance.FreezeTimer[pair.Value] = 0f;
                    ClassicGamemode.instance.RoleblockTimer[pair.Value] = 0f;
                    ClassicGamemode.instance.IsFrozen[pair.Value] = false;
                    var player = Utils.GetPlayerById(pair.Value);
                    if (player != null)
                    {
                        player.RpcSetRoleblock(false);
                        player.SyncPlayerSettings();
                        player.RpcSetVentInteraction();
                    }
                    TrappedPlayers[pair.Key] = byte.MaxValue;
                }
            }
        }

        public override void OnFixedUpdate()
        {
            foreach (KeyValuePair<byte, byte> pair in TrappedPlayers)
            {
                var player = Utils.GetPlayerById(pair.Value);
                if (pair.Value != byte.MaxValue && (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected))
                {
                    ClassicGamemode.instance.FreezeTimer[pair.Value] = 0f;
                    ClassicGamemode.instance.RoleblockTimer[pair.Value] = 0f;
                    ClassicGamemode.instance.IsFrozen[pair.Value] = false;
                    if (player != null)
                    {
                        player.RpcSetRoleblock(false);
                        player.SyncPlayerSettings();
                        player.RpcSetVentInteraction();
                    }
                    TrappedPlayers[pair.Key] = byte.MaxValue;
                }
            }
        }

        public bool OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            if (target != null && TrappedPlayers.ContainsKey(target.PlayerId) && TrappedPlayers[target.PlayerId] == byte.MaxValue)
            {
                TrappedPlayers[target.PlayerId] = reporter.PlayerId;
                ClassicGamemode.instance.SetFreezeTimer(reporter, float.MaxValue);
                ClassicGamemode.instance.SetRoleblockTimer(reporter, float.MaxValue);
                foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
                {
                    if (deadBody.ParentId == target.PlayerId)
                    {
                        var position = deadBody.transform.position;
                        reporter.RpcTeleport(new Vector2(position.x, position.y + 0.3636f));
                        break;
                    }
                }
                reporter.SyncPlayerSettings();
                reporter.RpcSetVentInteraction();
                new LateTask(() => {
                    if (!Player.Data.IsDead)
                        Player.RpcSetKillTimer(Math.Max(Main.KillCooldowns[Player.PlayerId] - KillCooldownDecreaseOnTrap.GetFloat(), 0.001f));
                }, 0.5f);
                Player.RpcReactorFlash(0.3f, Color);
                Player.Notify(Utils.ColorString(Color.red, Main.StandardNames[reporter.PlayerId] + " got trapped on " + Main.StandardNames[target.PlayerId] + "'s body\nKill cooldown decreased!"));
                return false;
            }
            return true;
        }

        public static bool OnGlobalReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            if (target == null) return true;
            bool result = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Trapster)
                {
                    Trapster trapsterRole = pc.GetRole() as Trapster;
                    if (trapsterRole == null) continue;
                    if (!trapsterRole.OnReportDeadBody(reporter, target))
                        result = false;
                }
            }
            return result;
        }

        public Trapster(PlayerControl player)
        {
            Role = CustomRoles.Trapster;
            BaseRole = BaseRoles.Impostor;
            Player = player;
            Color = Palette.ImpostorRed;
            RoleName = "Trapster";
            RoleDescription = "Trap players on dead bodies";
            RoleDescriptionLong = "Trapster (Impostor): After killing someone, you place trap on dead body. Next player, who tries to report that body (or interact with it in any way) will be trapped on it unable to move and use abilities. Dead body can trap only 1 person at the time. If someone gets trapped on your body, your kill cooldown will decrease and you will get alerted.";
            AbilityUses = -1f;
            TrappedPlayers = new Dictionary<byte, byte>();
        }

        public Dictionary<byte, byte> TrappedPlayers;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem KillCooldownDecreaseOnTrap;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(700100, "Trapster", new(0, 100, 5), 0, TabGroup.ImpostorRoles, false)
                .SetColor(Palette.ImpostorRed)
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(700101, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            KillCooldownDecreaseOnTrap = FloatOptionItem.Create(700102, "Kill cooldown decrease on trap", new(1f, 30f, 1f), 10f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            Options.RolesChance[CustomRoles.Trapster] = Chance;
            Options.RolesCount[CustomRoles.Trapster] = Count;
        }
    }
}