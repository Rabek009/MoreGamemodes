using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
    class TryPetPatch
    {
        public static void Prefix(PlayerControl __instance)
        {
            if (!(AmongUsClient.Instance.AmHost && AmongUsClient.Instance.AmClient)) return;
            if (__instance.Data.IsDead || MeetingHud.Instance) return;
            var cancel = (Options.CurrentGamemode == Gamemodes.RandomItems || Options.CurrentGamemode == Gamemodes.PaintBattle) && Main.GameStarted;
            if (cancel)
                __instance.petting = true;
            ExternalRpcPetPatch.Prefix(__instance.MyPhysics, 51, new MessageReader());
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (__instance.Data.IsDead || MeetingHud.Instance) return;
            var cancel = (Options.CurrentGamemode == Gamemodes.RandomItems || Options.CurrentGamemode == Gamemodes.PaintBattle) && Main.GameStarted;
            if (cancel)
            {
                __instance.petting = false;
                if (__instance.AmOwner)
                    __instance.MyPhysics.RpcCancelPet();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    class ExternalRpcPetPatch
    {
        public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!Main.GameStarted) return;
            var rpcType = callId == 51 ? RpcCalls.Pet : (RpcCalls)callId;
            if (rpcType != RpcCalls.Pet) return;

            PlayerControl pc = __instance.myPlayer;
            if (pc.Data.IsDead || MeetingHud.Instance) return;
            if (PaintBattleGamemode.instance != null)
            {
                if (PaintBattleGamemode.instance.CreateBodyCooldown[pc.PlayerId] > 0f)
                    return;
            }

            if (callId == 51 && CustomGamemode.Instance.PetAction)
                __instance.CancelPet();
            if (callId != 51 && CustomGamemode.Instance.PetAction)
            {
                __instance.CancelPet();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 50, SendOption.None, player.GetClientId()));
            }

            CustomGamemode.Instance.OnPet(pc);
        }
    }
}