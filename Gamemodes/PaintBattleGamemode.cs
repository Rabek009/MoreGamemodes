using UnityEngine;
using AmongUs.GameOptions;
using System.Collections.Generic;
using Hazel;

namespace MoreGamemodes
{
    public class PaintBattleGamemode : CustomGamemode
    {
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            __instance.KillButton.SetDisabled();
            __instance.KillButton.ToggleVisible(false);
            __instance.MapButton.gameObject.SetActive(false);
            if (IsPaintActive)
            {
                __instance.AbilityButton.ToggleVisible(true);
                __instance.AbilityButton.OverrideText("Paint");
            }
            else
            {
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
            }
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            __instance.FilterText.text = "Player";
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.taskText.text = Utils.ColorString(Color.gray, "Painter:\nPaint something in theme.\nThe theme is " + Theme + ".");
        }

        public override void OnShowNormalMap(MapBehaviour __instance)
        {
            __instance.Close();
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Painter";
            __instance.TeamTitle.color = Color.gray;
            __instance.BackgroundBar.material.color = Color.gray;
            __instance.ImpostorText.text = "";
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            __instance.RoleText.text = "Painter";
            __instance.RoleText.color = Color.gray;
            __instance.RoleBlurbText.text = "Paint something in theme.";
            __instance.RoleBlurbText.color = Color.gray;
            __instance.YouAreText.color = Color.gray;
        }

        public override bool OnSelectRolesPrefix()
        {
            Utils.RpcSetDesyncRoles(RoleTypes.Shapeshifter, RoleTypes.Crewmate);
            return false;
        }

