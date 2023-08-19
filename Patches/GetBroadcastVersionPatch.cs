using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    public static class GetBroadcastVersionPatch
    {
        public static bool Prefix(ref int __result)
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
            {
                return true;
            }
            __result = Constants.GetVersion(2222, 0, 0, 0);
            return false;
        }
    }
}