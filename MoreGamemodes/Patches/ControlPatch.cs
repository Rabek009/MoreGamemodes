using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class ControllerManagerUpdatePatch
    {
        static readonly System.Random random = new();
        public static void Postfix(ControllerManager __instance)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && !Main.GameStarted)
                PlayerControl.LocalPlayer.GetComponent<CircleCollider2D>().enabled = !PlayerControl.LocalPlayer.gameObject.GetComponent<CircleCollider2D>().enabled;

            if (!AmongUsClient.Instance.AmHost) return;
            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.L, KeyCode.LeftShift }) && Main.GameStarted)
            {
                List<byte> winners = new List<byte>();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    winners.Add(pc.PlayerId);
                }
                CheckEndCriteriaPatch.StartEndGame(GameOverReason.HumansByVote, winners);
            }
            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.Z, KeyCode.LeftShift }) && Main.GameStarted && !PlayerControl.LocalPlayer.Data.IsDead)
                PlayerControl.LocalPlayer.Exiled();

            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.M, KeyCode.LeftShift }) && Main.GameStarted)
                MeetingHud.Instance.RpcClose();

            if (Input.GetKeyDown(KeyCode.C))
                GameStartManager.Instance.ResetStartState();

            if (Input.GetKeyDown(KeyCode.LeftShift))
                GameStartManager.Instance.countDownTimer = 0f;

            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.N, KeyCode.LeftShift }))
            {
                if (GameData.Instance.AllPlayers.Count < 255)
                {
                    Utils.CreateNPC((byte)GameData.Instance.AllPlayers.Count, PlayerControl.LocalPlayer.transform.position, new byte[0], (byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId, PlayerControl.LocalPlayer.name, PlayerControl.LocalPlayer.CurrentOutfit.HatId, PlayerControl.LocalPlayer.CurrentOutfit.SkinId, PlayerControl.LocalPlayer.CurrentOutfit.PetId, PlayerControl.LocalPlayer.CurrentOutfit.VisorId, PlayerControl.LocalPlayer.Data.PlayerLevel, PlayerControl.LocalPlayer.CurrentOutfit.NamePlateId);
                    Utils.SendGameData();
                }
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