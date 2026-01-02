using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    static class AddOnsHelper
    {
        public static Dictionary<string, AddOns> CommandAddOnNames = new() 
        {
            {"诱饵", AddOns.Bait},
            {"watcher", AddOns.Watcher},
            {"radar", AddOns.Radar},
            {"guesser", AddOns.Guesser},
            {"oblivious", AddOns.Oblivious},
            {"blind", AddOns.Blind},
            {"lurker", AddOns.Lurker},
        };

        public static Dictionary<AddOns, string> AddOnNames = new() 
        {
            {AddOns.Bait, "诱饵"},
            {AddOns.Watcher, "Watcher"},
            {AddOns.Radar, "Radar"},
            {AddOns.Guesser, "Guesser"},
            {AddOns.Oblivious, "Oblivious"},
            {AddOns.Blind, "Blind"},
            {AddOns.Lurker, "Lurker"},
        };

        public static Dictionary<AddOns, string> AddOnDescriptions = new() 
        {
            {AddOns.Bait, "Force killer to self report"},
            {AddOns.Watcher, "See everyone's votes"},
            {AddOns.Radar, "Locate closest player"},
            {AddOns.Guesser, "Can guess roles"},
            {AddOns.Oblivious, "Can't report dead bodies"},
            {AddOns.Blind, "Your vision is lower"},
            {AddOns.Lurker, "Your cooldown decreases in vent"},
        };

        public static Dictionary<AddOns, string> AddOnDescriptionsLong = new() 
        {
            {AddOns.Bait, "Bait (Add on): When you're killed, your killer instantly self report. Depending on options there might be report delay."},
            {AddOns.Watcher, "Watcher (Add on): You see who votes for who in meeting, like with anonymous votes turned off."},
            {AddOns.Radar, "Radar (Add on): You see arrow to nearest player. That arrow is always updated."},
            {AddOns.Guesser, "Guesser (Add on): You can guess roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 10 is jester, you should type <i>/guess 10 jester</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead."},
            {AddOns.Oblivious, "Oblivious (Add on): You can't report dead bodies. Depending on options you also avoid bait self report. Mortician can't be oblivious."},
            {AddOns.Blind, "Blind (Add on): Your vision is decreased."},
            {AddOns.Lurker, "Lurker (Add on): Your kill cooldown continues to go down when you're in a vent. Only impostors can get this add on."},
        };

        public static Dictionary<AddOns, Color> AddOnColors = new() 
        {
            {AddOns.Bait, Utils.HexToColor("#1dd7de")},
            {AddOns.Watcher, Utils.HexToColor("#521166")},
            {AddOns.Radar, Utils.HexToColor("#13ed07")},
            {AddOns.Guesser, Utils.HexToColor("#ded74e")},
            {AddOns.Oblivious, Utils.HexToColor("#555e63")},
            {AddOns.Blind, Utils.HexToColor("#141414")},
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
                case AddOns.Watcher:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Watcher(player));
                    break;
                case AddOns.Radar:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Radar(player));
                    break;
                case AddOns.Guesser:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Guesser(player));
                    break;
                case AddOns.Oblivious:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Oblivious(player));
                    break;
                case AddOns.Blind:
                    ClassicGamemode.instance.AllPlayersAddOns[player.PlayerId].Add(new Blind(player));
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
                AddOns.Guesser => Guesser.CrewmatesCanBecomeGuesser.GetBool() && (!Options.EnableGuesserMode.GetBool() || !Options.CrewmatesCanGuess.GetBool()),
                _ => true,
            };
        }

        public static bool BenignNeutralsCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return false;
            return addOn switch
            {
                AddOns.Bait => Bait.BenignNeutralsCanBecomeBait.GetBool(),
                AddOns.Guesser => Guesser.BenignNeutralsCanBecomeGuesser.GetBool() && (!Options.EnableGuesserMode.GetBool() || !Options.NeutralBenignCanGuess.GetBool()),
                _ => true,
            };
        }

        public static bool EvilNeutralsCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return false;
            return addOn switch
            {
                AddOns.Bait => Bait.EvilNeutralsCanBecomeBait.GetBool(),
                AddOns.Guesser => Guesser.EvilNeutralsCanBecomeGuesser.GetBool() && (!Options.EnableGuesserMode.GetBool() || !Options.NeutralEvilCanGuess.GetBool()),
                _ => true,
            };
        }

        public static bool KillingNeutralsCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return false;
            return addOn switch
            {
                AddOns.Bait => Bait.KillingNeutralsCanBecomeBait.GetBool(),
                AddOns.Guesser => Guesser.KillingNeutralsCanBecomeGuesser.GetBool() && (!Options.EnableGuesserMode.GetBool() || !Options.NeutralKillingCanGuess.GetBool()),
                _ => true,
            };
        }

        public static bool ImpostorsCanGet(AddOns addOn)
        {
            if (IsImpostorOnly(addOn)) return true;
            return addOn switch
            {
                AddOns.Bait => Bait.ImpostorsCanBecomeBait.GetBool(),
                AddOns.Guesser => Guesser.ImpostorsCanBecomeGuesser.GetBool() && (!Options.EnableGuesserMode.GetBool() || !Options.ImpostorsCanGuess.GetBool()),
                _ => true,
            };
        }

        public static bool IsImpostorOnly(AddOns addOn)
        {
            return addOn is AddOns.Lurker;
        }

        public static int GetAddOnChance(AddOns addOn)
        {
            if (addOn == AddOns.Watcher && (Main.RealOptions != null ? !Main.RealOptions.GetBool(BoolOptionNames.AnonymousVotes) : !GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.AnonymousVotes))) return 0;
            return Options.AddOnsChance.ContainsKey(addOn) ? Options.AddOnsChance[addOn].GetInt() : 0;
        }

        public static int GetAddOnCount(AddOns addOn)
        {
            if (addOn == AddOns.Watcher && (Main.RealOptions != null ? !Main.RealOptions.GetBool(BoolOptionNames.AnonymousVotes) : !GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.AnonymousVotes))) return 0;
            return Options.AddOnsCount.ContainsKey(addOn) ? Options.AddOnsCount[addOn].GetInt() : 0;
        }
    }
}

public enum AddOns
{
    Bait,
    Watcher,
    Radar,
    Guesser,
    Oblivious,
    Blind,
    Lurker,
}