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
            __instance.ReportButton.OverrideText("Report");
            __instance.KillButton.OverrideText("Kill");
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

        public virtual bool OnCheckMurderLate(PlayerControl target)
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

        public virtual void OnAppear()
        {

        }

        public virtual void OnAddVote(PlayerControl target)
        {
            
        }

        public virtual bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            return true;
        }

        public virtual bool OnClimbLadder(Ladder source, bool ladderUsed)
        {
            return true;
        }

        public virtual bool OnUsePlatform()
        {
            return true;
        }

        public virtual bool OnCheckUseZipline(ZiplineBehaviour ziplineBehaviour, bool fromTop)
        {
            return true;
        }

        public virtual bool OnCheckSporeTrigger(Mushroom mushroom)
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
            return CustomRoleType is CustomRoleTypes.CrewmateVanilla or
            CustomRoleTypes.CrewmateInvestigative or
            CustomRoleTypes.CrewmateKilling or
            CustomRoleTypes.CrewmateProtective or
            CustomRoleTypes.CrewmateSupport;
        }

        public bool IsImpostor()
        {
            return CustomRoleType is CustomRoleTypes.ImpostorVanilla or
            CustomRoleTypes.ImpostorConcealing or
            CustomRoleTypes.ImpostorKilling or
            CustomRoleTypes.ImpostorSupport;
        }

        public bool IsNeutral()
        {
            return IsNeutralBenign() || IsNeutralEvil() || IsNeutralKilling();
        }

        public bool IsNeutralBenign()
        {
            return CustomRoleType == CustomRoleTypes.NeutralBenign;
        }

        public bool IsNeutralEvil()
        {
            return CustomRoleType == CustomRoleTypes.NeutralEvil;
        }

        public bool IsNeutralKilling()
        {
            return CustomRoleType == CustomRoleTypes.NeutralKilling;
        }

        public bool IsCrewmateKilling()
        {
            return CustomRoleType == CustomRoleTypes.CrewmateKilling;
        }

        public bool HasTasks()
        {
            return CustomRolesHelper.HasTasks(Role);
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
            return IsNeutralKilling() ||
            Role is CustomRoles.Investigator or
            CustomRoles.Sheriff or
            CustomRoles.Medic;
        }

        public virtual string GetProgressText(bool gameEnded)
        {
            string text = "";
            if (AbilityUses > -1)
                text += " (" + AbilityUses + ")";
            if (HasTasks())
            {
                int totalTasks = ClassicGamemode.instance.DefaultTasks[Player.PlayerId].Count;
                int completedTasks = ClassicGamemode.instance.CompletedTasks[Player.PlayerId].Count;
                if (Utils.IsActive(SystemTypes.Comms) && !gameEnded)
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

        public virtual bool SeePlayerRole(PlayerControl player)
        {
            return false;
        }

        public virtual bool IsRoleRevealed(PlayerControl seer)
        {
            return false;
        }

        public virtual bool CanGuess(PlayerControl target, CustomRoles role)
        {
            return false;
        }

        public virtual bool CanGuess(PlayerControl target, AddOns addOn)
        {
            return false;
        }

        public virtual bool CanGetGuessed(PlayerControl guesser, CustomRoles? role)
        {
            return true;
        }

        public virtual bool CheckEndCriteria()
        {
            return false;
        }

        public virtual bool IsCompatible(AddOns addOn)
        {
            return true;
        }

        public virtual bool IsCounted()
        {
            return true;
        }

        public virtual bool PreventGameEnd()
        {
            return false;
        }

        public virtual void OnRevive()
        {

        }

        public virtual void ReceiveRPC(MessageReader reader)
        {

        }

        public CustomRoles Role;
        public BaseRoles BaseRole;
        public PlayerControl Player;
        public Color Color;
        public CustomRoleTypes CustomRoleType;
        public string RoleName;
        public string RoleDescription;
        public string RoleDescriptionLong; 
        public float AbilityUses;
    }
}