using System.Collections.Generic;
using Il2CppSystem;
using UnityEngine;

namespace MoreGamemodes
{
    public class RickrollManager
    {
        public static bool ShouldRickrollMode()
        {
            try
		    {
			    if (DestroyableSingleton<EOSManager>.Instance.HasServerTimestamp)
			    {
			    	DateTime approximateServerTime = DestroyableSingleton<EOSManager>.Instance.ApproximateServerTime;
			    	DateTime t = new(approximateServerTime.Year, 4, 1, 7, 0, 0, 0, DateTimeKind.Utc);
			    	DateTime t2 = new(approximateServerTime.Year, 4, 2, 7, 0, 0, 0, DateTimeKind.Utc);
			    	if (approximateServerTime >= t && approximateServerTime <= t2)
			    	{
			    		return true;
			    	}
			    }
			    return false;
		    }
		    catch
		    {
		    }
		    return false;
        }

        public static void OnStart()
        {
            TimeSinceLastTip = 0f;
            var rand = new System.Random();
            TipTime = rand.Next(15, 45);
        }

        public static void OnUpdate()
        {
            if ((AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined || MeetingHud.Instance  || (Options.MidGameChat.GetBool() && !Options.ProximityChat.GetBool())) && AmongUsClient.Instance.AmHost)
            {
                TimeSinceLastTip += Time.fixedDeltaTime;
                if (TimeSinceLastTip > TipTime)
                {
                    TimeSinceLastTip = 0f;
                    ShowTip();
                    var rand = new System.Random();
                    TipTime = rand.Next(60, 90);
                }
            }
        }

        public static void Rickroll()
        {
            Application.OpenURL("https://shattereddisk.github.io/rickroll/rickroll.mp4");
        }

        public static void ShowTip()
        {
            var rand = new System.Random();
            Utils.SendChat(Tips[rand.Next(0, Tips.Count)], "Tip");
        }


        public static float TimeSinceLastTip;
        public static float TipTime;

        public static List<string> RickrollNames = new()
        {
            "Rick Astley", "Rick Sustley", "Sus Astley", "Sus Sustley", "Rickroll", "NEVER GONNA GIVE YOU UP", "Rickus Astley", "Rick Sustleyus",
            "Rickus Astleyus", "Rickstley", "yeltsA kciR", "Sussy Astley", "Rick Susser", "PU UOY EVIG ANNOG REVEN", "Not Astley", "Rick Antisusser",
            "R.A.", "<color=#ff0000>Rick Astley</color>", "<rotate=180>yeltsA kciR</rotate>", "<b><i><u>Rickroll</b></i></u>", "<s>RICK ASTLEYYY</s>",
            "Not rickroll", "Fake Astley", "<color=#00ff00>Real Astley</color>", "<color=#0000ff>Astley playing SUS AMOGUS!</color>"            
        };

        public static List<string> Tips = new()
        {
            "https://blogs.mtdv.me/articles/SRg9ec3T48", "Click ALT + F4 to win!", "Red is always sus", "Never gonna give you up<br>Never gonna" +
            "let you down<br>Never gonna run around and desert you<br>Never gonna make you cry<br>Never gonna say goodbye<br>Never gonna tell a lie and hurt you",
            "This tip is useful!!!", "You will get killed after this meeting.", "Amongus is sus.", "Drink H<sub>2</sub>SO<sub>4</sub> to be healthy.",
            "The earth is flat.", "Moon is made up with cheese.", "If your nick is red tell everyone about it!", "Your dad will come back with milk in: 10 years",
            "Rick Astley rickrolled you!", "Pay 69,69$ to have x10 impostor chance.", "Free bobux for everyone!!!", "Are you little sussy baka imposter?"
        };
    }
}