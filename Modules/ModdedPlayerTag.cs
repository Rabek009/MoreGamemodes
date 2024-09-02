using System.Collections.Generic;
using System.Linq;
using Hazel;

namespace MoreGamemodes
{
    public class ModdedPlayerTag
    {
        public string FriendCode { get; set; }
        public string PreferredColor { get; set; }
        public string Tag { get; set; }

        public ModdedPlayerTag(string friendCode, string preferredColor = "ffffff", string tag = "")
        {
            FriendCode = friendCode;
            PreferredColor = preferredColor;
            Tag = tag;
        }

        public bool HasTag() => Tag != null;

        public string GetFormattedTag()
        {
            if (string.IsNullOrEmpty(Tag)) return null;

            string tagText = Tag switch
            {
                "#Dev" => "Developer",
                "#YT" => "Youtuber",
                "#VIP" => "VIP",
                "#Tester" => "Tester",
                "#Contributor" => "Contributor",
                "#Host" => "Host",
                _ => Tag
            };

            string displayColor = PreferredColor;
            return $"<size=1.5><color=#{displayColor}>{tagText}</color></size>";
        }
    }

    public static class PlayerTagManager
    {
        public static List<ModdedPlayerTag> PlayersWithTags = new();
        public static void Initialize()
        {
            PlayersWithTags.Clear();
            PlayersWithTags.Add(new(friendCode: "wallstate#7631",  preferredColor: "ff0000", tag: "#Dev"));
            PlayersWithTags.Add(new(friendCode: "puncool#9009",    preferredColor: "00ffff", tag: "#Dev"));
            PlayersWithTags.Add(new(friendCode: "stiltedgap#2406", preferredColor: "ffc0cb", tag: "#YT"));
            PlayersWithTags.Add(new(friendCode: "displayfey#3464", preferredColor: "2949e3", tag: "#YT"));
            PlayersWithTags.Add(new(friendCode: "pairseated#4990", preferredColor: "ff0000", tag: "#YT"));
            PlayersWithTags.Add(new(friendCode: "primether#5348",  preferredColor: "ff0000", tag: "#YT"));
            PlayersWithTags.Add(new(friendCode: "motorstack#2287", preferredColor: "e2bd51", tag: "#Tester"));
            PlayersWithTags.Add(new(friendCode: "leadenjoke#3670", preferredColor: "00ff00", tag: "#Tester"));
            PlayersWithTags.Add(new(friendCode: "cannylinke#0564", preferredColor: "ffffff", tag: "#Tester"));
        }

        public static bool IsPlayerTagged(string friendCode)
        {
            return PlayersWithTags.Any(x => x.FriendCode == friendCode);
        }

        public static void UpdateNameAndTag(string name, string friendCode, string newColor, bool host)
        {
            var tag = host ? GetHostTag(friendCode) : GetPlayerTag(friendCode);
            if (tag != null)
            {
                RemovePlayerTag(friendCode, host);
                tag.PreferredColor = newColor;
                PlayersWithTags.Add(tag);

                PlayerControl player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.FriendCode == friendCode);
                player.RpcSetName(name);
            }
        }

        public static ModdedPlayerTag GetPlayerTag(string friendCode)
        {
            return PlayersWithTags.FirstOrDefault(x => x.FriendCode == friendCode && x.Tag != "#Host");
        }

        public static ModdedPlayerTag GetHostTag(string friendCode)
        {
            return PlayersWithTags.FirstOrDefault(x => x.FriendCode == friendCode && x.Tag == "#Host");
        }

        public static void ResetPlayerTags()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!Main.StandardNames.ContainsKey(pc.PlayerId) || pc.Data == null || !IsPlayerTagged(pc.Data.FriendCode)) continue;
                string name = Main.StandardNames[pc.PlayerId];
                if (AmongUsClient.Instance.AmClient)
		        {
			        pc.SetName(name);
		        }
		        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.SetName, SendOption.None, -1);
		        writer.Write(pc.Data.NetId);
                writer.Write(name);
		        AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static void RemovePlayerTag(string friendCode, bool host)
        {
            var playerTag = host ? GetHostTag(friendCode) : GetPlayerTag(friendCode);
            if (playerTag != null)
            {
                PlayersWithTags.Remove(playerTag);
            }
        }
    }
}
