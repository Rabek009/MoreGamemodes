namespace MoreGamemodes
{
    public class Radar : AddOn
    {
        public override string GetNamePostfix()
        {
            var target = Player.GetClosestPlayer();
            if (target != null)
                return Utils.ColorString(AddOnsHelper.AddOnColors[AddOns.Radar], "\n" + Utils.GetArrow(Player.GetRealPosition(), target.transform.position));
            return "";
        }

        public Radar(PlayerControl player)
        {
            Type = AddOns.Radar;
            Player = player;
            Utils.SetupAddOnInfo(this);
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static void SetupOptionItem()
        {
            Chance = AddOnOptionItem.Create(1100300, AddOns.Radar, TabGroup.AddOns, false);
            Count = IntegerOptionItem.Create(1100301, "Max", new(1, 15, 1), 1, TabGroup.AddOns, false)
                .SetParent(Chance);
            Options.AddOnsChance[AddOns.Radar] = Chance;
            Options.AddOnsCount[AddOns.Radar] = Count;
        }
    }
}