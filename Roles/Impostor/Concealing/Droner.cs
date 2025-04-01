using AmongUs.GameOptions;
using UnityEngine;
using Hazel;

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
            if (RealPosition != null)
                EndAbility();
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
                EndAbility();
            }
            if (Player.AmOwner && ControlledDrone != null)
                DronePosition = Player.transform.position;
        }

        public override bool OnCheckVanish()
        {
            ControlledDrone = Utils.RpcCreateDrone(Player, Player.GetRealPosition());
            DronePosition = Player.GetRealPosition();
            RealPosition = Player.GetRealPosition();
            SendRPC();
            bool doSend = false;
            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc.AmOwner) continue;
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
                doSend = true;
            }
            sender.SendMessage(doSend);
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

        public override bool OnClimbLadder(Ladder source, bool ladderUsed)
        {
            if (RealPosition != null)
            {
                if (ladderUsed && AbilityDuration > 0.2f)
                {
                    new LateTask(() => {
                        if (!MeetingHud.Instance)
                            Player.RpcSetAbilityCooldown(AbilityDuration + 0.99f);
                    }, 0.2f);
                }
                return false;
            }
            return true;
        }

        public override bool OnUsePlatform()
        {
            return RealPosition == null;
        }

        public override bool OnCheckUseZipline(ZiplineBehaviour ziplineBehaviour, bool fromTop)
        {
            return RealPosition == null;
        }

        public override bool OnCheckSporeTrigger(Mushroom mushroom)
        {
            return RealPosition == null;
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

        public override void OnRevive()
        {
            AbilityDuration = -1f;
            ControlledDrone = null;
            DronePosition = Vector2.zero;
            RealPosition = null;
            SendRPC();
        }

        public void SendRPC()
        {
            if (Player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable);
            writer.Write(RealPosition != null);
            if (RealPosition != null)
                NetHelpers.WriteVector2((Vector2)RealPosition, writer);
            writer.EndMessage();
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            RealPosition = reader.ReadBoolean() ? NetHelpers.ReadVector2(reader) : null;
            if (Player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
        }

        public void EndAbility()
        {
            AbilityDuration = -1f;
            if (Player.AmOwner)
                Player.Visible = true;
            if (MeetingHud.Instance)
            {
                Vector2 vector = Vector2.up;
		        vector = vector.Rotate((Player.PlayerId - 1) * (360f / GameData.Instance.PlayerCount));
		        vector *= ShipStatus.Instance.SpawnRadius;
		        Vector2 position = ShipStatus.Instance.MeetingSpawnCenter + vector + new Vector2(0f, 0.3636f);
                Player.NetTransform.SnapTo(position, (ushort)(Player.NetTransform.lastSequenceId + 128));
            }
            else
                Player.NetTransform.SnapTo((Vector2)RealPosition, (ushort)(Player.NetTransform.lastSequenceId + 128));
            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
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
            if (!MeetingHud.Instance)
                ControlledDrone.Despawn();
            ControlledDrone = null;
            DronePosition = Vector2.zero;
            RealPosition = null;
            SendRPC();
            Player.SyncPlayerSettings();
            Player.RpcSetVentInteraction();
            Player.RpcResetAbilityCooldown();
        }

        public Droner(PlayerControl player)
        {
            Role = CustomRoles.Droner;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
            ControlledDrone = null;
            DronePosition = Vector2.zero;
            RealPosition = null;
        }

        public float AbilityDuration;
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
            Chance = RoleOptionItem.Create(500300, CustomRoles.Droner, TabGroup.ImpostorRoles, false);
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