using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class ZombiesGamemode : CustomGamemode
    {
        public override void OnExile(GameData.PlayerInfo exiled)
        {
            Main.Timer = 0f;
            Utils.SyncAllSettings();
        }
        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead && __instance.HauntTarget.IsZombie())
                __instance.FilterText.text = "Zombie Ghost";
            else if (__instance.HauntTarget.Data.IsDead && __instance.HauntTarget.Data.Role.IsImpostor)
                __instance.FilterText.text = "Impostor Ghost";
            else if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Crewmate Ghost";
            else if (__instance.HauntTarget.IsZombie())
                __instance.FilterText.text = "Zombie";
            else if (__instance.HauntTarget.Data.Role.IsImpostor)
                __instance.FilterText.text = "Impostor";
            else
                __instance.FilterText.text = "Crewmate";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            if ((PlayerControl.LocalPlayer.GetZombieType() == ZombieTypes.FullZombie || (!PlayerControl.LocalPlayer.IsZombie() && PlayerControl.LocalPlayer.KillsRemain() > 0)) && GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod) > 0f)
            {
                __instance.PetButton.ToggleVisible(true);
                __instance.PetButton.OverrideText("Kill");
            }
            else
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
            if (PlayerControl.LocalPlayer.IsZombie())
            {
                __instance.ReportButton.SetDisabled();
                __instance.ReportButton.ToggleVisible(false);
            }
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.SabotageButton.SetDisabled();
                __instance.SabotageButton.ToggleVisible(false);
            }
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            if (PlayerControl.LocalPlayer.IsZombie())
                __instance.taskText.text = Utils.ColorString(Palette.PlayerColors[2], "Zombie\nHelp impostor by killing crewmates.");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnVotingComplete()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetZombieType() == ZombieTypes.JustTurned)
                    pc.RpcSetZombieType(ZombieTypes.FullZombie);
                if (pc.IsZombie() && pc.GetZombieType() != ZombieTypes.Dead)
                {
                    pc.Data.IsDead = false;
                }  
            }
            Utils.SendGameData();
            new LateTask(() =>{
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsZombie() && pc.GetZombieType() != ZombieTypes.Dead)
                    {
                        var rand = new System.Random();
                        pc.MyPhysics.RpcBootFromVent(rand.Next(0, ShipStatus.Instance.AllVents.Count));
                        pc.RpcShapeshift(pc, false);
                    }
                }
            }, 5f, "Fix pet & Make visible");
        }

        public override bool OnCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            var voter = Utils.GetPlayerById(srcPlayerId);
            var target = Utils.GetPlayerById(suspectPlayerId);
            if (voter.IsZombie())
            {
                voter.RpcSendMessage("You can't vote as zombie!", "Warning");
                return false;
            }
            if (target.IsZombie())
            {
                voter.RpcSendMessage("You can't vote for zombie. Zombies can't be ejected!", "Warning");
                return false;
            }
            return true;
        }

        public override void OnPet(PlayerControl pc)
        {
            if (pc.GetZombieType() == ZombieTypes.FullZombie && Main.Timer >= Options.ZombieBlindTime.GetFloat())
            {
                var nearest = pc.GetClosestPlayer();
                if (Vector2.Distance(pc.transform.position, nearest.transform.position) <= 2f && !nearest.IsZombie() && !nearest.Data.Role.IsImpostor)
                {
                    if (Options.ZombieKillsTurnIntoZombie.GetBool())
                    {
                        pc.RpcTeleport(nearest.transform.position);
                        int alivePlayers = 0;
                        int impostors = 0;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (!player.Data.IsDead && !player.IsZombie()) ++alivePlayers;
                            if (!player.Data.IsDead && player.Data.Role.IsImpostor) ++impostors;
                        }
                        if (impostors * 2 < alivePlayers - 1 || Options.NoGameEnd.GetBool())
                            Utils.RpcCreateDeadBody(nearest.transform.position, Main.StandardColors[nearest.PlayerId], nearest);
                        nearest.RpcSetZombieType(ZombieTypes.JustTurned);
                        nearest.RpcSetOutfit(18, "", "", Main.StandardPets[nearest.PlayerId], "");
                        GameData.Instance.RpcSetTasks(nearest.PlayerId, new byte[0]);
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player.Data.Role.IsImpostor)
                                Main.NameColors[(player.PlayerId, nearest.PlayerId)] = Color.red;
                            Main.NameColors[(nearest.PlayerId, player.PlayerId)] = Palette.PlayerColors[2];
                        }
                        nearest.SyncPlayerSettings();
                    }
                    else
                        pc.RpcFixedMurderPlayer(nearest);
                }
            }
            else if (!pc.IsZombie() && pc.KillsRemain() > 0)
            {
                var nearest = pc.GetClosestZombie();
                if (Vector2.Distance(pc.transform.position, nearest.transform.position) <= 2f)
                {
                    pc.RpcFixedMurderPlayer(nearest);
                    nearest.RpcSetZombieType(ZombieTypes.Dead);
                    pc.RpcSetKillsRemain(pc.KillsRemain() - 1);
                }
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (target.IsZombie()) return false;
            killer.RpcSetKillTimer(Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown));
            killer.RpcTeleport(target.transform.position);
            int alivePlayers = 0;
            int impostors = 0;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead && !player.IsZombie()) ++alivePlayers;
                if (!player.Data.IsDead && player.Data.Role.IsImpostor) ++impostors;
            }
            if (impostors * 2 < alivePlayers - 1 || Options.NoGameEnd.GetBool())
                Utils.RpcCreateDeadBody(target.transform.position, Main.StandardColors[target.PlayerId], target);
            target.RpcSetZombieType(ZombieTypes.JustTurned);
            target.RpcSetOutfit(18, "", "", Main.StandardPets[target.PlayerId], "");
            GameData.Instance.RpcSetTasks(target.PlayerId, new byte[0]);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    Main.NameColors[(pc.PlayerId, target.PlayerId)] = Color.red;
                Main.NameColors[(target.PlayerId, pc.PlayerId)] = Palette.PlayerColors[2];
            }
            target.SyncPlayerSettings();
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, GameData.PlayerInfo target)
        {
            if (__instance.IsZombie())
                return false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsZombie())
                    pc.Data.IsDead = true;
            }
            Utils.SendGameData();
            return true;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.ZombieBlindTime.GetFloat() && Main.Timer < Options.ZombieBlindTime.GetFloat() + 1f && Main.GameStarted)
            {
                Utils.SyncAllSettings();
                Main.Timer += 1f;
            }
        }

        public override void OnCompleteTask(PlayerControl __instance)
        {
            if (Options.CanKillZombiesAfterTasks.GetBool() && __instance.AllTasksCompleted() && !__instance.IsZombie())
                __instance.RpcSetKillsRemain(Options.NumberOfKills.GetInt());
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
        {
            if (systemType == SystemTypes.Sabotage)
                return false;
            return true;
        }

        public ZombiesGamemode()
        {
            Gamemode = Gamemodes.Zombies;
            PetAction = true;
            DisableTasks = false;
            ZombieType = new Dictionary<byte, ZombieTypes>();
            KillsRemain = new Dictionary<byte, int>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                ZombieType[pc.PlayerId] = ZombieTypes.None;
                KillsRemain[pc.PlayerId] = 0;
            }
        }

        public static ZombiesGamemode instance;
        public Dictionary<byte, ZombieTypes> ZombieType;
        public Dictionary<byte, int> KillsRemain;
    }

    public enum TrackingZombiesModes
    {
        None = 0,
        Nearest,
        Every,
        All = int.MaxValue,
    }

    public enum ZombieTypes
    {
        None,
        JustTurned,
        FullZombie,
        Dead,
    }
}