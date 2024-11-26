using System;
using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    public class Bait : AddOn
    {
        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (killer == Player) return;
            if (killer.HasAddOn(AddOns.Oblivious) && !Oblivious.ReportAfterKillingBait.GetBool()) return;
            if (WarnKillerAboutSelfReport.GetBool())
            {
                killer.RpcReactorFlash(0.3f, Color);
                killer.Notify(Utils.ColorString(Color, "YOU KILLED BAIT"));
            }
            new LateTask(() => {
                bool bodyExists = false;
                foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
                {
                    if (deadBody.ParentId == Player.PlayerId)
                    {
                        bodyExists = true;
                        break;
                    }
                }
                if (!MeetingHud.Instance && killer != null && killer.Data != null && !killer.Data.IsDead && !killer.Data.Disconnected && Player != null && Player.Data != null && bodyExists)
                    killer.ForceReportDeadBody(Player.Data);
            }, Math.Max(0.15f, ReportDelay.GetFloat()), "Bait self report");
        }

        public Bait(PlayerControl player)
        {
            Type = AddOns.Bait;
            Player = player;
            Utils.SetupAddOnInfo(this);
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem ReportDelay;
        public static OptionItem WarnKillerAboutSelfReport;
        public static OptionItem NeutralsCanBecomeBait;
        public static OptionItem ImpostorsCanBecomeBait;
        public static OptionItem CanBeGuessed;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(1100100, "Bait", new(0, 100, 5), 0, TabGroup.AddOns, false)
                .SetColor(AddOnsHelper.AddOnColors[AddOns.Bait])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(1100101, "Max", new(1, 15, 1), 1, TabGroup.AddOns, false)
                .SetParent(Chance);
            ReportDelay = FloatOptionItem.Create(1100102, "Report delay", new(0f, 10f, 0.5f), 0f, TabGroup.AddOns, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            WarnKillerAboutSelfReport = BooleanOptionItem.Create(1100103, "Warn killer about self report", false, TabGroup.AddOns, false)
                .SetParent(Chance);
            NeutralsCanBecomeBait = BooleanOptionItem.Create(1100104, "Neutrals can become bait", true, TabGroup.AddOns, false)
                .SetParent(Chance);
            ImpostorsCanBecomeBait = BooleanOptionItem.Create(1100105, "Impostors can become bait", true, TabGroup.AddOns, false)
                .SetParent(Chance);
            CanBeGuessed = BooleanOptionItem.Create(1100106, "Can be guessed", true, TabGroup.AddOns, false)
                .SetParent(Chance);
            Options.AddOnsChance[AddOns.Bait] = Chance;
            Options.AddOnsCount[AddOns.Bait] = Count;
        }
    }
}