using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
    class TryPetPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var cancel = Main.GameStarted && CustomGamemode.Instance != null && CustomGamemode.Instance.PetAction;
            ExternalRpcPetPatch.Prefix(__instance.MyPhysics, 49, new MessageReader());
            if (cancel)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    class ExternalRpcPetPatch
    {
        public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (!Main.GameStarted) return true;
            var rpcType = (RpcCalls)callId;
            if (rpcType != RpcCalls.Pet) return true;

            PlayerControl pc = __instance.myPlayer;
            if (pc.Data.IsDead || MeetingHud.Instance) return true;
            if (PaintBattleGamemode.instance != null)
            {
                if (PaintBattleGamemode.instance.CreateBodyCooldown[pc.PlayerId] > 0f)
                    return true;
            }

            if (CustomGamemode.Instance.PetAction)
            {
                AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 50, SendOption.None, -1));
                CustomGamemode.Instance.OnPet(pc);
                return false;
            }
            return true;
        }
    }
}