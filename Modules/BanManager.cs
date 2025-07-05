using System;
using System.Collections.Generic;
using System.IO;

namespace MoreGamemodes
{
    // https://github.com/tukasa0001/TownOfHost/blob/main/Modules/BanManager.cs
    public static class BanManager
    {
        private static readonly string BAN_LIST_FRIEND_CODE_PATH = @"./MGM_DATA/BanListFriendCode.txt";
        private static readonly string BAN_LIST_PUID_PATH = @"./MGM_DATA/BanListPuid.txt";

        public static void Init()
        {
            Directory.CreateDirectory("MGM_DATA");
            if (!File.Exists(BAN_LIST_FRIEND_CODE_PATH)) File.Create(BAN_LIST_FRIEND_CODE_PATH).Close();
            if (!File.Exists(BAN_LIST_PUID_PATH)) File.Create(BAN_LIST_PUID_PATH).Close();
        }
        public static void AddBanPlayer(InnerNet.ClientData player)
        {
            if (!AmongUsClient.Instance.AmHost || player == null) return;
            if (!CheckBanFriendCodeList(player) && player.FriendCode != "")
            {
                File.AppendAllText(BAN_LIST_FRIEND_CODE_PATH, $"{player.FriendCode},{player.PlayerName}\n");
            }
            if (!CheckBanPuidList(player) && player.GetHashedPuid() != "")
            {
                File.AppendAllText(BAN_LIST_PUID_PATH, $"{player.GetHashedPuid()},{player.PlayerName}\n");
            }
        }
        public static bool CheckBanPlayer(InnerNet.ClientData player)
        {
            if (!AmongUsClient.Instance.AmHost || !Main.ApplyBanList.Value) return false;
            if (CheckBanFriendCodeList(player)) return true;
            if (CheckBanPuidList(player)) return true;
            return false;
        }
        public static bool CheckBanFriendCodeList(InnerNet.ClientData player)
        {
            if (player == null || player.FriendCode == "") return false;
            try
            {
                Directory.CreateDirectory("MGM_DATA");
                if (!File.Exists(BAN_LIST_FRIEND_CODE_PATH)) File.Create(BAN_LIST_FRIEND_CODE_PATH).Close();
                using StreamReader sr = new(BAN_LIST_FRIEND_CODE_PATH);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == "") continue;
                    if (player.FriendCode == line.Split(",")[0]) return true;
                }
            }
            catch (Exception ex)
            {
                Main.Instance.Log.LogError(ex.ToString());
            }
            return false;
        }
        public static bool CheckBanPuidList(InnerNet.ClientData player)
        {
            if (player == null || player.GetHashedPuid() == "") return false;
            try
            {
                Directory.CreateDirectory("MGM_DATA");
                if (!File.Exists(BAN_LIST_PUID_PATH)) File.Create(BAN_LIST_PUID_PATH).Close();
                using StreamReader sr = new(BAN_LIST_PUID_PATH);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == "") continue;
                    if (player.GetHashedPuid() == line.Split(",")[0]) return true;
                }
            }
            catch (Exception ex)
            {
                Main.Instance.Log.LogError(ex.ToString());
            }
            return false;
        }

        // Players that are banned from entire mod
        public static readonly List<string> BannedFriendCodes = new(){
            "silkyvase#1350",
            "skewgram#9364",
            "metersmall#7725",
            "dashsheer#3493",
            // "curltwisty#5938", (got second chance)
            "cat#2761",
            "tenpalace#1924",
            "giltgram#4461",
        };
        public static readonly List<string> BannedHashedPuids = new(){
            "b0437b1e6",
            "8da2722fd",
            "f70d4ddb1",
            "6a259fcd9",
        };
    }
}