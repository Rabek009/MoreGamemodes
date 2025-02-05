using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using System.Linq;
using System.Collections.Generic;
using System.Data;

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
            new LateTask(() => {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (IsZombie(pc) && GetZombieType(pc) != ZombieTypes.Dead && (exiled == null || exiled.Object == null || pc != exiled.Object))
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, pc);
                }
                Utils.SetAllVentInteractions();
            }, 1f);
        }
        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead && IsZombie(__instance.HauntTarget))
                __instance.FilterText.text = "Zombie Ghost";
            else if (__instance.HauntTarget.Data.IsDead && Main.StandardRoles[__instance.HauntTarget.PlayerId].IsImpostor())
                __instance.FilterText.text = "Impostor Ghost";
            else if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Crewmate Ghost";
            else if (IsZombie(__instance.HauntTarget))
                __instance.FilterText.text = "Zombie";
            else if (Main.StandardRoles[__instance.HauntTarget.PlayerId].IsImpostor())
                __instance.FilterText.text = "Impostor";
            else
                __instance.FilterText.text = "Crewmate";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (!IsZombie(player) && GetKillsRemain(player) > 0)
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.SabotageButton.SetDisabled();
                __instance.SabotageButton.ToggleVisible(false);
            }
            if (IsZombie(player))
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
            if (Main.StandardRoles[player.PlayerId].IsImpostor())
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
            if (IsZombie(PlayerControl.LocalPlayer))
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
                if (GetZombieType(pc) == ZombieTypes.JustTurned)
                    pc.RpcSetZombieType(ZombieTypes.FullZombie);
                if (IsZombie(pc) && GetZombieType(pc) != ZombieTypes.Dead)
                {
                    pc.Data.IsDead = false;
                    pc.Data.MarkDirty();
                    new LateTask(() => pc.RpcSetDesyncRole(RoleTypes.Crewmate, pc), 0.5f);
                }
            }
            if (exiled != null && exiled.Object != null && !Main.StandardRoles[exiled.PlayerId].IsImpostor() && GetKillsRemain(exiled.Object) > 0)
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
                    if ((IsZombie(pc) && GetZombieType(pc) != ZombieTypes.Dead) || (exiled != null && pc.PlayerId == exiled.PlayerId && Options.EjectedPlayersAreZombies.GetBool()))
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
            if (IsZombie(voter))
            {
                voter.RpcSendMessage("You can't vote as zombie!", "Warning");
                return false;
            }
            if (IsZombie(target))
            {
                voter.RpcSendMessage("You can't vote for zombie. Zombies can't be ejected!", "Warning");
                return false;
            }
            return true;
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (IsZombie(killer) && GetZombieType(killer) != ZombieTypes.FullZombie)
                return false;
            if (IsZombie(target) && (Main.StandardRoles[killer.PlayerId].IsImpostor() || IsZombie(killer)))
                return false;
            if (IsZombie(killer) && Main.Timer < Options.ZombieBlindTime.GetFloat())
                return false;
            if (IsZombie(target) && GetZombieType(target) != ZombieTypes.FullZombie)
                return false;
            if (!Main.StandardRoles[killer.PlayerId].IsImpostor() && !IsZombie(target) && !IsZombie(killer))
                return false;
            if (!Main.StandardRoles[killer.PlayerId].IsImpostor() && GetKillsRemain(killer) <= 0 && !IsZombie(killer))
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
            if (!Main.StandardRoles[killer.PlayerId].IsImpostor() && IsZombie(target))
            {
                target.RpcSetZombieType(ZombieTypes.Dead);
                killer.RpcSetKillsRemain(GetKillsRemain(killer) - 1);
                killer.RpcSetVentInteraction();
                if (GetKillsRemain(killer) <= 0)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                            pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], killer);
                    }
                    Main.NameColors[(killer.PlayerId, killer.PlayerId)] = Color.clear;
                }
            }
            if (!Main.StandardRoles[target.PlayerId].IsImpostor() && GetKillsRemain(target) > 0)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                        pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], target);
                }
                Main.NameColors[(target.PlayerId, target.PlayerId)] = Color.clear;
            }
            if (Main.StandardRoles[killer.PlayerId].IsImpostor() || (IsZombie(killer) && Options.ZombieKillsTurnIntoZombie.GetBool()))
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
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (IsZombie(target)) return false;
            return true;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            if (IsZombie(__instance))
                return false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsZombie(pc))
                {
                    pc.Data.IsDead = true;
                    pc.Data.MarkDirty();
                }
            }
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

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            return ((Main.StandardRoles[player.PlayerId].IsImpostor() && Options.ZoImpostorsCanVent.GetBool()) || (GetZombieType(player) == ZombieTypes.FullZombie && Options.ZombiesCanVent.GetBool()) || (Main.StandardRoles[player.PlayerId] == RoleTypes.Engineer && !IsZombie(player) && GetKillsRemain(player) <= 0)) && GameManager.Instance.LogicOptions.MapId != 3;
        }

        public override void OnCompleteTask(PlayerControl __instance)
        {
            if (Options.CanKillZombiesAfterTasks.GetBool() && __instance.AllTasksCompleted() && !IsZombie(__instance) && !__instance.Data.IsDead)
            {
                __instance.RpcSetKillsRemain(Options.NumberOfKills.GetInt());
                __instance.RpcSetDesyncRole(RoleTypes.Impostor, __instance);
                __instance.RpcSetVentInteraction();
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

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            if (Options.CanKillZombiesAfterTasks.GetBool())
                opt.RoleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
            if (IsZombie(player))
            {
                if (Main.Timer >= Options.ZombieBlindTime.GetFloat() && GetZombieType(player) != ZombieTypes.JustTurned)
                {
                    opt.SetFloat(FloatOptionNames.PlayerSpeedMod, Options.ZombieSpeed.GetFloat());
                    opt.SetFloat(FloatOptionNames.ImpostorLightMod, Options.ZombieVision.GetFloat());
                    opt.SetFloat(FloatOptionNames.CrewLightMod, Options.ZombieVision.GetFloat());
                }
                else
                {
                    opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                    opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                    opt.SetFloat(FloatOptionNames.CrewLightMod, 0f);
                }
                opt.SetFloat(FloatOptionNames.KillCooldown, 1f);
            }
            if (Main.StandardRoles.ContainsKey(player.PlayerId) && !Main.StandardRoles[player.PlayerId].IsImpostor() && !IsZombie(player))
            {
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
                opt.SetFloat(FloatOptionNames.KillCooldown, 1f);
            }
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (player == seer && !IsZombie(player) && !player.Data.IsDead)
            {
                if (Options.CurrentTrackingZombiesMode == TrackingZombiesModes.Nearest)
                {
                    var nearest = GetClosestZombie(player);
                    if (nearest != null)
                        name += "\n" + Utils.ColorString(Palette.PlayerColors[2], Utils.GetArrow(player.GetRealPosition(), nearest.transform.position)); 
                }
                else if (Options.CurrentTrackingZombiesMode == TrackingZombiesModes.Every && GetClosestZombie(player) != null)
                {
                    name += "\n";
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (GetZombieType(pc) == ZombieTypes.FullZombie && !pc.Data.IsDead)
                            name += Utils.ColorString(Palette.PlayerColors[2], Utils.GetArrow(player.GetRealPosition(), pc.transform.position));
                    }
                }
                if (GetKillsRemain(player) > 0)
                    name += "\n" + Utils.ColorString(Color.cyan, "YOU CAN KILL " + GetKillsRemain(player) + " " + (GetKillsRemain(player) == 1 ? "ZOMBIE" : "ZOMBIES") + "!");
            }
            if (player != seer && seer.Data.IsDead && !player.Data.IsDead && GetKillsRemain(player) > 0)
                name += "\n" + Utils.ColorString(Color.cyan, "CAN KILL " + GetKillsRemain(player) + " " + (GetKillsRemain(player) == 1 ? "ZOMBIE" : "ZOMBIES") + "!");
            return name;
        }

        public ZombieTypes GetZombieType(PlayerControl player)
        {
            if (player == null) return ZombieTypes.None;
            if (!ZombieType.ContainsKey(player.PlayerId)) return ZombieTypes.None;
            return ZombieType[player.PlayerId];
        }

        public bool IsZombie(PlayerControl player)
        {
            return GetZombieType(player) != ZombieTypes.None;
        }

        public int GetKillsRemain(PlayerControl player)
        {
            if (player == null) return 0;
            if (!KillsRemain.ContainsKey(player.PlayerId)) return 0;
            return KillsRemain[player.PlayerId];
        }

        public PlayerControl GetClosestZombie(PlayerControl player)
        {
            Vector2 playerpos = player.GetRealPosition();
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && GetZombieType(p) == ZombieTypes.FullZombie)
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