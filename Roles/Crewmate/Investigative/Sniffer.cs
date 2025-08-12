using AmongUs.GameOptions;
using UnityEngine;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    public class Sniffer : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = SniffCooldown.GetFloat();
        }

        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Sniff");
            if (Target != byte.MaxValue)
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
            if (Target != byte.MaxValue) return false;
            Target = target.PlayerId;
            SendRPC();
            NearbyPlayers.Clear();
            Player.RpcSetKillTimer(SniffCooldown.GetFloat());
            Cooldown = SniffCooldown.GetFloat();
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
            if (Target != byte.MaxValue)
            {
                string text = Main.StandardNames[Target] + " was nearby ";
                if (NearbyPlayers.Count == 0)
                    text += "<b>NO ONE</b>";
                else
                {
                    while (NearbyPlayers.Count > 0)
                    {
                        var rand = new System.Random();
                        int index = rand.Next(0, NearbyPlayers.Count);
                        text += Main.StandardNames[NearbyPlayers[index]];
                        if (NearbyPlayers.Count == 2)
                            text += " and ";
                        else if (NearbyPlayers.Count > 2)
                            text += ", ";
                        NearbyPlayers.RemoveAt(index);
                    }
                }
                text += " this round.";
                Player.RpcSendMessage(text, "Sniffer");
            }
            Target = byte.MaxValue;
            SendRPC();
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
            if (Target == byte.MaxValue) return;
            var player = Utils.GetPlayerById(Target);
            if (!player.Data.IsDead && (Main.IsInvisible[player.PlayerId] || player.inVent)) return;
            var playerBody = Object.FindObjectsOfType<DeadBody>().FirstOrDefault((DeadBody b) => b.ParentId == Target);
            Vector2? playerPos = !player.Data.IsDead ? player.transform.position : (playerBody != null ? playerBody.transform.position : null);
            if (playerPos == null) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc == player || pc.Data.IsDead || Main.IsInvisible[pc.PlayerId] || pc.inVent) continue;
                if (Vector2.Distance(playerPos.Value, pc.transform.position) <= SniffRadius.GetFloat() * 1.5f && !NearbyPlayers.Contains(Main.AllShapeshifts[pc.PlayerId]))
                    NearbyPlayers.Add(Main.AllShapeshifts[pc.PlayerId]);
            }
            foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == Player.PlayerId || deadBody.ParentId == player.PlayerId) continue;
                if (Vector2.Distance(playerPos.Value, deadBody.transform.position) <= SniffRadius.GetFloat() * 1.5f && !NearbyPlayers.Contains(deadBody.ParentId))
                    NearbyPlayers.Add(deadBody.ParentId);
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
            opt.SetFloat(FloatOptionNames.KillCooldown, SniffCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Main.IsModded[Player.PlayerId]) return "\nTarget: " + (Target == byte.MaxValue ? "<b>None</b>" : Main.StandardNames[Target]);
            if (BaseRole == BaseRoles.Crewmate)
            {
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Task\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")\n</size>") +
                    Utils.ColorString(Color.red, "<size=1.8>Sniff cooldown: " + (int)(Cooldown + 0.99f) + "s\n") +
                    Utils.ColorString(Color, "Target: " + (Target == byte.MaxValue ? "<b>None</b>" : Main.StandardNames[Target]) + "</size>");
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Sniff\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")\n</size>") +
                    Utils.ColorString(Color, "Target: " + (Target == byte.MaxValue ? "<b>None</b>" : Main.StandardNames[Target]) + "</size>");
            }
            return "";
        }

        public override void OnRevive()
        {
            Cooldown = 10f;
        }

        public void SendRPC()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable, -1);
            writer.Write(Target);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            Target = reader.ReadByte();
        }

        public Sniffer(PlayerControl player)
        {
            Role = CustomRoles.Sniffer;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Target = byte.MaxValue;
            NearbyPlayers = new List<byte>();
        }

        public float Cooldown;
        public byte Target;
        public List<byte> NearbyPlayers;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem SniffCooldown;
        public static OptionItem SniffRadius;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(100400, CustomRoles.Sniffer, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(100401, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            SniffCooldown = FloatOptionItem.Create(100402, "Sniff cooldown", new(5f, 60f, 2.5f), 15f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            SniffRadius = FloatOptionItem.Create(100403, "Sniff radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Multiplier);
            Options.RolesChance[CustomRoles.Sniffer] = Chance;
            Options.RolesCount[CustomRoles.Sniffer] = Count;
        }
    }
}