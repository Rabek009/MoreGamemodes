using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace MoreGamemodes
{
    public class BombTagGamemode : CustomGamemode
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
            else if (HasBomb(__instance.HauntTarget))
                __instance.FilterText.text = "Has Bomb";
            else
               __instance.FilterText.text = "Hasn't Bomb"; 
        }

        public override void OnHudUpdate(HudManager __instance)
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
            if (HasBomb(player) && !player.Data.IsDead)
            {
                __instance.KillButton.ToggleVisible(true);
                __instance.KillButton.OverrideText("Bomb");
                if (__instance.KillButton.currentTarget != null && HasBomb(__instance.KillButton.currentTarget))
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
            else if (HasBomb(player))
                __instance.taskText.text = Utils.ColorString(Color.black, "You have bomb!\nGive your bomb away.");
            else
                __instance.taskText.text = Utils.ColorString(Color.green, "You haven't bomb!\nDon't get bomb.");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnToggleHighlight(PlayerControl __instance)
        {
            __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.gray);
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Bomb Tag";
            __instance.TeamTitle.color = Color.green;
            __instance.BackgroundBar.material.color = Color.green;
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (HasBomb(PlayerControl.LocalPlayer))
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
            var players = AllPlayers.Count;
            var bombs = System.Math.Max(System.Math.Min((int)(players * Options.PlayersWithBomb.GetInt() / 100f), Options.MaxPlayersWithBomb.GetInt()), 1);
            if (bombs == 0)
                bombs = 1;
            for (int i = 0; i < bombs; ++i)
            {
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                SendRPC(player, true);
                AllPlayers.Remove(player);
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (HasBomb(pc))
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
            if (HasBomb(killer) && !HasBomb(target))
            {
                SendRPC(killer, false);
                SendRPC(target, true);
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

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.ExplosionDelay.GetInt())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (HasBomb(pc) && !pc.Data.IsDead)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Bombed);
                        pc.RpcMurderPlayer(pc, true);
                        Utils.RpcCreateExplosion(3f, 1f, Options.BtExplosionCreatesHole.GetBool(), Options.BtHoleSpeedDecrease.GetInt(), pc.transform.position);
                    }
                }
                Main.Timer = 0f;
            }
            bool bombedExists = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (HasBomb(pc) && !pc.Data.IsDead)
                    bombedExists = true;
            }
            if (!bombedExists)
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
                var players = AllPlayers.Count;
                var bombs = System.Math.Max(System.Math.Min((int)(players * Options.PlayersWithBomb.GetInt() / 100f), Options.MaxPlayersWithBomb.GetInt()), 1);
                if (bombs == 0)
                    bombs = 1;
                for (int i = 0; i < bombs; ++i)
                {
                    var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                    SendRPC(player, true);
                    player.RpcSetColor(6);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.black;
                    player.SyncPlayerSettings();
                    AllPlayers.Remove(player);
                }
                if (Options.TeleportAfterExplosion.GetBool())
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
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, Options.ExplosionDelay.GetInt() + 0.1f);
            opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
            opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
            opt.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
            if (!HasBomb(player))
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (HasBomb(player) && Options.ArrowToNearestNonBombed.GetBool() && player == seer && GetClosestNonBombed(player) != null && !player.Data.IsDead)
                name += "\n" + Utils.ColorString(Color.green, Utils.GetArrow(player.transform.position, GetClosestNonBombed(player).transform.position));
            return name;
        }

        public void SendRPC(PlayerControl player, bool hasBomb)
        {
            if (PlayerHasBomb[player.PlayerId] == hasBomb) return;
            PlayerHasBomb[player.PlayerId] = hasBomb;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write(hasBomb);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(PlayerControl player, MessageReader reader)
        {
            PlayerHasBomb[player.PlayerId] = reader.ReadBoolean();
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public bool HasBomb(PlayerControl player)
        {
            if (player == null) return false;
            if (!PlayerHasBomb.ContainsKey(player.PlayerId)) return false;
            return PlayerHasBomb[player.PlayerId];
        }

        public PlayerControl GetClosestNonBombed(PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && !HasBomb(p))
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

        public BombTagGamemode()
        {
            Gamemode = Gamemodes.BombTag;
            PetAction = false;
            DisableTasks = true;
            PlayerHasBomb = new Dictionary<byte, bool>();
            foreach (var pc in PlayerControl.AllPlayerControls)
                PlayerHasBomb[pc.PlayerId] = false;
        }

        public static BombTagGamemode instance;
        public Dictionary<byte, bool> PlayerHasBomb;
    }
}