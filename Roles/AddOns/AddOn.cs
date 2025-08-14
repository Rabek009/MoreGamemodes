using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

namespace MoreGamemodes
{
    public class AddOn
    {
        public virtual void OnExile(NetworkedPlayerInfo exiled)
        {

        }

        public virtual void OnHudUpdate(HudManager __instance)
        {

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
            return true;
        }

        public virtual void OnCompleteTask()
        {

        }

        public virtual bool OnCheckVanish()
        {
            return true;
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

        public virtual string GetNamePostfix()
        {
            return "";
        }

        public virtual bool IsCompatible(AddOns addOn)
        {
            return true;
        }

        public virtual void OnRevive()
        {

        }

        public virtual int AdditionalVotes()
        {
            return 0;
        }

        public virtual int AdditionalVisualVotes()
        {
            return 0;
        }

        public AddOns Type;
        public PlayerControl Player;
        public Color Color;
        public string AddOnName;
        public string AddOnDescription;
        public string AddOnDescriptionLong; 
    }
}