using System;
using UnityEngine;
using Hazel;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    public class Judge : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (DieAfterUsingAbility.GetBool() && AbilityUsed && !Player.Data.IsDead && !Player.Data.Disconnected)
            {
                Player.RpcSetDeathReason(DeathReasons.Suicide);
                Player.RpcExileV2();
            }
        }

        public override void OnAddVote(PlayerControl target)
        {
            if (target == null || target.Data.IsDead || AbilityUsed || !MeetingHud.Instance || !(MeetingHud.Instance.state is MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Voted)) return;
            MeetingHud.Instance.RpcVotingComplete(new MeetingHud.VoterState[]{ new ()
            {
                VoterId = Player.PlayerId,
                VotedForId = target.PlayerId
            }}, target.Data, false);
            SendRPC();
        }

        public void SendRPC()
        {
            AmongUsClient.Instance.SendRpc(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable);
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            AbilityUsed = true;
        }

        // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Modules/GuessManager.cs#L638
        public static void CreateMeetingButton(MeetingHud __instance)
        {
            foreach (var pva in __instance.playerStates)
            {
                if (pva.transform.FindChild("MeetingButton") != null)
                    Object.Destroy(pva.transform.FindChild("MeetingButton").gameObject);
                var player = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
                var judgeRole = PlayerControl.LocalPlayer.GetRole() as Judge;
                if (player.IsDead || player.Disconnected || player.ClientId == AmongUsClient.Instance.ClientId || PlayerControl.LocalPlayer.Data.IsDead || judgeRole.AbilityUsed) continue;
                GameObject template = pva.Buttons.transform.Find("CancelButton").gameObject;
                GameObject targetBox = Object.Instantiate(template, pva.transform);
                targetBox.name = "MeetingButton";
                targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.31f);
                SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                renderer.sprite = Utils.LoadSprite("MoreGamemodes.Resources.JudgeIcon.png", 115f);
                PassiveButton button = targetBox.GetComponent<PassiveButton>();
                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener((Action)(() => VoteBanSystem.Instance.CmdAddVote(player.ClientId)));
            }
        }

        public Judge(PlayerControl player)
        {
            Role = CustomRoles.Judge;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityUsed = false;
        }

        public bool AbilityUsed;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem DieAfterUsingAbility;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(400300, CustomRoles.Judge, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(400301, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            DieAfterUsingAbility = BooleanOptionItem.Create(400302, "Die after using ability", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Judge] = Chance;
            Options.RolesCount[CustomRoles.Judge] = Count;
        }
    }
}