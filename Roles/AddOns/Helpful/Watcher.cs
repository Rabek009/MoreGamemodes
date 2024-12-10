using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Watcher : AddOn
    {
        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetBool(BoolOptionNames.AnonymousVotes, false);
            return opt;
        }

        public Watcher(PlayerControl player)
        {
            Type = AddOns.Watcher;
            Player = player;
            Utils.SetupAddOnInfo(this);
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(1100200, "Watcher", new(0, 100, 5), 0, TabGroup.AddOns, false)
                .SetColor(AddOnsHelper.AddOnColors[AddOns.Watcher])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(1100201, "Max", new(1, 15, 1), 1, TabGroup.AddOns, false)
                .SetParent(Chance);
            Options.AddOnsChance[AddOns.Watcher] = Chance;
            Options.AddOnsCount[AddOns.Watcher] = Count;
        }
    }
}