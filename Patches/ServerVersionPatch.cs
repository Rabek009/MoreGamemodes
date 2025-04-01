using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    class GetBroadcastVersionPatch
    {
        public static void Postfix(ref int __result)
        {
            if (!Main.ModdedProtocol.Value)
			{
				Main.Instance.Log.LogMessage("Modded protocol disabled");
			}
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && Main.ModdedProtocol.Value)
            {
                __result += 25;
            }
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
            {
                Main.Instance.Log.LogMessage($"IsLocalGame : {__result}");
            }
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
    class IsVersionModdedPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = Main.ModdedProtocol.Value;
            return false;
        }
    }
}