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
        public bool IsDeveloper { get; set; }

        public ModdedPlayerTag(string friendCode, string preferredColor = "FFFFFF", string tag = "", bool isDeveloper = false)
        {
            FriendCode = friendCode;
            PreferredColor = preferredColor;
            Tag = tag;
            IsDeveloper = isDeveloper;
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
            PlayersWithTags.Add(new(friendCode: "wallstate#7631",  preferredColor: "ff0000",  tag: "#Dev",    isDeveloper: true));
            PlayersWithTags.Add(new(friendCode: "motorstack#2287", preferredColor: "e2bd51",  tag: "#Tester", isDeveloper: false));
            PlayersWithTags.Add(new(friendCode: "leadenjoke#3670", preferredColor: "00ff00",  tag: "#Tester", isDeveloper: false));
            PlayersWithTags.Add(new(friendCode: "cannylinke#0564", preferredColor: "ffffff", tag: "#Tester", isDeveloper: false));
            PlayersWithTags.Add(new(friendCode: "stiltedgap#2406", preferredColor:  "ffc0cb", tag:"#YT",      isDeveloper:false));
        }

        public static bool IsPlayerTagged(string friendCode)
        {
            return PlayersWithTags.Any(x => x.FriendCode == friendCode);
        }

        public static void UpdateNameAndTag(string name, string friendCode, string newColor)
        {
            var tag = GetPlayerTag(friendCode);
            if (tag != null)
            {
                RemovePlayerTag(friendCode);
                tag.PreferredColor = newColor;
                PlayersWithTags.Add(tag);

                PlayerControl player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.FriendCode == friendCode);
                player.RpcSetName(name);
            }
        }

        public static List<ModdedPlayerTag> GetAllPlayersTags(string Friendcode)
        {
            return PlayersWithTags.Where(x => x.FriendCode == Friendcode).ToList();
        }

        public static ModdedPlayerTag GetPlayerTag(string friendCode)
        {
            return PlayersWithTags.FirstOrDefault(x => x.FriendCode == friendCode);
        }

        public static void ResetPlayerTags()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
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

        public static void RemovePlayerTag(string friendCode)
        {
            var playerTag = PlayersWithTags.FirstOrDefault(tag => tag.FriendCode == friendCode);
            if (playerTag != null)
            {
                PlayersWithTags.Remove(playerTag);
            }
        }
    }
}
