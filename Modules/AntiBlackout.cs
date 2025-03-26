using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/AntiBlackout.cs
namespace MoreGamemodes
{
    public static class AntiBlackout
    {
        public static bool ShowDoubleAnimation => Options.NoGameEnd.GetBool() || (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && Main.RealOptions.GetInt(Int32OptionNames.NumImpostors) == 1) || GameData.Instance.PlayerCount <= 3;
        public static bool IsCached = false;
        private static Dictionary<byte, (bool isDead, bool Disconnected)> isDeadCache = new();

        public static void SetIsDead(bool doSend = true)
        {
            isDeadCache.Clear();
            foreach (var info in GameData.Instance.AllPlayers)
            {
                if (info == null) continue;
                isDeadCache[info.PlayerId] = (info.IsDead, info.Disconnected);
                info.IsDead = false;
                info.Disconnected = false;
            }
            IsCached = true;
            if (doSend) Utils.SendGameData();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.IsDead)
                    pc.SetChatVisible(false);
            }
        }
        public static void RestoreIsDead(bool doSend = true)
        {
            foreach (var info in GameData.Instance.AllPlayers)
            {
                if (info == null) continue;
                if (isDeadCache.TryGetValue(info.PlayerId, out var val))
                {
                    info.IsDead = val.isDead;
                    info.Disconnected = val.Disconnected;
                }
            }
            isDeadCache.Clear();
            if (doSend) Utils.SendGameData();
        }

        public static void OnDisconnect(NetworkedPlayerInfo player)
        {
            if (!AmongUsClient.Instance.AmHost || !IsCached || !player.Disconnected) return;
            isDeadCache[player.PlayerId] = (true, true);
            player.IsDead = player.Disconnected = false;
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage(5);
            {
                writer.Write(AmongUsClient.Instance.GameId);
                writer.StartMessage(1);
                {
                    writer.WritePacked(player.NetId);
                    player.Serialize(writer, false);
                }
                writer.EndMessage();
            }
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
            player.IsDead = player.Disconnected = false;
        }

        public static void Reset()
        {
            if (isDeadCache == null) isDeadCache = new();
            isDeadCache.Clear();
            IsCached = false;
            ExileControllerWrapUpPatch.AntiBlackout_LastExiled = null;
        }
    }
}