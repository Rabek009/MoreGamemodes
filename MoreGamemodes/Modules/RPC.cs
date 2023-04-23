using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    enum CustomRPC
    {
        SyncCustomOptions = 51,
        SetBomb,
        SetItem,
        AddImpostor,
        ToggleCanVent,
        ToggleCanUseKillButton,
        ToggleCanBeKilled,
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.SetName:
                    string name = subReader.ReadString();
                    if (subReader.BytesRemaining > 0 && subReader.ReadBoolean()) return false;
                    break;
                case RpcCalls.SendChat:
                    var text = subReader.ReadString();
                    if (!SendChatPatch.OnReceiveChat(__instance, text)) return false;
                    break;
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.SyncCustomOptions:
                    foreach (var co in OptionItem.AllOptions)
                        co.CurrentValue = reader.ReadInt32();
                    break;
                case CustomRPC.SetBomb:
                    Main.HasBomb[reader.ReadByte()] = reader.ReadBoolean();
                    break;
                case CustomRPC.SetItem:
                    Main.AllPlayersItems[reader.ReadByte()] = (Items)reader.ReadPackedInt32();
                    break;
                case CustomRPC.AddImpostor:
                    Main.Impostors.Add(reader.ReadByte());
                    break;
                case CustomRPC.ToggleCanVent:
                    Utils.GetPlayerById(reader.ReadByte()).Data.Role.CanVent = reader.ReadBoolean();
                    break;
                case CustomRPC.ToggleCanUseKillButton:
                    Utils.GetPlayerById(reader.ReadByte()).Data.Role.CanUseKillButton = reader.ReadBoolean();
                    break;
                case CustomRPC.ToggleCanBeKilled:
                    Utils.GetPlayerById(reader.ReadByte()).Data.Role.CanBeKilled = reader.ReadBoolean();
                    break;
            }
        }
    }

    static class RPC
    {
        public static void RpcSyncCustomOptions()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable, -1);
            foreach (var co in OptionItem.AllOptions)
            {
                writer.Write(co.CurrentValue);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetBomb(this PlayerControl player, bool hasBomb)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBomb, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(hasBomb);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.HasBomb[player.PlayerId] = hasBomb;
        }

        public static void RpcSetItem(this PlayerControl player, Items item)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetItem, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write((int)item);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.AllPlayersItems[player.PlayerId] = item;
        }

        public static void RpcAddImpostor(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AddImpostor, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.Impostors.Add(player.PlayerId);
        }

        public static void RpcToggleCanVent(this PlayerControl player, bool canVent)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ToggleCanVent, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(canVent);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            player.Data.Role.CanVent = canVent;
        }

        public static void RpcToggleCanUseKillButton(this PlayerControl player, bool canUseKillButton)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ToggleCanUseKillButton, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(canUseKillButton);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            player.Data.Role.CanUseKillButton = canUseKillButton;
        }

        public static void RpcToggleCanBeKilled(this PlayerControl player, bool canBeKilled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ToggleCanBeKilled, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(canBeKilled);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            player.Data.Role.CanBeKilled = canBeKilled;
        }
    }
}