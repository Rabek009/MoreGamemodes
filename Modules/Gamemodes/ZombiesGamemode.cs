using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class ZombiesGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (exiled != null && Options.EjectedPlayersAreZombies.GetBool())
            {
                exiled.Object.RpcSetZombieType(ZombieTypes.FullZombie);
                new LateTask(() => exiled.Object.RpcSetDesyncRole(RoleTypes.Impostor, exiled.Object.GetClientId()), 0.5f);
                new LateTask(() => exiled.Object.RpcSetRoleV2(RoleTypes.Crewmate, false), 1f);
                exiled.Object.RpcSetOutfit(18, "", "", "", "");
                exiled.RpcSetTasks(new byte[0]);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.StandardRoles[pc.PlayerId].IsImpostor())
                        Main.NameColors[(pc.PlayerId, exiled.PlayerId)] = Color.red;
                    Main.NameColors[(exiled.PlayerId, pc.PlayerId)] = Palette.PlayerColors[2];
                }
            }
            Main.Timer = 0f;
            Utils.SyncAllSettings();
        }
        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead && __instance.HauntTarget.IsZombie())
                __instance.FilterText.text = "Zombie Ghost";
            else if (__instance.HauntTarget.Data.IsDead && Main.StandardRoles[__instance.HauntTarget.PlayerId].IsImpostor())
                __instance.FilterText.text = "Impostor Ghost";
            else if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Crewmate Ghost";
            else if (__instance.HauntTarget.IsZombie())
                __instance.FilterText.text = "Zombie";
            else if (Main.StandardRoles[__instance.HauntTarget.PlayerId].IsImpostor())
                __instance.FilterText.text = "Impostor";
            else
                __instance.FilterText.text = "Crewmate";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            if (!PlayerControl.LocalPlayer.IsZombie() && PlayerControl.LocalPlayer.KillsRemain() > 0)
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
                if (!Options.ZombiesCanVent.GetBool())
                {
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                }
                __instance.ReportButton.SetDisabled();
                __instance.ReportButton.ToggleVisible(false);
                __instance.SabotageButton.SetDisabled();
                __instance.SabotageButton.ToggleVisible(false);
            }
            if (Main.StandardRoles[PlayerControl.LocalPlayer.PlayerId].IsImpostor())
            {
                if (!Options.ZoImpostorsCanVent.GetBool())
                {
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                }
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

        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
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
                    if ((pc.IsZombie() && pc.GetZombieType() != ZombieTypes.Dead) || (exiled != null && pc.PlayerId == exiled.PlayerId && Options.EjectedPlayersAreZombies.GetBool()))
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
            if (voter == null || target == null) return true;
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
            if (!pc.IsZombie() && pc.KillsRemain() > 0)
            {
                var nearest = pc.GetClosestZombie();
                if (Vector2.Distance(pc.transform.position, nearest.transform.position) <= Main.RealOptions.GetInt(Int32OptionNames.KillDistance) + 1f)
                {
                    pc.RpcFixedMurderPlayer(nearest);
                    nearest.RpcSetZombieType(ZombieTypes.Dead);
                    pc.RpcSetKillsRemain(pc.KillsRemain() - 1);
                }
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target, bool __state)
        {
            if (killer.IsZombie() && killer.GetZombieType() != ZombieTypes.FullZombie)
                return false;
            if (target.IsZombie() && (Main.StandardRoles[killer.PlayerId].IsImpostor() || killer.IsZombie()))
                return false;
            if (killer.IsZombie() && Main.Timer < Options.ZombieBlindTime.GetFloat())
                return false;
            return true;
        }

        public override void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (Main.StandardRoles[killer.PlayerId].IsImpostor() || (killer.IsZombie() && Options.ZombieKillsTurnIntoZombie.GetBool()))
            {
                target.RpcSetZombieType(ZombieTypes.JustTurned);
                new LateTask(() => target.RpcSetDesyncRole(RoleTypes.Impostor, target.GetClientId()), 0.5f);
                new LateTask(() => target.RpcSetRoleV2(RoleTypes.Crewmate, false), 1f);
                target.RpcSetOutfit(18, "", "", "", "");
                target.Data.RpcSetTasks(new byte[0]);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.StandardRoles[pc.PlayerId].IsImpostor())
                        Main.NameColors[(pc.PlayerId, target.PlayerId)] = Color.red;
                    Main.NameColors[(target.PlayerId, pc.PlayerId)] = Palette.PlayerColors[2];
                }
                target.SyncPlayerSettings();
            }
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
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