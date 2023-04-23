using HarmonyLib;
using InnerNet;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (client.Character == PlayerControl.LocalPlayer && !__instance.AmHost)
                Options.Gamemode.SetValue(0);
            if (!AmongUsClient.Instance.AmHost) return;
            Main.StandardNames[client.Character.PlayerId] = client.Character.Data.PlayerName;
            Main.StandardColors[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.ColorId;
            Main.StandardHats[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.HatId;
            Main.StandardSkins[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.SkinId;
            Main.StandardPets[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.PetId;
            Main.StandardVisors[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.VisorId;
            Main.StandardNamePlates[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.NamePlateId;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!__instance.AmHost) return;
            if (client.Character.AmOwner) return;
            new LateTask(() =>
            {
                client.Character.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes! This lobby uses More Gamemodes v0.1.0! If you use other verison of this mod, please leave!", "Welcome");
            }, 1.5f, "Welcome Message");
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client, [HarmonyArgument(1)] DisconnectReasons reason)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Main.GameStarted && client.Character.GetDeathReason() == DeathReasons.Alive)
                client.Character.RpcSetDeathReason(DeathReasons.Disconnected);
        }
    }
}