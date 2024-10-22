using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

namespace MoreGamemodes
{
    public class CustomRole
    {
        public virtual void OnExile(NetworkedPlayerInfo exiled)
        {

        }

        public virtual void OnHudUpate(HudManager __instance)
        {
            __instance.PetButton.SetDisabled();
            __instance.PetButton.ToggleVisible(false);
        }

        public virtual void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            
        }

        public virtual bool OnCastVote(MeetingHud __instance, byte suspectPlayerId)
        {
            return true;
        }

        public virtual void OnIntroDestroy()
        {
            
        }

        public virtual void OnPet()
        {

        }

        public virtual bool OnCheckProtect(PlayerControl target)
        {
            return true;
        }

        public virtual bool OnCheckMurder(PlayerControl target)
        {
            return true;
        }

        public virtual bool OnCheckMurderAsTarget(PlayerControl killer)
        {
            return true;
        }

        public virtual void OnMurderPlayer(PlayerControl target)
        {

        }

        public virtual void OnMurderPlayerAsTarget(PlayerControl killer)
        {

        }

        public virtual void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {

        }

        public virtual bool OnCheckShapeshift(PlayerControl target)
        {
            return true;
        }

        public virtual void OnShapeshift(PlayerControl target)
        {

        }

        public virtual bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            return true;
        }

        public virtual void OnMeeting()
        {

        }

        public virtual void OnFixedUpdate()
        {

        }

        public virtual bool OnEnterVent(int id)
        {
            return BaseRole is BaseRoles.Engineer or BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom or BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter  or BaseRoles.DesyncPhantom;
        }

        public virtual void OnCompleteTask()
        {

        }

        public virtual bool OnCheckVanish()
        {
            return BaseRole is BaseRoles.Phantom or
            BaseRoles.DesyncPhantom;
        }

        public virtual bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            return true;
        }

        public virtual IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            return opt;
        }

        public int GetChance()
        {
            return CustomRolesHelper.GetRoleChance(Role);
        }

        public int GetCount()
        {
            return CustomRolesHelper.GetRoleCount(Role);
        }

        public bool IsCrewmate()
        {
            return CustomRolesHelper.IsCrewmate(Role);
        }

        public bool IsImpostor()
        {
            return CustomRolesHelper.IsImpostor(Role);
        }

        public bool IsNeutral()
        {
            return CustomRolesHelper.IsNeutral(Role);
        }

        public bool IsNeutralBenign()
        {
            return CustomRolesHelper.IsNeutralBenign(Role);
        }

        public bool IsNeutralEvil()
        {
            return CustomRolesHelper.IsNeutralEvil(Role);
        }

        public bool IsNeutralKilling()
        {
            return CustomRolesHelper.IsNeutralKilling(Role);
        }

        public bool IsCrewmateKilling()
        {
            return CustomRolesHelper.IsCrewmateKilling(Role);
        }

        public bool CanUseProtectButton()
        {
            return false;
        }

        public bool CanUseKillButton()
        {
            return BaseRole is BaseRoles.Impostor or
            BaseRoles.Shapeshifter or
            BaseRoles.Phantom or
            BaseRoles.DesyncImpostor or
            BaseRoles.DesyncShapeshifter or
            BaseRoles.DesyncPhantom;
        }

        public bool CanUseShiftButton()
        {
            return BaseRole is BaseRoles.Shapeshifter or
            BaseRoles.DesyncShapeshifter;
        }

        public bool CanUseVentButton()
        {
            return CanUseKillButton() || BaseRole is BaseRoles.Engineer;
        }

        public bool ForceKillButton()
        {
            return Role is CustomRoles.Sheriff or
            CustomRoles.Investigator;
        }

        public virtual string GetProgressText()
        {
            string text = "";
            if (AbilityUses > -1)
                text += " (" + AbilityUses + ")";
            if (IsCrewmate())
            {
                int totalTasks = 0;
                int completedTasks = 0;
                foreach (var task in Player.Data.Tasks)
                {
                    ++totalTasks;
                    if (task.Complete)
                        ++completedTasks;
                }
                if (Utils.IsActive(SystemTypes.Comms))
                    text += " (?/" + totalTasks + ")";
                else
                    text += " (" + completedTasks + "/" + totalTasks + ")";
            }
            return text;
        }

        public virtual string GetNamePostfix()
        {
            return "";
        }

        public virtual bool CanGuess(PlayerControl target, CustomRoles role)
        {
            return false;
        }

        public virtual bool CanBeGuessed(PlayerControl guesser, CustomRoles role)
        {
            return true;
        }

        public virtual bool CheckEndCriteria()
        {
            return false;
        }

        public CustomRoles Role;
        public BaseRoles BaseRole;
        public PlayerControl Player;
        public Color Color;
        public string RoleName;
        public string RoleDescription;
        public string RoleDescriptionLong; 
        public float AbilityUses;
    }
}