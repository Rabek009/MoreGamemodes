using HarmonyLib;
using InnerNet;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Main.StandardNames[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.PlayerName;
            Main.StandardColors[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.ColorId;
            Main.StandardHats[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.HatId;
            Main.StandardSkins[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.SkinId;
            Main.StandardPets[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.PetId;
            Main.StandardVisors[client.Character.PlayerId] = client.Character.Data.DefaultOutfit.VisorId;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (client.Character.AmOwner) return;
            new LateTask(() =>
            {
                client.Character.RpcSendMessage("Welcome to More Gamemodes lobby! This is mod that addes new gamemodes. Type '/h gm' to see current gamemode description and '/n' to see current options. You can also type '/cm' to see other commands. Have fun playing these new gamemodes!", "Welcome");
            }, 1.5f, "Welcome Message");
        }
    }
}