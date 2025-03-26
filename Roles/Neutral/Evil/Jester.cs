using AmongUs.GameOptions;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class Jester : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (exiled != null && exiled.Object != null && exiled.Object == Player && GameManager.Instance.ShouldCheckForGameEnd && !Options.NoGameEnd.GetBool())
            {
                List<byte> winners = new();
                winners.Add(Player.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.CrewmatesByVote, winners, CustomWinners.Jester);
            }
        }

        public override bool OnCastVote(MeetingHud __instance, byte suspectPlayerId)
        {
            if (suspectPlayerId == Player.PlayerId && !CanVoteForHimself.GetBool())
            {
                Player.RpcSendMessage("You can't vote for yourself!", "Jester");
                return false;
            }
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            if (HasImpostorVision.GetBool())
            {
                if (Utils.IsActive(SystemTypes.Electrical))
                    opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod) * 5f);
                else
                    opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod));
            }
            opt.SetFloat(FloatOptionNames.EngineerCooldown, 0.001f);
            opt.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 0f);
            return opt;
        }

        public Jester(PlayerControl player)
        {
            Role = CustomRoles.Jester;
            BaseRole = CanUseVents.GetBool() ? BaseRoles.Engineer : BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
        }

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CanUseVents;
        public static OptionItem HasImpostorVision;
        public static OptionItem CanVoteForHimself;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(900100, CustomRoles.Jester, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(900101, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            CanUseVents = BooleanOptionItem.Create(900102, "Can use vents", false, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            HasImpostorVision = BooleanOptionItem.Create(900103, "Has impostor vision", false, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            CanVoteForHimself = BooleanOptionItem.Create(900104, "Can vote for himself", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Jester] = Chance;
            Options.RolesCount[CustomRoles.Jester] = Count;
        }
    }
}