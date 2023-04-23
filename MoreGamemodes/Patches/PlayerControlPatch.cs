using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                if (Main.HackTimer > 1f)
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return false;
            var killer = __instance;

            if (killer == target) return true;

            if (Options.CurrentGamemode == Gamemodes.HideAndSeek && Main.Timer < Options.HnSImpostorsBlindTime.GetFloat() && !Options.HnSImpostorsCanKillDuringBlind.GetBool()) return false;
            if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && Main.Timer < Options.SnSImpostorsBlindTime.GetFloat() && !Options.SnSImpostorsCanKillDuringBlind.GetBool()) return false;

            if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                if (Main.AllShapeshifts[killer.PlayerId] != target.PlayerId)
                {
                    return false;
                }
            }

            if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                if (killer.HasBomb() && !target.HasBomb())
                {
                    killer.RpcSetBomb(false);
                    target.RpcSetBomb(true);
                    killer.RpcSetColor(2);
                    killer.RpcSetName(Utils.ColorString(Color.green, Main.StandardNames[killer.PlayerId]));
                    target.RpcSetColor(6);
                    target.RpcSetName(Utils.ColorString(Color.black, Main.StandardNames[target.PlayerId]));
                    killer.RpcToggleCanUseKillButton(false);
                    killer.RpcToggleCanBeKilled(true);
                    target.RpcToggleCanUseKillButton(true);
                    target.RpcToggleCanBeKilled(false);
                    Utils.SendGameData();
                }
                return false;
            }
            if (Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                if (Main.HackTimer > 1f && Options.HackAffectsImpostors.GetBool())
                    return false;
                if (Main.Impostors.Contains(killer.PlayerId) && Main.ShieldTimer[target.PlayerId] > 1f)
                {
                    if (Options.SeeWhoTriedKill.GetBool())
                        killer.RpcSetNamePrivate(Utils.ColorString(Color.red, Main.StandardNames[killer.PlayerId]), target);
                    target.RpcSetNamePrivate(Utils.ColorString(Color.cyan, Main.StandardNames[target.PlayerId]), killer);
                    return false;
                }
                
                if (Main.CamouflageTimer > 0f)
                    target.RpcSetColor((byte)Main.StandardColors[target.PlayerId]);
            }
            if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                if (Main.Timer < Options.GracePeriod.GetFloat())
                    return false;
                if (target.Lives() > 1)
                {
                    --Main.Lives[target.PlayerId];
                    killer.RpcSetKillTimer(Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown));
                }
                else
                {
                    Main.Lives[target.PlayerId] = 0;
                    killer.RpcMurderPlayer(target);
                    killer.RpcSetKillTimer(Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown));
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    class MurderPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!target.Data.IsDead || !AmongUsClient.Instance.AmHost) return;
            PlayerControl killer = __instance;
            if (target.GetDeathReason() == DeathReasons.Alive)
                target.RpcSetDeathReason(DeathReasons.Killed);
            if (Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                if (Main.Impostors.Contains(killer.PlayerId))
                    killer.RpcSetItem(Utils.RandomItemImpostor());
                target.RpcSetItem(Items.None);
            }
            if (Main.Impostors.Contains(target.PlayerId) || Options.CurrentGamemode == Gamemodes.BombTag || Options.CurrentGamemode == Gamemodes.BattleRoyale)
                target.RpcSetRole(RoleTypes.ImpostorGhost);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    class ShapeshiftPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var shapeshifter = __instance;
            var shapeshifting = shapeshifter != target;
            switch (shapeshifting)
            {
                case true:
                    Main.AllShapeshifts[shapeshifter.PlayerId] = target.PlayerId;
                    break;
                case false:
                    Main.AllShapeshifts[shapeshifter.PlayerId] = shapeshifter.PlayerId;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        shapeshifter.RpcSetNamePrivate(Main.LastNotifyNames[(shapeshifter.PlayerId, pc.PlayerId)], pc, true);
                    break;
            }
            if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
                shapeshifter.RpcSetName(Utils.ColorString(Color.red, Main.StandardNames[target.PlayerId]));
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    class ReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek || Options.CurrentGamemode == Gamemodes.BombTag || Options.CurrentGamemode == Gamemodes.BattleRoyale) return false;
            if (Options.CurrentGamemode == Gamemodes.RandomItems && Main.HackTimer > 1f && (Main.Impostors.Contains(__instance.PlayerId) == false || Options.HackAffectsImpostors.GetBool())) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    static class FixedUpdatePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (Main.GameStarted && __instance == PlayerControl.LocalPlayer)
            {
                Main.Timer += Time.fixedDeltaTime;
            }
            if (!AmongUsClient.Instance.AmHost) return;
            if (!__instance.AmOwner) return;
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek && Main.GameStarted)
            {
                if (Main.Timer >= Options.HnSImpostorsBlindTime.GetFloat() && Main.Timer < Options.HnSImpostorsBlindTime.GetFloat() + 1f && Main.GameStarted)
                {
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod));
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    Main.Timer += 1f;
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && Main.GameStarted)
            {
                if (Main.Timer >= Options.HnSImpostorsBlindTime.GetFloat() && Main.Timer < Options.SnSImpostorsBlindTime.GetFloat() + 1f && Main.GameStarted)
                {
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod));
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    Main.Timer += 1f;
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag && Main.GameStarted && Main.Timer >= Options.ExplosionDelay.GetInt())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.HasBomb() && !pc.Data.IsDead)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Bombed);
                        pc.RpcMurderPlayer(pc);
                    }
                }
                var rand = new System.Random();
                List<PlayerControl> AllPlayers = new List<PlayerControl>();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                    {
                        AllPlayers.Add(pc);
                        pc.RpcResetAbilityCooldown();
                    }
                }
                var players = AllPlayers.Count();
                var bombs = System.Math.Max(System.Math.Min((players * Options.PlayersWithBomb.GetInt()) / 100, Options.MaxPlayersWithBomb.GetInt()), 1);
                if (bombs == 0)
                    bombs = 1;
                for (int i = 0; i < bombs; ++i)
                {
                    var player = AllPlayers[rand.Next(0, AllPlayers.Count())];
                    player.RpcSetBomb(true);
                    player.RpcToggleCanUseKillButton(true);
                    player.RpcToggleCanBeKilled(false);
                    player.RpcSetColor(6);
                    player.RpcSetName(Utils.ColorString(Color.black, Main.StandardNames[player.PlayerId]));
                    AllPlayers.Remove(player);
                }
                Utils.SendGameData();
                if (Options.TeleportAfterExplosion.GetBool())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                            pc.RpcRandomVentTeleport();
                    }
                }
                Main.Timer = 0f;
            }
            else if (Options.CurrentGamemode == Gamemodes.RandomItems && Main.GameStarted)
            {
                if (Main.FlashTimer > 1f)
                {
                    Main.FlashTimer -= Time.fixedDeltaTime;
                }
                if (Main.FlashTimer > 0f && Main.FlashTimer <= 1f)
                {
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.CrewLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod));
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    Main.FlashTimer = 0f;
                }
                if (Main.HackTimer > 1f)
                {
                    Main.HackTimer -= Time.fixedDeltaTime;
                }
                if (Main.HackTimer > 0f && Main.HackTimer <= 1f)
                {
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterDuration, Main.RealOptions.GetFloat(FloatOptionNames.ShapeshifterDuration));
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, Main.RealOptions.GetFloat(FloatOptionNames.ShapeshifterCooldown));
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerInVentMaxTime, Main.RealOptions.GetFloat(FloatOptionNames.EngineerInVentMaxTime));
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, Main.RealOptions.GetFloat(FloatOptionNames.EngineerCooldown));
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistBatteryCharge, Main.RealOptions.GetFloat(FloatOptionNames.ScientistBatteryCharge));
                    GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, Main.RealOptions.GetFloat(FloatOptionNames.ScientistCooldown));
                    GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, Main.RealOptions.GetBool(BoolOptionNames.ShapeshifterLeaveSkin));
                    Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                    Main.HackTimer = 0f;
                }
                if (Main.CamouflageTimer > 0f && !Main.IsMeeting)
                {
                    Main.CamouflageTimer -= Time.fixedDeltaTime;
                }
                if (Main.CamouflageTimer > 0f && Main.CamouflageTimer <= 1f)
                {
                    Utils.RevertCamouflage();
                    Main.CamouflageTimer = 0f;
                }
                if (Main.NoBombTimer > 0f)
                {
                    Main.NoBombTimer -= Time.fixedDeltaTime;
                }
                if (Main.NoBombTimer > 0f && Main.NoBombTimer <= 1f)
                {
                    Main.NoBombTimer = 0f;
                }

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.ShieldTimer[pc.PlayerId] > 1f)
                    {
                        Main.ShieldTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (Main.ShieldTimer[pc.PlayerId] > 0f && Main.ShieldTimer[pc.PlayerId] <= 1f)
                    {
                        Main.ShieldTimer[pc.PlayerId] = 0f;
                    }
                    if (!Main.IsMeeting)
                    {
                        if (Main.HackTimer > 1f)
                        {
                            if (pc.GetItem() != Items.None)
                                pc.RpcSetNamePrivate(Utils.ColorString(Color.yellow, Main.StandardNames[pc.PlayerId]) + "\n" + Utils.ColorString(Color.magenta, Utils.ItemString(pc.GetItem()) + ": " + Utils.ItemDescription(pc.GetItem())));
                            else
                                pc.RpcSetNamePrivate(Utils.ColorString(Color.yellow, Main.StandardNames[pc.PlayerId]));
                        }
                        else
                        {
                            if (pc.GetItem() != Items.None)
                                pc.RpcSetNamePrivate(Main.StandardNames[pc.PlayerId] + "\n" + Utils.ColorString(Color.magenta, Utils.ItemString(pc.GetItem()) + ": " + Utils.ItemDescription(pc.GetItem())));
                            else
                                pc.RpcSetNamePrivate(Main.StandardNames[pc.PlayerId]);
                        }
                    }
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale && Main.GameStarted)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    var livesText = "";
                    if (pc.Lives() <= 5)
                    {
                        for (int i = 1; i <= pc.Lives(); i++)
                            livesText += "♥";
                    }
                    else
                        livesText = "Lives: " + pc.Lives();

                    var arrow = Utils.GetArrow(pc.transform.position, pc.GetClosestPlayer().transform.position);
                    livesText = Utils.ColorString(Color.red, livesText);
                    arrow = Utils.ColorString(Palette.PlayerColors[Main.StandardColors[pc.GetClosestPlayer().PlayerId]], arrow);
                    if (pc.Data.IsDead)
                        arrow = "";
                    if (Options.LivesVisibleToOthers.GetBool())
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            pc.RpcSetNamePrivate(Utils.ColorString(Color.white, Main.StandardNames[pc.PlayerId]) + (pc == ar && Options.ArrowToNearestPlayer.GetBool() ? " " + arrow : "") + "\n" + livesText, ar);
                    }
                    else
                        pc.RpcSetNamePrivate(Utils.ColorString(Color.white, Main.StandardNames[pc.PlayerId]) + (Options.ArrowToNearestPlayer.GetBool() ? " " + arrow : "") + "\n" + livesText);
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.Speedrun && Main.GameStarted)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    var tasksCompleted = 0;
                    var totalTasks = 0;
                    foreach (var task in pc.myTasks)
                    {
                        ++totalTasks;
                        if (task.IsComplete)
                            ++tasksCompleted;
                    }
                    if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                        --totalTasks;
                    if (Options.TasksVisibleToOthers.GetBool())
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            pc.RpcSetNamePrivate(Main.StandardNames[pc.PlayerId] + Utils.ColorString(Color.yellow, "(" + tasksCompleted + "/" + totalTasks + ")"), ar);
                    }
                    else
                        pc.RpcSetNamePrivate(Main.StandardNames[pc.PlayerId] + Utils.ColorString(Color.yellow, "(" + tasksCompleted + "/" + totalTasks + ")"));
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
    class CoEnterVentPatch
    {
        public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (!__instance.myPlayer.CanVent())
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                    writer.WritePacked(127);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    new LateTask(() =>
                    {
                        int clientId = __instance.myPlayer.GetClientId();
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                        writer2.Write(id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    }, 0.5f, "Fix DesyncImpostor Stuck");
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    class CompleteTaskPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var pc = __instance;

            if (Options.CurrentGamemode == Gamemodes.RandomItems && !pc.Data.IsDead)
                pc.RpcSetItem(Utils.RandomItemCrewmate());
        }
    }
}