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
            if (!Main.CanGameEnd) return false;

            if (Options.CurrentGamemode == Gamemodes.Classic || Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                return true;
            }
            else if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
            {
                if (CheckAndEndGameForEveryoneDied(__instance)) return false;
                if (CheckAndEndGameForHideAndSeek(__instance)) return false;
                if (CheckAndEndGameForTaskWin(__instance)) return false;
                if (CheckAndEndGameForCrewmateWin(__instance)) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                if (CheckAndEndGameForEveryoneDied(__instance)) return false;
                if (CheckAndEndGameForHideAndSeek(__instance)) return false;
                if (CheckAndEndGameForTaskWin(__instance)) return false;
                if (CheckAndEndGameForCrewmateWin(__instance)) return false;
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                if (CheckAndEndGameForEveryoneDied(__instance)) return false;
                if (CheckAndEndGameForBattleRoyale(__instance)) return false;          
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                if (CheckAndEndGameForEveryoneDied(__instance)) return false;
                if (CheckAndEndGameForBattleRoyale(__instance)) return false;
            }
            return false;
        }
        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                List<byte> winners = new List<byte>();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!Main.Impostors.Contains(pc.PlayerId))
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.HumansByTask, winners);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance)
        {
            var numImpostorAlive = 0;
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Main.Impostors.Contains(pc.PlayerId) && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (numImpostorAlive == 0)
            {
                List<byte> winners = new List<byte>();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!Main.Impostors.Contains(pc.PlayerId))
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

        private static bool CheckAndEndGameForHideAndSeek(ShipStatus __instance)
        {
            var numImpostorAlive = 0;
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Main.Impostors.Contains(pc.PlayerId) && !pc.Data.IsDead) ++numImpostorAlive;
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (AllAlivePlayers.Count - numImpostorAlive == 0)
            {
                List<byte> winners = new List<byte>();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.Impostors.Contains(pc.PlayerId))
                        winners.Add(pc.PlayerId);
                }
                StartEndGame(GameOverReason.ImpostorByKill, winners);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForBattleRoyale(ShipStatus __instance)
        {
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (AllAlivePlayers.Count == 1)
            {
                List<byte> winners = new List<byte>();
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
        private static bool CheckAndEndGameForEveryoneDied(ShipStatus __instance)
        {
            List<PlayerControl> AllAlivePlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead) AllAlivePlayers.Add(pc);
            }
            if (AllAlivePlayers.Count == 0)
            {
                List<byte> winners = new List<byte>();
                StartEndGame(GameOverReason.ImpostorByKill, winners);
                return true;
            }
            return false;
        }
        public static void StartEndGame(GameOverReason reason, List<byte> winners)
        {
            var sender = new CustomRpcSender("EndGameSender", SendOption.Reliable, true);
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
            foreach (var pc in Main.AllPlayerControls)
            {
                if (!pc.Data.IsDead) ReviveReqiredPlayerIds.Add(pc.PlayerId);
                if (winners.Contains(pc.PlayerId))
                {
                    if (ImpostorWin)
                        pc.RpcSetRole(RoleTypes.ImpostorGhost);
                    else
                        pc.RpcSetRole(RoleTypes.CrewmateGhost);
                }
                else
                {
                    if (ImpostorWin)
                        pc.RpcSetRole(RoleTypes.CrewmateGhost);
                    else
                        pc.RpcSetRole(RoleTypes.ImpostorGhost);
                }
            }
            foreach (var pc in Main.AllPlayerControls)
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
                        pc.SetRole(RoleTypes.Crewmate);
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