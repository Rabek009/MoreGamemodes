using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using InnerNet;

using Object = UnityEngine.Object;

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
            
            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.L, KeyCode.LeftShift }) && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                    winners.Add(pc.PlayerId);
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.HumansByVote, winners);
            }

            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.Z, KeyCode.LeftShift }) && Main.GameStarted && !PlayerControl.LocalPlayer.Data.IsDead &&
                !(CustomGamemode.Instance.Gamemode is Gamemodes.PaintBattle or Gamemodes.Speedrun or Gamemodes.Jailbreak or Gamemodes.BaseWars or Gamemodes.FreezeTag))
            {
                PlayerControl.LocalPlayer.RpcSetDeathReason(DeathReasons.Command);
                PlayerControl.LocalPlayer.RpcExileV2();
            }

            if (GetKeysDown(new[] { KeyCode.Return, KeyCode.M, KeyCode.LeftShift }) && MeetingHud.Instance)
            {
                VotingCompletePatch.Postfix(MeetingHud.Instance, Array.Empty<MeetingHud.VoterState>(), null, true);
                MeetingHud.Instance.RpcClose();
            }

            if (Input.GetKeyDown(KeyCode.C) && GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown && Main.ModdedProtocol.Value)
                GameStartManager.Instance.ResetStartState();

            if (Input.GetKeyDown(KeyCode.LeftShift) && GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown && Main.ModdedProtocol.Value)
                GameStartManager.Instance.countDownTimer = 0f;

            if (GetKeysDown(new[] { KeyCode.LeftControl, KeyCode.Delete }) && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                OptionItem.AllOptions.ToArray().Where(x => x.Id > 0).Do(x => x.CurrentValue = x.DefaultValue);
                GameManager.Instance.RpcSyncCustomOptions();
                var menu = Object.FindObjectOfType<GameOptionsMenu>();
                if (menu)
                {
                    foreach (var tab in Enum.GetValues<TabGroup>())
                        GameOptionsMenuPatch.ReCreateSettings(menu, tab);
                    for (int index = 0; index < OptionItem.AllOptions.Count; index++)
                    {
                        var option = OptionItem.AllOptions[index];
                        if (option != null && option is not TextOptionItem && ModGameOptionsMenu.BehaviourList.TryGetValue(index, out var optionBehaviour))
                        {
                            if (option is IntegerOptionItem && optionBehaviour.TryCast<NumberOption>(out var numberOption))
                                numberOption.Value = option.GetInt();
                            else if (option is FloatOptionItem && optionBehaviour.TryCast<NumberOption>(out var numberOption2))
                                numberOption2.Value = option.GetFloat();
                            else if (option is BooleanOptionItem && optionBehaviour.TryCast<ToggleOption>(out var toggleOption))
                                toggleOption.CheckMark.enabled = option.GetBool();
                            else if ((option is StringOptionItem || option is PresetOptionItem) && optionBehaviour.TryCast<StringOption>(out var stringOption))
                                stringOption.Value = option.GetInt();
                        }
                    }
                }
                var viewSettingsPane = Object.FindObjectOfType<LobbyViewSettingsPane>();
                if (viewSettingsPane != null)
                {
                    if (viewSettingsPane.currentTab != StringNames.OverviewCategory && viewSettingsPane.currentTab != StringNames.RolesCategory)
                        viewSettingsPane.RefreshTab();
                    viewSettingsPane.gameModeText.text = Options.Gamemode.GetString();
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