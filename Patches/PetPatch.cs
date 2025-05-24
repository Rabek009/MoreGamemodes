using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    // https://github.com/discus-sions/TownOfHost-TheOtherRoles/blob/main/Patches/PetActionsPatch.cs
    // Thanks to ImaMapleTree
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
            if (callId != 75)
            {
                if (AntiCheat.PlayerPhysicsReceiveRpc(__instance, callId, reader)) return false;
            }
            if (callId != 49 && callId != 75) return true;

            PlayerControl pc = __instance.myPlayer;
            if (pc.Data.IsDead || MeetingHud.Instance || Main.TimeSinceLastPet[pc.PlayerId] < 0.5f) return true;
            if (CustomGamemode.Instance.PetAction)
            {
                if (callId == 49)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.CancelPet, SendOption.None, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.CancelPet, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                }
                CustomGamemode.Instance.OnPet(pc);
                Main.TimeSinceLastPet[pc.PlayerId] = 0f;
                return false;
            }
            return true;
        }
    }
}