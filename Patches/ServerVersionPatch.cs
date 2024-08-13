using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    class GetBroadcastVersionPatch
    {
        public static void Postfix(ref int __result)
        {
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
            {
                __result += 25; // Protocol Broken
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
            __result = true;
            return false;
        }
    }
}