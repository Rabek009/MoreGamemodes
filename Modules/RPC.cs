using HarmonyLib;
using Hazel;
using InnerNet;

namespace MoreGamemodes
{
    enum CustomRPC
    {
        VersionCheck = 70,
        SyncCustomOptions,
        SetBomb,
        SetItem,
        SetHackTimer,
        SetPaintTime,
        SetTheme,
        SetIsKiller,
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
    class ShipStatusHandleRpc
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.UpdateSystem:
                    __instance.UpdateSystem((SystemTypes)subReader.ReadByte(), subReader.ReadNetObject<PlayerControl>(), subReader.ReadByte());
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class PlayerControlHandleRpc
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.SendChat:
                    var text = subReader.ReadString();
                    if (!SendChatPatch.OnReceiveChat(__instance, text)) return false;
                    break;
                case RpcCalls.UsePlatform:
                    if (Options.DisableGapPlatform.GetBool()) return false;
                    break;
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.VersionCheck:
                    if (!AmongUsClient.Instance.AmHost) break;
                    if (reader.ReadString() != Main.CurrentVersion)
                    {
                        Utils.SendChat(__instance.Data.PlayerName + " was kicked for having other version of More Gamemodes.", "AutoKick");
                        AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
                    }
                    break;
                case CustomRPC.SetBomb:
                    __instance.SetBomb(reader.ReadBoolean());
                    break;
                case CustomRPC.SetItem:
                    __instance.SetItem((Items)reader.ReadPackedUInt32());
                    break;
                case CustomRPC.SetIsKiller:
                    __instance.SetIsKiller(reader.ReadBoolean());
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.HandleRpc))]
    class GameManagerHandleRpc
    {
        public static void Postfix(GameManager __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.SyncCustomOptions:
                    foreach (var co in OptionItem.AllOptions)
                        co.CurrentValue = reader.ReadInt32();
                    break;
                case CustomRPC.SetHackTimer:
                    if (RandomItemsGamemode.instance == null) break;
                    RandomItemsGamemode.instance.HackTimer = reader.ReadInt32();
                    break;
                case CustomRPC.SetPaintTime:
                    if (PaintBattleGamemode.instance == null) break;
                    PaintBattleGamemode.instance.PaintTime = reader.ReadInt32();
                    break;
                case CustomRPC.SetTheme:
                    __instance.SetTheme(reader.ReadString());
                    break;
            }
        }
    }

    static class RPC
    {
        public static void SetBomb(this PlayerControl player, bool hasBomb)
        {
            if (BombTagGamemode.instance == null) return;
            BombTagGamemode.instance.HasBomb[player.PlayerId] = hasBomb;
            HudManager.Instance.TaskPanel.SetTaskText("");
        }
        
        public static void SetItem(this PlayerControl player, Items item)
        {
            if (RandomItemsGamemode.instance == null) return;
            RandomItemsGamemode.instance.AllPlayersItems[player.PlayerId] = item;
        }

        public static void SetTheme(this GameManager manager, string theme)
        {
            if (PaintBattleGamemode.instance == null)
            PaintBattleGamemode.instance.Theme = theme;
            HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void SetIsKiller(this PlayerControl player, bool isKiller)
        {
            if (KillOrDieGamemode.instance == null) return;
            KillOrDieGamemode.instance.IsKiller[player.PlayerId] = isKiller;
            HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void RpcVersionCheck(this PlayerControl player, string version)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.VersionCheck, SendOption.Reliable, AmongUsClient.Instance.HostId);
            writer.Write(version);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSyncCustomOptions(this GameManager manager)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable, -1);
            foreach (var co in OptionItem.AllOptions)
            {
                writer.Write(co.CurrentValue);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetBomb(this PlayerControl player, bool hasBomb)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            player.SetBomb(hasBomb);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetBomb, SendOption.Reliable, -1);
            writer.Write(hasBomb);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetItem(this PlayerControl player, Items item)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            player.SetItem(item);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetItem, SendOption.Reliable, -1);
            writer.Write((uint)item);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetHackTimer(this GameManager manager, int time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (RandomItemsGamemode.instance == null) return;
            RandomItemsGamemode.instance.HackTimer = time;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetHackTimer, SendOption.Reliable, -1);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetPaintTime(this GameManager manager, int time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PaintBattleGamemode.instance == null) return;
            PaintBattleGamemode.instance.PaintTime = time;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetPaintTime, SendOption.Reliable, -1);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetTheme(this GameManager manager, string theme)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PaintBattleGamemode.instance == null) return;
            manager.SetTheme(theme);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetTheme, SendOption.Reliable, -1);
            writer.Write(theme);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetIsKiller(this PlayerControl player, bool isKiller)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            player.SetIsKiller(isKiller);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetIsKiller, SendOption.Reliable, -1);
            writer.Write(isKiller);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}