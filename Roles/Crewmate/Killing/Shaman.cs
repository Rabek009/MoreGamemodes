using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    public class Shaman : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (Target == byte.MaxValue) return;
            var player = Utils.GetPlayerById(Target);
            if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected || !(player.GetRole().IsImpostor() || player.GetRole().IsNeutralKilling() || player.GetRole().Role == CustomRoles.Sheriff))
            {
                Target = byte.MaxValue;
                Player.RpcSetShamanTarget(Target);
            }
            else
                player.Notify(Utils.ColorString(Color, "<size=1.8>You are cursed by shaman! Kill someone to remove curse or you will die when meeting is called!</size>"));
        }

        public override void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (Target != byte.MaxValue && killer.PlayerId == Target)
            {
                Target = byte.MaxValue;
                Player.RpcSetShamanTarget(Target);
            }
        }

        public override bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            return target != null;
        }

        public override void OnCompleteTask()
        {
            if (AbilityUseGainWithEachTaskCompleted.GetFloat() <= 0f) return;
            Player.RpcSetAbilityUses(AbilityUses + AbilityUseGainWithEachTaskCompleted.GetFloat());
        }

        public override void OnAddVote(PlayerControl target)
        {
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected || AbilityUses < 1f || Target != byte.MaxValue || !MeetingHud.Instance || !(MeetingHud.Instance.state is MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Voted)) return;
            Target = target.PlayerId;
            Player.RpcSetShamanTarget(Target);
            Player.RpcSendMessage("You cursed " + Main.StandardNames[Target] + "!", "Shaman");
            Player.RpcSetAbilityUses(AbilityUses - 1f);
        }

        public void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            if (Target == byte.MaxValue) return;
            var player = Utils.GetPlayerById(Target);
            if (player != null && player.Data != null && !player.Data.IsDead && !player.Data.Disconnected && (player.GetRole().IsImpostor() || player.GetRole().IsNeutralKilling() || player.GetRole().Role == CustomRoles.Sheriff))
            {
                player.RpcSetDeathReason(DeathReasons.Cursed);
                player.RpcMurderPlayer(player, true);
                ++Main.PlayerKills[Player.PlayerId];
                ClassicGamemode.instance.PlayerKiller[player.PlayerId] = Player.PlayerId;
            }
            Target = byte.MaxValue;
            Player.RpcSetShamanTarget(Target);
        }

        public override void OnFixedUpdate()
        {
            if (Target == byte.MaxValue) return;
            var player = Utils.GetPlayerById(Target);
            if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == player || pc.Data.IsDead)
                    ClassicGamemode.instance.NameSymbols[(Target, pc.PlayerId)][CustomRoles.Shaman] = ("乂", Color);
            }
        }

        public static void OnGlobalReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Shaman)
                {
                    Shaman shamanRole = pc.GetRole() as Shaman;
                    if (shamanRole == null) continue;
                    shamanRole.OnReportDeadBody(reporter, target);
                }
            }
        }

        // https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Modules/GuessManager.cs#L638
        public static void CreateMeetingButton(MeetingHud __instance)
        {
            foreach (var pva in __instance.playerStates)
            {
                if (pva.transform.FindChild("MeetingButton") != null)
                    Object.Destroy(pva.transform.FindChild("MeetingButton").gameObject);
                var player = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
                Shaman shamanRole = PlayerControl.LocalPlayer.GetRole() as Shaman;
                if (player.IsDead || player.Disconnected || player.ClientId == AmongUsClient.Instance.ClientId || PlayerControl.LocalPlayer.Data.IsDead || shamanRole.AbilityUses < 1f || shamanRole.Target != byte.MaxValue) continue;
                GameObject template = pva.Buttons.transform.Find("CancelButton").gameObject;
                GameObject targetBox = Object.Instantiate(template, pva.transform);
                targetBox.name = "MeetingButton";
                targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.31f);
                SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                renderer.sprite = Utils.LoadSprite("MoreGamemodes.Resources.ShamanIcon.png", 115f);
                PassiveButton button = targetBox.GetComponent<PassiveButton>();
                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener((Action)(() => VoteBanSystem.Instance.CmdAddVote(player.ClientId)));
            }
        }

        public Shaman(PlayerControl player)
        {
            Role = CustomRoles.Shaman;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = InitialAbilityUseLimit.GetFloat();
            Target = byte.MaxValue;
        }

        public byte Target;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem DieAfterUsingAbility;
        public static OptionItem InitialAbilityUseLimit;
        public static OptionItem AbilityUseGainWithEachTaskCompleted;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(200300, CustomRoles.Shaman, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(200301, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            InitialAbilityUseLimit = FloatOptionItem.Create(200302, "Initial ability use limit", new(0f, 15f, 1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(200303, "Ability use gain with each task completed", new(0f, 2f, 0.1f), 0.4f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Shaman] = Chance;
            Options.RolesCount[CustomRoles.Shaman] = Count;
        }
    }
}