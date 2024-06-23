using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class SpeedrunGamemode : CustomGamemode
    {
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
            {
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
            }
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Ghost";
            else
                __instance.FilterText.text = "Player";
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.taskText.text = Utils.ColorString(Color.yellow, "Speedrunner:\nFinish tasks as fast as you can.\n\n") + str;
        }

        public override List<PlayerControl> OnBeginCrewmatePrefix(IntroCutscene __instance)
        {
            var Team = new List<PlayerControl>();
            Team.Add(PlayerControl.LocalPlayer);
            return Team;
        }

        public override void OnBeginCrewmatePostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Speedrun";
            __instance.TeamTitle.color = Color.yellow;
            __instance.BackgroundBar.material.color = Color.yellow;
            __instance.ImpostorText.text = "";
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            __instance.RoleText.text = "Speedrunner";
            __instance.RoleText.color = Color.yellow;
            __instance.RoleBlurbText.text = "Finish tasks as fast as you can";
            __instance.RoleBlurbText.color = Color.yellow;
            __instance.YouAreText.color = Color.yellow;
        }

        public override bool OnSelectRolesPrefix()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Options.CurrentBodyType == SpeedrunBodyTypes.Engineer)
                    pc.RpcSetRole(RoleTypes.Engineer, true);
                else
                    pc.RpcSetRole(RoleTypes.Crewmate, true);
            }
            return false;
        }

        public override void OnIntroDestroy()
        {
            if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.RpcSetDeathReason(DeathReasons.Command);
                    pc.RpcSetRole(RoleTypes.GuardianAngel, true);
                }       
            }
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return false;
        }

        public SpeedrunGamemode()
        {
            Gamemode = Gamemodes.Speedrun;
            PetAction = false;
            DisableTasks = false;
        }

        public static SpeedrunGamemode instance;
    }
    
    public enum SpeedrunBodyTypes
    {
        Crewmate,
        Engineer,
        Ghost,
        All = int.MaxValue,
    }
}