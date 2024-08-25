using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using System;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    class CloseDoorsPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            return CustomGamemode.Instance.OnCloseDoors(__instance);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
    class MessageReaderUpdateSystemPatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            MessageReader reader2 = MessageReader.Get(reader);
            return CustomGamemode.Instance.OnUpdateSystem(__instance, systemType, player, reader2);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(byte))]
    class ByteUpdateSystemPatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] byte amount)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            MessageWriter writer = MessageWriter.Get(0);
			writer.Write(amount);
			MessageReader reader = MessageReader.Get(writer.ToByteArray(false));
            return CustomGamemode.Instance.OnUpdateSystem(__instance, systemType, player, reader);     
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
    class AddTasksFromListPatch
    {
        public static void Prefix(ShipStatus __instance,
            [HarmonyArgument(4)] Il2CppSystem.Collections.Generic.List<NormalPlayerTask> unusedTasks)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            if (CustomGamemode.Instance.Gamemode != Gamemodes.Deathrun) return;
            List<NormalPlayerTask> disabledTasks = new();
            for (var i = 0; i < unusedTasks.Count; i++)
            {
                var task = unusedTasks[i];
                if (task.TaskType == TaskTypes.UploadData && CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun) disabledTasks.Add(task);
            }
            foreach (var task in disabledTasks)
                unusedTasks.Remove(task);
        }
    }

    [HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.MushroomMixUp))]
    class MushroomMixUpPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (MeetingHud.Instance) return;
            new LateTask(() =>
            {
                if (!MeetingHud.Instance)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, false), ar, true);
                    }
                }
            }, 1f, "Set MixUp Name");
        }
    }

    [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.PerformVentOp))]
    class PerformVentOpPatch
    {
        public static bool Prefix(VentilationSystem __instance, [HarmonyArgument(0)] byte playerId, [HarmonyArgument(1)] VentilationSystem.Operation op, [HarmonyArgument(2)] byte ventId, [HarmonyArgument(3)] SequenceBuffer<VentilationSystem.VentMoveInfo> seqBuffer)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Utils.GetPlayerById(playerId) == null) return true;
            switch (op)
            {
                case VentilationSystem.Operation.Enter:
                    if (CoEnterVentPatch.PlayersToKick.Contains(playerId))
                    {
                        var player = Utils.GetPlayerById(playerId);
                        if (player == null) return false;
                        AntiCheat.TimeSinceVentCancel[playerId] = 0f;
                        seqBuffer.BumpSid();
		                Vector2 vector = Utils.GetVentById(ventId).transform.position;
		                vector -= player.Collider.offset;
		                player.NetTransform.SnapTo(vector);
                        MessageWriter writer = AmongUsClient.Instance.StartRpc(player.MyPhysics.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable);
		                writer.WritePacked(ventId);
		                writer.EndMessage();
                        CoEnterVentPatch.PlayersToKick.Remove(playerId);
                        return false;
                    }
                    break;
                case VentilationSystem.Operation.Move:
                    if (!__instance.PlayersInsideVents.ContainsKey(playerId))
                    {
                        seqBuffer.BumpSid();
                        return false;
                    }
                    break;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Serialize))]
    class ShipStatusSerializePatch
    {
        public static void Prefix(ShipStatus __instance, [HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] bool initialState)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (initialState) return;
            bool cancel = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (VentilationSystemDeterioratePatch.BlockVentInteraction(pc))
                    cancel = true;
            }
            var ventilationSystem = __instance.Systems[SystemTypes.Ventilation].Cast<VentilationSystem>();
            if (cancel && ventilationSystem != null && ventilationSystem.IsDirty)
            {
                Utils.SetAllVentInteractions();
                ventilationSystem.IsDirty = false;
            }
                
        }
    }

    [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deteriorate))]
    class VentilationSystemDeterioratePatch
    {
        public static Dictionary<byte, List<int>> LastClosestVents;
        public static void Postfix(VentilationSystem __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!Main.GameStarted) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (BlockVentInteraction(pc))
                {
                    List<Vent> CurrentClosestVents = pc.GetVentsFromClosest();
                    List<int> CurrentClosestVentIds = new();
                    foreach (var vent in CurrentClosestVents)
                        CurrentClosestVentIds.Add(vent.Id);
                    bool serialize = false;
                    for (int i = 0; i < LastClosestVents[pc.PlayerId].Count; ++i)
                    {
                        if (LastClosestVents[pc.PlayerId][i] != CurrentClosestVentIds[i])
                        {
                            serialize = true;
                            break;
                        }
                    }
                    if (!serialize) continue;
                    MessageWriter writer = MessageWriter.Get(SendOption.None);
                    writer.StartMessage(6);
                    writer.Write(AmongUsClient.Instance.GameId);
                    writer.WritePacked(pc.GetClientId());
                    writer.StartMessage(1);
                    writer.WritePacked(ShipStatus.Instance.NetId);
                    writer.StartMessage((byte)SystemTypes.Ventilation);
                    int vents = 0;
                    foreach (var vent in ShipStatus.Instance.AllVents)
                    {
                        if (!CustomGamemode.Instance.OnEnterVent(pc, vent.Id))
                            ++vents;
                    }
                    List<NetworkedPlayerInfo> AllPlayers = new();
                    foreach (var playerInfo in GameData.Instance.AllPlayers)
                    {
                        if (playerInfo != null && !playerInfo.Disconnected)
                            AllPlayers.Add(playerInfo);
                    }
                    int maxVents = Math.Min(vents, AllPlayers.Count);
                    int blockedVents = 0;
				    writer.WritePacked(maxVents);
                    foreach (var vent in pc.GetVentsFromClosest())
                    {
                        if (!CustomGamemode.Instance.OnEnterVent(pc, vent.Id))
                        {
                            writer.Write(AllPlayers[blockedVents].PlayerId);
			                writer.Write((byte)vent.Id);
                            ++blockedVents;
                        }
                        if (blockedVents >= maxVents)
                            break;
                    }
                    writer.WritePacked(__instance.PlayersInsideVents.Count);
		            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<byte, byte> keyValuePair2 in __instance.PlayersInsideVents)
		            {
			            writer.Write(keyValuePair2.Key);
			            writer.Write(keyValuePair2.Value);
		            }
				    writer.EndMessage();
                    writer.EndMessage();
                    writer.EndMessage();
                    AmongUsClient.Instance.SendOrDisconnect(writer);
                    writer.Recycle();
                }
            }
        }
        public static bool BlockVentInteraction(PlayerControl pc)
        {
            if (!pc.AmOwner && !Main.IsModded[pc.PlayerId] && !pc.Data.IsDead && Main.StandardRoles.ContainsKey(pc.PlayerId) && (pc.GetSelfRole().IsImpostor() || pc.GetSelfRole() == RoleTypes.Engineer))
            {
                foreach (var vent in ShipStatus.Instance.AllVents)
                {
                    if (!CustomGamemode.Instance.OnEnterVent(pc, vent.Id))
                        return true;
                }
            }
            return false;
        }
        public static void SerializeV2(VentilationSystem __instance, PlayerControl player = null)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                if (player != null && pc != player) continue;
                if (BlockVentInteraction(pc))
                {
                    MessageWriter writer = MessageWriter.Get(SendOption.None);
                    writer.StartMessage(6);
                    writer.Write(AmongUsClient.Instance.GameId);
                    writer.WritePacked(pc.GetClientId());
                    writer.StartMessage(1);
                    writer.WritePacked(ShipStatus.Instance.NetId);
                    writer.StartMessage((byte)SystemTypes.Ventilation);
                    int vents = 0;
                    foreach (var vent in ShipStatus.Instance.AllVents)
                    {
                        if (!CustomGamemode.Instance.OnEnterVent(pc, vent.Id))
                            ++vents;
                    }
                    List<NetworkedPlayerInfo> AllPlayers = new();
                    foreach (var playerInfo in GameData.Instance.AllPlayers)
                    {
                        if (playerInfo != null && !playerInfo.Disconnected)
                            AllPlayers.Add(playerInfo);
                    }
                    int maxVents = Math.Min(vents, AllPlayers.Count);
                    int blockedVents = 0;
				    writer.WritePacked(maxVents);
                    foreach (var vent in pc.GetVentsFromClosest())
                    {
                        if (!CustomGamemode.Instance.OnEnterVent(pc, vent.Id))
                        {
                            writer.Write(AllPlayers[blockedVents].PlayerId);
			                writer.Write((byte)vent.Id);
                            ++blockedVents;
                        }
                        if (blockedVents >= maxVents)
                            break;
                    }
                    writer.WritePacked(__instance.PlayersInsideVents.Count);
		            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<byte, byte> keyValuePair2 in __instance.PlayersInsideVents)
		            {
			            writer.Write(keyValuePair2.Key);
			            writer.Write(keyValuePair2.Value);
		            }
				    writer.EndMessage();
                    writer.EndMessage();
                    writer.EndMessage();
                    AmongUsClient.Instance.SendOrDisconnect(writer);
                    writer.Recycle();
                }
                else
                {
                    MessageWriter writer = MessageWriter.Get(SendOption.None);
                    writer.StartMessage(6);
                    writer.Write(AmongUsClient.Instance.GameId);
                    writer.WritePacked(pc.GetClientId());
                    writer.StartMessage(1);
                    writer.WritePacked(ShipStatus.Instance.NetId);
                    writer.StartMessage((byte)SystemTypes.Ventilation);
				    __instance.Serialize(writer, false);
			        writer.EndMessage();
                    writer.EndMessage();
                    writer.EndMessage();
                    AmongUsClient.Instance.SendOrDisconnect(writer);
                    writer.Recycle();
                }
            }
        }
    }
}