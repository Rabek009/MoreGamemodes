using AmongUs.GameOptions;
using UnityEngine;
using Hazel;
using System;

namespace MoreGamemodes
{
    public class Droner : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            __instance.ReportButton.OverrideText(TranslationController.Instance.GetString(StringNames.ReportButton));
            if (Player.Data.IsDead) return;
            __instance.AbilityButton.OverrideText("Drone");
            if (RealPosition != null)
            {
                __instance.PetButton.OverrideText("Stop Drone");
                __instance.SabotageButton.SetDisabled();
                __instance.SabotageButton.ToggleVisible(false);
                __instance.AdminButton.SetDisabled();
                __instance.AdminButton.ToggleVisible(false);
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.ReportButton.SetDisabled();
                __instance.ReportButton.ToggleVisible(false);
                __instance.UseButton.SetDisabled();
                __instance.UseButton.ToggleVisible(false);
            }
            else
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
        }

        public override void OnPet()
        {
            if (ControlledDrone != null)
                EndAbility();
        }

        public override bool OnCheckMurderLate(PlayerControl target)
        {
            if (ControlledDrone == null) return true;
            ClassicGamemode.instance.PlayerKiller[target.PlayerId] = Player.PlayerId;
            ++Main.PlayerKills[Player.PlayerId];
            target.RpcMurderPlayer(target, true);
            Player.RpcSetKillTimer(Main.OptionKillCooldowns[Player.PlayerId]);
            return false;
        }

        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (ControlledDrone != null)
                EndAbility();
        }

        public override bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            return ControlledDrone == null;
        }

        public override void OnMeeting()
        {
            EndAbility();
        }

        public override void OnFixedUpdate()
        {
            if (AbilityDuration <= -1f)
            {
                TimeSinceAbilityUse += Time.fixedDeltaTime;
            }
            if (AbilityDuration > -1f)
            {
                AbilityDuration -= Time.fixedDeltaTime;
            }
            if (AbilityDuration <= 0f && AbilityDuration > -1f)
            {
                EndAbility();
            }
            if (Player.AmOwner && ControlledDrone != null)
                DronePosition = Player.transform.position;
        }

        public override bool OnCheckVanish()
        {
            if (AbilityDuration > 0f) return false;
            if (TimeSinceAbilityUse < 1f) return false;
            ControlledDrone = Utils.RpcCreateDrone(Player, Player.transform.position);
            DronePosition = Player.transform.position;
            Player.RpcSetDronerRealPosition(Player.transform.position);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc.AmOwner) continue;
                CustomRpcSender sender = CustomRpcSender.Create("DronerAbilityStart", SendOption.Reliable);
                sender.StartMessage(pc.GetClientId());
                sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                    .WriteVector2(Player.transform.position)
                    .Write(Player.NetTransform.lastSequenceId)
                    .EndRpc();
                sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                    .WriteVector2(Player.transform.position)
                    .Write((ushort)(Player.NetTransform.lastSequenceId + 16383))
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
            if (Player.AmOwner)
                Player.Visible = false;
            Player.SyncPlayerSettings();
            Player.RpcSetVentInteraction();
            AbilityDuration = DroneDuration.GetFloat();
            new LateTask(() => Player.RpcSetAbilityCooldown(DroneDuration.GetFloat()), 0.2f);
            return false;
        }

        public override bool OnEnterVent(int id)
        {
            return RealPosition == null;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            return ControlledDrone == null;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.PhantomCooldown, DroneCooldown.GetFloat());
            if (ControlledDrone != null)
            {
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, DroneSpeed.GetFloat());
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, DroneVision.GetFloat());
                opt.SetInt(Int32OptionNames.KillDistance, DroneKillDistance.GetValue());
            }
            return opt;
        }

        public void EndAbility()
        {
            AbilityDuration = -1f;
            TimeSinceAbilityUse = 0f;
            if (Player.AmOwner)
                Player.Visible = true;
            Player.NetTransform.SnapTo((Vector2)RealPosition, (ushort)(Player.NetTransform.lastSequenceId + 328));
            CustomRpcSender sender = CustomRpcSender.Create("DronerAbilityEnd", SendOption.Reliable);
            sender.StartMessage(-1);
            sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(Player.transform.position)
                .Write((ushort)(Player.NetTransform.lastSequenceId + 32767))
                .EndRpc();
            sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(Player.transform.position)
                .Write((ushort)(Player.NetTransform.lastSequenceId + 32767 + 16383))
                .EndRpc();
            sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(Player.transform.position)
                .Write(Player.NetTransform.lastSequenceId)
                .EndRpc();
            sender.EndMessage();
            sender.SendMessage();
            ControlledDrone.Despawn();
            ControlledDrone = null;
            DronePosition = Vector2.zero;
            Player.RpcSetDronerRealPosition(null);
            Player.SyncPlayerSettings();
            Player.RpcSetVentInteraction();
            Player.RpcResetAbilityCooldown();
        }

        public void CancelLadder()
        {
            if (Player.Data.IsDead || MeetingHud.Instance) return;
            Player.Die(DeathReason.Exile, false);
            var role = Main.DesyncRoles.ContainsKey((Player.PlayerId, PlayerControl.LocalPlayer.PlayerId)) ? Main.DesyncRoles[(Player.PlayerId, PlayerControl.LocalPlayer.PlayerId)] : Main.StandardRoles[Player.PlayerId];
            Player.StartCoroutine(Player.CoSetRole(role, true));
            Player.MyPhysics.CancelPet();
            Player.NetTransform.SnapTo(Player.transform.position, (ushort)(Player.NetTransform.lastSequenceId + 328));
            CustomRpcSender sender = CustomRpcSender.Create("CancelLadderDroner", SendOption.Reliable);
            sender.StartMessage(Player.GetClientId());
            sender.StartRpc(Player.NetId, (byte)RpcCalls.Exiled)
                .EndRpc();
            var role2 = Main.DesyncRoles.ContainsKey((Player.PlayerId, Player.PlayerId)) ? Main.DesyncRoles[(Player.PlayerId, Player.PlayerId)] : Main.StandardRoles[Player.PlayerId];
            sender.StartRpc(Player.NetId, (byte)RpcCalls.SetRole)
                .Write((ushort)role2)
                .Write(true)
                .EndRpc();
            sender.StartRpc(Player.MyPhysics.NetId, (byte)RpcCalls.CancelPet)
                .EndRpc();
            sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(DronePosition)
                .Write(Player.NetTransform.lastSequenceId)
                .EndRpc();
            sender.EndMessage();
            sender.SendMessage();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.AmOwner && pc != Player)
                {
                    CustomRpcSender sender2 = CustomRpcSender.Create("CancelLadder", SendOption.Reliable);
                    sender2.StartMessage(pc.GetClientId());
                    sender2.StartRpc(Player.NetId, (byte)RpcCalls.Exiled)
                        .EndRpc();
                    var role3 = Main.DesyncRoles.ContainsKey((Player.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(Player.PlayerId, pc.PlayerId)] : Main.StandardRoles[Player.PlayerId];
                    sender2.StartRpc(Player.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)role3)
                        .Write(true)
                        .EndRpc();
                    sender2.StartRpc(Player.MyPhysics.NetId, (byte)RpcCalls.CancelPet)
                        .EndRpc();
                    sender2.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(Player.transform.position)
                        .Write((ushort)(Player.NetTransform.lastSequenceId + 32767))
                        .EndRpc();
                    sender2.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(Player.transform.position)
                        .Write((ushort)(Player.NetTransform.lastSequenceId + 32767 + 16383))
                        .EndRpc();
                    sender2.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(Player.transform.position)
                        .Write(Player.NetTransform.lastSequenceId)
                        .EndRpc();
                    sender2.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                        .WriteVector2(Player.transform.position)
                        .Write((ushort)(Player.NetTransform.lastSequenceId + 16383))
                        .EndRpc();
                    sender2.EndMessage();
                    sender2.SendMessage();
                }
            }
            new LateTask(() => {
                if (!MeetingHud.Instance)
                {
                    Player.RpcSetKillTimer(Math.Max(Main.KillCooldowns[Player.PlayerId], 0.001f));
                    if (AbilityDuration > 0f)
                        Player.RpcSetAbilityCooldown(AbilityDuration + 0.99f);
                    if (Options.EnableMidGameChat.GetBool())
                        Player.SetChatVisible(true);
                }        
            }, 0.2f);
        }

        public Droner(PlayerControl player)
        {
            Role = CustomRoles.Droner;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
            TimeSinceAbilityUse = 0f;
            ControlledDrone = null;
            DronePosition = Vector2.zero;
            RealPosition = null;
        }

        public float AbilityDuration;
        public float TimeSinceAbilityUse;
        public Drone ControlledDrone;
        public Vector2 DronePosition;
        public Vector2? RealPosition;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem DroneCooldown;
        public static OptionItem DroneDuration;
        public static OptionItem DroneSpeed;
        public static OptionItem DroneVision;
        public static OptionItem DroneKillDistance;
        public static readonly string[] droneKillDistances =
        {
            "Short", "Medium", "Long"
        };
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(500300, "Droner", new(0, 100, 5), 0, TabGroup.ImpostorRoles, false)
                .SetColor(CustomRolesHelper.RoleColors[CustomRoles.Droner])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(500301, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            DroneCooldown = FloatOptionItem.Create(500302, "Drone cooldown", new(10f, 60f, 5f), 25f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            DroneDuration = FloatOptionItem.Create(500303, "Drone duration", new(5f, 60f, 5f), 10f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            DroneSpeed = FloatOptionItem.Create(500304, "Drone speed", new(0.5f, 5f, 0.25f), 1.75f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Multiplier);
            DroneVision = FloatOptionItem.Create(500305, "Drone vision", new(0.25f, 5f, 0.25f), 1.5f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Multiplier);
            DroneKillDistance = StringOptionItem.Create(500306, "Drone kill distance", droneKillDistances, 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Droner] = Chance;
            Options.RolesCount[CustomRoles.Droner] = Count;
        }
    }
}