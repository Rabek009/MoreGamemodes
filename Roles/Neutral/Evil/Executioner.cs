using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MoreGamemodes
{
    public class Executioner : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (exiled != null && exiled.PlayerId == Target)
            {
                List<byte> winners = new();
                winners.Add(Player.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.HumansByVote, winners, CustomWinners.Executioner);
            }
        }

        public override bool OnCastVote(MeetingHud __instance, byte suspectPlayerId)
        {
            if (suspectPlayerId == Target && !CanVoteForTarget.GetBool())
            {
                Player.RpcSendMessage("You can't vote for your target!", "Executioner");
                return false;
            }
            return true;
        }

        public override void OnIntroDestroy()
        {
            List<PlayerControl> PotentialTargets = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data != null && !pc.Data.IsDead && !pc.Data.Disconnected && pc.GetRole().IsCrewmate())
                    PotentialTargets.Add(pc);
            }
            if (PotentialTargets.Any())
            {
                var rand = new System.Random();
                Target = PotentialTargets[rand.Next(0, PotentialTargets.Count)].PlayerId;
                Main.NameColors[(Target, Player.PlayerId)] = Color.black;
            }
        }

        public override void OnFixedUpdate()
        {
            PlayerControl targetPlayer = Utils.GetPlayerById(Target);
            if (Target == byte.MaxValue || targetPlayer == null || targetPlayer.Data == null || targetPlayer.Data.IsDead || targetPlayer.Data.Disconnected || !targetPlayer.GetRole().IsCrewmate())
            {
                switch (CurrentRoleAfterTargetDeath)
                {
                    case RolesAfterTargetDeath.Amnesiac:
                        ClassicGamemode.instance.ChangeRole(Player, CustomRoles.Amnesiac);
                        break;
                    case RolesAfterTargetDeath.Opportunist:
                        ClassicGamemode.instance.ChangeRole(Player, CustomRoles.Opportunist);
                        break;
                    case RolesAfterTargetDeath.Jester:
                        ClassicGamemode.instance.ChangeRole(Player, CustomRoles.Jester);
                        break;
                    case RolesAfterTargetDeath.Crewmate:
                        ClassicGamemode.instance.ChangeRole(Player, CustomRoles.Crewmate);
                        break;
                }
            }
        }

        public Executioner(PlayerControl player)
        {
            Role = CustomRoles.Executioner;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Target = byte.MaxValue;
        }
        public byte Target;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem RoleAfterTargetDeath;
        public static readonly string[] rolesAfterTargetDeath =
        {
            "Amnesiac", "Opportunist", "Jester", "Crewmate"
        };
        public static RolesAfterTargetDeath CurrentRoleAfterTargetDeath => (RolesAfterTargetDeath)RoleAfterTargetDeath.GetValue();
        public static OptionItem CanVoteForTarget;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(900200, CustomRoles.Executioner, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(900201, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            RoleAfterTargetDeath = StringOptionItem.Create(900202, "Role after target death", rolesAfterTargetDeath, 0, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            CanVoteForTarget = BooleanOptionItem.Create(900203, "Can vote for target", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Executioner] = Chance;
            Options.RolesCount[CustomRoles.Executioner] = Count;
        }
    }

    public enum RolesAfterTargetDeath
    {
        Amnesiac,
        Opportunist,
        Jester,
        Crewmate,
    }
}