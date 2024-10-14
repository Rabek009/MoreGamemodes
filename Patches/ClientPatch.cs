using HarmonyLib;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return;
            if (client != null && client.FriendCode == "silkyvase#1350" || client.FriendCode == "sablewire#9833"/* || client.FriendCode == "rallybrass#1326"*/)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                return;
            }
            if (client != null && client.Id == __instance.ClientId)
            {
                AntiCheat.Init();
                PlayerTagManager.Initialize();
                PlayerTagManager.PlayersWithTags.Add(new ModdedPlayerTag(client.FriendCode, "00ff00", "#Host"));
                return;
            }
            if (client != null && Options.AntiCheat.GetBool() && !Utils.IsValidFriendCode(client.FriendCode) && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, Options.CurrentCheatingPenalty == CheatingPenalties.Ban);
                return;
            }
            OptionItem.SyncAllOptions();
            new LateTask(() => 
            {
                if (client != null && client.Character != null)
                    client.Character.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v" + Main.CurrentVersion + "! You can play without mod installed!", "Welcome");
                else if (client != null)
                    __instance.KickPlayer(client.Id, false);
            }, 2f, "Welcome Message");
            new LateTask(() => 
            {
                if (client != null && client.Character != null && (client.Character.Data == null || client.Character.Data.IsIncomplete))
                {
                    client.Character.Despawn();
                    __instance.KickPlayer(client.Id, false);
                }
            }, 3f, "Kick Fortegreen");
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
                Main.StandardNames = new Dictionary<byte, string>();
                new LateTask(() => PlayerControl.LocalPlayer.RpcVersionCheck(Main.CurrentVersion), 2f, "Version Check");
            }
        }
    }

    [HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
    class BanMenuSetVisiblePatch
    {
        public static bool Prefix(BanMenu __instance, bool show)
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

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnBecomeHost))]
    class OnBecomeHostPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            if (__instance.IsGameStarted)
            {
                List<byte> winners = new();
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

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    class AddVotePatch
    {
        public static bool Prefix(VoteBanSystem __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            new LateTask(() => __instance.SetDirtyBit(1U), 0.5f);
            return false;
        }
    }

    // https://github.com/Gurge44/EndlessHostRoles/blob/main/Patches/ClientPatch.cs
    static class InnerNetClientPatch
    {
        private static byte Timer;

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendInitialData))]
        [HarmonyPrefix]
        public static bool SendInitialDataPrefix(InnerNetClient __instance, int clientId)
        {
            if (!Constants.IsVersionModded() || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
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
            var players = GameData.Instance.AllPlayers.ToArray();

            foreach (var batch in players.Chunk(5))
            {
                MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
                messageWriter.StartMessage(6);
                messageWriter.Write(__instance.GameId);
                messageWriter.WritePacked(clientId);

                batch.DoIf(p => p != null && p.ClientId != clientId && !p.Disconnected, p => __instance.WriteSpawnMessage(p, p.OwnerId, p.SpawnFlags, messageWriter));

                messageWriter.EndMessage();
                __instance.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
        [HarmonyPrefix]
        public static bool SendAllStreamedObjectsPrefix(InnerNetClient __instance, ref bool __result)
        {
            if (!Constants.IsVersionModded() || __instance.NetworkMode != NetworkModes.OnlineGame) return true;
            __result = false;
            Il2CppSystem.Collections.Generic.List<InnerNetObject> obj = __instance.allObjects;
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
                        if (innerNetObject.Serialize(messageWriter, false)) messageWriter.EndMessage();
                        else messageWriter.CancelMessage();

                        if (innerNetObject.Chunked && innerNetObject.IsDirty)
                            __result = true;
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

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
        [HarmonyPostfix]
        public static void FixedUpdatePostfix(InnerNetClient __instance)
        {
            if (!Constants.IsVersionModded() || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || !__instance.AmHost || __instance.Streams == null || __instance.NetworkMode != NetworkModes.OnlineGame) return;

            if (Timer == 0)
            {
                Timer = 1;
                return;
            }

            var player = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(x => x.IsDirty);
            if (player != null)
            {
                Timer = 0;
                MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
                messageWriter.StartMessage(5);
                messageWriter.Write(__instance.GameId);
                messageWriter.StartMessage(1);
                messageWriter.WritePacked(player.NetId);
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
        }
    }
}