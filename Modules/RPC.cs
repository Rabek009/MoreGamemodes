using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    enum CustomRPC
    {
        VersionCheck = 52,
        SyncCustomOptions,
        AddImpostor,
        SetBomb,
        SetItem,
        SetHackTimer,
        SetPaintTime,
        SetTheme,
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
            byte playerId = 255;
            switch (rpcType)
            {
                case CustomRPC.VersionCheck:
                    if (!AmongUsClient.Instance.AmHost) break;
                    playerId = reader.ReadByte();
                    if (reader.ReadString() != Main.CurrentVersion)
                    {
                        Utils.SendChat(Utils.GetPlayerById(playerId).Data.PlayerName + " was kicked for having other version of More Gamemodes.", "AutoKick");
                        AmongUsClient.Instance.KickPlayer(Utils.GetPlayerById(playerId).GetClientId(), false);
                    }
                    break;
                case CustomRPC.SyncCustomOptions:
                    foreach (var co in OptionItem.AllOptions)
                        co.CurrentValue = reader.ReadInt32();
                    break;
                case CustomRPC.AddImpostor:
                    playerId = reader.ReadByte();
                    if (Utils.GetPlayerById(playerId) == null) break;
                    Utils.GetPlayerById(playerId).AddImpostor();
                    break;
                case CustomRPC.SetBomb:
                    playerId = reader.ReadByte();
                    if (Utils.GetPlayerById(playerId) == null) break;
                    Utils.GetPlayerById(playerId).SetBomb(reader.ReadBoolean());
                    HudManager.Instance.TaskPanel.SetTaskText("");
                    break;
                case CustomRPC.SetItem:
                    playerId = reader.ReadByte();
                    if (Utils.GetPlayerById(playerId) == null) break;
                    Utils.GetPlayerById(playerId).SetItem((Items)reader.ReadPackedUInt32());
                    break; 
                case CustomRPC.SetHackTimer:
                    Main.HackTimer = reader.ReadInt32();
                    break;
                case CustomRPC.SetPaintTime:
                    Main.PaintTime = reader.ReadInt32();
                    break;
                case CustomRPC.SetTheme:
                    Main.Theme = reader.ReadString();
                    HudManager.Instance.TaskPanel.SetTaskText("");
                    break;
            }
        }
    }

    static class RPC
    {
        public static void AddImpostor(this PlayerControl player)
        {
            Main.Impostors.Add(player.PlayerId);
        }

        public static void SetBomb(this PlayerControl player, bool hasBomb)
        {
            Main.HasBomb[player.PlayerId] = hasBomb;
            HudManager.Instance.TaskPanel.SetTaskText("");
        }
        
        public static void SetItem(this PlayerControl player, Items item)
        {
            Main.AllPlayersItems[player.PlayerId] = item;
        }

        public static void SetTheme(string theme)
        {
            Main.Theme = theme;
            HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void RpcVersionCheck(PlayerControl player, string version)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionCheck, SendOption.Reliable, AmongUsClient.Instance.HostId);
            writer.Write(player.PlayerId);
            writer.Write(version);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

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

        public static void RpcAddImpostor(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            player.AddImpostor();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AddImpostor, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetBomb(this PlayerControl player, bool hasBomb)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            player.SetBomb(hasBomb);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBomb, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(hasBomb);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetItem(this PlayerControl player, Items item)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            player.SetItem(item);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetItem, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write((uint)item);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetHackTimer(int time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetHackTimer, SendOption.Reliable, -1);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.HackTimer = time;
        }

        public static void RpcSetPaintTime(int time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPaintTime, SendOption.Reliable, -1);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.PaintTime = time;
        }

        public static void RpcSetTheme(string theme)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            SetTheme(theme);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTheme, SendOption.Reliable, -1);
            writer.Write(theme);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}