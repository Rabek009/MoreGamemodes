using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class ControllerManagerUpdatePatch
    {
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && !Main.GameStarted)
                PlayerControl.LocalPlayer.GetComponent<CircleCollider2D>().enabled = !PlayerControl.LocalPlayer.gameObject.GetComponent<CircleCollider2D>().enabled;

            if (!AmongUsClient.Instance.AmHost) return;
            
            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.L, KeyCode.LeftShift }) && Main.GameStarted)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                    winners.Add(pc.PlayerId);
                CheckEndCriteriaPatch.StartEndGame(GameOverReason.HumansByVote, winners);
            }

            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.Z, KeyCode.LeftShift }) && Main.GameStarted && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                PlayerControl.LocalPlayer.RpcSetDeathReason(DeathReasons.Command);
                PlayerControl.LocalPlayer.RpcExileV2();
            }

            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.M, KeyCode.LeftShift }) && MeetingHud.Instance)
            {
                foreach (var pva in MeetingHud.Instance.playerStates)
                {
                    if (pva == null) continue;
                    if (pva.DidVote)
                        MeetingHud.Instance.RpcClearVote(pva.TargetPlayerId);
                }
                List<MeetingHud.VoterState> statesList = new();
                MeetingHud.Instance.RpcVotingComplete(statesList.ToArray(), null, true);
            }

            if (Input.GetKeyDown(KeyCode.C) && GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                GameStartManager.Instance.ResetStartState();

            if (Input.GetKeyDown(KeyCode.LeftShift) && GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                GameStartManager.Instance.countDownTimer = 0f;

            if (GetKeysDown(new[] { KeyCode.LeftControl, KeyCode.Delete }) && !Main.GameStarted)
            {
                OptionItem.AllOptions.ToArray().Where(x => x.Id > 0).Do(x => x.CurrentValue = x.DefaultValue);
            }
        }

        static bool GetKeysDown(KeyCode[] keys)
        {
            if (keys.Any(k => Input.GetKeyDown(k)) && keys.All(k => Input.GetKey(k)))
            {
                return true;
            }
            return false;
        }
    }
}