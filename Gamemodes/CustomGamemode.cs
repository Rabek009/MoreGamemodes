using Il2CppSystem.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class CustomGamemode
    {
        public virtual void OnExile(NetworkedPlayerInfo exiled)
        {

        }

        public virtual void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Scientist)
                __instance.FilterText.text = "Scientist";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Engineer)
                __instance.FilterText.text = "Engineer";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.CrewmateGhost)
                __instance.FilterText.text = "Crewmate Ghost";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.GuardianAngel)
                __instance.FilterText.text = "Guardian Angel";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Shapeshifter)
                __instance.FilterText.text = "Shapeshifter";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.ImpostorGhost)
                __instance.FilterText.text = "Impostor Ghost";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Noisemaker)
                __instance.FilterText.text = "Noisemaker";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Phantom)
                __instance.FilterText.text = "Phantom";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Tracker)
                __instance.FilterText.text = "Tracker";
        }

        public virtual void OnHudUpate(HudManager __instance)
        {
            
        }

        public virtual void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            
        }

        public virtual void OnShowNormalMap(MapBehaviour __instance)
        {
            
        }

        public virtual void OnShowSabotageMap(MapBehaviour __instance)
        {
            
        }

        public virtual void OnToggleHighlight(PlayerControl __instance)
        {
            
        }

        public virtual void OnSetOutline(Vent __instance, bool mainTarget)
        {
            
        }

        public virtual List<PlayerControl> OnBeginCrewmatePrefix(IntroCutscene __instance)
        {
            var Team = new List<PlayerControl>();
            Team.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc != PlayerControl.LocalPlayer)
                    Team.Add(pc);
            }
            return Team;
        }

        public virtual void OnBeginCrewmatePostfix(IntroCutscene __instance)
        {
            
        }

        public virtual List<PlayerControl> OnBeginImpostorPrefix(IntroCutscene __instance)
        {
            var Team = new List<PlayerControl>();
            Team.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor && pc != PlayerControl.LocalPlayer)
                    Team.Add(pc);
            }
            return Team;
        }

        public virtual void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            
        }

        public virtual void OnShowRole(IntroCutscene __instance)
        {
            
        }

        public virtual void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            
        }

        public virtual bool OnCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            return true;
        }

        public virtual bool OnSelectRolesPrefix()
        {
            return true;
        }

        public virtual void OnSelectRolesPostfix()
        {

        }

        public virtual void OnIntroDestroy()
        {
            
        }

        public virtual void OnPet(PlayerControl pc)
        {

        }

        public virtual bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            return true;
        } 

        public virtual bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            return true;
        }

        public virtual void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {

        }

        public virtual bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            return true;
        }

        public virtual void OnShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {

        }

        public virtual bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return true;
        }

        public virtual void OnFixedUpdate()
        {

        }

        public virtual bool OnEnterVent(PlayerControl player, int id)
        {
            return (player.Data.Role.Role == RoleTypes.Engineer || player.Data.Role.IsImpostor) && GameManager.Instance.LogicOptions.MapId != 3;
        }

        public virtual void OnCompleteTask(PlayerControl __instance)
        {

        }

        public virtual bool OnCheckVanish(PlayerControl phantom)
        {
            return true;
        }

        public virtual bool OnCloseDoors(ShipStatus __instance)
        {
            return true;
        }

        public virtual bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            return true;
        }

        public virtual void OnDisconnect(PlayerControl player)
        {
            
        }

        public virtual IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            return opt;
        }

        public virtual string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            return name;
        }

        public static CustomGamemode Instance;
        public Gamemodes Gamemode;
        public bool PetAction;
        public bool DisableTasks;
    }
}