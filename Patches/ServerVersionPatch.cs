using HarmonyLib;
using Hazel;
using InnerNet;

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
            __result = Main.ModdedProtocol.Value;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
	class PreventAntiCheat
	{
		public static void Postfix(GameStartManager __instance)
		{
			if (!AmongUsClient.Instance.AmHost || Main.ModdedProtocol.Value) return;
			MessageWriter msg = MessageWriter.Get(SendOption.Reliable);
			msg.StartMessage(6);
			msg.Write(AmongUsClient.Instance.GameId);
			msg.WritePacked(int.MaxValue);
			for (int i = 0; i < AmongUsClient.Instance.allObjects.Count; i++)
			{
				InnerNetObject innerNetObject = AmongUsClient.Instance.allObjects[i];
				msg.StartMessage(4);
				msg.WritePacked(GameStartManager.Instance.LobbyPrefab.SpawnId);
				msg.WritePacked(innerNetObject.OwnerId);
				msg.Write((byte)innerNetObject.SpawnFlags);
				msg.WritePacked(1);
				msg.WritePacked(innerNetObject.NetId);
				msg.StartMessage(1);
				msg.EndMessage();
				msg.EndMessage();
			}
			msg.EndMessage();
			AmongUsClient.Instance.SendOrDisconnect(msg);
			msg.Recycle();
			__instance.FinallyBegin();
		}
	}
}