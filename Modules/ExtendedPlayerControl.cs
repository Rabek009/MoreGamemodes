using System.Linq;
using InnerNet;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Data;
using System;

namespace MoreGamemodes
{
    static class ExtendedPlayerControl
    {
        public static void RpcTeleport(this PlayerControl player, Vector2 location)
        {
            if (AmongUsClient.Instance.AmHost) player.NetTransform.SnapTo(location);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None);
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
            if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 3) return false;
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
            var opt2 = player.BuildGameOptions();
            Utils.SyncSettings(opt2, player.GetClientId());
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

        public static void RpcReactorFlash(this PlayerControl pc, float duration, Color color)
        {
            if (pc == null) return;
            if (pc.PlayerId == 0)
            {
                var hud = DestroyableSingleton<HudManager>.Instance;
                if (hud.FullScreen == null) return;
                var obj = hud.transform.FindChild("FlashColor_FullScreen")?.gameObject;
                if (obj == null)
                {
                    obj = GameObject.Instantiate(hud.FullScreen.gameObject, hud.transform);
                    obj.name = "FlashColor_FullScreen";
                }
                hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((t) =>
                {
                    obj.SetActive(t != 1f);
                    obj.GetComponent<SpriteRenderer>().color = new(color.r, color.g, color.b, Mathf.Clamp01((-2f * Mathf.Abs(t - 0.5f) + 1) * color.a));
                })));
                return;
            }
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
                    if (Main.Timer < Options.HnSImpostorsBlindTime.GetFloat())
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                    break;
                case Gamemodes.ShiftAndSeek:
                    opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 15, 100);
                    opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 15, 100);
                    if (Main.Timer < Options.SnSImpostorsBlindTime.GetFloat())
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
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
                        opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
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
                GameOptionsManager.Instance.CurrentGameOptions = opt;
            }
            else
                Utils.SyncSettings(opt, player.GetClientId());
        }
    }
}