using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch 
    {
        public static Dictionary<byte, float> TimeSinceLastProtect;
        public static void Update()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            for (byte i = 0; i < 15; i++)
            {
                if (TimeSinceLastProtect.ContainsKey(i))
                {
                    TimeSinceLastProtect[i] += Time.deltaTime;
                    if (15f < TimeSinceLastProtect[i]) TimeSinceLastProtect.Remove(i);
                }
            }
        }
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            PlayerControl guardian = __instance;
            if (!CheckForInvalidProtection(guardian, target))
                return false;
            
            if (CustomGamemode.Instance.OnCheckProtect(guardian, target))
            {
                guardian.SyncPlayerSettings();
                guardian.RpcProtectPlayer(target, guardian.Data.DefaultOutfit.ColorId);
            }     

            return false;
        }
        public static bool CheckForInvalidProtection(PlayerControl guardian, PlayerControl target)
        {
		    if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost) return false;
            if (!Main.GameStarted) return false;
		    if (!target || guardian.Data.Disconnected) return false;
		    NetworkedPlayerInfo data = target.Data;
		    if (data == null || data.IsDead) return false;
            float minTime = Mathf.Max(0.02f, AmongUsClient.Instance.Ping / 1000f * 6f);
            if (TimeSinceLastProtect.TryGetValue(guardian.PlayerId, out var time) && time < minTime) return false;
            TimeSinceLastProtect[guardian.PlayerId] = 0f;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch 
    {
        public static Dictionary<byte, float> TimeSinceLastKill;
        public static void Update()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            for (byte i = 0; i < 15; i++)
            {
                if (TimeSinceLastKill.ContainsKey(i))
                {
                    TimeSinceLastKill[i] += Time.deltaTime;
                    if (15f < TimeSinceLastKill[i]) TimeSinceLastKill.Remove(i);
                }
            }
        }
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) 
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            PlayerControl killer = __instance;
            if (!CheckForInvalidMurdering(killer, target))
            {
                killer.RpcMurderPlayer(target, false);
                return false;
            } 
            
            if (CustomGamemode.Instance.OnCheckMurder(killer, target))
                killer.RpcMurderPlayer(target, true);
            else
                killer.RpcMurderPlayer(target, false);

            return false;
        }
        public static bool CheckForInvalidMurdering(PlayerControl killer, PlayerControl target)
        {
            if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost) return false;
            if (!Main.GameStarted) return false;
		    if (!target || killer.Data.IsDead || killer.Data.Disconnected) return false;
		    NetworkedPlayerInfo data = target.Data;
		    if (data == null || data.IsDead || target.inVent || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat) return false;
		    if (MeetingHud.Instance) return false;
            float minTime = Mathf.Max(0.02f, AmongUsClient.Instance.Ping / 1000f * 6f);
            if (TimeSinceLastKill.TryGetValue(killer.PlayerId, out var time) && time < minTime) return false;
            TimeSinceLastKill[killer.PlayerId] = 0f;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    class MurderPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            PlayerControl killer = __instance;
            if (!target.Data.IsDead) return;

            if (target.GetDeathReason() == DeathReasons.Alive)
                target.RpcSetDeathReason(DeathReasons.Killed);
            CustomGamemode.Instance.OnMurderPlayer(killer, target);
            if (Main.StandardRoles[target.PlayerId].IsImpostor() && CustomGamemode.Instance.Gamemode != Gamemodes.BombTag && CustomGamemode.Instance.Gamemode != Gamemodes.BattleRoyale && CustomGamemode.Instance.Gamemode != Gamemodes.KillOrDie)
                target.RpcSetRole(RoleTypes.ImpostorGhost, true);
            if (killer != target)
                ++Main.PlayerKills[killer.PlayerId];
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
    class CheckShapeshiftPatch 
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool shouldAnimate) 
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            PlayerControl shapeshifter = __instance;
            if (!CheckForInvalidShapeshifting(shapeshifter, target, shouldAnimate))
            {
                shapeshifter.RpcRejectShapeshift();
                return false;
            } 
            if (!shouldAnimate)
                shapeshifter.RpcShapeshift(target, shouldAnimate);
            else if (CustomGamemode.Instance.OnCheckShapeshift(shapeshifter, target))
                shapeshifter.RpcShapeshift(target, shouldAnimate);
            else
                shapeshifter.RpcRejectShapeshift();
            
            return false;
        }
        public static bool CheckForInvalidShapeshifting(PlayerControl shapeshifter, PlayerControl target, bool shouldAnimate)
        {
            if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost) return false;
            if (!Main.GameStarted) return false;
		    if (!target || target.Data == null || shapeshifter.Data.IsDead || shapeshifter.Data.Disconnected) return false;
		    if (target.IsMushroomMixupActive() && shouldAnimate) return false;
            if (MeetingHud.Instance && shouldAnimate) return false;
            return true;
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
                    break;
            }
            new LateTask(() =>
            {
                if (MeetingHud.Instance) return;
                foreach (var pc in PlayerControl.AllPlayerControls)
                    shapeshifter.RpcSetNamePrivate(shapeshifter.BuildPlayerName(pc, false), pc, true);
            }, 1.2f, "Set Shapeshift Appearance");
            CustomGamemode.Instance.OnShapeshift(shapeshifter, target);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    class ReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (target != null && !target.IsDead && CustomGamemode.Instance.Gamemode != Gamemodes.Zombies) return false;
            if (!CustomGamemode.Instance.OnReportDeadBody(__instance, target)) return false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, true), ar, true);  
            }
            foreach (CustomNetObject netObject in CustomNetObject.CustomObjects)
                new LateTask(() => netObject.Despawn(), 0f);
            AntiCheat.OnMeeting();
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
            if (Main.GameStarted && !MeetingHud.Instance)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, false), ar, false);
                }
            }
            if (Main.GameStarted)
            {
                CustomGamemode.Instance.OnFixedUpdate();
                foreach (CustomNetObject netObject in CustomNetObject.CustomObjects)
                    netObject.OnFixedUpdate();
            }
            AntiCheat.OnUpdate();         
            if (Options.EnableMidGameChat.GetBool() && Options.ProximityChat.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    for (int i = 0; i < Main.ProximityMessages[pc.PlayerId].Count; ++i)
                    {
                        Main.ProximityMessages[pc.PlayerId][i] = (Main.ProximityMessages[pc.PlayerId][i].Item1, Main.ProximityMessages[pc.PlayerId][i].Item2 + Time.fixedDeltaTime);
                        if (Main.ProximityMessages[pc.PlayerId][i].Item2 > 3f + (Main.ProximityMessages[pc.PlayerId][i].Item1.Length / 10f))
                        {
                            Main.ProximityMessages[pc.PlayerId].RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
    class CoEnterVentPatch
    {
        public static List<byte> PlayersToKick;
        public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (PlayersToKick.Contains(__instance.myPlayer.PlayerId)) return false;
            if (!CustomGamemode.Instance.OnEnterVent(__instance.myPlayer, id))
            {
                PlayersToKick.Add(__instance.myPlayer.PlayerId);
                __instance.myPlayer.RpcSetVentInteraction();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
    class CoExitVentPatch
    {
        public static bool Prefix(PlayerPhysics __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CoEnterVentPatch.PlayersToKick.Contains(__instance.myPlayer.PlayerId) || (AntiCheat.TimeSinceVentCancel.ContainsKey(__instance.myPlayer.PlayerId) && AntiCheat.TimeSinceVentCancel[__instance.myPlayer.PlayerId] <= 1f))
            {
                __instance.myPlayer.NetTransform.lastSequenceId += 2;
                return false;
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
            CustomGamemode.Instance.OnCompleteTask(pc);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    class RpcUsePlatformPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!__instance.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (Options.EnableDisableGapPlatform.GetBool()) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckUseZipline))]
    class CheckUseZiplinePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!target.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (Options.EnableDisableZipline.GetBool())
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetRole))]
    class SetRolePatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!Main.StandardRoles.ContainsKey(__instance.PlayerId) && !RoleManager.IsGhostRole(roleType))
                Main.StandardRoles[__instance.PlayerId] = roleType;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    class RpcSetRolePatch
    {
        public static Dictionary<byte, bool> RoleAssigned;
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType, [HarmonyArgument(1)] bool canOverrideRole)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (RoleAssigned[__instance.PlayerId] && !RoleManager.IsGhostRole(roleType))
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Zombies && RoleManager.IsGhostRole(roleType))
                return false;
            if (!canOverrideRole)
            {
                __instance.RpcSetRole(roleType, true);
                return false;
            }
            if (RoleManager.IsGhostRole(roleType)) return true;
            int assignedRoles = 0;
            int playerCount = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == __instance) continue;
                ++playerCount;
                if (pc.Data != null && (RoleAssigned[pc.PlayerId] || pc.Data.Disconnected))
                    ++assignedRoles;
            }
            if (assignedRoles >= playerCount)
            {
                RoleAssigned[__instance.PlayerId] = true;
                new LateTask(() => {
                    Dictionary<byte, bool> Disconnected = new();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        Disconnected[pc.PlayerId] = pc.Data.Disconnected;
                        pc.Data.Disconnected = true;
                    }
                    Utils.SendGameData();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        pc.Data.Disconnected = Disconnected[pc.PlayerId];
                }, 0.5f);
                new LateTask(() => __instance.RpcSetRoleV3(roleType, false), 1f);
                new LateTask(() => {
                    foreach (var pc in PlayerControl.AllPlayerControls)
						PlayerNameColor.Set(pc);
					DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
					DestroyableSingleton<HudManager>.Instance.HideGameLoader();
                }, 1.2f);
                new LateTask(() => Utils.SendGameData(), 1.5f);
                return false;
            }
            RoleAssigned[__instance.PlayerId] = true;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    class RpcMurderPlayerPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool didSucceed)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            MurderResultFlags murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;
            if (murderResultFlags == MurderResultFlags.Succeeded && target.protectedByGuardianId > -1)
                murderResultFlags = MurderResultFlags.FailedProtected;
            if (murderResultFlags != MurderResultFlags.FailedError)
                __instance.SyncPlayerSettings();
            if (AmongUsClient.Instance.AmClient)
		    {
		    	__instance.MurderPlayer(target, murderResultFlags);
		    }
		    MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, -1);
		    messageWriter.WriteNetObject(target);
		    messageWriter.Write((int)murderResultFlags);
		    AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    class PlayerDiePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] DeathReason reason, [HarmonyArgument(1)] bool assignGhostRole)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && reason == DeathReason.Kill && Options.EnableMedicine.GetBool() && assignGhostRole)
            {
                __instance.Die(reason, false);
                RandomItemsGamemode.instance.PlayersDiedThisRound.Add(__instance.PlayerId);
                return false;
            }
            if (CustomGamemode.Instance.Gamemode != Gamemodes.PaintBattle)
                __instance.RpcSetPet("");
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixMixedUpOutfit))]
    class FixMixedUpOutfitPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (MeetingHud.Instance) return;
            new LateTask(() =>
            {
                if (!MeetingHud.Instance)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        __instance.RpcSetNamePrivate(__instance.BuildPlayerName(pc, false), pc, true);
                }
            }, 1f, "Fix After MixUp Name");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
    class CheckColorPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Options.CanUseColorCommand.GetBool())
            {
                __instance.RpcSetColor(bodyColor);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckName))]
    class CheckNamePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string name)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Options.CanUseNameCommand.GetBool() && Options.EnableNameRepeating.GetBool())
            {
                __instance.RpcSetName(name);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
    class RpcSetNamePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string name)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (AntiCheat.BannedPlayers.Contains(__instance.NetId)) return false;
            if (AmongUsClient.Instance.AmClient)
		    {
			    __instance.SetName(name);
		    }
		    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetName, SendOption.None, -1);
		    writer.Write(__instance.Data.NetId);
            writer.Write(name);
		    AmongUsClient.Instance.FinishRpcImmediately(writer);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
    class CmdCheckVanishPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            __instance.CheckVanish();
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
    class CmdCheckAppearPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            __instance.CheckAppear(shouldAnimate);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckVanish))]
    class CheckVanishPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            PlayerControl phantom = __instance;
            if (CustomGamemode.Instance.OnCheckVanish(phantom))
            {
                phantom.SyncPlayerSettings();
			    phantom.SetRoleInvisibility(true, true, true);
                phantom.RpcVanish();
            }
            else
            {
                if (phantom.AmOwner) return false;
                CustomRpcSender sender = CustomRpcSender.Create("Cancel Vanish", SendOption.None);
                sender.StartMessage(phantom.GetClientId());
                sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartVanish)
                    .EndRpc();
                sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartAppear)
                    .Write(true)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
            return false;
        }
    }
}