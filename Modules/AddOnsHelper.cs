using System.Collections.Generic;
using UnityEngine;

namespace MoreGamemodes
{
    static class AddOnsHelper
    {
        public static Dictionary<string, AddOns> CommandAddOnNames = new() 
        {
            {"bait", AddOns.Bait},
            {"oblivious", AddOns.Oblivious},
            {"lurker", AddOns.Lurker},
        };

        public static Dictionary<AddOns, string> AddOnNames = new() 
        {
            {AddOns.Bait, "Bait"},
            {AddOns.Oblivious, "Oblivious"},
            {AddOns.Lurker, "Lurker"},
        };

        public static Dictionary<AddOns, string> AddOnDescriptions = new() 
        {
            {AddOns.Bait, "Force killer to self report"},
            {AddOns.Oblivious, "Can't report dead bodies"},
            {AddOns.Lurker, "Your cooldown decreases in vent"},
        };

        public static Dictionary<AddOns, string> AddOnDescriptionsLong = new() 
        {
            {AddOns.Bait, "Bait (Add on): When you're killed, your killer instantly self report. Depending on options there might be report delay."},
            {AddOns.Oblivious, "Oblivious (Add on): You can't report dead bodies. Depending on options you also avoid bait self report. Mortician can't be oblivious."},
            {AddOns.Lurker, "Lurker (Add on): Your kill cooldown continues to go down when you're in a vent. Only impostors can get this add on."},
        };

        public static Dictionary<AddOns, Color> AddOnColors = new() 
        {
            {AddOns.Bait, Utils.HexToColor("#1dd7de")},
            {AddOns.Oblivious, Utils.HexToColor("#555e63")},
            {AddOns.Lurker, Palette.ImpostorRed},
        };

        public static void SetAddOn(this PlayerControl player, AddOns addOn)
        {
            if (ClassicGamemode.instance == null) return;
            if (player.HasAddOn(addOn)) return;
            switch (addOn)
            {
                case AddOns.Bait:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Bait(player));
                    break;
                case AddOns.Oblivious:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Oblivious(player));
                    break;
                case AddOns.Lurker:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Lurker(player));
                    break;
            }
        }

        public static bool CrewmatesCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return false;
            return addOn switch
            {
                _ => true,
            };
        }

        public static bool NeutralsCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return false;
            return addOn switch
            {
                AddOns.Bait => Bait.NeutralsCanBecomeBait.GetBool(),
                _ => true,
            };
        }

        public static bool ImpostorsCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return true;
            return addOn switch
            {
                AddOns.Bait => Bait.ImpostorsCanBecomeBait.GetBool(),
                _ => true,
            };
        }

        public static bool IsImpostorOnly(AddOns addOn)
        {
            return addOn is AddOns.Lurker;
        }

        public static int GetAddOnChance(AddOns addOn)
        {
            return Options.AddOnsChance.ContainsKey(addOn) ? Options.AddOnsChance[addOn].GetInt() : 0;
        }

        public static int GetAddOnCount(AddOns addOn)
        {
            return Options.AddOnsCount.ContainsKey(addOn) ? Options.AddOnsCount[addOn].GetInt() : 0;
        }
    }
}

public enum AddOns
{
    Bait,
    Oblivious,
    Lurker,
}