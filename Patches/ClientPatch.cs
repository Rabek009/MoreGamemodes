using System;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using UnityEngine;
using System.Linq;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return true;
            if (client != null && BanManager.BannedFriendCodes.Contains(client.FriendCode))
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                return false;
            }
            if (client != null && client.Id == __instance.ClientId)
            {
                AntiCheat.Init();
                PlayerTagManager.Initialize();
                PlayerTagManager.PlayersWithTags.Add(new ModdedPlayerTag(client.FriendCode, "00ff00", "#Host"));
                return true;
            }
            if (client != null && Options.AntiCheat.GetBool() && (!Utils.IsValidFriendCode(client.FriendCode) || client.ProductUserId == "") && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                return false;
            }
            if (client != null && BanManager.CheckBanPlayer(client))
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                return false;
            }
            OptionItem.SyncAllOptions();
            new LateTask(() => 
            {
                if (client != null && client.Character != null)
                    client.Character.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v" + Main.CurrentVersion + "! You can play without mod installed!", "Welcome");
            }, 2f, "Welcome Message");
            return true;
        }
        // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Patches/PlayerJoinAndLeftPatch.cs#L164
        public static bool IsDisconnected(ClientData client)
        {
            var __instance = AmongUsClient.Instance;
            for (int i = 0; i < __instance.allClients.Count; i++)
            {
                ClientData clientData = __instance.allClients[i];
                if (clientData.Id == client.Id)
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return;
            if (__instance.GameState != InnerNetClient.GameStates.Started) return;
            if (Main.AllPlayersDeathReason.ContainsKey(client.Character.PlayerId))
            {
                if (client.Character.GetDeathReason() == DeathReasons.Alive)
                    client.Character.RpcSetDeathReason(DeathReasons.Disconnected);
            }
            CustomGamemode.Instance.OnDisconnect(client.Character);
            new LateTask(() => {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (VentilationSystemDeterioratePatch.BlockVentInteraction(pc))
                    {
                        Utils.SetAllVentInteractions();
                        break;
                    }
                }
            }, 0.2f);
            AntiBlackout.OnDisconnect(client.Character.Data);
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    class OnGameJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            if (!__instance.AmHost)
            {
                Options.Gamemode.SetValue(0);
                Main.StandardNames = new System.Collections.Generic.Dictionary<byte, string>();
                new LateTask(() => PlayerControl.LocalPlayer.RpcVersionCheck(Main.CurrentVersion), 2f, "Version Check");
            }
        }
    }

    [HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
    class BanMenuSetVisiblePatch
    {
        public static bool Prefix(BanMenu __instance, [HarmonyArgument(0)] bool show)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;
            __instance.BanButton.gameObject.SetActive(AmongUsClient.Instance.CanBan());
            __instance.KickButton.gameObject.SetActive(AmongUsClient.Instance.CanKick());
            __instance.MenuButton.gameObject.SetActive(show);
            return false;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
    class InnerNetClientCanBanPatch
    {
        public static bool Prefix(InnerNetClient __instance, ref bool __result)
        {
            __result = __instance.AmHost;
            return false;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanKick))]
    class InnerNetClientCanKickPatch
    {
        public static bool Prefix(InnerNetClient __instance, ref bool __result)
        {
            __result = __instance.AmHost;
            return false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnBecomeHost))]
    class OnBecomeHostPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            if (__instance.IsGameStarted)
            {
                System.Collections.Generic.List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                    winners.Add(pc.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.HumansByVote, winners);
            }
            else
            {
                AntiCheat.Init();
                PlayerTagManager.Initialize();
                PlayerTagManager.PlayersWithTags.Add(new ModdedPlayerTag(PlayerControl.LocalPlayer.GetClient().FriendCode, "00ff00", "#Host"));
                PlayerControl.LocalPlayer.RpcSendMessage("You are new host! Now this lobby is modded with More Gamemodes v" + Main.CurrentVersion + "!", "Host");
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.RpcSetName(Main.StandardNames[pc.PlayerId]);
                    if (!pc.AmOwner)
                        pc.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v" + Main.CurrentVersion + "! You can play without mod installed!", "Welcome");
                }
                PlayerControl.LocalPlayer.RpcRequestVersionCheck();
            }
        }
    }

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
    class CmdAddVotePatch
    {
        public static bool Prefix(VoteBanSystem __instance, [HarmonyArgument(0)] int clientId)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            CustomGamemode.Instance.OnAddVote(AmongUsClient.Instance.ClientId, clientId);
            return false;
        }
    }

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    class AddVotePatch
    {
        public static bool Prefix(VoteBanSystem __instance, [HarmonyArgument(0)] int srcClient, [HarmonyArgument(1)] int clientId)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            CustomGamemode.Instance.OnAddVote(srcClient, clientId);
            if (AmongUsClient.Instance.ClientId == srcClient || __instance != VoteBanSystem.Instance) return false;
            VoteBanSystem.Instance = Object.Instantiate(AmongUsClient.Instance.VoteBanPrefab);
			AmongUsClient.Instance.Spawn(VoteBanSystem.Instance, -2, SpawnFlags.None);
            new LateTask(() => {
                MessageWriter writer = MessageWriter.Get(SendOption.None);
                writer.StartMessage(5);
                writer.Write(AmongUsClient.Instance.GameId);
			    writer.StartMessage(5);
			    writer.WritePacked(__instance.NetId);
			    writer.EndMessage();
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
            }, 0.5f);
            new LateTask(() => {
                AmongUsClient.Instance.RemoveNetObject(__instance);
                Object.Destroy(__instance.gameObject);
            }, 5f);
            return false;
        }
    }

    [HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Kick))]
    class KickPatch
    {
        public static void Prefix(BanMenu __instance, [HarmonyArgument(0)] bool ban)
        {
            if (!AmongUsClient.Instance.AmHost || !Main.ApplyBanList.Value || !ban) return;
            BanManager.AddBanPlayer(AmongUsClient.Instance.GetClient(__instance.selectedClientId));
        }
    }

    //https://github.com/Gurge44/EndlessHostRoles/blob/main/Patches/ClientPatch.cs

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendInitialData))]
    class SendInitialDataPatch
    {
        public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] int clientId)
        {
            if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
            MessageWriter messageWriter = MessageWriter.Get(SendOption.None);
            messageWriter.StartMessage(6);
            messageWriter.Write(__instance.GameId);
            messageWriter.WritePacked(clientId);
            List<InnerNetObject> obj = __instance.allObjects;
            lock (obj)
            {
                System.Collections.Generic.HashSet<GameObject> hashSet = new();
                for (int i = 0; i < __instance.allObjects.Count; i++)
                {
                    InnerNetObject innerNetObject = __instance.allObjects[i];
                    if (innerNetObject && (innerNetObject.OwnerId != -4 || __instance.AmModdedHost) && hashSet.Add(innerNetObject.gameObject))
                    {
                        GameManager gameManager = innerNetObject as GameManager;
                        if (gameManager != null)
                        {
                            __instance.SendGameManager(clientId, gameManager);
                        }
                        else
                        {
                            if (innerNetObject is not NetworkedPlayerInfo)
                                __instance.WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
                        }
                    }
                }
                messageWriter.EndMessage();
                __instance.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
            DelaySpawnPlayerInfo(__instance, clientId);
			return false;
        }
        private static void DelaySpawnPlayerInfo(InnerNetClient __instance, int clientId)
        {
            var players = GameData.Instance.AllPlayers.ToArray().ToList();
            while (players.Count > 0)
            {
                var batch = players.Take(5).ToList();
                MessageWriter messageWriter = MessageWriter.Get(SendOption.None);
                messageWriter.StartMessage(6);
                messageWriter.Write(__instance.GameId);
                messageWriter.WritePacked(clientId);
                foreach (var player in batch)
                {
                    if (messageWriter.Length > 500) break;
                    if (player != null && player.ClientId != clientId && !player.Disconnected)
                    {
                        __instance.WriteSpawnMessage(player, player.OwnerId, player.SpawnFlags, messageWriter);
                    }
                    players.Remove(player);
                }
                messageWriter.EndMessage();
                __instance.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
    class SendAllStreamedObjectsPatch
    {
        public static bool Prefix(InnerNetClient __instance, ref bool __result)
        {
            if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
            __result = false;
            List<InnerNetObject> obj = __instance.allObjects;
            lock (obj)
            {
                for (int i = 0; i < __instance.allObjects.Count; i++)
                {
                    InnerNetObject innerNetObject = __instance.allObjects[i];
                    if (innerNetObject && innerNetObject is not NetworkedPlayerInfo && innerNetObject.IsDirty && (innerNetObject.AmOwner || (innerNetObject.OwnerId == -2 && __instance.AmHost)))
                    {
                        MessageWriter messageWriter = __instance.Streams[(int)innerNetObject.sendMode];
                        messageWriter.StartMessage(1);
                        messageWriter.WritePacked(innerNetObject.NetId);
                        try
                        {
                            if (innerNetObject.Serialize(messageWriter, false))
                            {
                                messageWriter.EndMessage();
                            }
                            else
                            {
                                messageWriter.CancelMessage();
                            }
                            if (innerNetObject.Chunked && innerNetObject.IsDirty)
                            {
                                __result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Main.Instance.Log.LogError(ex);
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
    }

    
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Spawn))]
    class SpawnPatch
    {
        public static void Postfix(InnerNetClient __instance, [HarmonyArgument(0)] InnerNetObject netObjParent)
        {
            if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return;
            if (__instance.AmHost)
            {
                switch (netObjParent)
                {
                    case NetworkedPlayerInfo playerinfo:
                        new LateTask(() => {
                            if (playerinfo != null && AmongUsClient.Instance.AmConnected)
                            {
                                var client = AmongUsClient.Instance.GetClient(playerinfo.ClientId);
                                if (client != null && !CreatePlayerPatch.IsDisconnected(client) && playerinfo.IsIncomplete)
                                {
                                    Main.Instance.Log.LogInfo($"Disconnecting Client [{client.Id}]{client.PlayerName} {client.FriendCode} for playerinfo timeout");
                                    AmongUsClient.Instance.SendLateRejection(client.Id, DisconnectReasons.ClientTimeout);
                                    __instance.OnPlayerLeft(client, DisconnectReasons.ClientTimeout);
                                }
                            }
                        }, 5f, "PlayerInfo Green Bean Kick");
                        break;
                    case PlayerControl player:
                        new LateTask(() => {
                            if (player != null && !player.notRealPlayer && !player.isDummy && AmongUsClient.Instance.AmConnected)
                            {
                                var client = AmongUsClient.Instance.GetClient(player.OwnerId);
                                if (client != null && !CreatePlayerPatch.IsDisconnected(client) && (player.Data == null || player.Data.IsIncomplete))
                                {
                                    Main.Instance.Log.LogInfo($"Disconnecting Client [{client.Id}]{client.PlayerName} {client.FriendCode} for playercontrol timeout");
                                    AmongUsClient.Instance.SendLateRejection(client.Id, DisconnectReasons.ClientTimeout);
                                    __instance.OnPlayerLeft(client, DisconnectReasons.ClientTimeout);
                                }
                            }
                        }, 5.5f, "PlayerControl Green Bean Kick");
                        break;
                }
            }
            if (!__instance.AmHost)
            {
                Debug.LogError("Tried to spawn while not host:" + (netObjParent?.ToString()));
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
    public class InnerNetClientFixedUpdatePatch
    {
        private static byte Timer = 0;
        public static void Postfix(InnerNetClient __instance)
        {
            if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return;
            if (!__instance.AmHost || __instance.Streams == null) return;
            if (Timer == 0)
            {
                Timer = 1;
                return;
            }
            var player = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(x => x.IsDirty);
            if (player != null)
            {
                Timer = 0;
                MessageWriter messageWriter = MessageWriter.Get(SendOption.None);
                messageWriter.StartMessage(5);
                messageWriter.Write(__instance.GameId);
                messageWriter.StartMessage(1);
                messageWriter.WritePacked(player.NetId);
                try
                {
                    if (player.Serialize(messageWriter, false))
                    {
                        messageWriter.EndMessage();
                    }
                    else
                    {
                        messageWriter.CancelMessage();
                        player.ClearDirtyBits();
                        return;
                    }
                    messageWriter.EndMessage();
                    __instance.SendOrDisconnect(messageWriter);
                    messageWriter.Recycle();
                }
                catch (Exception ex)
                {
                    Main.Instance.Log.LogError(ex);
                    messageWriter.CancelMessage();
                    player.ClearDirtyBits();
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.DirtyAllData))]
    class DirtyAllDataPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}