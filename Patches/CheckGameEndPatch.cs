using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    class CheckEndCriteriaNormalPatch
    {
        public static bool Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (!GameData.Instance) return false;
            if (TutorialManager.InstanceExists) return true;
            if (Options.NoGameEnd.GetBool()) return false;

            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().PreventGameEnd() && !pc.Data.IsDead) return false;
                }
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForCrewmateWinClassic()) return false;
                if (CheckAndEndGameForImpostorWinClassic()) return false;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().CheckEndCriteria()) return false;
                }
                if (CheckAndEndGameForSabotageWin()) return false;
                if (CheckAndEndGameForTaskWinClassic()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.HideAndSeek)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForHideAndSeek()) return false;
                if (CheckAndEndGameForTaskWin()) return false;
                if (CheckAndEndGameForCrewmateWin()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.ShiftAndSeek)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForHideAndSeek()) return false;
                if (CheckAndEndGameForTaskWin()) return false;
                if (CheckAndEndGameForCrewmateWin()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.BombTag)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBattleRoyale()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems)
            {
                return true;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.BattleRoyale)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBattleRoyale()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.Speedrun)
            {
                if (CheckAndEndGameForSpeedrun()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.KillOrDie)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBattleRoyale()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.Zombies)
            {
                if (CheckAndEndGameForZombiesImpostorWin()) return false;
                if (CheckAndEndGameForZombiesCrewmateWin()) return false;
                if (CheckAndEndGameForZombiesTaskWin()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.Jailbreak)
            {
                if (CheckAndEndGameForEveryoneEscape()) return false;
                if (CheckAndEndGameForTimeEnd()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun)
            {
                if (Options.DisableMeetings.GetBool())
                {
                    if (CheckAndEndGameForEveryoneDied()) return false;
                    if (CheckAndEndGameForHideAndSeek()) return false;
                    if (CheckAndEndGameForTaskWin()) return false;
                    if (CheckAndEndGameForCrewmateWin()) return false;
                    return false;
                }
                return true;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.BaseWars)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBaseWars()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.FreezeTag)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForFreezeTag()) return false;
                if (CheckAndEndGameForTaskWin()) return false;
                if (CheckAndEndGameForCrewmateWin()) return false;
            }
            else if (CustomGamemode.Instance.Gamemode == Gamemodes.ColorWars)
            {
                if (CheckAndEndGameForColorWars()) return false;
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin()
        {
            if (GameData.Instance.TotalTasks <= 0) return false;
            if (GameData.Instance.TotalTasks * (Options.TasksNeededToWin.GetInt() / 100f) <= GameData.Instance.CompletedTasks)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.Role.IsImpostor)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmatesByTask, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWinClassic()
        {
            if (GameData.Instance.TotalTasks <= 0) return false;
            if (GameData.Instance.TotalTasks * (Options.TasksNeededToWin.GetInt() / 100f) <= GameData.Instance.CompletedTasks)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsCrewmate())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmatesByTask, winners, CustomWinners.Crewmates);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWinClassic()
        {
            bool isKillerAlive = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if ((pc.GetRole().IsImpostor() || pc.GetRole().IsNeutralKilling()) && !pc.Data.IsDead)
                    isKillerAlive = true;
            }
            if (!isKillerAlive)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsCrewmate())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmatesByVote, winners, CustomWinners.Crewmates);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWinClassic()
        {
            int impostors = 0;
            int playerCount = 0;
            bool isKillerAlive = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().IsImpostor() && !pc.Data.IsDead)
                    ++impostors;
                if ((pc.GetRole().IsNeutralKilling() || pc.GetRole().ShouldContinueGame()) && !pc.Data.IsDead)
                    isKillerAlive = true;
                if (!pc.Data.IsDead)
                    playerCount += pc.GetRole().GetPlayerCount();
            }
            if (!isKillerAlive && impostors * 2 >= playerCount)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsImpostor())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners, CustomWinners.Impostors);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin()
        {
            if (ShipStatus.Instance.Systems == null) return false;
            var systems = ShipStatus.Instance.Systems;
            LifeSuppSystemType LifeSupp;
            if (systems.ContainsKey(SystemTypes.LifeSupp) &&
                (LifeSupp = systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>()) != null &&
                LifeSupp.Countdown < 0f)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsImpostor())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsBySabotage, winners, CustomWinners.Impostors);
                LifeSupp.Countdown = 10000f;
                return true;
            }

            ISystemType sys = null;
            if (systems.ContainsKey(SystemTypes.Reactor)) sys = systems[SystemTypes.Reactor];
            else if (systems.ContainsKey(SystemTypes.Laboratory)) sys = systems[SystemTypes.Laboratory];
            else if (systems.ContainsKey(SystemTypes.HeliSabotage)) sys = systems[SystemTypes.HeliSabotage];

            ICriticalSabotage critical;
            if (sys != null &&
                (critical = sys.TryCast<ICriticalSabotage>()) != null &&
                critical.Countdown < 0f)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().IsImpostor())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsBySabotage, winners, CustomWinners.Impostors);
                critical.ClearSabotage();
                return true;
            }

            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin()
        {
            var numImpostorAlive = 0;
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (numImpostorAlive == 0)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.Role.IsImpostor)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmatesByVote, winners, CustomWinners.Crewmates);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForHideAndSeek()
        {
            var numImpostorAlive = 0;
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (AllAlivePlayers.Count - numImpostorAlive == 0)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners, CustomWinners.Impostors);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForBattleRoyale()
        {
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (AllAlivePlayers.Count == 1)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForEveryoneDied()
        {
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (AllAlivePlayers.Count == 0)
            {
                List<byte> winners = new();
                StartEndGame(GameOverReason.ImpostorsByKill, winners, CustomWinners.NoOne);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSpeedrun()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var tasksCompleted = 0;
                var totalTasks = 0;
                foreach (var task in pc.Data.Tasks)
                {
                    ++totalTasks;
                    if (task.Complete)
                        ++tasksCompleted;
                }
                if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                    --totalTasks;
                if (tasksCompleted >= totalTasks)
                {
                    List<byte> winners = new();
                    winners.Add(pc.PlayerId);
                    StartEndGame(GameOverReason.CrewmatesByTask, winners);
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForZombiesImpostorWin()
        {
            var numImpostorAlive = 0;
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Main.StandardRoles[pc.PlayerId].IsImpostor() && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead && !ZombiesGamemode.instance.IsZombie(pc)) AllAlivePlayers.Add(pc);
            }
            if (numImpostorAlive * 2 >= AllAlivePlayers.Count)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.StandardRoles[pc.PlayerId].IsImpostor() || ZombiesGamemode.instance.IsZombie(pc))
                        winners.Add(pc.PlayerId);
                }
                var reason = GameOverReason.ImpostorsByKill;
                if (GameData.LastDeathReason == DeathReason.Exile)
                    reason = GameOverReason.ImpostorsByVote;
                if (GameData.LastDeathReason == DeathReason.Disconnect)
                    reason = GameOverReason.CrewmateDisconnect;
                StartEndGame(reason, winners, CustomWinners.Impostors);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForZombiesCrewmateWin()
        {
            var numImpostorAlive = 0;
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Main.StandardRoles[pc.PlayerId].IsImpostor() && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead && !ZombiesGamemode.instance.IsZombie(pc)) AllAlivePlayers.Add(pc);
            }
            if (numImpostorAlive == 0)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!Main.StandardRoles[pc.PlayerId].IsImpostor() && !ZombiesGamemode.instance.IsZombie(pc))
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmatesByVote, winners, CustomWinners.Crewmates);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForZombiesTaskWin()
        {
            if (GameData.Instance.TotalTasks <= 0) return false;
            if (GameData.Instance.TotalTasks * (Options.TasksNeededToWin.GetInt() / 100f) <= GameData.Instance.CompletedTasks)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!Main.StandardRoles[pc.PlayerId].IsImpostor() && !ZombiesGamemode.instance.IsZombie(pc))
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmatesByTask, winners, CustomWinners.Crewmates);
            }
            return true;
        }

        private static bool CheckAndEndGameForEveryoneEscape()
        {
            bool someoneRemain = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!JailbreakGamemode.instance.IsGuard(pc) && !JailbreakGamemode.instance.HasEscaped(pc))
                    someoneRemain = true;
            }
            if (!someoneRemain)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!JailbreakGamemode.instance.IsGuard(pc))
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForTimeEnd()
        {
            if (Main.Timer >= Options.GameTime.GetFloat())
            {
                List<byte> winners = new();
                int prisoners = 0;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    ++prisoners;
                    if (!JailbreakGamemode.instance.IsGuard(pc) && JailbreakGamemode.instance.HasEscaped(pc))
                        winners.Add(pc.PlayerId);
                }
                if (winners.Count * 2 < prisoners)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (JailbreakGamemode.instance.IsGuard(pc))
                            winners.Add(pc.PlayerId);
                    }
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForBaseWars()
        {
            BaseWarsTeams winner = BaseWarsTeams.None;
            int redMembers = 0;
            int blueMembers = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (BaseWarsGamemode.instance.GetTeam(pc) == BaseWarsTeams.Red)
                    ++redMembers;
                else if (BaseWarsGamemode.instance.GetTeam(pc) == BaseWarsTeams.Blue)
                    ++blueMembers;
            }
            if (redMembers == 0)
                winner = BaseWarsTeams.Blue;
            else if (blueMembers == 0)
                winner = BaseWarsTeams.Red;
            if (winner != BaseWarsTeams.None)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (BaseWarsGamemode.instance.GetTeam(pc) == winner)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.CrewmateDisconnect, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForFreezeTag()
        {
            bool crewmateRemain = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.Role.IsImpostor && !FreezeTagGamemode.instance.IsFrozen(pc))
                    crewmateRemain = true;
            }
            if (!crewmateRemain)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners, CustomWinners.Impostors);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForColorWars()
        {
            List<byte> leaders = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (ColorWarsGamemode.instance.IsLeader(pc) && !pc.Data.IsDead && !leaders.Contains(ColorWarsGamemode.instance.GetTeam(pc)))
                    leaders.Add(ColorWarsGamemode.instance.GetTeam(pc));
            }
            if (leaders.Count == 0)
            {
                List<byte> winners = new();
                StartEndGame(GameOverReason.ImpostorsByKill, winners);
                return true;
            }
            if (leaders.Count == 1)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (ColorWarsGamemode.instance.GetTeam(pc) == leaders[0])
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorsByKill, winners);
                return true;
            }
            return false;
        }

        public static void StartEndGame(GameOverReason reason, List<byte> winners, CustomWinners winner = CustomWinners.None)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                ClassicGamemode.instance.Winner = winner;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    switch (pc.GetRole().Role)
                    {
                        case CustomRoles.Opportunist:
                            if (!pc.Data.IsDead && !winners.Contains(pc.PlayerId))
                            {
                                if (!ClassicGamemode.instance.AdditionalWinners.Contains(AdditionalWinners.Opportunist))
                                    ClassicGamemode.instance.AdditionalWinners.Add(AdditionalWinners.Opportunist);
                                winners.Add(pc.PlayerId);
                            }
                            break;
                    }
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    switch (pc.GetRole().Role)
                    {
                        case CustomRoles.Romantic:
                            Romantic romanticRole = pc.GetRole() as Romantic;
                            if (romanticRole != null && romanticRole.LoverId != byte.MaxValue && winners.Contains(romanticRole.LoverId) && !winners.Contains(pc.PlayerId))
                            {
                                if (!ClassicGamemode.instance.AdditionalWinners.Contains(AdditionalWinners.Romantic))
                                    ClassicGamemode.instance.AdditionalWinners.Add(AdditionalWinners.Romantic);
                                winners.Add(pc.PlayerId);
                            }
                            break;
                    }
                }
                GameManager.Instance.RpcSyncCustomWinner();
            }
            var ImpostorWin = false;
            switch (reason)
            {
                case GameOverReason.CrewmateDisconnect: ImpostorWin = true; break;
                case GameOverReason.ImpostorsByVote: ImpostorWin = true; break;
                case GameOverReason.ImpostorsByKill: ImpostorWin = true; break;
                case GameOverReason.ImpostorsBySabotage: ImpostorWin = true; break;
                case GameOverReason.HideAndSeek_ImpostorsByKills: ImpostorWin = true; break;
                default: ImpostorWin = false; break;
            }
            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
            sender.StartMessage(-1);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (winners.Contains(pc.PlayerId))
                {
                    if (ImpostorWin)
                        SetRole(pc.Data.IsDead ? RoleTypes.ImpostorGhost : RoleTypes.Impostor);
                    else
                        SetRole(pc.Data.IsDead ? RoleTypes.CrewmateGhost : RoleTypes.Crewmate);
                }
                else
                {
                    if (ImpostorWin)
                        SetRole(pc.Data.IsDead ? RoleTypes.CrewmateGhost : RoleTypes.Crewmate);
                    else
                        SetRole(pc.Data.IsDead ? RoleTypes.ImpostorGhost : RoleTypes.Impostor);
                }

                void SetRole(RoleTypes roleType)
                {
                    RoleManager.Instance.SetRole(pc, roleType);
                    sender.StartRpc(pc.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)roleType)
                        .Write(true)
                        .EndRpc();
                }
            }
            sender.EndMessage();
            sender.SendMessage();
            GameManager.Instance.ShouldCheckForGameEnd = false;
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage(8);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.Write((byte)reason);
            writer.Write(false);
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
    class CheckEndCriteriaHnSPatch
    {
        public static bool Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (!GameData.Instance) return false;
            if (TutorialManager.InstanceExists) return true;
            if (Options.NoGameEnd.GetBool()) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
    class CheckTaskCompletionPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckEndGameViaTasks))]
    class CheckEndGameViaTasksPatch
    {
        public static bool Prefix(GameManager __instance, ref bool __result)
        {
            GameData.Instance.RecomputeTaskCounts();
            if (GameData.Instance.TotalTasks * (Options.TasksNeededToWin.GetInt() / 100f) <= GameData.Instance.CompletedTasks)
            {
                __instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
                __result = true;
            }
            __result = false;
            return false;
        }
    }
}