using HarmonyLib;
using Hazel;
using InnerNet;
using AmongUs.GameOptions;
using AmongUs.Data;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData clientData)
        {
            if (!__instance.AmHost) return true;
            if (clientData != null && (GameStartManager.Instance == null || GameStartManager.Instance.startState != GameStartManager.StartingStates.NotStarting))
            {
                AmongUsClient.Instance.KickPlayer(clientData.Id, false);
                return false;
            }
            if (clientData != null && (BanManager.BannedFriendCodes.Contains(clientData.FriendCode) || BanManager.BannedHashedPuids.Contains(clientData.GetHashedPuid())))
            {
                AmongUsClient.Instance.KickPlayer(clientData.Id, true);
                return false;
            }
            if (clientData != null && clientData.Id == __instance.ClientId)
            {
                AntiCheat.Init();
                PlayerTagManager.Initialize();
                PlayerTagManager.PlayersWithTags.Add(new ModdedPlayerTag(clientData.FriendCode, "00ff00", "#Host"));
                return true;
            }
            if (clientData != null && Options.AntiCheat.GetBool() && Utils.IsVanillaServer() && (!Utils.IsValidFriendCode(clientData.FriendCode) || clientData.ProductUserId == "") && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
            {
                AmongUsClient.Instance.KickPlayer(clientData.Id, true);
                return false;
            }
            if (clientData != null && BanManager.CheckBanPlayer(clientData))
            {
                AmongUsClient.Instance.KickPlayer(clientData.Id, true);
                return false;
            }
            OptionItem.SyncAllOptions();
            new LateTask(() => 
            {
                if (clientData != null && clientData.Character != null)
                    clientData.Character.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v" + Main.CurrentVersion + "! You can play without mod installed!", "Welcome");
            }, 2f, "Welcome Message");
            return true;
        }
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData clientData)
        {
            if (!__instance.AmHost) return;
            new LateTask(() =>
            {
                try
                {
                    if (AmongUsClient.Instance.AmHost && !IsDisconnected(clientData) && clientData.Character.Data.IsIncomplete)
                    {
                        AmongUsClient.Instance.KickPlayer(clientData.Id, false);
                        Main.Instance.Log.LogInfo($"Kicked client {clientData.Id}/{clientData.PlayerName} bcz PlayerControl is not spawned in time.");
                        return;
                    }
                }
                catch { }
            }, 4.5f, "green bean kick late task");
        }
        // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Patches/PlayerJoinAndLeftPatch.cs#L164
        public static bool IsDisconnected(ClientData client)
        {
            var __instance = AmongUsClient.Instance;
            for (int i = 0; i < __instance.allClients.Count; i++)
            {
                ClientData clientData = __instance.allClients[i];
                if (clientData.Id == client.Id)
                    return true;
            }
            return false;
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
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.CrewmatesByVote, winners);
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
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
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

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
    class HostGamePatch
    {
        public static void Prefix(InnerNetClient __instance, [HarmonyArgument(0)] ref IGameOptions settings)
        {
            if (!Utils.IsVanillaServer()) return;
            if (settings.MaxPlayers < 4)
                settings.SetInt(Int32OptionNames.MaxPlayers, 4);
            if (settings.MaxPlayers > 15)
                settings.SetInt(Int32OptionNames.MaxPlayers, 15);
            if (settings.NumImpostors < 1)
                settings.SetInt(Int32OptionNames.NumImpostors, 1);
            if (settings.NumImpostors > 3)
                settings.SetInt(Int32OptionNames.NumImpostors, 3);
            if (settings.GetInt(Int32OptionNames.KillDistance) < 0)
                settings.SetInt(Int32OptionNames.KillDistance, 0);
            if (settings.GetInt(Int32OptionNames.KillDistance) > 2)
                settings.SetInt(Int32OptionNames.KillDistance, 2);
            if (settings.GetFloat(FloatOptionNames.PlayerSpeedMod) <= 0f)
                settings.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.0001f);
            if (settings.GetFloat(FloatOptionNames.PlayerSpeedMod) > 3f)
                settings.SetFloat(FloatOptionNames.PlayerSpeedMod, 3f);
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
    class SendAllStreamedObjectsPatch
    {
        public static void Prefix(InnerNetClient __instance)
        {
            if (!__instance.AmHost || !AntiBlackout.IsCached) return;
            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.IsDirty)
                    playerInfo.ClearDirtyBits();
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CheckOnlinePermissions))]
    class CheckOnlinePermissionsPatch
    {
        public static void Prefix()
        {
            DataManager.Player.Ban.banPoints = 0f;
        }
    }
}