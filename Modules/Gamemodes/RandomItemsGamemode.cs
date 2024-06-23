using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class RandomItemsGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            NoItemTimer = 10f;
            if (exiled != null)
                exiled.Object.RpcSetItem(Items.None); 
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            base.OnSetFilterText(__instance);
            if (__instance.HauntTarget.GetItem() != Items.None)
                __instance.FilterText.text += " (" + Utils.ItemString(__instance.HauntTarget.GetItem()) + ")";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ReportButton.OverrideText(player.GetItem() == Items.Medicine ? "Revive" : "Report");
            if (player.GetItem() == Items.None || player.GetItem() == Items.Stop || player.GetItem() == Items.Newsletter || player.GetItem() == Items.Medicine)
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
            if (__instance.PetButton.isActiveAndEnabled)
            {
                switch (player.GetItem())
                {
                    case Items.TimeSlower:
                        __instance.PetButton.OverrideText("Slow Time");
                        break;
                    case Items.Knowledge:
                        __instance.PetButton.OverrideText("Reveal");
                         break;
                    case Items.Shield:
                        __instance.PetButton.OverrideText("Shield");
                        break;
                    case Items.Gun:
                        __instance.PetButton.OverrideText("Kill");
                        break;
                    case Items.Illusion:
                        __instance.PetButton.OverrideText("Manipulate");
                        break;
                    case Items.Radar:
                        __instance.PetButton.OverrideText("Radar");
                        break;
                    case Items.Swap:
                        __instance.PetButton.OverrideText("Swap");
                        break;
                    case Items.TimeSpeeder:
                        __instance.PetButton.OverrideText("Speed Time");
                        break;
                    case Items.Flash:
                        __instance.PetButton.OverrideText("Flash");
                        break;
                    case Items.Hack:
                        __instance.PetButton.OverrideText("Hack");
                        break;
                    case Items.Camouflage:
                        __instance.PetButton.OverrideText("Camouflage");
                        break;
                    case Items.MultiTeleport:
                        __instance.PetButton.OverrideText("Multi TP");
                        break;
                    case Items.Bomb:
                        __instance.PetButton.OverrideText("Detonated");
                        break;
                    case Items.Trap:
                        __instance.PetButton.OverrideText("Place");
                        break;
                    case Items.TeamChanger:
                        __instance.PetButton.OverrideText("Change");
                        break;
                    case Items.Teleport:
                        __instance.PetButton.OverrideText("Teleport");
                        break;
                    case Items.Button:
                        __instance.PetButton.OverrideText("Emergency");
                        break;
                    case Items.Finder:
                        __instance.PetButton.OverrideText("Find");
                        break;
                    case Items.Rope:
                        __instance.PetButton.OverrideText("Pull");
                        break;
                    case Items.Compass:
                        __instance.PetButton.OverrideText("Track");
                        break;
                }
            }
            if (IsHackActive && (!player.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()))
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.ReportButton.SetDisabled();
                __instance.ReportButton.ToggleVisible(false);
                __instance.SabotageButton.SetDisabled();
                __instance.SabotageButton.ToggleVisible(false);
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
                __instance.AdminButton.SetDisabled();
                __instance.AdminButton.ToggleVisible(false);
                __instance.UseButton.SetDisabled();
            }
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (IsHackActive && Options.HackAffectsImpostors.GetBool())
            {
                __instance.Close();
                __instance.ShowNormalMap();
                __instance.taskOverlay.Hide();
            }
        }

        public override void OnIntroDestroy()
        {
            NoItemTimer = 10f;
        }

        public override void OnPet(PlayerControl pc)
        {
            if ((!IsHackActive || (pc.Data.Role.IsImpostor && Options.HackAffectsImpostors.GetBool() == false)) && NoItemTimer == 0f)
            {
                PlayerControl target = pc.GetClosestPlayer(true);
                switch (pc.GetItem())
                {
                    case Items.TimeSlower:
                        ++TimeSlowersUsed;
                        Utils.SyncAllSettings();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Knowledge:
                        if (target == null || Vector2.Distance(pc.transform.position, target.transform.position) > 2f) break;
                        if (target.Data.Role.IsImpostor)
                        {
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.red;
                            if (Options.ImpostorsSeeReveal.GetBool())
                                Main.NameColors[(pc.PlayerId, target.PlayerId)] = Color.gray;
                        }
                        else
                        {
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.green;
                            if (Options.CrewmatesSeeReveal.GetBool())
                                Main.NameColors[(pc.PlayerId, target.PlayerId)] = Color.gray;
                        }
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Shield:
                        ShieldTimer[pc.PlayerId] = Options.ShieldDuration.GetFloat();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Gun:
                        if (target == null || Vector2.Distance(pc.transform.position, target.transform.position) > 2f) break;
                        if (target.Data.Role.IsImpostor)
                            pc.RpcFixedMurderPlayer(target);
                        else
                        {
                            if (Options.CanKillCrewmate.GetBool())
                                pc.RpcFixedMurderPlayer(target);
                            else
                            {
                                if (Options.MisfireKillsCrewmate.GetBool())
                                    target.RpcMurderPlayer(target, true);
                                pc.RpcSetDeathReason(DeathReasons.Misfire);
                                pc.RpcMurderPlayer(pc, true);
                            }
                        }
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Illusion:
                        if (target == null || Vector2.Distance(pc.transform.position, target.transform.position) > 2f) break;
                        if (target.Data.Role.IsImpostor)
                            target.RpcMurderPlayer(pc, true);
                        else
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.green;
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Radar:
                        bool showReactorFlash = false;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player.Data.Role.IsImpostor && Vector2.Distance(pc.transform.position, player.transform.position) <= Options.RadarRange.GetFloat() * 9 && !player.Data.IsDead)
                                showReactorFlash = true;
                        }
                        if (showReactorFlash)
                            pc.RpcReactorFlash(0.5f, Palette.Orange);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Swap:
                        if (target == null || Vector2.Distance(pc.transform.position, target.transform.position) > 2f) break;
                        if (NoItemGive) break;
                        List<byte> playerTasks = new();
                        List<byte> targetTasks = new();
                        List<uint> completedTasksPlayer = new();
                        List<uint> completedTasksTarget = new();
                        foreach (var task in pc.Data.Tasks)
                        {
                            playerTasks.Add(task.TypeId);
                            if (task.Complete)
                                completedTasksPlayer.Add(task.Id);
                        }        
                        foreach (var task in target.Data.Tasks)
                        {
                            targetTasks.Add(task.TypeId);
                            if (task.Complete)
                                completedTasksTarget.Add(task.Id);
                        }
                        pc.Data.RpcSetTasks(targetTasks.ToArray());
                        if (!AntiCheat.ChangedTasks.Contains(pc.PlayerId))
                            AntiCheat.ChangedTasks.Add(pc.PlayerId);
                        target.Data.RpcSetTasks(playerTasks.ToArray());
                        if (!AntiCheat.ChangedTasks.Contains(target.PlayerId))
                            AntiCheat.ChangedTasks.Add(target.PlayerId);
                        new LateTask(() =>
                        {
                            NoItemGive = true;
                            foreach (var task in pc.Data.Tasks)
                            {
                                if (completedTasksTarget.Contains(task.Id))
                                    pc.RpcCompleteTask(task.Id);
                            }
                            foreach (var task in target.Data.Tasks)
                            {
                                if (completedTasksPlayer.Contains(task.Id))
                                    target.RpcCompleteTask(task.Id);
                            }
                            NoItemGive = false;
                        }, 0.1f, "Set tasks done");   
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.TimeSpeeder:
                        ++TimeSpeedersUsed;
                        Utils.SyncAllSettings();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Flash:
                        FlashTimer = Options.FlashDuration.GetFloat();
                        Utils.SyncAllSettings();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Hack:
                        HackTimer = Options.HackDuration.GetFloat();
                        GameManager.Instance.RpcSetHackActive(true);
                        Utils.SyncAllSettings();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            Main.NameColors[(ar.PlayerId, ar.PlayerId)] = Color.yellow;
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Camouflage:
                        if (pc.IsMushroomMixupActive()) break;
                        pc.RpcSetItem(Items.None);
                        CamouflageTimer = Options.CamouflageDuration.GetFloat();
                        Utils.Camouflage();
                        break;
                    case Items.MultiTeleport:
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar != pc)
                                ar.RpcTeleport(pc.transform.position);
                        }
                        NoBombTimer = 10f;
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Bomb:
                        if (NoBombTimer > 0f) return;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if ((!player.Data.Role.IsImpostor || Options.CanKillImpostors.GetBool()) && Vector2.Distance(pc.transform.position, player.transform.position) <= Options.BombRadius.GetFloat() * 2f && !player.Data.IsDead && player != pc && ShieldTimer[player.PlayerId] <= 0f)
                            {
                                player.RpcSetDeathReason(DeathReasons.Bombed);
                                player.RpcMurderPlayer(player, true);
                            }
                        }
                        pc.RpcSetDeathReason(DeathReasons.Suicide);
                        pc.RpcMurderPlayer(pc, true);
                        if (Options.RiShowExplosionAnimation.GetBool())
                            Utils.RpcCreateExplosion(Options.BombRadius.GetFloat() * 20f / 3f, 1.5f, pc.transform.position);
                        pc.RpcSetItem(Items.None);
                        if (Options.NoGameEnd.GetBool()) break;
                        var isSomeoneAlive = false;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (!player.Data.IsDead)
                                isSomeoneAlive= true;
                        }
                        if (!isSomeoneAlive)
                        {
                            List<byte> winners = new();
                            foreach (var player in PlayerControl.AllPlayerControls)
                            {
                                if (player.Data.Role.IsImpostor)
                                    winners.Add(player.PlayerId);
                            }
                            CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorByKill, winners);            
                        }
                        break;
                    case Items.Trap:
                        List<byte> visibleList = new();
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if ((!player.Data.Role.IsImpostor && Options.CrewmatesSeeTrap.GetBool()) || (player.Data.Role.IsImpostor && Options.ImpostorsSeeTrap.GetBool()) || player.Data.Role.IsDead || player == pc)
                                visibleList.Add(player.PlayerId);
                        }
                        Utils.RpcCreateTrapArea(Options.TrapRadius.GetFloat(), Options.TrapWaitTime.GetFloat(), pc.transform.position, visibleList);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.TeamChanger:
                        if (target == null || Vector2.Distance(pc.transform.position, target.transform.position) > 2f || target.Data.Role.IsImpostor) break;
                        var role = Options.TargetGetsYourRole.GetBool() ? pc.Data.Role.Role : RoleTypes.Impostor;
                        target.RpcSetRoleV2(role);
                        pc.RpcMurderPlayer(pc, true);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Teleport:
                        pc.RpcRandomVentTeleport();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Button:
                        if (Utils.IsSabotage() && !Options.CanUseDuringSabotage.GetBool()) break;
                        pc.ForceReportDeadBody(null);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Finder:
                        target = pc.GetClosestPlayer();
                        if (target.onLadder || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat)
                            break;
                        pc.RpcTeleport(target.transform.position);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Rope:
                        target = pc.GetClosestPlayer();
                        target.RpcTeleport(pc.transform.position);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Compass:
                        CompassTimer[pc.PlayerId] = Options.CompassDuration.GetFloat();
                        pc.RpcSetItem(Items.None);
                        break;
                }
            }
        }

        public override bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            if (IsHackActive) return false;
            return true;
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (IsHackActive && Options.HackAffectsImpostors.GetBool())
                return false;
            if (killer.Data.Role.IsImpostor && ShieldTimer[target.PlayerId] > 0f)
            {
                if (Options.SeeWhoTriedKill.GetBool())
                    Main.NameColors[(killer.PlayerId, target.PlayerId)] = Color.red;
                Main.NameColors[(target.PlayerId, killer.PlayerId)] = Color.cyan;
                return false;
            }
            if (CamouflageTimer > -1f)
                target.RpcSetColor(Main.StandardColors[target.PlayerId]);
            return true;
        }

        public override void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (killer.Data.Role.IsImpostor && !NoItemGive)
                killer.RpcSetItem(Utils.RandomItemImpostor());
            target.RpcSetItem(Items.None);
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (CamouflageTimer > -1f) return false;
            if (IsHackActive && Options.HackAffectsImpostors.GetBool() && shapeshifter != target) return false;
            return true;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            if (IsHackActive && (!__instance.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool())) return false;
            if (__instance.GetItem() == Items.Medicine && target != null && target.Object != null && !target.Disconnected)
            {
                var player = target.Object;
                player.RpcRevive();
                if (Options.DieOnRevive.GetBool())
                {
                    __instance.RpcSetDeathReason(DeathReasons.Suicide);
                    __instance.RpcExileV2();
                }
                __instance.RpcSetItem(Items.None);
                return false;
            }
            if (CamouflageTimer > -1f)
            {
                CamouflageTimer = -1f;
                Utils.RevertCamouflage();
            }
            if (Options.EnableMedicine.GetBool())
            {
                foreach (var playerId in PlayersDiedThisRound)
                {
                    var player = Utils.GetPlayerById(playerId);
                    if (player != null && !player.Data.Disconnected && !player.Data.IsDead)
                        DestroyableSingleton<RoleManager>.Instance.AssignRoleOnDeath(player, true);
                }
                PlayersDiedThisRound.Clear();
            }
            return true;
        }

        public override void OnFixedUpdate()
        {
            if (FlashTimer > -1f)
            {
                FlashTimer -= Time.fixedDeltaTime;
            }
            if (FlashTimer <= 0f && FlashTimer > -1f)
            {
                FlashTimer = -1f;
                Utils.SyncAllSettings();
            }
            if (IsHackActive && HackTimer > 0f)
            {
                HackTimer -= Time.fixedDeltaTime;
            }
            if (IsHackActive && HackTimer <= 0f)
            {
                HackTimer = 0f;
                GameManager.Instance.RpcSetHackActive(false);
                Utils.SyncAllSettings();
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(pc.PlayerId, pc.PlayerId)] = Color.clear;
            }
            if (CamouflageTimer > -1f && !MeetingHud.Instance)
            {
                CamouflageTimer -= Time.fixedDeltaTime;
            }
            if (CamouflageTimer <= 0f && CamouflageTimer > -1f)
            {
                CamouflageTimer = -1f;
                Utils.RevertCamouflage();
            }
            if (NoBombTimer > 0f)
            {
                NoBombTimer -= Time.fixedDeltaTime;
            }
            if (NoBombTimer < 0f)
            {
                NoBombTimer = 0f;
            }
            if (NoItemTimer > 0f)
            {
                NoItemTimer -= Time.fixedDeltaTime;
            }
            if (NoItemTimer < 0f)
            {
                NoItemTimer = 0f;
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (ShieldTimer[pc.PlayerId] > 0f)
                {
                    ShieldTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (ShieldTimer[pc.PlayerId] < 0f)
                {
                    ShieldTimer[pc.PlayerId] = 0f;
                }
                if (CompassTimer[pc.PlayerId] > 0f)
                {
                    CompassTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (CompassTimer[pc.PlayerId] < 0f)
                {
                    CompassTimer[pc.PlayerId] = 0f;
                }
            }
        }

        public override void OnCompleteTask(PlayerControl pc)
        {
            if (!pc.Data.IsDead && !NoItemGive)
                pc.RpcSetItem(Utils.RandomItemCrewmate());
        }

        public override bool OnCheckVanish(PlayerControl phantom)
        {
            if (IsHackActive && Options.HackAffectsImpostors.GetBool()) return false;
            return true;
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            if (IsHackActive && Options.HackAffectsImpostors.GetBool()) return false;
            return true;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (IsHackActive && (!player.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool())) return false;
            if (systemType == SystemTypes.MushroomMixupSabotage && reader.ReadByte() == 1)
                CamouflageTimer = -1f;
            return true;
        }

        public RandomItemsGamemode()
        {
            Gamemode = Gamemodes.RandomItems;
            PetAction = true;
            DisableTasks = false;
            AllPlayersItems = new Dictionary<byte, Items>();
            FlashTimer = -1f;
            HackTimer = 0f;
            IsHackActive = false;
            CamouflageTimer = -1f;
            ShieldTimer = new Dictionary<byte, float>();
            NoBombTimer = 0f;
            NoItemTimer = 0f;
            NoItemGive = false;
            CompassTimer = new Dictionary<byte, float>();
            TimeSlowersUsed = 0;
            TimeSpeedersUsed = 0;
            PlayersDiedThisRound = new List<byte>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                AllPlayersItems[pc.PlayerId] = Items.None;
                ShieldTimer[pc.PlayerId] = 0f;
                CompassTimer[pc.PlayerId] = 0f;
            }
        }

        public static RandomItemsGamemode instance;
        public Dictionary<byte, Items> AllPlayersItems;
        public float FlashTimer;
        public float HackTimer;
        public bool IsHackActive;
        public float CamouflageTimer;
        public Dictionary<byte, float> ShieldTimer;
        public float NoBombTimer;
        public float NoItemTimer;
        public bool NoItemGive;
        public Dictionary<byte, float> CompassTimer;
        public int TimeSlowersUsed;
        public int TimeSpeedersUsed;
        public List<byte> PlayersDiedThisRound;
    }

    public enum Items
    {
        None = 0,
        //crewmate
        TimeSlower,
        Knowledge,
        Shield,
        Gun,
        Illusion,
        Radar,
        Swap,
        Medicine,
        //impostor
        TimeSpeeder,
        Flash,
        Hack,
        Camouflage,
        MultiTeleport,
        Bomb,
        Trap,
        TeamChanger,
        //both
        Teleport,
        Button,
        Finder,
        Rope,
        Stop,
        Newsletter,
        Compass,
    }
}