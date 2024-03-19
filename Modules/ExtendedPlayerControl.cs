using System.Linq;
using InnerNet;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Data;
using System;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    static class ExtendedPlayerControl
    {
        public static void RpcTeleport(this PlayerControl player, Vector2 location)
        {
            if (player.inVent || player.MyPhysics.Animations.IsPlayingEnterVentAnimation())
                player.MyPhysics.RpcBootFromVent(0);
            if (player.onLadder || player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
            {
                new LateTask(() => player.RpcTeleport(location), 0.01f, "Retry Teleport");
                return;
            }
            var sId = player.NetTransform.lastSequenceId + 5;
            player.NetTransform.SnapTo(location, (ushort)sId);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.Reliable);
            NetHelpers.WriteVector2(location, writer);
            writer.Write(player.NetTransform.lastSequenceId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRandomVentTeleport(this PlayerControl player)
        {
            var vents = UnityEngine.Object.FindObjectsOfType<Vent>();
            var rand = new System.Random();
            var vent = vents[rand.Next(0, vents.Count)];
            player.RpcTeleport(new Vector2(vent.transform.position.x, vent.transform.position.y + 0.3636f));
        }

        public static void RpcSendMessage(this PlayerControl player, string message, string title)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Main.MessagesToSend.Add((message, player.PlayerId, title));
        }

        public static void RpcSetDesyncRole(this PlayerControl player, RoleTypes role, int clientId)
        {
            if (player == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetNamePrivate(this PlayerControl player, string name, PlayerControl seer = null, bool isRaw = false)
        {
            if (player == null || name == null || !AmongUsClient.Instance.AmHost) return;
            if (seer == null) seer = player;
            if (Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] == name && !isRaw) return;
            
            if (seer.AmOwner)
            {
                player.cosmetics.nameText.SetText(name);
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
                return;
            }
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, clientId);
            writer.Write(name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
        }

        public static bool TryCast<T>(this Il2CppObjectBase obj, out T casted)
        where T : Il2CppObjectBase
        {
            casted = obj.TryCast<T>();
            return casted != null;
        }

        public static void RpcResetAbilityCooldown(this PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.LocalPlayer == target)
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.None, target.GetClientId());
                writer.WriteNetObject(target);
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static bool HasBomb(this PlayerControl player)
        {
            if (BombTagGamemode.instance == null) return false;
            if (player == null) return false;
            if (!BombTagGamemode.instance.HasBomb.ContainsKey(player.PlayerId)) return false;
            return BombTagGamemode.instance.HasBomb[player.PlayerId];
        }

        public static Items GetItem(this PlayerControl player)
        {
            if (RandomItemsGamemode.instance == null) return Items.None;
            if (player == null) return Items.None;
            if (!RandomItemsGamemode.instance.AllPlayersItems.ContainsKey(player.PlayerId)) return Items.None;
            return RandomItemsGamemode.instance.AllPlayersItems[player.PlayerId];
        }

        public static int Lives(this PlayerControl player)
        {
            if (BattleRoyaleGamemode.instance == null) return 0;
            if (player == null) return 0;
            if (!BattleRoyaleGamemode.instance.Lives.ContainsKey(player.PlayerId)) return 0;
            return BattleRoyaleGamemode.instance.Lives[player.PlayerId];
        }

        public static bool IsKiller(this PlayerControl player)
        {
            if (KillOrDieGamemode.instance == null) return false;
            if (player == null) return false;
            if (!KillOrDieGamemode.instance.IsKiller.ContainsKey(player.PlayerId)) return false;
            return KillOrDieGamemode.instance.IsKiller[player.PlayerId];
        }

        public static bool CanVent(this PlayerControl player)
        {
            if (GameOptionsManager.Instance.currentGameOptions.MapId == 3) return false;
            if (((Options.CurrentGamemode == Gamemodes.HideAndSeek && !Options.HnSImpostorsCanVent.GetBool()) || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && !Options.SnSImpostorsCanVent.GetBool())) && player.Data.Role.IsImpostor)
                return false;
            if (Options.CurrentGamemode == Gamemodes.BombTag)
                return false;
            if (Options.CurrentGamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.HackTimer > 0f && (!player.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()))
                return false;
            if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
                return false;
            if (Options.CurrentGamemode == Gamemodes.KillOrDie)
                return false;
            if (Options.CurrentGamemode == Gamemodes.Zombies)
                return false;
            if (Options.CurrentGamemode == Gamemodes.Jailbreak)
                return player.IsGuard() || player.HasItem(InventoryItems.Screwdriver);
            if (Options.CurrentGamemode == Gamemodes.Classic && GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek && !player.Data.Role.IsImpostor)
                return int.Parse(HudManager.Instance.AbilityButton.usesRemainingText.text) > 0;
            return player.Data.Role.Role == RoleTypes.Engineer || player.Data.Role.IsImpostor;
        }

        public static PlayerControl GetClosestPlayer(this PlayerControl player, bool forTarget = false)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && (!forTarget || !(p.inVent || p.MyPhysics.Animations.IsPlayingEnterVentAnimation() || p.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || p.inMovingPlat)))
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

        public static void RpcGuardAndKill(this PlayerControl killer, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (killer.AmOwner)
            {
                killer.MurderPlayer(target, MurderResultFlags.FailedProtected);
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, killer.GetClientId());
                writer.WriteNetObject(target);
                writer.Write((int)MurderResultFlags.FailedProtected);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static void RpcSetKillTimer(this PlayerControl player, float time)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (player.AmOwner)
            {
                player.SetKillTimer(time);
                return;
            }
            var opt = player.BuildGameOptions(time * 2);
            Utils.SyncSettings(opt, player.GetClientId());
            player.RpcGuardAndKill(player);
        }

        public static void RpcExileV2(this PlayerControl player)
        {
            player.Exiled();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcFixedMurderPlayer(this PlayerControl killer, PlayerControl target)
        {
            new LateTask(() => 
            {
                killer.RpcMurderPlayer(target, true);
                if (killer.AmOwner)
                    killer.MyPhysics.RpcCancelPet();
            }, 0.01f, "Late Murder");
        }

        public static void RpcUnmoddedReactorFlash(this PlayerControl pc, float duration)
        {
            if (pc == null) return;
            int clientId = pc.GetClientId();
            byte reactorId = 3;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) reactorId = 21;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4) reactorId = 58;

            MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
            SabotageWriter.Write(reactorId);
            MessageExtensions.WriteNetObject(SabotageWriter, pc);
            SabotageWriter.Write((byte)128);
            AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);

            new LateTask(() =>
            {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, duration, "Fix Desync Reactor");

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                new LateTask(() =>
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, duration, "Fix Desync Reactor 2");
        }

        public static void RpcSetDeathReason(this PlayerControl player, DeathReasons reason)
        {
            Main.AllPlayersDeathReason[player.PlayerId] = reason;
        }

        public static DeathReasons GetDeathReason(this PlayerControl player)
        {
            return Main.AllPlayersDeathReason[player.PlayerId];
        }

        public static DeadBody GetClosestBody(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<DeadBody, float> bodydistance = new();
            float dis;
            foreach (DeadBody body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                dis = Vector2.Distance(playerpos, body.transform.position);
                bodydistance.Add(body, dis);
            }
            var min = bodydistance.OrderBy(c => c.Value).FirstOrDefault();
            DeadBody target = min.Key;
            return target;
        }

        public static Vector2 GetPaintBattleLocation(this PlayerControl player)
        {
            int x, y;
            if (player.PlayerId < 8)
            {
                x = (player.PlayerId % 4 * -12) - 8;
                y = (player.PlayerId / 4 * -12) - 30;
            }
            else
            {
                x = (player.PlayerId % 4 * 12) - 8;
                y = (player.PlayerId / 4 * 12) + 10;
            }
            return new Vector2(x, y);
        }

        public static IGameOptions BuildGameOptions(this PlayerControl player, float killCooldown = -1f)
        {
            IGameOptions opt = Main.RealOptions.Restore(new NormalGameOptionsV07(new UnityLogger().Cast<Hazel.ILogger>()).Cast<IGameOptions>());
            switch (Options.CurrentGamemode)
            {
                case Gamemodes.HideAndSeek:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    if (Main.Timer < Options.HnSImpostorsBlindTime.GetFloat() && player.Data.Role.IsImpostor)
                    {
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                        opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                    }
                    break;
                case Gamemodes.ShiftAndSeek:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 15, 100);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 15, 100);
                    if (Main.Timer < Options.SnSImpostorsBlindTime.GetFloat() && player.Data.Role.IsImpostor)
                    {
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                        opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                    } 
                    break;
                case Gamemodes.BombTag:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                    opt.SetFloat(FloatOptionNames.KillCooldown, 0.001f);
                    opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, Options.ExplosionDelay.GetInt() + 0.1f);
                    opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                    opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
                    if (!player.HasBomb())
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
                    break;
                case Gamemodes.RandomItems:
                    int increasedDiscussionTime = RandomItemsGamemode.instance.TimeSlowersUsed * Options.DiscussionTimeIncrease.GetInt();
                    int increasedVotingTime = RandomItemsGamemode.instance.TimeSlowersUsed * Options.VotingTimeIncrease.GetInt();
                    int decreasedDiscussionTime = RandomItemsGamemode.instance.TimeSpeedersUsed * Options.DiscussionTimeDecrease.GetInt();
                    int decreasedVotingTime = RandomItemsGamemode.instance.TimeSpeedersUsed * Options.VotingTimeDecrease.GetInt();
                    opt.SetInt(Int32OptionNames.DiscussionTime, Math.Max(Main.RealOptions.GetInt(Int32OptionNames.DiscussionTime) + increasedDiscussionTime - decreasedDiscussionTime, 0));
                    opt.SetInt(Int32OptionNames.VotingTime, Math.Max(Main.RealOptions.GetInt(Int32OptionNames.VotingTime) + increasedVotingTime - decreasedVotingTime, 10));
                    if (RandomItemsGamemode.instance.FlashTimer > 0f)
                    {
                        opt.SetFloat(FloatOptionNames.CrewLightMod, 0f);
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, Options.ImpostorVisionInFlash.GetFloat());
                    }
                    if (RandomItemsGamemode.instance.HackTimer > 0f)
                    {
                        opt.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 1f);
                        opt.SetFloat(FloatOptionNames.EngineerCooldown, 0.001f);
                        opt.SetFloat(FloatOptionNames.ScientistBatteryCharge, 1f);
                        opt.SetFloat(FloatOptionNames.ScientistCooldown, 0.001f);
                    }
                    break;
                case Gamemodes.BattleRoyale:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                    opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                    opt.SetFloat(FloatOptionNames.KillCooldown, Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown));
                    break;
                case Gamemodes.Speedrun:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    break;
                case Gamemodes.PaintBattle:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.SetFloat(FloatOptionNames.GuardianAngelCooldown, Options.PaintingTime.GetInt() + 1f);
                    opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                    opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
                    break;
                case Gamemodes.KillOrDie:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                    opt.SetFloat(FloatOptionNames.KillCooldown, 0.001f);
                    opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, Options.TimeToKill.GetInt() + Options.KillerBlindTime.GetFloat() + 0.1f);
                    opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                    opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
                    if (!player.IsKiller())
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
                    else if (Main.Timer < Options.KillerBlindTime.GetFloat())
                    {
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                        opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                    }
                    break;
                case Gamemodes.Zombies:
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                    if (player.IsZombie())
                    {
                        if (Main.Timer >= Options.ZombieBlindTime.GetFloat() && player.GetZombieType() != ZombieTypes.JustTurned)
                        {
                            opt.SetFloat(FloatOptionNames.PlayerSpeedMod, Options.ZombieSpeed.GetFloat());
                            opt.SetFloat(FloatOptionNames.ImpostorLightMod, Options.ZombieVision.GetFloat());
                        }
                        else
                        {
                            opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                            opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                        }
                        opt.SetFloat(FloatOptionNames.KillCooldown, 1f);
                    }
                    break;
                case Gamemodes.Jailbreak:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                    opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, 1f);
                    opt.SetFloat(FloatOptionNames.ShapeshifterDuration, 0f);
                    opt.SetFloat(FloatOptionNames.GuardianAngelCooldown, Options.HelpCooldown.GetFloat());
                    opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                    opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
                    if (!player.IsGuard())
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
                    if (player.IsGuard() && JailbreakGamemode.instance.EnergyDrinkDuration[player.PlayerId] > 0f)
                        opt.SetFloat(FloatOptionNames.PlayerSpeedMod, Main.RealOptions.GetFloat(FloatOptionNames.PlayerSpeedMod) * (1f + (Options.EnergyDrinkSpeedIncrease.GetFloat() / 100f)));
                    if (player.Data.IsDead)
                        opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                    break;
            }
            if (killCooldown >= 0) opt.SetFloat(FloatOptionNames.KillCooldown, killCooldown);
            return opt;
        }

        public static string BuildPlayerName(this PlayerControl player, PlayerControl seer, bool isMeeting = false)
        {
            string name = Main.StandardNames[Main.AllShapeshifts[player.PlayerId]];
            if (isMeeting)
                name = Main.StandardNames[player.PlayerId];
            if (Main.NameColors[(player.PlayerId, seer.PlayerId)] != Color.clear)
                name = Utils.ColorString(Main.NameColors[(player.PlayerId, seer.PlayerId)], name);
            if (isMeeting) return name;
            switch (Options.CurrentGamemode)
            {
                case Gamemodes.RandomItems:
                    if (player.GetItem() != Items.None && player == seer)
                        name += "\n" + Utils.ColorString(Color.magenta, Utils.ItemString(player.GetItem()) + ": " + Utils.ItemDescription(player.GetItem()));
                    if (RandomItemsGamemode.instance.ShieldTimer[player.PlayerId] > 0f && player == seer)
                        name += "\n" + Utils.ColorString(Color.cyan, "Shield: " + (int)(RandomItemsGamemode.instance.ShieldTimer[player.PlayerId] + 0.99f) + "s");
                    if (RandomItemsGamemode.instance.CompassTimer[player.PlayerId] > 0f && player == seer)
                    {
                        name += "\n" + Utils.ColorString(Color.cyan, "Compass: " + (int)(RandomItemsGamemode.instance.CompassTimer[player.PlayerId] + 0.99f) + "s") + "\n";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc != player && !pc.Data.IsDead)
                                name += Utils.ColorString(RandomItemsGamemode.instance.CamouflageTimer > 0f ? Palette.PlayerColors[15] : Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId], Utils.GetArrow(player.transform.position, pc.transform.position));
                        }
                    }
                    if (RandomItemsGamemode.instance.CamouflageTimer > 0f && player != seer)
                        name = Utils.ColorString(Color.clear, "Player");
                    break;
                case Gamemodes.BattleRoyale:
                    var livesText = "";
                    if (player.Lives() <= 5)
                    {
                        for (int i = 1; i <= player.Lives(); i++)
                            livesText += "♥";
                    }
                    else
                        livesText = "Lives: " + player.Lives();
                    
                    if (Options.ArrowToNearestPlayer.GetBool() && player == seer)
                        name += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[player.GetClosestPlayer().PlayerId]], Utils.GetArrow(player.transform.position, player.GetClosestPlayer().transform.position));
                    if (player == seer || Options.LivesVisibleToOthers.GetBool())
                        name += "\n" + livesText;
                    break;
                case Gamemodes.Speedrun:
                    var tasksCompleted = 0;
                    var totalTasks = 0;
                    foreach (var task in player.myTasks)
                    {
                        ++totalTasks;
                        if (task.IsComplete)
                            ++tasksCompleted;
                    }
                    if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                        --totalTasks;
                    if (player == seer || Options.TasksVisibleToOthers.GetBool())
                        name += Utils.ColorString(Color.yellow, "(" + tasksCompleted + "/" + totalTasks + ")");
                    break;
                case Gamemodes.Zombies:
                    if (player == seer && !player.IsZombie() && !player.Data.IsDead)
                    {
                        if (Options.CurrentTrackingZombiesMode == TrackingZombiesModes.Nearest)
                        {
                            var nearest = player.GetClosestZombie();
                            if (nearest != null)
                                name += "\n" + Utils.ColorString(Palette.PlayerColors[2], Utils.GetArrow(player.transform.position, nearest.transform.position)); 
                        }
                        else if (Options.CurrentTrackingZombiesMode == TrackingZombiesModes.Every && player.GetClosestZombie() != null)
                        {
                            name += "\n";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc.GetZombieType() == ZombieTypes.FullZombie && !pc.Data.IsDead)
                                    name += Utils.ColorString(Palette.PlayerColors[2], Utils.GetArrow(player.transform.position, pc.transform.position));
                            }
                        }
                        if (player.KillsRemain() > 0)
                            name += "\n" + Utils.ColorString(Color.cyan, "YOU CAN KILL " + player.KillsRemain() + " " + (player.KillsRemain() == 1 ? "ZOMBIE" : "ZOMBIES") + "!");
                    }
                    break;
                case Gamemodes.Jailbreak:
                    if (player.IsGuard() && player == seer)
                    {
                        name += "\nHealth: " + (int)(JailbreakGamemode.instance.PlayerHealth[player.PlayerId] + 0.99f) + "/" + Options.GuardHealth.GetFloat();
                        name += Utils.ColorString(Color.blue, "\nSearch cooldown: " + (int)(JailbreakGamemode.instance.SearchCooldown[player.PlayerId] + 0.99f) + "s");
                        if (JailbreakGamemode.instance.EnergyDrinkDuration[player.PlayerId] > 0f)
                            name += Utils.ColorString(Color.yellow, "\nEnergy drink: " + (int)(JailbreakGamemode.instance.EnergyDrinkDuration[player.PlayerId] + 0.99f) + "s");
                        name += "<color=" + Utils.ColorToHex(Color.green) + ">";
                        name += "\nMoney: " + player.GetItemAmount(InventoryItems.Resources) + "$";
                        name += "\nWeapon lvl." + player.GetItemAmount(InventoryItems.Weapon) + "</color>";
                        name += Utils.ColorString(Color.white, "\nShop item:\n");
                        switch ((Recipes)player.GetCurrentRecipe())
                        {
                            case Recipes.GuardWeapon:
                                name += Utils.ColorString(Color.magenta, "Upgrade weapon - " + (Options.GuardWeaponPrice.GetInt() * (player.GetItemAmount(InventoryItems.Weapon) + 1)) + "$");
                                break;
                            case Recipes.EnergyDrink:
                                name += Utils.ColorString(Color.magenta, "Energy drink - " + Options.EnergyDrinkPrice.GetInt() + "$");
                                break;
                        }
                    }
                    else if (!player.IsGuard() && !player.HasEscaped() && player == seer)
                    {
                        name += "\nHealth: " + (int)(JailbreakGamemode.instance.PlayerHealth[player.PlayerId] + 0.99f) + "/" + Options.PrisonerHealth.GetFloat();
                        name += "<color=" + Utils.ColorToHex(Color.green) + ">";
                        name += "\nResources: " + player.GetItemAmount(InventoryItems.Resources) + "/" + Options.MaximumPrisonerResources.GetInt();
                        if (player.HasItem(InventoryItems.BreathingMaskWithoutOxygen))
                            name += "\nBreathing mask without oxygen" + (player.GetItemAmount(InventoryItems.BreathingMaskWithoutOxygen) > 1 ? " x" + player.GetItemAmount(InventoryItems.BreathingMaskWithoutOxygen) : "");
                        if (player.HasItem(InventoryItems.BreathingMaskWithOxygen))
                            name += "\nBreathing mask with oxygen" + (player.GetItemAmount(InventoryItems.BreathingMaskWithOxygen) > 1 ? " x" + player.GetItemAmount(InventoryItems.BreathingMaskWithOxygen) : "");
                        name += "</color>";
                        if (player.HasItem(InventoryItems.Screwdriver))
                            name += "\nScrewdriver";
                        if (player.HasItem(InventoryItems.Weapon))
                            name += "\nWeapon lvl." + player.GetItemAmount(InventoryItems.Weapon);
                        if (player.HasItem(InventoryItems.Pickaxe))
                            name += "\nPickaxe lvl." + player.GetItemAmount(InventoryItems.Pickaxe);
                        if (player.HasItem(InventoryItems.SpaceshipParts))
                            name += "\nSpaceship part" + (player.GetItemAmount(InventoryItems.SpaceshipParts) > 1 ? " x" + player.GetItemAmount(InventoryItems.SpaceshipParts) : "");
                        if (player.HasItem(InventoryItems.SpaceshipWithoutFuel))
                            name += "\nSpaceship without fuel" + (player.GetItemAmount(InventoryItems.SpaceshipWithoutFuel) > 1 ? " x" + player.GetItemAmount(InventoryItems.SpaceshipWithoutFuel) : "");
                        if (player.HasItem(InventoryItems.SpaceshipWithFuel))
                            name += "\nSpaceship with fuel" + (player.GetItemAmount(InventoryItems.SpaceshipWithFuel) > 1 ? " x" + player.GetItemAmount(InventoryItems.SpaceshipWithFuel) : "");
                        if (player.HasItem(InventoryItems.GuardOutfit))
                            name += "\nGuard outfit" + (player.GetItemAmount(InventoryItems.GuardOutfit) > 1 ? " x" + player.GetItemAmount(InventoryItems.GuardOutfit) : "");
                        name += Utils.ColorString(Color.white, "\nRecipe:\n");
                        switch ((Recipes)player.GetCurrentRecipe())
                        {
                            case Recipes.Screwdriver:
                                name += Utils.ColorString(Color.magenta, "Screwdriver - " + Options.ScrewdriverPrice.GetInt() + " res");
                                break;
                            case Recipes.PrisonerWeapon:
                                name += Utils.ColorString(Color.magenta, "Upgrade weapon - " + (Options.PrisonerWeaponPrice.GetInt() * (player.GetItemAmount(InventoryItems.Weapon) + 1)) + " res");
                                break;
                            case Recipes.Pickaxe:
                                name += Utils.ColorString(Color.magenta, "Upgrade pickaxe - " + (Options.PickaxePrice.GetInt() * (player.GetItemAmount(InventoryItems.Pickaxe) + 1)) + " res");
                                break;
                            case Recipes.SpaceshipPart:
                                name += Utils.ColorString(Color.magenta, "Spaceship part - " + Options.SpaceshipPartPrice.GetInt() + " res");
                                break;
                            case Recipes.Spaceship:
                                name += Utils.ColorString(Color.magenta, "Spaceship - " + Options.RequiredSpaceshipParts.GetInt() + " parts");
                                break;
                            case Recipes.BreathingMask:
                                name += Utils.ColorString(Color.magenta, "Breathing mask - " + Options.BreathingMaskPrice.GetInt() + " res");
                                break;
                            case Recipes.GuardOutfit:
                                name += Utils.ColorString(Color.magenta, "Guard outfit - " + Options.GuardOutfitPrice.GetInt() + " res");
                                break;
                        }
                    }
                    if (player.GetPlainShipRoom() != null && player.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && player == seer)
                        name += "WALL [" + JailbreakGamemode.instance.ReactorWallHealth + "%]";
                    if (JailbreakGamemode.instance.ReactorWallHealth <= 0f && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && player == seer)
                        name += "\nWALL DESTROYED!";
                   
                    if (!player.IsGuard() && player == seer)
                    {
                        bool isGuardAlive = false;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.IsGuard() && !pc.Data.IsDead)
                                isGuardAlive = true;
                        }
                        if (!isGuardAlive)
                            name += "\nNO GUARDS!";
                    }
                    if (JailbreakGamemode.instance.TakeoverTimer > 0f && player == seer)
                        name += "\nTAKEOVER [" + (int)(JailbreakGamemode.instance.TakeoverTimer / Options.PrisonTakeoverDuration.GetFloat() * 100f + 0.99f) + "%]";
                    if (player == seer)
                        name += Utils.ColorString(Color.cyan, "\nTIME: " + (int)((float)Options.GameTime.GetInt() - Main.Timer + 0.99f) + "s");
                    break;
            }
            if (Options.MidGameChat.GetBool() && Options.ProximityChat.GetBool() && player == seer)
            {
                foreach (var message in Main.ProximityMessages[player.PlayerId])
                    name += "\n" + Utils.ColorString(Color.white, message.Item1);
            }
            if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 5 && Utils.IsActive(SystemTypes.MushroomMixupSabotage) && player != seer && !seer.Data.Role.IsImpostor)
                name = Utils.ColorString(Color.clear, "Player");
            return name;
        }

        public static void SendProximityMessage(this PlayerControl player, string appearance, string message)
        {
            string toSend = appearance + ": " + message;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (Vector2.Distance(player.transform.position, pc.transform.position) <= Options.MessagesRadius.GetFloat() * 2.5f && pc != player)
                {
                    if (player.Data.IsDead && !pc.Data.IsDead) continue;
                    Main.ProximityMessages[pc.PlayerId].Add((toSend, 0f));
                }
            }
        }

        public static void SyncPlayerSettings(this PlayerControl player)
        {
            var opt = player.BuildGameOptions();
            if (player.AmOwner)
            {
                foreach (var com in GameManager.Instance.LogicComponents)
                {
                    if (com.TryCast<LogicOptions>(out var lo))
                        lo.SetGameOptions(opt);
                }
                GameOptionsManager.Instance.currentGameOptions = opt;
            }
            else
                Utils.SyncSettings(opt, player.GetClientId());
        }

        public static void RpcSetOutfit(this PlayerControl player, byte colorId, string hatId, string skinId, string petId, string visorId)
        {
            player.RpcSetColor(colorId);
            player.RpcSetHat(hatId);
            player.RpcSetSkin(skinId);
            player.RpcSetVisor(visorId);
            player.RpcSetPet(player.Data.IsDead ? "" : petId);
        }

        public static ZombieTypes GetZombieType(this PlayerControl player)
        {
            if (ZombiesGamemode.instance == null) return ZombieTypes.None;
            if (player == null) return ZombieTypes.None;
            if (!ZombiesGamemode.instance.ZombieType.ContainsKey(player.PlayerId)) return ZombieTypes.None;
            return ZombiesGamemode.instance.ZombieType[player.PlayerId];
        }

        public static bool IsZombie(this PlayerControl player)
        {
            return player.GetZombieType() != ZombieTypes.None;
        }

        public static int KillsRemain(this PlayerControl player)
        {
            if (ZombiesGamemode.instance == null) return 0;
            if (player == null) return 0;
            if (!ZombiesGamemode.instance.KillsRemain.ContainsKey(player.PlayerId)) return 0;
            return ZombiesGamemode.instance.KillsRemain[player.PlayerId];
        }

        public static PlayerControl GetClosestZombie(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player && p.GetZombieType() == ZombieTypes.FullZombie)
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

        public static void RpcSetRoleV2(this PlayerControl player, RoleTypes role)
        {
            player.SetRole(role);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, -1);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static PlainShipRoom GetPlainShipRoom(this PlayerControl pc)
        {
            if (pc.Data.IsDead) return null;
            var Rooms = ShipStatus.Instance.AllRooms;
            if (Rooms == null) return null;
            foreach (var room in Rooms)
            {
                if (!room.roomArea) continue;
                if (pc.Collider.IsTouching(room.roomArea))
                    return room;
            }
            return null;
        }

        public static JailbreakPlayerTypes GetJailbreakPlayerType(this PlayerControl player)
        {
            if (JailbreakGamemode.instance == null) return JailbreakPlayerTypes.None;
            if (player == null) return JailbreakPlayerTypes.None;
            if (!JailbreakGamemode.instance.PlayerType.ContainsKey(player.PlayerId)) return JailbreakPlayerTypes.None;
            return JailbreakGamemode.instance.PlayerType[player.PlayerId];
        }

        public static bool IsGuard(this PlayerControl player)
        {
            return player.GetJailbreakPlayerType() == JailbreakPlayerTypes.Guard;
        }

        public static bool IsWanted(this PlayerControl player)
        {
            return player.GetJailbreakPlayerType() == JailbreakPlayerTypes.Wanted;
        }

        public static bool HasEscaped(this PlayerControl player)
        {
            return player.GetJailbreakPlayerType() == JailbreakPlayerTypes.Escapist;
        }

        public static int GetItemAmount(this PlayerControl player, InventoryItems itemType)
        {
            if (JailbreakGamemode.instance == null) return 0;
            if (player == null) return 0;
            if (!JailbreakGamemode.instance.Inventory.ContainsKey((player.PlayerId, itemType))) return 0;
            return JailbreakGamemode.instance.Inventory[(player.PlayerId, itemType)];
        }

        public static bool HasItem(this PlayerControl player, InventoryItems itemType)
        {
            return player.GetItemAmount(itemType) > 0;
        }

        public static int GetCurrentRecipe(this PlayerControl player)
        {
            if (JailbreakGamemode.instance == null) return -1;
            if (player == null) return -1;
            if (!JailbreakGamemode.instance.CurrentRecipe.ContainsKey(player.PlayerId)) return -1;
            return JailbreakGamemode.instance.CurrentRecipe[player.PlayerId];
        }

        public static bool IsDoingIllegalThing(this PlayerControl player)
        {
            if (JailbreakGamemode.instance == null) return false;
            if (player.IsGuard()) return false;
            if (player.GetPlainShipRoom() != null &&(player.GetPlainShipRoom().RoomId == SystemTypes.Reactor || player.GetPlainShipRoom().RoomId == SystemTypes.Security || player.GetPlainShipRoom().RoomId == SystemTypes.Storage ||
            player.GetPlainShipRoom().RoomId == SystemTypes.Admin || player.GetPlainShipRoom().RoomId == SystemTypes.Nav) && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3))
                return true;
            if (player.walkingToVent || player.MyPhysics.Animations.IsPlayingEnterVentAnimation() || (player.MyPhysics.Animations.Animator.GetCurrentAnimation() == player.MyPhysics.Animations.group.ExitVentAnim && player.HasItem(InventoryItems.Screwdriver)))
                return true;
            return false;
        }

        public static bool HasIllegalItem(this PlayerControl player)
        {
            if (player.HasItem(InventoryItems.Screwdriver)) return true;
            if (player.HasItem(InventoryItems.Weapon)) return true;
            if (player.HasItem(InventoryItems.Pickaxe)) return true;
            if (player.HasItem(InventoryItems.Screwdriver)) return true;
            if (player.HasItem(InventoryItems.SpaceshipParts)) return true;
            if (player.HasItem(InventoryItems.SpaceshipWithoutFuel)) return true;
            if (player.HasItem(InventoryItems.SpaceshipWithFuel)) return true;
            if (player.HasItem(InventoryItems.GuardOutfit)) return true;
            return false;
        }

        public static void RpcEscape(this PlayerControl player)
        {
            player.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Escapist);
            player.RpcShapeshift(player, false);
            player.RpcSetDeathReason(DeathReasons.Escaped);
            player.RpcSetRole(RoleTypes.GuardianAngel);
            player.RpcResetAbilityCooldown();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.green;
                pc.RpcReactorFlash(0.4f, Color.blue);
            } 
        }
    }
}