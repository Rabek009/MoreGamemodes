using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    class CheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
            if (Options.NoGameEnd.GetBool()) return false;

            if (Options.CurrentGamemode == Gamemodes.Classic || Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                return true;
            }
            else if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForHideAndSeek()) return false;
                if (CheckAndEndGameForTaskWin()) return false;
                if (CheckAndEndGameForCrewmateWin()) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForHideAndSeek()) return false;
                if (CheckAndEndGameForTaskWin()) return false;
                if (CheckAndEndGameForCrewmateWin()) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBattleRoyale()) return false;          
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBattleRoyale()) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.Speedrun)
            {
                if (CheckAndEndGameForSpeedrun()) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.KillOrDie)
            {
                if (CheckAndEndGameForEveryoneDied()) return false;
                if (CheckAndEndGameForBattleRoyale()) return false;          
            }
            else if (Options.CurrentGamemode == Gamemodes.Zombies)
            {
                if (CheckAndEndGameForZombiesImpostorWin()) return false;
                if (CheckAndEndGameForZombiesCrewmateWin()) return false;
                if (CheckAndEndGameForZombiesTaskWin()) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.Jailbreak)
            {
                if (CheckAndEndGameForEveryoneEscape()) return false;
                if (CheckAndEndGameForTimeEnd()) return false;          
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin()
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.Role.IsImpostor)
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.HumansByTask, winners);
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
                var reason = GameOverReason.HumansByVote;
                if (TempData.LastDeathReason == DeathReason.Disconnect)
                    reason = GameOverReason.ImpostorDisconnect;
                StartEndGame(reason, winners);
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
                StartEndGame(GameOverReason.ImpostorByKill, winners);
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
                StartEndGame(GameOverReason.ImpostorByKill, winners);
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
                StartEndGame(GameOverReason.ImpostorByKill, winners);
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
                foreach (var task in pc.myTasks)
                {
                    ++totalTasks;
                    if (task.IsComplete)
                        ++tasksCompleted;
                }
                if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                    --totalTasks;
                if (tasksCompleted >= totalTasks)
                {
                    List<byte> winners = new();
                    winners.Add(pc.PlayerId);
                    StartEndGame(GameOverReason.HumansByTask, winners);
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
                if (pc.Data.Role.IsImpostor && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead && !pc.IsZombie()) AllAlivePlayers.Add(pc);
            }
            if (numImpostorAlive * 2 >= AllAlivePlayers.Count)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor || pc.IsZombie())
                        winners.Add(pc.PlayerId);
                }
                var reason = GameOverReason.ImpostorByKill;
                if (TempData.LastDeathReason == DeathReason.Exile)
                    reason = GameOverReason.ImpostorByVote;
                if (TempData.LastDeathReason == DeathReason.Disconnect)
                    reason = GameOverReason.HumansDisconnect;
                StartEndGame(reason, winners);
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
                if (pc.Data.Role.IsImpostor && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead && !pc.IsZombie()) AllAlivePlayers.Add(pc);
            }
            if (numImpostorAlive == 0)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.Role.IsImpostor && !pc.IsZombie())
                        winners.Add(pc.PlayerId);
                }
                var reason = GameOverReason.HumansByVote;
                if (TempData.LastDeathReason == DeathReason.Disconnect)
                    reason = GameOverReason.ImpostorDisconnect;
                StartEndGame(reason, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForZombiesTaskWin()
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.Role.IsImpostor && !pc.IsZombie())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.HumansByTask, winners);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForEveryoneEscape()
        {
            bool someoneRemain = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.IsGuard() && !pc.HasEscaped())
                    someoneRemain = true;
            }
            if (!someoneRemain)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.IsGuard())
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorByKill, winners);
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
                    if (!pc.IsGuard() && pc.HasEscaped())
                        winners.Add(pc.PlayerId);
                }
                if (winners.Count * 2 < prisoners)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.IsGuard())
                            winners.Add(pc.PlayerId);
                    }
                }
                StartEndGame(GameOverReason.ImpostorByKill, winners);
                return true;
            }
            return false;
        }

        public static void StartEndGame(GameOverReason reason, List<byte> winners)
        {
            var sender = new CustomRpcSender("EndGameSender", SendOption.Reliable);
            sender.StartMessage(-1);
            MessageWriter writer = sender.stream;

            List<byte> ReviveReqiredPlayerIds = new();
            var ImpostorWin = false;
            switch (reason)
            {
                case GameOverReason.HumansDisconnect: ImpostorWin = true; break;
                case GameOverReason.ImpostorByVote: ImpostorWin = true; break;
                case GameOverReason.ImpostorByKill: ImpostorWin = true; break;
                case GameOverReason.ImpostorBySabotage: ImpostorWin = true; break;
                case GameOverReason.HideAndSeek_ByKills: ImpostorWin = true; break;
                default: ImpostorWin = false; break;
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead) ReviveReqiredPlayerIds.Add(pc.PlayerId);
                if (winners.Contains(pc.PlayerId))
                {
                    if (ImpostorWin)
                        pc.SetRole(RoleTypes.ImpostorGhost);
                    else
                        pc.SetRole(RoleTypes.CrewmateGhost);
                }
                else
                {
                    if (ImpostorWin)
                        pc.SetRole(RoleTypes.CrewmateGhost);
                    else
                        pc.SetRole(RoleTypes.ImpostorGhost);
                }
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (winners.Contains(pc.PlayerId))
                {
                    if (ImpostorWin)
                        SetGhostRole(ToGhostImpostor: true);
                    else
                        SetGhostRole(ToGhostImpostor: false);
                }
                else
                {
                    if (ImpostorWin)
                        SetGhostRole(ToGhostImpostor: false);
                    else
                        SetGhostRole(ToGhostImpostor: true);
                }

                void SetGhostRole(bool ToGhostImpostor)
                {
                    if (!pc.Data.IsDead) ReviveReqiredPlayerIds.Add(pc.PlayerId);
                    if (ToGhostImpostor)
                    {
                        sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                            .Write((ushort)RoleTypes.ImpostorGhost)
                            .EndRpc();
                        pc.SetRole(RoleTypes.ImpostorGhost);
                    }
                    else
                    {
                        sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                            .Write((ushort)RoleTypes.CrewmateGhost)
                            .EndRpc();
                        pc.SetRole(RoleTypes.CrewmateGhost);
                    }
                }
            }
            writer.StartMessage(1);
            {
                writer.WritePacked(GameData.Instance.NetId);
                foreach (var info in GameData.Instance.AllPlayers)
                {
                    if (ReviveReqiredPlayerIds.Contains(info.PlayerId))
                    {
                        info.IsDead = false;
                        writer.StartMessage(info.PlayerId);
                        info.Serialize(writer);
                        writer.EndMessage();
                    }
                }
                writer.EndMessage();
            }
            sender.EndMessage();

            writer.StartMessage(8);
            {
                writer.Write(AmongUsClient.Instance.GameId);
                writer.Write((byte)reason);
                writer.Write(false);
            }
            writer.EndMessage();

            sender.SendMessage();
        }
    }
}