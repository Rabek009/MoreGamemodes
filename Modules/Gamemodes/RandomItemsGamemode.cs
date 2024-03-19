using UnityEngine;
using System.Linq;

namespace MoreGamemodes
{
    public class RandomItemsGamemode : CustomGamemode
    {
        public override void OnExile(GameData.PlayerInfo exiled)
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
            if (player.GetItem() == Items.None || player.GetItem() == Items.Stop || player.GetItem() == Items.Newsletter)
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
            if (HackTimer > 0f && (!player.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()))
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
            }
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (HackTimer > 0f && Options.HackAffectsImpostors.GetBool())
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
            if ((HackTimer == 0f || (pc.Data.Role.IsImpostor && Options.HackAffectsImpostors.GetBool() == false)) && NoItemTimer == 0f)
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
                        System.Collections.Generic.List<byte> playerTasks = new();
                        System.Collections.Generic.List<byte> targetTasks = new();
                        System.Collections.Generic.List<uint> completedTasksPlayer = new();
                        System.Collections.Generic.List<uint> completedTasksTarget = new();
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
                        GameData.Instance.RpcSetTasks(pc.PlayerId, targetTasks.ToArray());
                        GameData.Instance.RpcSetTasks(target.PlayerId, playerTasks.ToArray());
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
                        GameManager.Instance.RpcSetHackTimer((int)Options.HackDuration.GetFloat());
                        HackTimer = Options.HackDuration.GetFloat();
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
                            if ((!player.Data.Role.IsImpostor || Options.CanKillImpostors.GetBool()) && Vector2.Distance(pc.transform.position, player.transform.position) <= Options.BombRadius.GetFloat() * 2 && !player.Data.IsDead && player != pc && ShieldTimer[player.PlayerId] <= 0f)
                            {
                                player.RpcSetDeathReason(DeathReasons.Bombed);
                                player.RpcMurderPlayer(player, true);
                            }
                        }
                        pc.RpcSetDeathReason(DeathReasons.Suicide);
                        pc.RpcMurderPlayer(pc, true);
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
                            System.Collections.Generic.List<byte> winners = new();
                            foreach (var player in PlayerControl.AllPlayerControls)
                            {
                                if (player.Data.Role.IsImpostor)
                                    winners.Add(player.PlayerId);
                            }
                            CheckEndCriteriaPatch.StartEndGame(GameOverReason.ImpostorByKill, winners);            
                        }
                        break;
                    case Items.Trap:
                        Traps.Add((pc.transform.position, Options.TrapWaitTime.GetFloat()));
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Teleport:
                        pc.RpcRandomVentTeleport();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Button:
                        if (Utils.IsSabotage() && !Options.CanUseDuringSabotage.GetBool()) break;
                        pc.ReportDeadBody(null);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Finder:
                        pc.RpcTeleport(target.transform.position);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Rope:
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

        public override bool OnCheckProtect(PlayerControl __instance, PlayerControl target)
        {
            if (HackTimer > 0f) return false;
            return true;
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (HackTimer > 0f && Options.HackAffectsImpostors.GetBool())
                return false;
            if (killer.Data.Role.IsImpostor && ShieldTimer[target.PlayerId] > 0f)
            {
                if (Options.SeeWhoTriedKill.GetBool())
                    Main.NameColors[(killer.PlayerId, target.PlayerId)] = Color.red;
                Main.NameColors[(target.PlayerId, killer.PlayerId)] = Color.cyan;
                return false;
            }
            if (CamouflageTimer > 0f)
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
            if (CamouflageTimer > 0f) return false;
            if (HackTimer > 0f && Options.HackAffectsImpostors.GetBool()) return false;
            return true;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, GameData.PlayerInfo target)
        {
            if (HackTimer > 0f && (!__instance.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool())) return false;
            if (CamouflageTimer > 0f)
            {
                CamouflageTimer = 0f;
                Utils.RevertCamouflage();
            }
            Traps.Clear();
            return true;
        }

        public override void OnFixedUpdate()
        {
            if (FlashTimer > 0f)
            {
                FlashTimer -= Time.fixedDeltaTime;
            }
            if (FlashTimer < 0f)
            {
                Utils.SyncAllSettings();
                FlashTimer = 0f;
            }
            if (HackTimer > 0f)
            {
                HackTimer -= Time.fixedDeltaTime;
            }
            if (HackTimer < 0f)
            {
                Utils.SyncAllSettings();
                GameManager.Instance.RpcSetHackTimer(0);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(pc.PlayerId, pc.PlayerId)] = Color.clear;
            }
            if (CamouflageTimer > 0f && !Main.IsMeeting)
            {
                CamouflageTimer -= Time.fixedDeltaTime;
            }
            if (CamouflageTimer < 0f)
            {
                Utils.RevertCamouflage();
                CamouflageTimer = 0f;
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
            for (int i = 0; i < Traps.Count; ++i)
            {
                if (Traps[i].Item2 > 0f)
                {
                    Traps[i] = (Traps[i].Item1, Traps[i].Item2 - Time.fixedDeltaTime);
                }
                if (Traps[i].Item2 < 0f)
                {
                    Traps[i] = (Traps[i].Item1, 0f);
                }
                if (Traps[i].Item2 > 0f) continue;
                Vector2 trappos = Traps[i].Item1;
                System.Collections.Generic.Dictionary<PlayerControl, float> pcdistance = new();
                float dis;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.inVent && !MeetingHud.Instance)
                    {
                        dis = Vector2.Distance(trappos, p.transform.position);
                        pcdistance.Add(p, dis);
                    }
                }
                var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
                PlayerControl target = min.Key;
                if (Vector2.Distance(Traps[i].Item1, target.transform.position) <= 1f * Options.TrapRadius.GetFloat())
                {
                    if (ShieldTimer[target.PlayerId] <= 0f)
                    {
                        target.RpcSetDeathReason(DeathReasons.Trapped);
                        target.RpcMurderPlayer(target, true);
                    }
                    Traps.RemoveAt(i);
                    --i;
                }
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

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            if (HackTimer > 0f && Options.HackAffectsImpostors.GetBool()) return false;
            return true;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
        {
            if (HackTimer > 0f && (!player.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool())) return false;
            if (systemType == SystemTypes.MushroomMixupSabotage && amount == 1)
                CamouflageTimer = 0f;
            return true;
        }

        public RandomItemsGamemode()
        {
            Gamemode = Gamemodes.RandomItems;
            PetAction = true;
            DisableTasks = false;
            AllPlayersItems = new System.Collections.Generic.Dictionary<byte, Items>();
            FlashTimer = 0f;
            HackTimer = 0f;
            CamouflageTimer = 0f;
            ShieldTimer = new System.Collections.Generic.Dictionary<byte, float>();
            NoBombTimer = 0f;
            NoItemTimer = 0f;
            NoItemGive = false;
            Traps = new System.Collections.Generic.List<(Vector2, float)>();
            CompassTimer = new System.Collections.Generic.Dictionary<byte, float>();
            TimeSlowersUsed = 0;
            TimeSpeedersUsed = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                AllPlayersItems[pc.PlayerId] = Items.None;
                ShieldTimer[pc.PlayerId] = 0f;
                CompassTimer[pc.PlayerId] = 0f;
            }
        }

        public static RandomItemsGamemode instance;
        public System.Collections.Generic.Dictionary<byte, Items> AllPlayersItems;
        public float FlashTimer;
        public float HackTimer;
        public float CamouflageTimer;
        public System.Collections.Generic.Dictionary<byte, float> ShieldTimer;
        public float NoBombTimer;
        public float NoItemTimer;
        public bool NoItemGive;
        public System.Collections.Generic.List<(Vector2, float)> Traps;
        public System.Collections.Generic.Dictionary<byte, float> CompassTimer;
        public int TimeSlowersUsed;
        public int TimeSpeedersUsed;
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
        //impostor
        TimeSpeeder,
        Flash,
        Hack,
        Camouflage,
        MultiTeleport,
        Bomb,
        Trap,
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