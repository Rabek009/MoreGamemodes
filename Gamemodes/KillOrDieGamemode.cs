using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace MoreGamemodes
{
    public class KillOrDieGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Main.Timer = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcResetAbilityCooldown();
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Ghost";
            else if (IsKiller(__instance.HauntTarget))
                __instance.FilterText.text = "Killer";
            else
                __instance.FilterText.text = "Runner";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            if (!player.Data.IsDead)
                __instance.AbilityButton.OverrideText("Timer");
            if (IsKiller(player) && !player.Data.IsDead)
            {
                __instance.KillButton.ToggleVisible(true);
            }
            else
            {
                __instance.KillButton.ToggleVisible(false);
                __instance.KillButton.SetTarget(null);
            }
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.IsDead)
                __instance.taskText.text = Utils.ColorString(Color.red, "You're dead. Enjoy the chaos.");
            else if (IsKiller(player))
                __instance.taskText.text = Utils.ColorString(Color.red, "Killer\nKill someone before timer runs out.");
            else
                __instance.taskText.text = Utils.ColorString(Color.blue, "Runner\nEscape from the killer.");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnShowNormalMap(MapBehaviour __instance)
        {
            __instance.taskOverlay.Hide();
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Kill or die";
            __instance.TeamTitle.color = Color.blue;
            __instance.BackgroundBar.material.color = Color.blue;
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (IsKiller(PlayerControl.LocalPlayer))
            {
                __instance.RoleText.text = "Killer";
                __instance.RoleText.color = Color.red;
                __instance.RoleBlurbText.text = "Kill someone";
                __instance.RoleBlurbText.color = Color.red;
                __instance.YouAreText.color = Color.clear;
            }
            else
            {
                __instance.RoleText.text = "Survivor";
                __instance.RoleText.color = Color.blue;
                __instance.RoleBlurbText.text = "Escape from killer";
                __instance.RoleBlurbText.color = Color.blue;
                __instance.YouAreText.color = Color.clear;
            }
        }

        public override bool OnSelectRolesPrefix()
        {
            Utils.RpcSetDesyncRoles(RoleTypes.Shapeshifter, RoleTypes.Crewmate);
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
            var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
            player.RpcSetIsKiller(true);
            player.SyncPlayerSettings();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsKiller(pc))
                {
                    pc.RpcSetColor(0);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.red;
                }   
                else
                {
                    pc.RpcSetColor(1);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.blue;
                }
            }
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcResetAbilityCooldown();
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (!IsKiller(killer) || Main.Timer < Options.KillerBlindTime.GetFloat()) return false;
            return true;
        }

        public override void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (killer == target) return;
            killer.RpcSetIsKiller(false);
            killer.RpcSetColor(1);
            foreach (var pc in PlayerControl.AllPlayerControls)
                Main.NameColors[(killer.PlayerId, pc.PlayerId)] = Color.blue;
            killer.SyncPlayerSettings();
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.KillerBlindTime.GetFloat() && Main.Timer < Options.KillerBlindTime.GetFloat() + 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (IsKiller(pc))
                        pc.SyncPlayerSettings();
                }
                Main.Timer += 1f;
            }
            if (Main.Timer >= Options.TimeToKill.GetInt() + Options.KillerBlindTime.GetFloat() + 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (IsKiller(pc) && !pc.Data.IsDead)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Suicide);
                        pc.RpcMurderPlayer(pc, true);
                    }   
                }
            }
            bool killerExists = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsKiller(pc) && !pc.Data.IsDead)
                    killerExists = true;
            }
            if (!killerExists)
            {
                var rand = new System.Random();
                List<PlayerControl> AllPlayers = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                    {
                        AllPlayers.Add(pc);
                        pc.RpcResetAbilityCooldown();
                    } 
                }
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                player.RpcSetIsKiller(true);
                player.RpcSetColor(0);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.red;
                Main.Timer = 0f;
                player.SyncPlayerSettings();
                if (Options.TeleportAfterRound.GetBool())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                            pc.RpcRandomVentTeleport();
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
            opt.SetFloat(FloatOptionNames.KillCooldown, 0.001f);
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, Options.TimeToKill.GetInt() + Options.KillerBlindTime.GetFloat() + 0.1f);
            opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
            opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
            opt.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
            if (!IsKiller(player))
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
            else if (Main.Timer < Options.KillerBlindTime.GetFloat())
            {
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            }
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (IsKiller(player) && Options.ArrowToNearestSurvivor.GetBool() && player == seer && GetClosestSurvivor(player) != null && !player.Data.IsDead)
                name += "\n" + Utils.ColorString(Color.blue, Utils.GetArrow(player.GetRealPosition(), GetClosestSurvivor(player).transform.position));
            return name;
        }

        public bool IsKiller(PlayerControl player)
        {
            if (player == null) return false;
            if (!IsPlayerKiller.ContainsKey(player.PlayerId)) return false;
            return IsPlayerKiller[player.PlayerId];
        }

        public PlayerControl GetClosestSurvivor(PlayerControl player)
        {
            Vector2 playerpos = player.GetRealPosition();
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && !IsKiller(p))
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

        public KillOrDieGamemode()
        {
            Gamemode = Gamemodes.KillOrDie;
            PetAction = false;
            DisableTasks = true;
            IsPlayerKiller = new Dictionary<byte, bool>();
            foreach (var pc in PlayerControl.AllPlayerControls)
                IsPlayerKiller[pc.PlayerId] = false;
        }

        public static KillOrDieGamemode instance;
        public Dictionary<byte, bool> IsPlayerKiller;
    }
}