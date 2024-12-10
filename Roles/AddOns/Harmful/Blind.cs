using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Blind : AddOn
    {
        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            if (Player.GetRole().IsImpostor() || Player.GetRole().IsNeutralKilling() || (Player.GetRole().Role == CustomRoles.Jester && Jester.HasImpostorVision.GetBool()))
            {
                opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.CrewLightMod) * ((100 - ImpostorVisionDecrease.GetInt()) / 100f));
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod) * ((100 - ImpostorVisionDecrease.GetInt()) / 100f));
            }
            else
            {
                opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.CrewLightMod) * ((100 - CrewmateVisionDecrease.GetInt()) / 100f));
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod) * ((100 - CrewmateVisionDecrease.GetInt()) / 100f));
            }
            return opt;
        }

        public Blind(PlayerControl player)
        {
            Type = AddOns.Blind;
            Player = player;
            Utils.SetupAddOnInfo(this);
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CrewmateVisionDecrease;
        public static OptionItem ImpostorVisionDecrease;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(1200200, "Blind", new(0, 100, 5), 0, TabGroup.AddOns, false)
                .SetColor(AddOnsHelper.AddOnColors[AddOns.Blind])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(1200201, "Max", new(1, 15, 1), 1, TabGroup.AddOns, false)
                .SetParent(Chance);
            CrewmateVisionDecrease = IntegerOptionItem.Create(1200202, "Crewmate vision decrease", new(5, 100, 5), 35, TabGroup.AddOns, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Percent);
            ImpostorVisionDecrease = IntegerOptionItem.Create(1200203, "Impostor vision decrease", new(5, 100, 5), 50, TabGroup.AddOns, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Percent);
            Options.AddOnsChance[AddOns.Blind] = Chance;
            Options.AddOnsCount[AddOns.Blind] = Count;
        }
    }
}