        public override void OnIntroDestroy()
        {
            PaintTime = Options.PaintingTime.GetInt();
            IsPaintActive = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcTeleport(GetPaintBattleLocation(pc.PlayerId));
            var rand = new System.Random();
            Theme = Main.PaintBattleThemes[rand.Next(0, Main.PaintBattleThemes.Count)];
            SendRPC(GameManager.Instance);
            Utils.SendChat("Start painting! The theme is " + Theme + "! Remember to evalute less paintings that are not in theme!", "Theme");
            new LateTask(() => Utils.RpcSetUnshiftButton(), 0.5f);
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            return false;
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (IsPaintActive && Vector2.Distance(shapeshifter.GetRealPosition(), GetPaintBattleLocation(shapeshifter.PlayerId)) < 5f)
                Utils.RpcCreateDeadBody(shapeshifter.GetRealPosition(), (byte)shapeshifter.CurrentOutfit.ColorId, shapeshifter);
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (IsPaintActive)
            {
                PaintTime -= Time.fixedDeltaTime;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Vector2.Distance(pc.GetRealPosition(), GetPaintBattleLocation(pc.PlayerId)) > 5f)
                        pc.RpcTeleport(GetPaintBattleLocation(pc.PlayerId));
                }
                if (PaintTime <= 0f)
                {
                    PaintTime = 0f;
                    IsPaintActive = false;
                    SendRPC(GameManager.Instance);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        pc.RpcSetRoleV2(RoleTypes.Crewmate);
                }
            }
            else
            {
                if (VotingPlayerId == 0 && PaintBattleVotingTime == 0f)
                {
                    PaintBattleVotingTime = Options.VotingTime.GetInt();
                    Utils.SendChat("Rate " + Main.StandardNames[VotingPlayerId] + "'s paint by typing 1-10 in chat!", "Voting");
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        HasVoted[pc.PlayerId] = false;
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Vector2.Distance(pc.GetRealPosition(), GetPaintBattleLocation(VotingPlayerId)) > 5f)
                        pc.RpcTeleport(GetPaintBattleLocation(VotingPlayerId));
                }
                PaintBattleVotingTime -= Time.fixedDeltaTime;
                if (PaintBattleVotingTime <= 0f)
                {
                    PaintBattleVotingTime = Options.VotingTime.GetInt();
                    ++VotingPlayerId;
                    if (VotingPlayerId > 14)
                        EndPaintBattleGame();
                    while (Utils.GetPlayerById(VotingPlayerId) == null)
                    {
                        ++VotingPlayerId;
                        if (VotingPlayerId > 14)
                        {
                            EndPaintBattleGame();
                            break;
                        }
                    }
                    Utils.SendChat("Rate " + Main.StandardNames[VotingPlayerId] + "'s paint by typing 1-10 in chat!", "Voting");
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        HasVoted[pc.PlayerId] = false;
                }
            }
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            return false;
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Tracker, 0, 0);
            opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
            opt.SetFloat(FloatOptionNames.CrewLightMod, 100f);
            opt.SetFloat(FloatOptionNames.ImpostorLightMod, 100f);
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.001f);
            opt.SetFloat(FloatOptionNames.KillCooldown, 1000000f);
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (player == seer && IsPaintActive)
                name = Utils.ColorString(Color.cyan, "<font=\"VCR SDF\"><size=10>REMAINING TIME: " + (int)(PaintTime + 0.99f) + "s</size><size=15>\n\n</size></font>") + name + "<font=\"VCR SDF\"><size=25>\n\n<size=0>.";
            if (player == seer && !IsPaintActive)
                name = Utils.ColorString(Color.magenta, "<font=\"VCR SDF\"><size=8>Rate " + Main.StandardNames[VotingPlayerId] + "'s painting!</size><size=17>\n\n</size></font>") + name + "<font=\"VCR SDF\"><size=25>\n\n<size=0>.";
            return name;
        }

        public void SendRPC(GameManager manager)
        {
            HudManager.Instance.TaskPanel.SetTaskText("");
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable);
            writer.Write(Theme);
            writer.Write(IsPaintActive);
            writer.EndMessage();
        }

        public override void ReceiveRPC(GameManager manager, MessageReader reader)
        {
            Theme = reader.ReadString();
            IsPaintActive = reader.ReadBoolean();
            HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public void EndPaintBattleGame()
        {
            List<byte> winners = new();
            List<byte> bestPlayers = new();
            float bestRate = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (PlayerVotes[pc.PlayerId].Item2 == 0)
                    winners.Add(pc.PlayerId);
                else if ((float)PlayerVotes[pc.PlayerId].Item1 / (float)PlayerVotes[pc.PlayerId].Item2 > bestRate)
                {
                    bestRate = (float)PlayerVotes[pc.PlayerId].Item1 / (float)PlayerVotes[pc.PlayerId].Item2;
                    bestPlayers.Clear();
                    bestPlayers.Add(pc.PlayerId);
                }
                else if ((float)PlayerVotes[pc.PlayerId].Item1 / (float)PlayerVotes[pc.PlayerId].Item2 == bestRate)
                    bestPlayers.Add(pc.PlayerId);
            }
            foreach (var id in bestPlayers)
                winners.Add(id);
            CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.CrewmatesByTask, winners);
        }

        public Vector2 GetPaintBattleLocation(byte playerId)
        {
            int x, y;
            if (playerId < 8)
            {
                x = (playerId % 4 * -12) - 8;
                y = (playerId / 4 * -12) - 30;
            }
            else
            {
                x = (playerId % 4 * 12) - 8;
                y = (playerId / 4 * 12) + 10;
            }
            return new Vector2(x, y);
        }

        public PaintBattleGamemode()
        {
            Gamemode = Gamemodes.PaintBattle;
            PetAction = false;
            DisableTasks = true;
            PaintTime = 0f;
            IsPaintActive = false;
            VotingPlayerId = 0;
            PaintBattleVotingTime = 0f;
            HasVoted = new Dictionary<byte, bool>();
            PlayerVotes = new Dictionary<byte, (int, int)>();
            Theme = "";
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                HasVoted[pc.PlayerId] = false;
                PlayerVotes[pc.PlayerId] = (0, 0);
            }
        }

        public static PaintBattleGamemode instance;
        public float PaintTime;
        public bool IsPaintActive;
        public byte VotingPlayerId;
        public float PaintBattleVotingTime;
        public Dictionary<byte, bool> HasVoted;
        public Dictionary<byte, (int, int)> PlayerVotes;
        public string Theme;
    }
}