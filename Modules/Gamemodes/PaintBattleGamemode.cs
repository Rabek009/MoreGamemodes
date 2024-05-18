using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class PaintBattleGamemode : CustomGamemode
    {
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            __instance.PetButton.OverrideText("Paint");
            __instance.AbilityButton.OverrideText("Remaining Time");
            if (IsPaintActive)
                __instance.AbilityButton.ToggleVisible(true);
            else
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
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

        public override Il2CppSystem.Collections.Generic.List<PlayerControl> OnBeginCrewmatePrefix(IntroCutscene __instance)
        {
            var Team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            Team.Add(PlayerControl.LocalPlayer);
            return Team;
        }

        public override void OnBeginCrewmatePostfix(IntroCutscene __instance)
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

        public override void OnSelectRolesPrefix()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcSetRole(RoleTypes.Crewmate);
        }

        public override void OnIntroDestroy()
        {
            new LateTask(() =>
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.RpcSetDeathReason(DeathReasons.Command);
                    pc.RpcSetRole(RoleTypes.GuardianAngel);
                    pc.RpcTeleport(pc.GetPaintBattleLocation());
                }
            }, 5f, "Set Dead");
            new LateTask(() =>
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.Data.IsDead = false;
                    pc.RpcResetAbilityCooldown();
                }
                Utils.SendGameData();
            }, 6f, "Alive Dead Role");
            new LateTask(() =>
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcShapeshift(pc, false);
            }, 7f, "Fix Pets");
            PaintTime = Options.PaintingTime.GetInt() + 5;
            GameManager.Instance.RpcSetPaintActive(true);
            var rand = new System.Random();
            GameManager.Instance.RpcSetTheme(Main.PaintBattleThemes[rand.Next(0, Main.PaintBattleThemes.Count)]);
            Utils.SendChat("Start painting! The theme is " + Theme + "! Remember to evalute less paintings that are not in theme!", "Theme");
        }

        public override void OnPet(PlayerControl pc)
        {
            if (IsPaintActive && Vector2.Distance(pc.transform.position, pc.GetPaintBattleLocation()) < 5f && Main.Timer >= 6f && CreateBodyCooldown[pc.PlayerId] <= 0f)
            {
                CreateBodyCooldown[pc.PlayerId] = 0.5f;
                Utils.RpcCreateDeadBody(pc.transform.position, (byte)pc.CurrentOutfit.ColorId, PlayerControl.LocalPlayer);
            }
        }

        public override bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, GameData.PlayerInfo target)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (IsPaintActive && PaintTime > 0f)
            {
                PaintTime -= Time.fixedDeltaTime;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (CreateBodyCooldown[pc.PlayerId] > 0f)
                    {
                        CreateBodyCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (CreateBodyCooldown[pc.PlayerId] < 0f)
                    {
                        CreateBodyCooldown[pc.PlayerId] = 0f;
                    }
                    if (Vector2.Distance(pc.transform.position, pc.GetPaintBattleLocation()) > 5f)
                        pc.RpcTeleport(pc.GetPaintBattleLocation());
                }
                if (PaintTime <= 0f)
                {
                    PaintTime = 0f;
                    GameManager.Instance.RpcSetPaintActive(false);
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
                    if (Vector2.Distance(pc.transform.position, Utils.GetPlayerById(VotingPlayerId).GetPaintBattleLocation()) > 5f)
                        pc.RpcTeleport(Utils.GetPlayerById(VotingPlayerId).GetPaintBattleLocation());
                }
                PaintBattleVotingTime -= Time.fixedDeltaTime;
                if (PaintBattleVotingTime <= 0f)
                {
                    PaintBattleVotingTime = Options.VotingTime.GetInt();
                    ++VotingPlayerId;
                    if (VotingPlayerId > 14)
                        Utils.EndPaintBattleGame();
                    while (Utils.GetPlayerById(VotingPlayerId) == null)
                    {
                        ++VotingPlayerId;
                        if (VotingPlayerId > 14)
                        {
                            Utils.EndPaintBattleGame();
                            break;
                        }
                    }
                    Utils.SendChat("Rate " + Main.StandardNames[VotingPlayerId] + "'s paint by typing 1-10 in chat!", "Voting");
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        HasVoted[pc.PlayerId] = false;
                }
            }
        }

        public PaintBattleGamemode()
        {
            Gamemode = Gamemodes.PaintBattle;
            PetAction = true;
            DisableTasks = true;
            PaintTime = 0f;
            IsPaintActive = false;
            VotingPlayerId = 0;
            PaintBattleVotingTime = 0f;
            HasVoted = new System.Collections.Generic.Dictionary<byte, bool>();
            PlayerVotes = new System.Collections.Generic.Dictionary<byte, (int, int)>();
            Theme = "";
            CreateBodyCooldown = new System.Collections.Generic.Dictionary<byte, float>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                HasVoted[pc.PlayerId] = false;
                PlayerVotes[pc.PlayerId] = (0, 0);
                CreateBodyCooldown[pc.PlayerId] = 0f;
            }
        }

        public static PaintBattleGamemode instance;
        public float PaintTime;
        public bool IsPaintActive;
        public byte VotingPlayerId;
        public float PaintBattleVotingTime;
        public System.Collections.Generic.Dictionary<byte, bool> HasVoted;
        public System.Collections.Generic.Dictionary<byte, (int, int)> PlayerVotes;
        public string Theme;
        public System.Collections.Generic.Dictionary<byte, float> CreateBodyCooldown;
    }
}