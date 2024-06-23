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
            if (client.Id == __instance.ClientId)
            {
                AntiCheat.Init();
                return;
            }
            OptionItem.SyncAllOptions();
            new LateTask(() => 
            {
                if (client.Character != null)
                    client.Character.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v" + Main.CurrentVersion + "! You can play without mod installed!", "Welcome");
            }, 2f, "Welcome Message");
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return;
            if (Main.AllPlayersDeathReason.ContainsKey(client.Character.PlayerId))
            {
                if (client.Character.GetDeathReason() == DeathReasons.Alive)
                    client.Character.RpcSetDeathReason(DeathReasons.Disconnected);
            }
            if (__instance.GameState == InnerNetClient.GameStates.Started)
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

    // [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnBecomeHost))]
    // class OnBecomeHostPatch
    // {
    //     public static void Postfix(AmongUsClient __instance)
    //     {
    //         if (__instance.IsGameStarted)
    //         {
    //             List<byte> winners = new();
    //             foreach (var pc in PlayerControl.AllPlayerControls)
    //                 winners.Add(pc.PlayerId);
    //             CheckEndCriteriaPatch.StartEndGame(GameOverReason.HumansByVote, winners);
    //         }
    //         else
    //         {
    //             AntiCheat.Init();
    //             PlayerControl.LocalPlayer.RpcSendMessage("You are new host! Now this lobby modded with More Gamemodes v" + Main.CurrentVersion + "!", "Host");
    //             foreach (var pc in PlayerControl.AllPlayerControls)
    //             {
    //                 if (!pc.AmOwner)
    //                     pc.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v" + Main.CurrentVersion + "! You can play without mod installed!", "Welcome");
    //             }
    //             PlayerControl.LocalPlayer.RpcRequestVersionCheck();
    //         }
    //     }
    // }
}