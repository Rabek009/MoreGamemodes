namespace MoreGamemodes
{
    public class Oblivious : AddOn
    {
        public override void OnHudUpdate(HudManager __instance)
        {
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
        }

        public override bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            return target == null;
        }

        public Oblivious(PlayerControl player)
        {
            Type = AddOns.Oblivious;
            Player = player;
            Utils.SetupAddOnInfo(this);
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem ReportAfterKillingBait;
        public static void SetupOptionItem()
        {
            Chance = AddOnOptionItem.Create(1200100, AddOns.Oblivious, TabGroup.AddOns, false);
            Count = IntegerOptionItem.Create(1200101, "Max", new(1, 15, 1), 1, TabGroup.AddOns, false)
                .SetParent(Chance);
            ReportAfterKillingBait = BooleanOptionItem.Create(1200102, "Report after killing bait", true, TabGroup.AddOns, false)
                .SetParent(Chance);
            Options.AddOnsChance[AddOns.Oblivious] = Chance;
            Options.AddOnsCount[AddOns.Oblivious] = Count;
        }
    }
}