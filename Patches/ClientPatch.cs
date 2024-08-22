using HarmonyLib;
using InnerNet;
using System.Collections.Generic;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return;
            if (client != null && client.FriendCode == "silkyvase#1350")
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
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return;
            if (__instance.GameState != InnerNetClient.GameStates.Started) return;
            if (Main.AllPlayersDeathReason.ContainsKey(client.Character.PlayerId))
            {
                if (client.Character.GetDeathReason() == DeathReasons.Alive)
                    client.Character.RpcSetDeathReason(DeathReasons.Disconnected);
            }
            AntiBlackout.OnDisconnect(client.Character.Data);
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
}