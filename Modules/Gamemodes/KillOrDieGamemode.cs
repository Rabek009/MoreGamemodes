using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using static MoreGamemodes.Translator;

namespace MoreGamemodes
{
    public class KillOrDieGamemode : CustomGamemode
    {
        public override void OnExile(GameData.PlayerInfo exiled)
        {
            Main.Timer = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcResetAbilityCooldown();
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = GetString("Ghost");
            else if (__instance.HauntTarget.IsKiller())
                __instance.FilterText.text = GetString("Killer");
            else
                __instance.FilterText.text = GetString("Runner");
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
            if (player.IsKiller() && !player.Data.IsDead)
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
                __instance.taskText.text = Utils.ColorString(Color.red, GetString("You're dead. Enjoy the chaos."));
            else if (player.IsKiller())
                __instance.taskText.text = Utils.ColorString(Color.red, GetString("Killer\nKill someone before timer runs out."));
            else
                __instance.taskText.text = Utils.ColorString(Color.blue, GetString("Runner\nEscape from the killer."));
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

        public override void OnBeginImpostor(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = GetString("Kill or die");
            __instance.TeamTitle.color = Color.blue;
            __instance.BackgroundBar.material.color = Color.blue;
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.IsKiller())
            {
                __instance.RoleText.text = GetString("Killer");
                __instance.RoleText.color = Color.red;
                __instance.RoleBlurbText.text = GetString("Kill someone");
                __instance.RoleBlurbText.color = Color.red;
                __instance.YouAreText.color = Color.clear;
            }
            else
            {
                __instance.RoleText.text = GetString("Survivor");
                __instance.RoleText.color = Color.blue;
                __instance.RoleBlurbText.text = GetString("Escape from killer");
                __instance.RoleBlurbText.color = Color.blue;
                __instance.YouAreText.color = Color.clear;
            }
        }

        public override void OnSelectRolesPrefix()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner)
                    pc.SetRole(RoleTypes.Shapeshifter);
                else
                    pc.SetRole(RoleTypes.Crewmate);
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {   
                if (!pc.AmOwner)
                {
                    pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, pc.GetClientId());
                    foreach (var ar in PlayerControl.AllPlayerControls)
                    {
                        if (pc != ar)
                            ar.RpcSetDesyncRole(RoleTypes.Crewmate, pc.GetClientId());
                    }
                }
            }
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
            player.RpcSetColor(0);
            foreach (var pc in PlayerControl.AllPlayerControls)
                Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.red;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsKiller())
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
            if (!killer.IsKiller() || Main.Timer < Options.KillerBlindTime.GetFloat()) return false;
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

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, GameData.PlayerInfo target)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.KillerBlindTime.GetFloat() && Main.Timer < Options.KillerBlindTime.GetFloat() + 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsKiller())
                        pc.SyncPlayerSettings();
                }
                Main.Timer += 1f;
            }
            if (Main.Timer >= Options.TimeToKill.GetInt() + Options.KillerBlindTime.GetFloat() + 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsKiller() && !pc.Data.IsDead)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Suicide);
                        pc.RpcMurderPlayer(pc, true);
                    }   
                }
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

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public KillOrDieGamemode()
        {
            Gamemode = Gamemodes.KillOrDie;
            PetAction = false;
            DisableTasks = true;
            IsKiller = new Dictionary<byte, bool>();
            foreach (var pc in PlayerControl.AllPlayerControls)
                IsKiller[pc.PlayerId] = false;
        }

        public static KillOrDieGamemode instance;
        public Dictionary<byte, bool> IsKiller;
    }
}