using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class ZombiesGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (exiled != null && exiled.Object != null && Options.EjectedPlayersAreZombies.GetBool() && !Main.StandardRoles[exiled.PlayerId].IsImpostor())
            {
                exiled.Object.RpcSetZombieType(ZombieTypes.FullZombie);
                new LateTask(() => exiled.Object.RpcSetRoleV2(RoleTypes.Crewmate), 0.5f);
                new LateTask(() => exiled.Object.RpcSetDesyncRole(RoleTypes.Impostor, exiled.Object), 1f);
                exiled.Object.RpcSetOutfit(18, "", "", "", "");
                exiled.RpcSetTasks(new byte[0]);
                if (!AntiCheat.ChangedTasks.Contains(exiled.PlayerId))
                    AntiCheat.ChangedTasks.Add(exiled.PlayerId);
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
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.SabotageButton.SetDisabled();
                __instance.SabotageButton.ToggleVisible(false);
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
                    pc.Data.IsDead = false;
            }
            Utils.SendGameData();
            if (exiled != null && exiled.Object != null && !Main.StandardRoles[exiled.PlayerId].IsImpostor() && exiled.Object.KillsRemain() > 0)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                        pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], exiled.Object);
                }
                Main.NameColors[(exiled.PlayerId, exiled.PlayerId)] = Color.clear;
            }
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

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (killer.IsZombie() && killer.GetZombieType() != ZombieTypes.FullZombie)
                return false;
            if (target.IsZombie() && (Main.StandardRoles[killer.PlayerId].IsImpostor() || killer.IsZombie()))
                return false;
            if (killer.IsZombie() && Main.Timer < Options.ZombieBlindTime.GetFloat())
                return false;
            if (target.IsZombie() && target.GetZombieType() != ZombieTypes.FullZombie)
                return false;
            if (!Main.StandardRoles[killer.PlayerId].IsImpostor() && !target.IsZombie() && !killer.IsZombie())
                return false;
            if (!Main.StandardRoles[killer.PlayerId].IsImpostor() && killer.KillsRemain() <= 0 && !killer.IsZombie())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                        pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], killer);
                }
                Main.NameColors[(killer.PlayerId, killer.PlayerId)] = Color.clear;
                return false;
            }
            return true;
        }

        public override void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (Main.StandardRoles[killer.PlayerId].IsImpostor() || (killer.IsZombie() && Options.ZombieKillsTurnIntoZombie.GetBool()))
            {
                target.RpcSetZombieType(ZombieTypes.JustTurned);
                new LateTask(() => target.RpcSetRoleV2(RoleTypes.Crewmate), 0.5f);
                new LateTask(() => target.RpcSetDesyncRole(RoleTypes.Impostor, target), 1f);
                target.RpcSetOutfit(18, "", "", "", "");
                target.Data.RpcSetTasks(new byte[0]);
                if (!AntiCheat.ChangedTasks.Contains(target.PlayerId))
                    AntiCheat.ChangedTasks.Add(target.PlayerId);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.StandardRoles[pc.PlayerId].IsImpostor())
                        Main.NameColors[(pc.PlayerId, target.PlayerId)] = Color.red;
                    Main.NameColors[(target.PlayerId, pc.PlayerId)] = Palette.PlayerColors[2];
                }
                target.SyncPlayerSettings();
            }
            if (!Main.StandardRoles[killer.PlayerId].IsImpostor() && target.IsZombie())
            {
                target.RpcSetZombieType(ZombieTypes.Dead);
                killer.RpcSetKillsRemain(killer.KillsRemain() - 1);
                if (killer.KillsRemain() <= 0)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                            pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], killer);
                    }
                    Main.NameColors[(killer.PlayerId, killer.PlayerId)] = Color.clear;
                }
            }
            if (!Main.StandardRoles[target.PlayerId].IsImpostor() && target.KillsRemain() <= 0)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                        pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], target);
                }
                Main.NameColors[(target.PlayerId, target.PlayerId)] = Color.clear;
            }
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (target.IsZombie()) return false;
            return true;
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
            {
                __instance.RpcSetKillsRemain(Options.NumberOfKills.GetInt());
                __instance.RpcSetDesyncRole(RoleTypes.Impostor, __instance);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.StandardRoles[pc.PlayerId].IsImpostor() && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Crewmate, __instance);
                }
                Main.NameColors[(__instance.PlayerId, __instance.PlayerId)] = Color.white;
            }
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage)
                return false;
            return true;
        }

        public ZombiesGamemode()
        {
            Gamemode = Gamemodes.Zombies;
            PetAction = false;
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