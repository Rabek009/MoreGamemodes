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
                return false;
            }
            if (RealAbilityUses > 0f)
            {
                killer.RpcGuardAndKill(Player);
                Main.KillCooldowns[killer.PlayerId] = Main.OptionKillCooldowns[killer.PlayerId] / 2f;
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
                return Utils.ColorString(Color.cyan, "\n<size=1.8>[PROTECTED]</size>");
            return "";
        }

        public Immortal(PlayerControl player)
        {
            Role = CustomRoles.Immortal;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            RealAbilityUses = -1f;
            ProtectionTime = 0f;
        }

        public float RealAbilityUses;
        public float ProtectionTime;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem ProtectionAfterCompletingTaskDuration;
        public static OptionItem TimesProtectedAfterCompletingAllTasks;
        public static OptionItem CanBeGuessed;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(300100, "Immortal", new(0, 100, 5), 0, TabGroup.CrewmateRoles, false)
                .SetColor(CustomRolesHelper.RoleColors[CustomRoles.Immortal])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(300101, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            ProtectionAfterCompletingTaskDuration = FloatOptionItem.Create(300102, "Protection after completing task duration", new(0f, 30f, 1f), 10f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            TimesProtectedAfterCompletingAllTasks = IntegerOptionItem.Create(300103, "Times protected after completing all tasks", new(1, 25, 1), 3, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanBeGuessed = BooleanOptionItem.Create(300104, "Can be guessed", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Immortal] = Chance;
            Options.RolesCount[CustomRoles.Immortal] = Count;
        }
    }
}