using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using System.Linq;

namespace MoreGamemodes
{
    public class ColorWarsGamemode : CustomGamemode
    {
        public override void OnHudUpdate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            if (!IsDead[player.PlayerId] && !player.Data.IsDead && GetTeam(player) != byte.MaxValue)
            {
                if (__instance.KillButton.currentTarget != null && GetTeam(__instance.KillButton.currentTarget) == byte.MaxValue && IsLeader(player))
                    __instance.KillButton.OverrideText("Recruit");
                else if (__instance.KillButton.currentTarget != null && IsLeader(__instance.KillButton.currentTarget))
                    __instance.KillButton.OverrideText("Attack");
                else
                    __instance.KillButton.OverrideText("Kill");
                __instance.KillButton.ToggleVisible(true);
                if (__instance.KillButton.currentTarget != null && GetTeam(__instance.KillButton.currentTarget) == GetTeam(player))
                    __instance.KillButton.SetTarget(null);
                if (__instance.KillButton.currentTarget != null && GetTeam(__instance.KillButton.currentTarget) == byte.MaxValue && !IsLeader(player))
                    __instance.KillButton.SetTarget(null);
            }
            else if (IsDead[player.PlayerId])
            {
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead && IsLeader(__instance.HauntTarget))
                __instance.FilterText.text = "Leader Ghost";
            else if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Ghost";
            else if (IsLeader(__instance.HauntTarget))
                __instance.FilterText.text = "Leader";
            else
                __instance.FilterText.text = "Player";
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.IsDead)
                __instance.taskText.text = Utils.ColorString(Color.red, "You're dead. Enjoy the chaos.");
            else if (IsLeader(player))
                __instance.taskText.text = Utils.ColorString(Palette.PlayerColors[GetTeam(player)], "Leader:\nRecruit players, survive and kill other leaders.");
            else
                __instance.taskText.text = Utils.ColorString(Palette.PlayerColors[GetTeam(player)], "Player:\nProtect leader and kill enemy leaders.");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            if (IsLeader(PlayerControl.LocalPlayer))
            {
                __instance.TeamTitle.text = "Leader";
                __instance.TeamTitle.color = Palette.PlayerColors[GetTeam(PlayerControl.LocalPlayer)];
                __instance.BackgroundBar.material.color = Palette.PlayerColors[GetTeam(PlayerControl.LocalPlayer)];
            }
            else
            {
                __instance.TeamTitle.text = "Player";
                __instance.TeamTitle.color = Color.gray;
                __instance.BackgroundBar.material.color = Color.gray;
            }
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (IsLeader(PlayerControl.LocalPlayer))
            {
                __instance.RoleText.text = "Leader";
                __instance.RoleText.color = Palette.PlayerColors[GetTeam(PlayerControl.LocalPlayer)];
                __instance.RoleBlurbText.text = "Recruit players and kill other leaders";
                __instance.RoleBlurbText.color = Palette.PlayerColors[GetTeam(PlayerControl.LocalPlayer)];
                __instance.YouAreText.color = Palette.PlayerColors[GetTeam(PlayerControl.LocalPlayer)];
            }
            else
            {
                __instance.RoleText.text = "No Team";
                __instance.RoleText.color = Color.gray;
                __instance.RoleBlurbText.text = "Get recruited by leader";
                __instance.RoleBlurbText.color = Color.gray;
                __instance.YouAreText.color = Color.clear;
            }
        }

        public override bool OnSelectRolesPrefix()
        {
            Utils.RpcSetDesyncRoles(RoleTypes.Impostor, RoleTypes.Crewmate);
            return false;
        }

        public override void OnSelectRolesPostfix()
        {
            var rand = new System.Random();
            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead)
                    AllPlayers.Add(pc);
            }
            byte leaders = 0;
            while (AllPlayers.Any() && leaders < Options.LeadersAmount.GetInt())
            {
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                SendRPC(player, leaders, true, IsDead[player.PlayerId]);
                player.RpcSetColor(leaders);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(player.PlayerId, pc.PlayerId)] = Palette.PlayerColors[leaders];
                Lives[player.PlayerId] = Options.LeaderLives.GetInt();
                AllPlayers.Remove(player);
                ++leaders;
            }
            foreach (var player in AllPlayers)
            {
                player.RpcSetColor(15);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.gray;
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < Options.CwGracePeriod.GetFloat())
                return false;         
            if (IsDead[killer.PlayerId] || IsDead[target.PlayerId])
                return false;
            if (GetTeam(killer) == byte.MaxValue || GetTeam(killer) == GetTeam(target))
                return false;
            if (GetTeam(target) == byte.MaxValue && !IsLeader(killer))
                return false;
            if (GetTeam(target) == byte.MaxValue)
            {
                SendRPC(target, GetTeam(killer), false, IsDead[target.PlayerId]);
                target.RpcSetColor(GetTeam(killer));
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(target.PlayerId, pc.PlayerId)] = Palette.PlayerColors[GetTeam(killer)];
                target.SyncPlayerSettings();
                killer.RpcSetKillTimer(Options.LeaderCooldown.GetFloat());
                return false;
            }
            if (IsLeader(target))
            {
                --Lives[target.PlayerId];
                if (Lives[target.PlayerId] <= 0)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc != target && GetTeam(pc) == GetTeam(target) && !pc.Data.IsDead)
                        {
                            if (IsDead[pc.PlayerId])
                            {
                                SendRPC(pc, GetTeam(pc), IsLeader(pc), false);
                                pc.RpcTeleport(PositionBeforeDeath[pc.PlayerId]);
                            }
                            pc.RpcSetDeathReason(DeathReasons.Suicide);
                            pc.RpcMurderPlayer(pc, true);
                            pc.SyncPlayerSettings();
                        }
                    }
                    return true;
                }
                killer.RpcSetKillTimer(IsLeader(killer) ? Options.LeaderCooldown.GetFloat() : Options.PlayerKillCooldown.GetFloat());
                return false;
            }
            if (Options.PlayerCanRespawn.GetBool())
            {
                PositionBeforeDeath[target.PlayerId] = target.transform.position;
                target.RpcTeleport(Utils.GetOutsideMapPosition());
                SendRPC(target, GetTeam(target), IsLeader(target), true);
                RespawnCooldown[target.PlayerId] = Options.CwRespawnCooldown.GetFloat();
                target.SyncPlayerSettings();
                ++Main.PlayerKills[killer.PlayerId];
                killer.RpcSetKillTimer(IsLeader(killer) ? Options.LeaderCooldown.GetFloat() : Options.PlayerKillCooldown.GetFloat());
                return false;
            }
            return true;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsDead[pc.PlayerId])
                {
                    if (RespawnCooldown[pc.PlayerId] > 0f)
                    {
                        RespawnCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (RespawnCooldown[pc.PlayerId] <= 0f)
                    {
                        RespawnCooldown[pc.PlayerId] = 0f;
                        SendRPC(pc, GetTeam(pc), IsLeader(pc), false);
                        pc.RpcTeleport(PositionBeforeDeath[pc.PlayerId]);
                        pc.SyncPlayerSettings();
                    }   
                }
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            return false;
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override void OnDisconnect(PlayerControl player)
        {
            if (IsLeader(player))
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != player && GetTeam(pc) == GetTeam(player))
                    {
                        SendRPC(pc, byte.MaxValue, false, IsDead[pc.PlayerId]);
                        pc.RpcSetColor(15);
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.gray;
                        pc.SyncPlayerSettings();
                    }
                }
            }
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
            if (IsLeader(player))
                opt.SetFloat(FloatOptionNames.KillCooldown, Options.LeaderCooldown.GetFloat());
            else
                opt.SetFloat(FloatOptionNames.KillCooldown, Options.PlayerKillCooldown.GetFloat());
            if (GetTeam(player) == byte.MaxValue)
            {
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, Options.NonTeamSpeed.GetFloat());
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, Options.NonTeamVision.GetFloat());
            }
            if (IsDead[player.PlayerId])
            {
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
            }
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            var livesText = "";
            if (GetLives(player) <= 5)
            {
                for (int i = 1; i <= GetLives(player); i++)
                    livesText += "♥";
            }
            else
                livesText = "Lives: " + GetLives(player);
            livesText = Utils.ColorString(Color.red, livesText);
            if (IsLeader(player))
                name = Utils.ColorString(Color.yellow, "★") + name;
            if (Options.ArrowToLeader.GetBool() && player == seer && !IsLeader(player) && GetLeader(player) != null && !player.Data.IsDead)
                name += Utils.ColorString(Palette.PlayerColors[GetTeam(player)], Utils.GetArrow(player.transform.position, GetLeader(player).transform.position));
            if (Options.ArrowToNearestEnemyLeader.GetBool() && player == seer && GetTeam(player) != byte.MaxValue && GetClosestEnemyLeader(player) != null && !player.Data.IsDead)
                name += Utils.ColorString(Palette.PlayerColors[GetTeam(GetClosestEnemyLeader(player))], Utils.GetArrow(player.transform.position, GetClosestEnemyLeader(player).transform.position));
            if (IsLeader(player) && (player == seer || GetTeam(player) == GetTeam(seer) || Options.LivesVisibleToEnemies.GetBool() || seer.Data.IsDead))
                name += "\n" + livesText;
            return name;
        }

        public void SendRPC(PlayerControl player, byte team, bool isLeader, bool isDead)
        {
            if (Team[player.PlayerId] == team && PlayerIsLeader[player.PlayerId] == isLeader && IsDead[player.PlayerId] == isDead) return;
            Team[player.PlayerId] = team;
            PlayerIsLeader[player.PlayerId] = isLeader;
            IsDead[player.PlayerId] = isDead;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write(team);
            writer.Write(isLeader);
            writer.Write(isDead);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(PlayerControl player, MessageReader reader)
        {
            Team[player.PlayerId] = reader.ReadByte();
            PlayerIsLeader[player.PlayerId] = reader.ReadBoolean();
            IsDead[player.PlayerId] = reader.ReadBoolean();
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public byte GetTeam(PlayerControl player)
        {
            if (player == null) return byte.MaxValue;
            if (!Team.ContainsKey(player.PlayerId)) return byte.MaxValue;
            return Team[player.PlayerId];
        }

        public bool IsLeader(PlayerControl player)
        {
            if (player == null) return false;
            if (!PlayerIsLeader.ContainsKey(player.PlayerId)) return false;
            return PlayerIsLeader[player.PlayerId];
        }

        public int GetLives(PlayerControl player)
        {
            if (player == null) return 0;
            if (!Lives.ContainsKey(player.PlayerId)) return 0;
            return Lives[player.PlayerId];
        }
        
        public PlayerControl GetLeader(PlayerControl player)
        {
            if (IsLeader(player)) return player;
            if (GetTeam(player) == byte.MaxValue) return null;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsLeader(pc) && GetTeam(pc) == GetTeam(player))
                    return pc;
            }
            return null;
        }

        public PlayerControl GetClosestEnemyLeader(PlayerControl player)
        {
            if (GetTeam(player) == byte.MaxValue) return null;
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && IsLeader(p) && GetTeam(p) != GetTeam(player))
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            if (pcdistance.Count == 0) return null;
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

        public ColorWarsGamemode()
        {
            Gamemode = Gamemodes.ColorWars;
            PetAction = false;
            DisableTasks = true;
            Team = new Dictionary<byte, byte>();
            PlayerIsLeader = new Dictionary<byte, bool>();
            Lives = new Dictionary<byte, int>();
            IsDead = new Dictionary<byte, bool>();
            RespawnCooldown = new Dictionary<byte, float>();
            PositionBeforeDeath = new Dictionary<byte, Vector2>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Team[pc.PlayerId] = byte.MaxValue;
                PlayerIsLeader[pc.PlayerId] = false;
                Lives[pc.PlayerId] = 0;
                IsDead[pc.PlayerId] = false;
                RespawnCooldown[pc.PlayerId] = 0f;
                PositionBeforeDeath[pc.PlayerId] = Vector2.zero;
            }
        }

        public static ColorWarsGamemode instance;
        public Dictionary<byte, byte> Team;
        public Dictionary<byte, bool> PlayerIsLeader;
        public Dictionary<byte, int> Lives;
        public Dictionary<byte, bool> IsDead;
        public Dictionary<byte, float> RespawnCooldown;
        public Dictionary<byte, Vector2> PositionBeforeDeath;
    }
}