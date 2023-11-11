using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class BombTagGamemode : CustomGamemode
    {
        public override void OnExile(GameData.PlayerInfo exiled)
        {
            Main.Timer = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcResetAbilityCooldown();
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.HasBomb())
                __instance.FilterText.text = "Has Bomb";
            else
               __instance.FilterText.text = "Hasn't Bomb"; 
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
                __instance.AbilityButton.OverrideText("Explosion");
            if (player.HasBomb() && !player.Data.IsDead)
            {
                __instance.KillButton.ToggleVisible(true);
                __instance.KillButton.OverrideText("Bomb");
                if (player.GetClosestPlayer(true) != null && player.GetClosestPlayer(true).HasBomb())
                    __instance.KillButton.SetTarget(null);
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
            else if (player.HasBomb())
                __instance.taskText.text = Utils.ColorString(Color.black, "You have bomb!\nGive your bomb away.");
            else
                __instance.taskText.text = Utils.ColorString(Color.green, "You haven't bomb!\nDon't get bomb.");
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

        public override void OnToggleHighlight(PlayerControl __instance)
        {
            if (PlayerControl.LocalPlayer.HasBomb())
            {
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.gray);
            }
        }

        public override void OnBeginImpostor(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Bomb Tag";
            __instance.TeamTitle.color = Color.green;
            __instance.BackgroundBar.material.color = Color.green;
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.HasBomb())
            {
                __instance.RoleText.text = "You have bomb";
                __instance.RoleText.color = Color.gray;
                __instance.RoleBlurbText.text = "Give your bomb away";
                __instance.RoleBlurbText.color = Color.gray;
                __instance.YouAreText.color = Color.clear;
            }
            else
            {
                __instance.RoleText.text = "You haven't bomb";
                __instance.RoleText.color = Color.green;
                __instance.RoleBlurbText.text = "Don't get bomb";
                __instance.RoleBlurbText.color = Color.green;
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
            var players = AllPlayers.Count;
            var bombs = System.Math.Max(System.Math.Min(players * Options.PlayersWithBomb.GetInt() / 100, Options.MaxPlayersWithBomb.GetInt()), 1);
            if (bombs == 0)
                bombs = 1;
            for (int i = 0; i < bombs; ++i)
            {
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                player.RpcSetBomb(true);
                player.RpcSetColor(6);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.black;
                AllPlayers.Remove(player);
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.HasBomb())
                {
                     pc.RpcSetColor(6);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.black;
                }   
                else
                {
                    pc.RpcSetColor(2);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.green;
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
            if (killer.HasBomb() && !target.HasBomb())
            {
                killer.RpcSetBomb(false);
                target.RpcSetBomb(true);
                killer.RpcSetColor(2);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(killer.PlayerId, pc.PlayerId)] = Color.green;
                target.RpcSetColor(6);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.black;
                killer.SyncPlayerSettings();
                target.SyncPlayerSettings();
            }
            return false;
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
            if (Main.Timer >= Options.ExplosionDelay.GetInt())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.HasBomb() && !pc.Data.IsDead)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Bombed);
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
                var players = AllPlayers.Count;
                var bombs = System.Math.Max(System.Math.Min(players * Options.PlayersWithBomb.GetInt() / 100, Options.MaxPlayersWithBomb.GetInt()), 1);
                if (bombs == 0)
                    bombs = 1;
                for (int i = 0; i < bombs; ++i)
                {
                    var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                    player.RpcSetBomb(true);
                    player.RpcSetColor(6);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.black;
                    AllPlayers.Remove(player);
                }
                Utils.SyncAllSettings();
                if (Options.TeleportAfterExplosion.GetBool())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                            pc.RpcRandomVentTeleport();
                    }
                }
                Main.Timer = 0f;
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

        public BombTagGamemode()
        {
            Gamemode = Gamemodes.BombTag;
            PetAction = false;
            HasBomb = new Dictionary<byte, bool>();
            foreach (var pc in PlayerControl.AllPlayerControls)
                HasBomb[pc.PlayerId] = false;
        }

        public static BombTagGamemode instance;
        public Dictionary<byte, bool> HasBomb;
    }
}