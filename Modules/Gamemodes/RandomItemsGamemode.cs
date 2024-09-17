using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using System;

namespace MoreGamemodes
{
    public class RandomItemsGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            NoItemTimer = 10f;
            if (exiled != null && exiled.Object != null)
                exiled.Object.RpcSetItem(Items.None); 
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            base.OnSetFilterText(__instance);
            if (GetItem(__instance.HauntTarget) != Items.None)
                __instance.FilterText.text += " (" + ItemString(GetItem(__instance.HauntTarget)) + ")";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ReportButton.OverrideText(GetItem(player) == Items.Medicine ? "Revive" : "Report");
            if (GetItem(player) == Items.None || GetItem(player) == Items.Stop || GetItem(player) == Items.Newsletter || GetItem(player) == Items.Medicine)
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
            if (__instance.PetButton.isActiveAndEnabled)
            {
                switch (GetItem(player))
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
                    case Items.Booster:
                        __instance.PetButton.OverrideText("Boost");
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
            if ((!IsHackActive || (pc.Data.Role.IsImpostor && Options.HackAffectsImpostors.GetBool() == false)) && NoItemTimer <= 0f)
            {
                PlayerControl target = pc.GetClosestPlayer(true);
                switch (GetItem(pc))
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
                            pc.RpcMurderPlayer(target, true);
                        else
                        {
                            if (Options.CanKillCrewmate.GetBool())
                                pc.RpcMurderPlayer(target, true);
                            else
                            {
                                pc.RpcSetDeathReason(DeathReasons.Misfire);
                                pc.RpcMurderPlayer(pc, true);
                                if (Options.MisfireKillsCrewmate.GetBool())
                                    pc.RpcMurderPlayer(target, true);
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
                        Utils.SetAllVentInteractions();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            Main.NameColors[(ar.PlayerId, ar.PlayerId)] = Color.yellow;
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Camouflage:
                        if (pc.IsMushroomMixupActive()) break;
                        pc.RpcSetItem(Items.None);
                        CamouflageTimer = Options.CamouflageDuration.GetFloat();
                        Camouflage();
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
                                ++Main.PlayerKills[pc.PlayerId];
                            }
                        }
                        pc.RpcSetDeathReason(DeathReasons.Suicide);
                        pc.RpcMurderPlayer(pc, true);
                        Utils.RpcCreateExplosion(Options.BombRadius.GetFloat() * 20f / 3f, 1.5f, Options.RiExplosionCreatesHole.GetBool(), Options.RiHoleSpeedDecrease.GetInt(), pc.transform.position);
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
                        Utils.RpcCreateTrapArea(Options.TrapRadius.GetFloat(), Options.TrapWaitTime.GetFloat(), pc.transform.position, visibleList, pc.PlayerId);
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
                    case Items.Booster:
                        BoosterTimer[pc.PlayerId] = Options.BoosterDuration.GetFloat();
                        pc.SyncPlayerSettings();
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
                killer.RpcSetItem(RandomItemImpostor());
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
            if (GetItem(__instance) == Items.Medicine && target != null && target.Object != null && !target.Disconnected)
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
                RevertCamouflage();
            }
            if (Options.EnableMedicine.GetBool())
            {
                foreach (var playerId in PlayersDiedThisRound)
                {
                    var player = Utils.GetPlayerById(playerId);
                    if (player != null && !player.Data.Disconnected)
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
                Utils.SetAllVentInteractions();
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
                RevertCamouflage();
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
                if (BoosterTimer[pc.PlayerId] > -1f)
                {
                    BoosterTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (BoosterTimer[pc.PlayerId] <= 0f && BoosterTimer[pc.PlayerId] > -1f)
                {
                    BoosterTimer[pc.PlayerId] = -1f;
                    pc.SyncPlayerSettings();
                }
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            if (IsHackActive && (!player.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()))
                return false;
            return base.OnEnterVent(player, id);
        }

        public override void OnCompleteTask(PlayerControl pc)
        {
            if (!pc.Data.IsDead && !NoItemGive)
                pc.RpcSetItem(RandomItemCrewmate());
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

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            int increasedDiscussionTime = TimeSlowersUsed * Options.DiscussionTimeIncrease.GetInt();
            int increasedVotingTime = TimeSlowersUsed * Options.VotingTimeIncrease.GetInt();
            int decreasedDiscussionTime = TimeSpeedersUsed * Options.DiscussionTimeDecrease.GetInt();
            int decreasedVotingTime = TimeSpeedersUsed * Options.VotingTimeDecrease.GetInt();
            opt.SetInt(Int32OptionNames.DiscussionTime, Math.Max(Main.RealOptions.GetInt(Int32OptionNames.DiscussionTime) + increasedDiscussionTime - decreasedDiscussionTime, 0));
            opt.SetInt(Int32OptionNames.VotingTime, Math.Max(Main.RealOptions.GetInt(Int32OptionNames.VotingTime) + increasedVotingTime - decreasedVotingTime, 10));
            if (FlashTimer > -1f)
            {
                opt.SetFloat(FloatOptionNames.CrewLightMod, 0f);
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, Options.ImpostorVisionInFlash.GetFloat());
            }
            if (IsHackActive)
            {
                opt.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 1f);
                opt.SetFloat(FloatOptionNames.EngineerCooldown, 0.001f);
                opt.SetFloat(FloatOptionNames.ScientistBatteryCharge, 1f);
                opt.SetFloat(FloatOptionNames.ScientistCooldown, 0.001f);
                opt.SetFloat(FloatOptionNames.ScientistBatteryCharge, 1f);
                opt.SetFloat(FloatOptionNames.TrackerCooldown, 0.001f);
                opt.SetFloat(FloatOptionNames.TrackerDuration, 1f);
                opt.SetFloat(FloatOptionNames.TrackerDelay, 255f);
            }
            if (BoosterTimer[player.PlayerId] > -1f)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, opt.GetFloat(FloatOptionNames.PlayerSpeedMod) * (1f + (Options.BoosterSpeedIncrease.GetInt() / 100f)));
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (GetItem(player) != Items.None && (player == seer || seer.Data.Role.IsDead))
                name += "\n" + Utils.ColorString(Color.magenta, ItemString(GetItem(player)) + ": " + ItemDescription(GetItem(player)));
            if (ShieldTimer[player.PlayerId] > 0f && (player == seer || seer.Data.Role.IsDead))
                name += "\n" + Utils.ColorString(Color.cyan, "Shield: " + (int)(ShieldTimer[player.PlayerId] + 0.99f) + "s");
            if (CompassTimer[player.PlayerId] > 0f && (player == seer || seer.Data.Role.IsDead))
            {
                name += "\n" + Utils.ColorString(Color.cyan, "Compass: " + (int)(CompassTimer[player.PlayerId] + 0.99f) + "s") + "\n";
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != player && !pc.Data.IsDead && player == seer)
                        name += Utils.ColorString(CamouflageTimer > -1f ? Palette.PlayerColors[15] : Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId], Utils.GetArrow(player.transform.position, pc.transform.position));
                }
            }
            if (BoosterTimer[player.PlayerId] > 0f && (player == seer || seer.Data.Role.IsDead))
                name += "\n" + Utils.ColorString(Color.cyan, "Booster: " + (int)(BoosterTimer[player.PlayerId] + 0.99f) + "s");
            if (CamouflageTimer > -1f && player != seer)
                name = Utils.ColorString(Color.clear, "Player");
            return name;
        }

        public Items RandomItemCrewmate()
        {
            List<Items> items = new();
            var rand = new System.Random();
            if (Options.EnableTimeSlower.GetBool()) items.Add(Items.TimeSlower);
            if (Options.EnableKnowledge.GetBool()) items.Add(Items.Knowledge);
            if (Options.EnableShield.GetBool()) items.Add(Items.Shield);
            if (Options.EnableGun.GetBool()) items.Add(Items.Gun);
            if (Options.EnableIllusion.GetBool()) items.Add(Items.Illusion);
            if (Options.EnableRadar.GetBool()) items.Add(Items.Radar);
            if (Options.EnableSwap.GetBool()) items.Add(Items.Swap);
            if (Options.EnableMedicine.GetBool()) items.Add(Items.Medicine);
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool() && Options.CanBeGivenToCrewmate.GetBool()) items.Add(Items.Stop);
            if (Options.EnableNewsletter.GetBool()) items.Add(Items.Newsletter);
            if (Options.EnableCompass.GetBool()) items.Add(Items.Compass);
            if (Options.EnableBooster.GetBool()) items.Add(Items.Booster);

            return items[rand.Next(0, items.Count)];
        }

        public Items RandomItemImpostor()
        {
            List<Items> items = new();
            var rand = new System.Random();
            if (Options.EnableTimeSpeeder.GetBool()) items.Add(Items.TimeSpeeder);
            if (Options.EnableFlash.GetBool()) items.Add(Items.Flash);
            if (Options.EnableHack.GetBool()) items.Add(Items.Hack);
            if (Options.EnableCamouflage.GetBool()) items.Add(Items.Camouflage);
            if (Options.EnableMultiTeleport.GetBool()) items.Add(Items.MultiTeleport);
            if (Options.EnableBomb.GetBool()) items.Add(Items.Bomb);
            if (Options.EnableTrap.GetBool()) items.Add(Items.Trap);
            if (Options.EnableTeamChanger.GetBool()) items.Add(Items.TeamChanger);
            if (Options.EnableTeleport.GetBool()) items.Add(Items.Teleport);
            if (Options.EnableButton.GetBool()) items.Add(Items.Button);
            if (Options.EnableFinder.GetBool()) items.Add(Items.Finder);
            if (Options.EnableRope.GetBool()) items.Add(Items.Rope);
            if (Options.EnableStop.GetBool()) items.Add(Items.Stop);
            if (Options.EnableNewsletter.GetBool()) items.Add(Items.Newsletter);
            if (Options.EnableCompass.GetBool()) items.Add(Items.Compass);
            if (Options.EnableBooster.GetBool()) items.Add(Items.Booster);

            return items[rand.Next(0, items.Count)];
        }

        public static string ItemString(Items item)
        {
            switch (item)
            {
                case Items.TimeSlower:
                    return "Time Slower";
                case Items.Knowledge:
                    return "Knowledge";
                case Items.Shield:
                    return "Shield";
                case Items.Gun:
                    return "Gun";
                case Items.Illusion:
                    return "Illusion";
                case Items.Radar:
                    return "Radar";
                case Items.Swap:
                    return "Swap";
                case Items.Medicine:
                    return "Medicine";
                case Items.TimeSpeeder:
                    return "Time Speeder";
                case Items.Flash:
                    return "Flash";
                case Items.Hack:
                    return "Hack";
                case Items.Camouflage:
                    return "Camouflage";
                case Items.MultiTeleport:
                    return "Multi Teleport";
                case Items.Bomb:
                    return "Bomb";
                case Items.Trap:
                    return "Trap";
                case Items.TeamChanger:
                    return "Team Changer";
                case Items.Teleport:
                    return "Teleport";
                case Items.Button:
                    return "Button";
                case Items.Finder:
                    return "Finder";
                case Items.Rope:
                    return "Rope";
                case Items.Stop:
                    return "Stop";
                case Items.Newsletter:
                    return "Newsletter";
                case Items.Compass:
                    return "Compass";
                case Items.Booster:
                    return "Booster";
                default:
                    return "INVALID ITEM";
            }
        }

        public static string ItemDescription(Items item)
        {
            switch (item)
            {
                case Items.TimeSlower:
                    return "Increase voting time";
                case Items.Knowledge:
                    return "Find out if someone is bad";
                case Items.Shield:
                    return "Grant yourself a shield";
                case Items.Gun:
                    return "Shoot impostor";
                case Items.Illusion:
                    return "Make impostor kill you";
                case Items.Radar:
                    return "See if impostors are near";
                case Items.Swap:
                    return "Swap tasks with someone";
                case Items.Medicine:
                    return "Revive a dead player (report)";
                case Items.TimeSpeeder:
                    return "Decrease voting time";
                case Items.Flash:
                    return "Blind all crewmates";
                case Items.Hack:
                    return "Prevent everyone from doing anything";
                case Items.Camouflage:
                    return "Make everyone look the same";
                case Items.MultiTeleport:
                    return "Teleport everyone to you";
                case Items.Bomb:
                    return "Sacrifice yourself to kill nearby players";
                case Items.Trap:
                    return "Create deadly trap";
                case Items.TeamChanger:
                    return "Turn someone into impostor";
                case Items.Teleport:
                    return "Teleport to random vent";
                case Items.Button:
                    return "Call emergency from anywhere";
                case Items.Finder:
                    return "Teleport to nearest player";
                case Items.Rope:
                    return "Teleport nearest player to you";
                case Items.Stop:
                    return "Type /stop command to end meeting";
                case Items.Newsletter:
                    return "Type /info to get extra informations";
                case Items.Compass:
                    return "Track other players";
                case Items.Booster:
                    return "Increase your speed";
                default:
                    return "INVALID DESCRIPTION";
            }
        }

        public static string ItemDescriptionLong(Items item)
        {
            switch (item)
            {
                case Items.TimeSlower:
                    return "Time Slower(Crewmate only): Increase discussion and voting time by amount in settings.";
                case Items.Knowledge:
                    return "Knowledge(Crewmate only): You can investigate nearby player. Green name means that he's crewmate, red name means impostor. Depending on options target can see that you investigated him. Black name means that this person investigated you";
                case Items.Shield:
                    return "Shield(Crewmate only): You grant yourself a shield for some time. If someone try kill you in this time, he can't. You will see that this person tried to kill you.";
                case Items.Gun:
                    return "Gun(Crewmate only): If nearby player is impostor, you kill him. Otherwise you die.";
                case Items.Illusion:
                    return "Illusion(Crewmate only): If nearby player is impostor, he kills you.";
                case Items.Radar:
                    return "Radar(Crewmate only): You see reactor flash if impostor is nearby.";
                case Items.Swap:
                    return "Swap(Crewmate only): Swap your tasks with nearby player tasks.";
                case Items.Medicine:
                    return "Medicine(Crewmate only): Use report button to bring back dead player. But depending on options you die after using it.";
                case Items.TimeSpeeder:
                    return "Time Speeder(Impostor only): Increase discussion and voting time by amount in settings.";
                case Items.Flash:
                    return "Flash(Impostor only): Throws flash for few seconds. During that time crewmates are blind, but impostor's vision is decreased.";
                case Items.Hack:
                    return "Hack(Impostor only): For some time crewmates can't do anything. Depending on options hack affects impostor. Yellow name means hack active. Hack prevents from: reporting, opening doors, repairing sabotages, venting, using items, using role abilities, killing, sabotaging, calling meetings or even doing tasks.";
                case Items.Camouflage:
                    return "Camouflage(Impostor only): Everyone turns into gray bean for few seconds.";
                case Items.MultiTeleport:
                    return "Multi Teleport(Impostor only): Everyone gets teleported to you.";
                case Items.Bomb:
                    return "Bomb(Impostor only): Everyone near you die, but you sacrifice yourself. Depending on options explosion can kill other impostors. If no one is alive after explosion impostors still win! You can't use bomb 10 seconds after meeting or multi teleport.";
                case Items.Trap:
                    return "Trap(Impostor only): Place trap that kills first player touches it. Trap is completely invisible and works after few seconds from placing.";
                case Items.TeamChanger:
                    return "Team Changer(Impostor only): You can turn nearby crewmate into impostor, but you die after doing it.";
                case Items.Teleport:
                    return "Teleport(Both): Teleports you to random vents.";
                case Items.Button:
                    return "Button(Both): Call meeting instantly. Depending on options you can use this during sabotage or not.";
                case Items.Finder:
                    return "Finder(Both): Teleports you to nearest player.";
                case Items.Rope:
                    return "Rope(Both): Teleports nearest player to you.";
                case Items.Stop:
                    return "Stop(Both/Impostor only): You can instantly end meeting without anyone ejected by typing /stop command. You can only use this item during voting time.";
                case Items.Newsletter:
                    return "Newsletter(Both): Sends you information about how amny roles are alive, how people died. Use this item by typing /info in chat";
                case Items.Compass:
                    return "Compass(Both): Show arrow to all players for short period of time.";
                case Items.Booster:
                    return "Booster(Both): Increases your speed temporarily.";
                default:
                    return "INVALID DESCRIPTION LONG";
            }
        }

        public void Camouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.Role == RoleTypes.Shapeshifter)
                    pc.RpcShapeshift(pc, false);
                pc.RpcSetOutfit(15, "", "", "pet_test", "");
            }
        }
        
        public void RevertCamouflage()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.RpcSetOutfit(Main.StandardColors[pc.PlayerId], Main.StandardHats[pc.PlayerId], Main.StandardSkins[pc.PlayerId], Main.StandardPets[pc.PlayerId], Main.StandardVisors[pc.PlayerId]);
        }

        public Items GetItem(PlayerControl player)
        {
            if (player == null) return Items.None;
            if (!AllPlayersItems.ContainsKey(player.PlayerId)) return Items.None;
            return AllPlayersItems[player.PlayerId];
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
            BoosterTimer = new Dictionary<byte, float>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                AllPlayersItems[pc.PlayerId] = Items.None;
                ShieldTimer[pc.PlayerId] = 0f;
                CompassTimer[pc.PlayerId] = 0f;
                BoosterTimer[pc.PlayerId] = -1f;
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
        public Dictionary<byte, float> BoosterTimer;
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
        Booster,
    }
}