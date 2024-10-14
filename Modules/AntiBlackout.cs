using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hazel;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public static class AntiBlackout
    {
        public static bool ShowDoubleAnimation => Options.NoGameEnd.GetBool() || (Main.RealOptions.GetInt(Int32OptionNames.NumImpostors) == 1 && CustomRolesHelper.IsNeutralKillerEnabled()) || GameData.Instance.PlayerCount <= 3;
        public static bool IsCached { get; private set; } = false;
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
            IsCached = false;
            if (doSend) Utils.SendGameData();
        }

        public static void OnDisconnect(NetworkedPlayerInfo player)
        {
            if (!AmongUsClient.Instance.AmHost || !IsCached || !player.Disconnected) return;
            isDeadCache[player.PlayerId] = (true, true);
            player.IsDead = player.Disconnected = false;
            Utils.SendGameData();
        }
    }
}