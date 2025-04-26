using Hazel;
using InnerNet;
using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(InnerNetClient))]
    public class InnerNetClientPatch
    {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendInitialData))]
        [HarmonyPrefix] 
        public static bool SendInitialDataPrefix(InnerNetClient __instance, [HarmonyArgument(0)] int clientId)
        {
            if (!__instance.AmHost || !Main.ModdedProtocol.Value || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
            MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
            messageWriter.StartMessage(6);
            messageWriter.Write(__instance.GameId);
            messageWriter.WritePacked(clientId);
            Il2CppSystem.Collections.Generic.List<InnerNetObject> obj = __instance.allObjects;
            lock (obj)
            {
                HashSet<GameObject> hashSet = new();
                for (int i = 0; i < __instance.allObjects.Count; i++)
                {
                    if (messageWriter.Length > 800)
                    {
                        messageWriter.EndMessage();
                        __instance.SendOrDisconnect(messageWriter);
                        messageWriter.Clear(SendOption.Reliable);
                        messageWriter.StartMessage(6);
                        messageWriter.Write(__instance.GameId);
                        messageWriter.WritePacked(clientId);
                    }
                    InnerNetObject innerNetObject = __instance.allObjects[i];
                    if (innerNetObject && (innerNetObject.OwnerId != -4 || __instance.AmModdedHost) && hashSet.Add(innerNetObject.gameObject))
                    {
                        GameManager gameManager = innerNetObject as GameManager;
                        if (gameManager != null)
                            __instance.SendGameManager(clientId, gameManager);
                        else
                            __instance.WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
                    }
                }
                messageWriter.EndMessage();
                __instance.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
            return false;
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
        [HarmonyPrefix]
        public static bool SendAllStreamedObjectsPrefix(InnerNetClient __instance, ref bool __result)
        {
            if (!__instance.AmHost || (!Main.ModdedProtocol.Value && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
            __result = false;
            Il2CppSystem.Collections.Generic.List<InnerNetObject> obj = __instance.allObjects;
            lock (obj)
            {
                for (int i = 0; i < __instance.allObjects.Count; i++)
                {
                    InnerNetObject innerNetObject = __instance.allObjects[i];
                    if (innerNetObject && innerNetObject.IsDirty && (innerNetObject.AmOwner || (innerNetObject.OwnerId == -2 && __instance.AmHost)))
                    {
                        MessageWriter messageWriter = __instance.Streams[(int)innerNetObject.sendMode];
                        if (messageWriter.Length > 800)
                        {
                            messageWriter.EndMessage();
                            __instance.SendOrDisconnect(messageWriter);
                            messageWriter.Clear(innerNetObject.sendMode);
                            messageWriter.StartMessage(5);
                            messageWriter.Write(__instance.GameId);
                        }
                        messageWriter.StartMessage(1);
                        messageWriter.WritePacked(innerNetObject.NetId);
                        try
                        {
                            if (innerNetObject.Serialize(messageWriter, false))
                                messageWriter.EndMessage();
                            else
                                messageWriter.CancelMessage();
                            if (innerNetObject.Chunked && innerNetObject.IsDirty)
                                __result = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.ToString());
                            messageWriter.CancelMessage();
                        }
                    }
                }
            }
            for (int j = 0; j < __instance.Streams.Length; j++)
            {
                MessageWriter messageWriter2 = __instance.Streams[j];
                if (messageWriter2.HasBytes(7))
                {
                    messageWriter2.EndMessage();
                    __instance.SendOrDisconnect(messageWriter2);
                    messageWriter2.Clear((SendOption)j);
                    messageWriter2.StartMessage(5);
                    messageWriter2.Write(__instance.GameId);
                }
            }
            return false;
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Spawn))]
        [HarmonyPostfix]
        public static void Spawn_Postfix(InnerNetClient __instance, [HarmonyArgument(0)] InnerNetObject netObjParent)
        {
            if (__instance.NetworkMode != NetworkModes.OnlineGame) return;
            if (__instance.AmHost)
            {
                if (netObjParent is NetworkedPlayerInfo playerinfo)
                {
                    new LateTask(() =>
                    {
                        if (playerinfo != null && AmongUsClient.Instance.AmConnected)
                        {
                            var client = AmongUsClient.Instance.GetClient(playerinfo.ClientId);
                            if (client != null && !CreatePlayerPatch.IsDisconnected(client))
                            {
                                if (playerinfo.IsIncomplete)
                                {
                                    Main.Instance.Log.LogInfo($"Disconnecting Client [{client.Id}]{client.PlayerName} {client.FriendCode} for playerinfo timeout");
                                    AmongUsClient.Instance.SendLateRejection(client.Id, DisconnectReasons.ClientTimeout);
                                    __instance.OnPlayerLeft(client, DisconnectReasons.ClientTimeout);
                                }
                            }
                        }
                    }, 5f, "PlayerInfo Green Bean Kick");
                }
                if (netObjParent is PlayerControl player)
                {
                    new LateTask(() =>
                    {
                        if (player != null && !player.notRealPlayer && !player.isDummy && AmongUsClient.Instance.AmConnected)
                        {
                            var client = AmongUsClient.Instance.GetClient(player.OwnerId);
                            if (client != null && !CreatePlayerPatch.IsDisconnected(client))
                            {
                                if (player.Data == null || player.Data.IsIncomplete)
                                {
                                    Main.Instance.Log.LogInfo($"Disconnecting Client [{client.Id}]{client.PlayerName} {client.FriendCode} for playercontrol timeout");
                                    AmongUsClient.Instance.SendLateRejection(client.Id, DisconnectReasons.ClientTimeout);
                                    __instance.OnPlayerLeft(client, DisconnectReasons.ClientTimeout);
                                }
                            }
                        }
                    }, 5.5f, "PlayerControl Green Bean Kick");
                }
            }
            if (!__instance.AmHost)
                Debug.LogError("Tried to spawn while not host:" + (netObjParent?.ToString()));
        }
    }
    [HarmonyPatch(typeof(GameData), nameof(GameData.DirtyAllData))]
    internal class DirtyAllDataPatch
    {
        public static bool Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.Serialize))]
    internal class NetworkedPlayerInfoSerializePatch
    {
        public static bool Prefix(NetworkedPlayerInfo __instance, MessageWriter writer, bool initialState, ref bool __result)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            writer.Write(__instance.PlayerId);
            writer.WritePacked(__instance.ClientId);
            writer.Write((byte)__instance.Outfits.Count);
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<PlayerOutfitType, NetworkedPlayerInfo.PlayerOutfit> keyValuePair in __instance.Outfits)
            {
                writer.Write((byte)keyValuePair.Key);
                keyValuePair.Value.Serialize(writer);
            }
            writer.WritePacked(__instance.PlayerLevel);
            byte b = 0;
            if (__instance.Disconnected)
                b |= 1;
            if (__instance.IsDead)
                b |= 4;
            writer.Write(b);
            writer.Write((ushort)__instance.Role.Role);
            writer.Write(false);
            if (__instance.Tasks != null)
            {
                writer.Write((byte)__instance.Tasks.Count);
                for (int i = 0; i < __instance.Tasks.Count; i++)
                    __instance.Tasks[i].Serialize(writer);
            }
            else
                writer.Write(0);
            writer.Write(__instance.FriendCode ?? string.Empty);
            if (Utils.IsVanillaServer())
                writer.Write(__instance.Puid ?? string.Empty);
            else
                writer.Write(string.Empty);

            if (!initialState)
                __instance.ClearDirtyBits();
            __result = true;
            return false;
        }
    }
}