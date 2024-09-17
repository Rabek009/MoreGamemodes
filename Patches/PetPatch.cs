using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
    class TryPetPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost && CustomGamemode.Instance != null && CustomGamemode.Instance.PetAction)
            {
                __instance.RpcPetAction();
                return false;
            }
            if (!AmongUsClient.Instance.AmHost) return true;
            var cancel = Main.GameStarted && CustomGamemode.Instance != null && CustomGamemode.Instance.PetAction;
            ExternalRpcPetPatch.Prefix(__instance.MyPhysics, (byte)CustomRPC.PetAction, new MessageReader());
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
            if (callId != 85)
            {
                if (AntiCheat.PlayerPhysicsReceiveRpc(__instance, callId, reader)) return false;
            }
            if (callId != 49 && callId != 85) return true;

            PlayerControl pc = __instance.myPlayer;
            if (pc.Data.IsDead || MeetingHud.Instance) return true;
            if (JailbreakGamemode.instance != null)
            {
                if (JailbreakGamemode.instance.ChangeRecipeCooldown[pc.PlayerId] > 0f)
                    return true;
            }
            if (CustomGamemode.Instance.PetAction)
            {
                if (callId == 49)
                    AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.CancelPet, SendOption.None, -1));
                CustomGamemode.Instance.OnPet(pc);
                return false;
            }
            return true;
        }
    }
}