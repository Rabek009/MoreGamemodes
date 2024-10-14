using UnityEngine;

namespace MoreGamemodes
{
    public class Immortal : CustomRole
    {
        public override bool OnCheckMurderAsTarget(PlayerControl killer)
        {
            if (ProtectionTime > 0f)
            {
                killer.RpcGuardAndKill(Player);
                Main.KillCooldowns[killer.PlayerId] = Main.OptionKillCooldowns[killer.PlayerId] / 2f;
                ++TimesSaved;
                return false;
            }
            if (RealAbilityUses > 0f)
            {
                killer.RpcGuardAndKill(Player);
                Main.KillCooldowns[killer.PlayerId] = Main.OptionKillCooldowns[killer.PlayerId] / 2f;
                ++TimesSaved;
                RealAbilityUses -= 1f;
                return false;
            }
            return true;
        }

        public override void OnMeeting()
        {
            ProtectionTime = 0f;
            if (AbilityUses != RealAbilityUses)
                Player.RpcSetAbilityUses(RealAbilityUses);
            if (TimesSaved == 0)
                Player.RpcSendMessage("No one tried to kill you during this round.", "Immortal");
            else if (TimesSaved == 1)
                Player.RpcSendMessage("Someone tried to kill you 1 time during this round.", "Immortal");
            else if (TimesSaved >= 2)
                Player.RpcSendMessage("Someone tried to kill you " + TimesSaved + " times during this round.", "Immortal");
            TimesSaved = 0;
        }

        public override void OnFixedUpdate()
        {
            if (ProtectionTime > 0f)
                ProtectionTime -= Time.fixedDeltaTime;
            if (ProtectionTime < 0f)
                ProtectionTime = 0f;
        }

        public override void OnCompleteTask()
        {
            ProtectionTime += ProtectionAfterCompletingTaskDuration.GetFloat();
            if (Player.AllTasksCompleted())
            {
                Player.RpcSetAbilityUses(TimesProtectedAfterCompletingAllTasks.GetInt());
                RealAbilityUses = TimesProtectedAfterCompletingAllTasks.GetInt();
            }
        }

        public override string GetNamePostfix()
        {
            if (ProtectionTime > 0f)
                return Utils.ColorString(Color.cyan, "\n[PROTECTED]");
            return "";
        }

        public override bool CanBeGuessed(PlayerControl guesser)
        {
            return !Player.AllTasksCompleted();
        }

        public Immortal(PlayerControl player)
        {
            Role = CustomRoles.Immortal;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            ColorUtility.TryParseHtmlString("#5e2a10", out Color);
            RoleName = "Immortal";
            RoleDescription = "Complete tasks to get protection";
            RoleDescriptionLong = "Immortal (Crewmate): After completing all tasks you can survive few kill attempts. In addition after you complete task, you get temporarily protection. If impostor try to kill you, his cooldown will reset to 50%. You will know that someone tried to kill you when meeting is called. After completing all tasks you can't be guessed.";
            AbilityUses = -1f;
            RealAbilityUses = -1f;
            ProtectionTime = 0f;
            TimesSaved = 0;
        }

        public float RealAbilityUses;
        public float ProtectionTime;
        public int TimesSaved;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem ProtectionAfterCompletingTaskDuration;
        public static OptionItem TimesProtectedAfterCompletingAllTasks;
        public static void SetupOptionItem()
        {
            ColorUtility.TryParseHtmlString("#5e2a10", out Color c);
            Chance = IntegerOptionItem.Create(300100, "Immortal", new(0, 100, 5), 0, TabGroup.CrewmateRoles, false)
                .SetColor(c)
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(300101, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            ProtectionAfterCompletingTaskDuration = FloatOptionItem.Create(300102, "Protection after completing task duration", new(0f, 30f, 1f), 10f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            TimesProtectedAfterCompletingAllTasks = IntegerOptionItem.Create(300103, "Times protected after completing all tasks", new(1, 25, 1), 3, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Immortal] = Chance;
            Options.RolesCount[CustomRoles.Immortal] = Count;
        }
    }
}