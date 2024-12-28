using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System.Linq;
using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch 
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            PlayerControl guardian = __instance;
            if (!CheckForInvalidProtection(guardian, target))
                return false;
            if (CustomGamemode.Instance.OnCheckProtect(guardian, target))
                guardian.RpcProtectPlayer(target, guardian.Data.DefaultOutfit.ColorId); 
            return false;
        }
        public static bool CheckForInvalidProtection(PlayerControl guardian, PlayerControl target)
        {
		    if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost) return false;
            if (!Main.GameStarted) return false;
		    if (!target || guardian.Data.Disconnected) return false;
		    NetworkedPlayerInfo data = target.Data;
		    if (data == null || data.IsDead) return false;
            if (MeetingHud.Instance || ExileController.Instance) return false;
            if (Main.ProtectCooldowns[guardian.PlayerId] > 0f) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ProtectPlayer))]
    class ProtectPlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            PlayerControl guardian = __instance;
            Main.ProtectCooldowns[guardian.PlayerId] = Main.OptionProtectCooldowns[guardian.PlayerId];
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch 
    {
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
		    if (data == null || data.IsDead || target.inVent || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat || target.shouldAppearInvisible || target.invisibilityAlpha < 1f) return false;
		    if (MeetingHud.Instance || ExileController.Instance) return false;
            if (Main.KillCooldowns[killer.PlayerId] > 0f) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    class MurderPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            PlayerControl killer = __instance;
            
            if (resultFlags == MurderResultFlags.Succeeded)
                Main.KillCooldowns[killer.PlayerId] = Main.OptionKillCooldowns[killer.PlayerId];
            else if (resultFlags == MurderResultFlags.FailedProtected)
                Main.KillCooldowns[killer.PlayerId] = Main.OptionKillCooldowns[killer.PlayerId] / 2f;

            if (resultFlags != MurderResultFlags.Succeeded) return;
            if (target.GetDeathReason() == DeathReasons.Alive)
                target.RpcSetDeathReason(DeathReasons.Killed);
            CustomGamemode.Instance.OnMurderPlayer(killer, target);
            if (Main.StandardRoles[target.PlayerId].IsImpostor() && CustomGamemode.Instance.Gamemode != Gamemodes.BombTag && CustomGamemode.Instance.Gamemode != Gamemodes.BattleRoyale && CustomGamemode.Instance.Gamemode != Gamemodes.KillOrDie && CustomGamemode.Instance.Gamemode != Gamemodes.ColorWars)
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
            if ((MeetingHud.Instance || ExileController.Instance) && shouldAnimate) return false;
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
            new LateTask(() => shapeshifter.Data.MarkDirty(), 1.3f, "Fix chat name");
            CustomGamemode.Instance.OnShapeshift(shapeshifter, target);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    class ReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (AmongUsClient.Instance.IsGameOver || MeetingHud.Instance || (target == null && Utils.IsSabotage()) || __instance.Data.IsDead) return false;
            if (target != null && !target.IsDead && CustomGamemode.Instance.Gamemode != Gamemodes.Zombies) return false;
            if (!CustomGamemode.Instance.OnReportDeadBody(__instance, target)) return false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                foreach (var ar in PlayerControl.AllPlayerControls)
                    pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, true, false), ar, true);  
            }
            foreach (CustomNetObject netObject in CustomNetObject.CustomObjects)
            {
                if (netObject.DespawnOnMeeting)
                    new LateTask(() => netObject.Despawn(), 0f);
            }
            AntiCheat.OnMeeting();
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    static class FixedUpdatePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (Main.GameStarted && __instance.AmOwner)
            {
                Main.Timer += Time.fixedDeltaTime;
                if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && !__instance.GetRole().CanUseKillButton() && __instance.GetRole().ForceKillButton() && !__instance.Data.IsDead)
                {
                    __instance.Data.Role.CanUseKillButton = true;
                    Il2CppSystem.Collections.Generic.List<PlayerControl> playersInAbilityRangeSorted = __instance.Data.Role.GetPlayersInAbilityRangeSorted(RoleBehaviour.GetTempPlayerList());
		            if (playersInAbilityRangeSorted.Count <= 0)
			            DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                    else
		                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(playersInAbilityRangeSorted[0]);
                }
            }
            if (!AmongUsClient.Instance.AmHost) return;
            if (!__instance.AmOwner) return;
            if (Main.GameStarted && !MeetingHud.Instance && !ExileController.Instance)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, false), ar, false);
                    if ((pc.moveable || pc.petting) && !pc.inVent && !pc.shapeshifting)
                    {
                        Main.KillCooldowns[pc.PlayerId] -= Time.fixedDeltaTime;
                        if (Main.KillCooldowns[pc.PlayerId] < 0f)
                            Main.KillCooldowns[pc.PlayerId] = 0f;
                        Main.ProtectCooldowns[pc.PlayerId] -= Time.fixedDeltaTime;
                        if (Main.ProtectCooldowns[pc.PlayerId] < 0f)
                            Main.ProtectCooldowns[pc.PlayerId] = 0f;
                    }
                    Main.TimeSinceLastPet[pc.PlayerId] += Time.fixedDeltaTime;
                }
            }
            if (Main.GameStarted)
            {
                CustomGamemode.Instance.OnFixedUpdate();
                ExplosionHole.FixedUpdate();
                foreach (CustomNetObject netObject in CustomNetObject.CustomObjects)
                    netObject.OnFixedUpdate();
            }
            AntiCheat.OnUpdate();
            if (Main.GameStarted)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.NameMessages[pc.PlayerId].Count == 0) continue;
                    for (int i = 0; i < Main.NameMessages[pc.PlayerId].Count; ++i)
                    {
                        Main.NameMessages[pc.PlayerId][i] = (Main.NameMessages[pc.PlayerId][i].Item1, Main.NameMessages[pc.PlayerId][i].Item2 + Time.fixedDeltaTime);
                        if (Main.NameMessages[pc.PlayerId][i].Item2 > 3f + (Main.NameMessages[pc.PlayerId][i].Item1.Length / 20f))
                        {
                            Main.NameMessages[pc.PlayerId].RemoveAt(i);
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
            if (CoEnterVentPatch.PlayersToKick.Contains(__instance.myPlayer.PlayerId))
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
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] uint idx)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var pc = __instance;
            CustomGamemode.Instance.OnCompleteTask(pc);
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
                ClassicGamemode.instance.CompletedTasks[__instance.PlayerId].Add(idx);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    class RpcUsePlatformPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[__instance.PlayerId])
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && (__instance.shouldAppearInvisible || __instance.invisibilityAlpha < 1f))
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!__instance.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (Options.EnableDisableGapPlatform.GetBool()) return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && __instance.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = __instance.GetRole() as Droner;
                if (dronerRole != null && dronerRole.RealPosition != null)
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckUseZipline))]
    class CheckUseZiplinePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[__instance.PlayerId])
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && (__instance.shouldAppearInvisible || __instance.invisibilityAlpha < 1f))
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!target.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (Options.EnableDisableZipline.GetBool())
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && target.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = target.GetRole() as Droner;
                if (dronerRole != null && dronerRole.ControlledDrone != null)
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetRole))]
    class SetRolePatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic) return;
            if (!Main.StandardRoles.ContainsKey(__instance.PlayerId) && !RoleManager.IsGhostRole(roleType))
                Main.StandardRoles[__instance.PlayerId] = roleType;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    class RpcSetRolePatch
    {
        public static Dictionary<byte, bool> RoleAssigned;
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (RoleAssigned[__instance.PlayerId] && !RoleManager.IsGhostRole(roleType))
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Zombies && RoleManager.IsGhostRole(roleType))
                return false;
            if (RoleManager.IsGhostRole(roleType)) return true;
            RoleAssigned[__instance.PlayerId] = true;
            __instance.StartCoroutine(__instance.CoSetRole(roleType, true));
            CustomRpcSender sender = CustomRpcSender.Create("RpcSetRole fix blackscreen", SendOption.Reliable);
            MessageWriter writer = sender.stream;
            sender.StartMessage(-1);
            bool disconnected = __instance.Data.Disconnected;
            __instance.Data.Disconnected = true;
            writer.StartMessage(1);
            writer.WritePacked(__instance.Data.NetId);
            __instance.Data.Serialize(writer, false);
            writer.EndMessage();
            __instance.Data.Disconnected = disconnected;
            sender.StartRpc(__instance.NetId, (byte)RpcCalls.SetRole)
                .Write((ushort)roleType)
                .Write(true)
                .EndRpc();
            sender.EndMessage();
            sender.SendMessage();
            new LateTask(() => {
                __instance.Data.MarkDirty();
            }, 0.5f);
            return false;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && reason == DeathReason.Kill && CustomRolesHelper.GetRoleChance(CustomRoles.Altruist) > 0 && assignGhostRole)
            {
                __instance.Die(reason, false);
                ClassicGamemode.instance.PlayersDiedThisRound.Add(__instance.PlayerId);
                return false;
            }
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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MixUpOutfit))]
    class MixUpOutfitPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (__instance.Data.IsDead || MeetingHud.Instance) return;
            new LateTask(() =>
            {
                if (!MeetingHud.Instance)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        pc.RpcSetNamePrivate(pc.BuildPlayerName(__instance, false), __instance, true);
                    if (__instance.AmOwner)
                        __instance.SetPet("pet_test");
                    else
                    {
                        CustomRpcSender sender = CustomRpcSender.Create("SetDesyncPet", SendOption.Reliable);
                        MessageWriter writer = sender.stream;
                        sender.StartMessage(__instance.GetClientId());
                        sender.StartRpc(__instance.NetId, (byte)RpcCalls.SetPetStr)
                            .Write("pet_test")
		                    .Write(__instance.GetNextRpcSequenceId(RpcCalls.SetPetStr))
                            .EndRpc();
                        writer.StartMessage(1);
                        writer.WritePacked(__instance.Data.NetId);
                        __instance.Data.Serialize(writer, false);
                        writer.EndMessage();
                        sender.EndMessage();
                        sender.SendMessage();
                    }
                }
            }, 0.2f, "Set MixUp Name");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixMixedUpOutfit))]
    class FixMixedUpOutfitPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (__instance.Data.IsDead || MeetingHud.Instance) return;
            new LateTask(() =>
            {
                if (!MeetingHud.Instance)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        pc.RpcSetNamePrivate(pc.BuildPlayerName(__instance, false), __instance, true);
                }
            }, 0.2f, "Fix After MixUp Name");
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
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string playerName)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Options.CanUseNameCommand.GetBool() && Options.EnableNameRepeating.GetBool())
            {
                __instance.RpcSetName(playerName);
                return false;
            }
            if (!__instance.Data)
		    {
			    Debug.LogWarning("CheckName was called while NetworkedPlayerInfo was null");
			    return false;
		    }
		    Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		    bool flag = allPlayers.ToArray().Any((NetworkedPlayerInfo i) => i.PlayerId != __instance.PlayerId && Main.StandardNames.ContainsKey(i.PlayerId) && Main.StandardNames[i.PlayerId].Equals(playerName, StringComparison.OrdinalIgnoreCase));
		    if (flag)
		    {
			    for (int k = 1; k < 100; k++)
			    {
				    string text = playerName + " " + k.ToString();
				    flag = false;
				    for (int j = 0; j < allPlayers.Count; j++)
				    {
					    if (allPlayers[j].PlayerId != __instance.PlayerId && allPlayers[j].PlayerName.Equals(text, StringComparison.OrdinalIgnoreCase))
					    {
						    flag = true;
						    break;
					    }
				    }
				    if (!flag)
				    {
					    playerName = text;
					    break;
				    }
			    }
		    }
		    __instance.Data.PlayerName = playerName;
            __instance.GetClient().UpdatePlayerName(playerName);
            __instance.Data.MarkDirty();
		    __instance.RpcSetName(__instance.Data.PlayerName);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
    class RpcSetNamePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string name)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (AntiCheat.BannedPlayers.Contains(__instance.NetId)) return false;
            var friendCode = __instance.Data.FriendCode;
            __instance.RpcSetStandardName(name, -1);
            if (PlayerTagManager.IsPlayerTagged(friendCode))
            {
                var tag = PlayerTagManager.GetPlayerTag(friendCode);
                var hostTag = PlayerTagManager.GetHostTag(friendCode);
                if (tag != null)
                {
                    Main.Instance.Log.LogInfo("Tag is not null proceeding...");
                    string coloredName = $"<color=#{tag.PreferredColor}>{name}</color>";
                    string coloredTag = tag.GetFormattedTag();
                    string formattedNameWithNewLine = $"{coloredName}\n{coloredTag}";
                    if (hostTag != null)
                    {
                        Main.Instance.Log.LogInfo("Host Tag is not null proceeding...");
                        formattedNameWithNewLine += "\n" + hostTag.GetFormattedTag();
                    }
                    
                    name = formattedNameWithNewLine;
                    Main.Instance.Log.LogMessage("Tag Now should be appeared with color");
                }
                else if (hostTag != null)
                {
                    Main.Instance.Log.LogInfo("Host Tag is not null proceeding...");
                    string coloredName = $"<color=#{hostTag.PreferredColor}>{name}</color>";
                    string coloredTag = hostTag.GetFormattedTag();
                    string formattedNameWithNewLine = $"{coloredName}\n{coloredTag}";
                    
                    name = formattedNameWithNewLine;
                    Main.Instance.Log.LogMessage("Host Tag Now should be appeared with color");
                }
            }
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
                if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && PlayerControl.LocalPlayer != phantom && !PlayerControl.LocalPlayer.GetRole().IsImpostor())
                {
                    RoleManager.Instance.SetRole(phantom, RoleTypes.Phantom);
                    phantom.SetRoleInvisibility(true, true, true);
                    var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, PlayerControl.LocalPlayer.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, PlayerControl.LocalPlayer.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                    RoleManager.Instance.SetRole(phantom, role);
                    new LateTask(() => {
                        if (MeetingHud.Instance || phantom == null || phantom.Data == null || phantom.Data.IsDead || phantom.Data.Disconnected) return;
                        phantom.invisibilityAlpha = 0f;
                        phantom.cosmetics.SetPhantomRoleAlpha(phantom.invisibilityAlpha);
                        phantom.shouldAppearInvisible = true;
			            phantom.Visible = false;
                        phantom.SyncPlayerSettings();
                        phantom.RpcSetVentInteraction();
                    }, 1.2f);
                }
                else
                {
                    phantom.SetRoleInvisibility(true, true, true);
                    new LateTask(() => {
                        if (MeetingHud.Instance || phantom == null || phantom.Data == null || phantom.Data.IsDead || phantom.Data.Disconnected) return;
                        phantom.SyncPlayerSettings();
                        phantom.RpcSetVentInteraction();
                    }, 1.3f);
                }
                phantom.RpcVanish();
            }
            else
            {
                if (phantom.AmOwner)
                {
                    DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(phantom.Data.Role.Ability);
                    phantom.Data.Role.SetCooldown();
                    return false;
                }
                CustomRpcSender sender = CustomRpcSender.Create("Cancel vanish", SendOption.Reliable);
                sender.StartMessage(phantom.GetClientId());
                sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)RoleTypes.Phantom)
                    .Write(true)
                    .EndRpc();
                sender.StartRpc(phantom.NetId, (byte)RpcCalls.ProtectPlayer)
                    .WriteNetObject(phantom)
                    .Write(0)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
                new LateTask(() => phantom.RpcSetKillTimer(Math.Max(Main.KillCooldowns[phantom.PlayerId], 0.001f)), 0.2f);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckAppear))]
    class CheckAppearPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            PlayerControl phantom = __instance;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && PlayerControl.LocalPlayer != phantom && !PlayerControl.LocalPlayer.GetRole().IsImpostor())
            {
                if (shouldAnimate)
                {
                    RoleManager.Instance.SetRole(phantom, RoleTypes.Phantom);
                    phantom.SetRoleInvisibility(true, true, true);
                    var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, PlayerControl.LocalPlayer.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, PlayerControl.LocalPlayer.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                    RoleManager.Instance.SetRole(phantom, role);
                    new LateTask(() => {
                    if (MeetingHud.Instance || phantom == null || phantom.Data == null || phantom.Data.IsDead || phantom.Data.Disconnected) return;
                        phantom.invisibilityAlpha = 1f;
                        phantom.cosmetics.SetPhantomRoleAlpha(phantom.invisibilityAlpha);
                        phantom.shouldAppearInvisible = false;
                        phantom.Visible = true;
                        phantom.SyncPlayerSettings();
                        phantom.RpcSetVentInteraction();
                    }, 1.2f);
                }
                else
                {
                    phantom.invisibilityAlpha = 1f;
                    phantom.cosmetics.SetPhantomRoleAlpha(phantom.invisibilityAlpha);
                    phantom.shouldAppearInvisible = false;
                    phantom.Visible = true;
                    phantom.SyncPlayerSettings();
                    phantom.RpcSetVentInteraction();
                }
            }
            else
            {
                phantom.SetRoleInvisibility(false, shouldAnimate, true);
                new LateTask(() => {
                    phantom.SyncPlayerSettings();
                    phantom.RpcSetVentInteraction();
                }, 1.8f);
            }
            phantom.RpcAppear(shouldAnimate);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    class SetKillTimerPatch
    {
        public static void Prefix(PlayerControl __instance)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && !__instance.GetRole().CanUseKillButton() && __instance.GetRole().ForceKillButton() && !__instance.Data.IsDead)
                __instance.Data.Role.CanUseKillButton = true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcVanish))]
    class RpcVanishPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic) return true;
            PlayerControl phantom = __instance;
            phantom.RpcSetPet("");
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                if (!pc.GetRole().IsImpostor() && pc.GetRole().BaseRole != BaseRoles.Tracker && pc != phantom)
                {
                    CustomRpcSender sender = CustomRpcSender.Create("PhantomAnimation", SendOption.Reliable);
                    sender.StartMessage(pc.GetClientId());
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Phantom)
                        .Write(true)
                        .EndRpc();
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartVanish)
                        .EndRpc();
                    var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, pc.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Crewmate)
                        .Write(true)
                        .EndRpc();
                    sender.EndMessage();
                    sender.SendMessage();
                    new LateTask(() => {
                        if (MeetingHud.Instance || phantom == null || phantom.Data == null || phantom.Data.IsDead || phantom.Data.Disconnected || pc == null || pc.Data == null || pc.Data.Disconnected) return;
                        CustomRpcSender sender = CustomRpcSender.Create("PhantomVanish", SendOption.Reliable);
                        sender.StartMessage(pc.GetClientId());
                        sender.StartRpc(phantom.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                            .WriteVector2(new Vector2(50f, 50f))
                            .Write(phantom.NetTransform.lastSequenceId)
                            .EndRpc();
                        sender.StartRpc(phantom.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                            .WriteVector2(new Vector2(50f, 50f))
                            .Write((ushort)(phantom.NetTransform.lastSequenceId + 16383))
                            .EndRpc();
                        sender.EndMessage();
                        sender.SendMessage();
                    }, 1.2f);
                }
                else
                {
                    MessageWriter msg = AmongUsClient.Instance.StartRpcImmediately(phantom.NetId, (byte)RpcCalls.StartVanish, SendOption.Reliable, pc.GetClientId());
		            AmongUsClient.Instance.FinishRpcImmediately(msg);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcAppear))]
    class RpcAppearPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            CustomGamemode.Instance.OnAppear(__instance);
            if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic) return true;
            PlayerControl phantom = __instance;
            if (shouldAnimate)
                new LateTask(() => phantom.RpcSetPet(Main.StandardPets[phantom.PlayerId]), 1.2f);
            else
                phantom.RpcSetPet(Main.StandardPets[phantom.PlayerId]);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                if (shouldAnimate && !pc.GetRole().IsImpostor() && pc.GetRole().BaseRole != BaseRoles.Tracker && pc != phantom)
                {
                    CustomRpcSender sender = CustomRpcSender.Create("PhantomAnimation", SendOption.Reliable);
                    sender.StartMessage(pc.GetClientId());
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Phantom)
                        .Write(true)
                        .EndRpc();
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartVanish)
                        .EndRpc();
                    var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, pc.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)role)
                        .Write(true)
                        .EndRpc();
                    sender.EndMessage();
                    sender.SendMessage();
                    new LateTask(() => {
                        if (MeetingHud.Instance || phantom == null || phantom.Data == null || phantom.Data.IsDead || phantom.Data.Disconnected || pc == null || pc.Data == null || pc.Data.Disconnected) return;
                        CustomRpcSender sender = CustomRpcSender.Create("PhantomAppear", SendOption.Reliable);
                        sender.StartMessage(pc.GetClientId());
                        sender.StartRpc(phantom.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                            .WriteVector2(new Vector2(50f, 50f))
                            .Write((ushort)(phantom.NetTransform.lastSequenceId + 32767))
                            .EndRpc();
                        sender.StartRpc(phantom.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                            .WriteVector2(new Vector2(50f, 50f))
                            .Write((ushort)(phantom.NetTransform.lastSequenceId + 32767 + 16383))
                            .EndRpc();
                        sender.StartRpc(phantom.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                            .WriteVector2(phantom.transform.position)
                            .Write(phantom.NetTransform.lastSequenceId)
                            .EndRpc();
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.Exiled)
                            .EndRpc();
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                            .Write((ushort)RoleTypes.Phantom)
                            .Write(true)
                            .EndRpc();
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartAppear)
                            .Write(false)
                            .EndRpc();
                        var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, pc.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                            .Write((ushort)role)
                            .Write(true)
                            .EndRpc();
                        sender.EndMessage();
                        sender.SendMessage();
                    }, 1f);
                    new LateTask(() => {
                        if (MeetingHud.Instance || phantom == null || phantom.Data == null || phantom.Data.IsDead || phantom.Data.Disconnected || pc == null || pc.Data == null || pc.Data.Disconnected) return;
                        CustomRpcSender sender = CustomRpcSender.Create("PhantomAppear", SendOption.Reliable);
                        sender.StartMessage(pc.GetClientId());
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                            .Write((ushort)RoleTypes.Phantom)
                            .Write(true)
                            .EndRpc();
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartAppear)
                            .Write(false)
                            .EndRpc();
                        var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, pc.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                        sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                            .Write((ushort)role)
                            .Write(true)
                            .EndRpc();
                        sender.EndMessage();
                        sender.SendMessage();
                    }, 1.2f);
                }
                else if (!shouldAnimate && !pc.GetRole().IsImpostor() && pc.GetRole().BaseRole != BaseRoles.Tracker && pc != phantom)
                {
                    CustomRpcSender sender = CustomRpcSender.Create("PhantomAppear", SendOption.Reliable);
                    sender.StartMessage(pc.GetClientId());
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Phantom)
                        .Write(true)
                        .EndRpc();
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.StartAppear)
                        .Write(false)
                        .EndRpc();
                    var role = Main.DesyncRoles.ContainsKey((phantom.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(phantom.PlayerId, pc.PlayerId)] : Main.StandardRoles[phantom.PlayerId];
                    sender.StartRpc(phantom.NetId, (byte)RpcCalls.SetRole)
                        .Write((ushort)role)
                        .Write(true)
                        .EndRpc();
                    sender.EndMessage();
                    sender.SendMessage();
                }
                else
                {
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(phantom.NetId, (byte)RpcCalls.StartAppear, SendOption.Reliable, pc.GetClientId());
		            messageWriter.Write(shouldAnimate);
		            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
    class ClimbLadderPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic) return;
            var player = __instance.myPlayer;
            if (player != null && (player.shouldAppearInvisible || player.invisibilityAlpha < 1f))
                player.RpcResetInvisibility();
            else if (player != null && player.GetRole().Role == CustomRoles.Droner)
            {
                var dronerRole = player.GetRole() as Droner;
                if (dronerRole != null && dronerRole.ControlledDrone != null)
                    dronerRole.CancelLadder();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRoleInvisibility))]
    class ServerApprovedPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] bool isActive, [HarmonyArgument(1)] bool shouldAnimate, [HarmonyArgument(2)] bool playFullAnimation)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            return playFullAnimation;
        }
    }

    [HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.RpcSetTasks))]
    class RpcSetTasksPatch
    {
        // https://github.com/tukasa0001/TownOfHost/blob/main/Patches/TaskAssignPatch.cs
        public static bool Prefix(NetworkedPlayerInfo __instance, [HarmonyArgument(0)] ref Il2CppStructArray<byte> taskTypeIds)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (ClassicGamemode.instance != null && !ClassicGamemode.instance.DefaultTasks.ContainsKey(__instance.PlayerId))
            {
                bool hasCommonTasks = true;
                int NumShortTasks = Main.RealOptions.GetInt(Int32OptionNames.NumShortTasks);
                int NumLongTasks = Main.RealOptions.GetInt(Int32OptionNames.NumLongTasks);
                if (__instance.GetRole().Role == CustomRoles.Snitch && (Snitch.AdditionalShortTasks.GetInt() > 0 || Snitch.AdditionalLongTasks.GetInt() > 0))
                {
                    NumShortTasks += Snitch.AdditionalShortTasks.GetInt();
                    NumLongTasks += Snitch.AdditionalLongTasks.GetInt();
                }
                if (!hasCommonTasks || NumShortTasks != Main.RealOptions.GetInt(Int32OptionNames.NumShortTasks) || NumLongTasks != Main.RealOptions.GetInt(Int32OptionNames.NumLongTasks))
                {
                    Il2CppSystem.Collections.Generic.List<byte> TasksList = new();
                    foreach (var num in taskTypeIds)
                        TasksList.Add(num);

                    int defaultCommonTasksNum = Main.RealOptions.GetInt(Int32OptionNames.NumCommonTasks);
                    if (hasCommonTasks) TasksList.RemoveRange(defaultCommonTasksNum, TasksList.Count - defaultCommonTasksNum);
                    else TasksList.Clear();

                    Il2CppSystem.Collections.Generic.HashSet<TaskTypes> usedTaskTypes = new();
                    int start2 = 0;
                    int start3 = 0;

                    Il2CppSystem.Collections.Generic.List<NormalPlayerTask> LongTasks = new();
                    foreach (var task in ShipStatus.Instance.LongTasks)
                        LongTasks.Add(task);
                    Shuffle(LongTasks);

                    Il2CppSystem.Collections.Generic.List<NormalPlayerTask> ShortTasks = new();
                    foreach (var task in ShipStatus.Instance.ShortTasks)
                        ShortTasks.Add(task);
                    Shuffle(ShortTasks);

                    ShipStatus.Instance.AddTasksFromList(
                        ref start2,
                        NumLongTasks,
                        TasksList,
                        usedTaskTypes,
                        LongTasks
                    );
                    ShipStatus.Instance.AddTasksFromList(
                        ref start3,
                        NumShortTasks,
                        TasksList,
                        usedTaskTypes,
                        ShortTasks
                    );

                    taskTypeIds = new Il2CppStructArray<byte>(TasksList.Count);
                    for (int i = 0; i < TasksList.Count; i++)
                    {
                        taskTypeIds[i] = TasksList[i];
                    }
                }
            }
            new LateTask(() => {
                if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
                {
                    ClassicGamemode.instance.DefaultTasks[__instance.PlayerId] = __instance.Tasks;
                    ClassicGamemode.instance.CompletedTasks[__instance.PlayerId] = new List<uint>();
                }
            }, 0f);
            if (!Main.GameStarted)
            {
                __instance.SetTasks(taskTypeIds);
                CustomRpcSender sender = CustomRpcSender.Create("RpcSetTasks fix blackscreen", SendOption.Reliable);
                MessageWriter writer = sender.stream;
                sender.StartMessage(-1);
                writer.StartMessage(1);
                writer.WritePacked(__instance.NetId);
                __instance.Serialize(writer, false);
                writer.EndMessage();
                sender.StartRpc(__instance.NetId, (byte)RpcCalls.SetTasks)
                    .WriteBytesAndSize(taskTypeIds)
                    .EndRpc();
                bool disconnected = __instance.Disconnected;
                __instance.Disconnected = true;
                writer.StartMessage(1);
                writer.WritePacked(__instance.NetId);
                __instance.Serialize(writer, false);
                writer.EndMessage();
                __instance.Disconnected = disconnected;
                sender.EndMessage();
                sender.SendMessage();
                return false;
            }
            return true;
        }
        public static void Shuffle<T>(Il2CppSystem.Collections.Generic.List<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                T obj = list[i];
                int rand = UnityEngine.Random.Range(i, list.Count);
                list[i] = list[rand];
                list[rand] = obj;
            }
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.Deserialize))]
    class CustomNetworkTransformDeserializePatch
    {
        public static bool Prefix(CustomNetworkTransform __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool initialState)
        {
            if (!AmongUsClient.Instance.AmHost || ClassicGamemode.instance == null || initialState) return true;
            if (__instance.myPlayer.GetRole().Role != CustomRoles.Droner) return true;
            var dronerRole = __instance.myPlayer.GetRole() as Droner;
            if (dronerRole == null || dronerRole.ControlledDrone == null) return true;
            if (__instance.isPaused || __instance.AmOwner) return false;
            ushort num = reader.ReadUInt16();
		    int num2 = reader.ReadPackedInt32();
            if (!NetHelpers.SidGreaterThan((ushort)(num + num2 - 1), __instance.lastSequenceId)) return false;
            for (int i = 0; i < num2 - 1; ++i)
                NetHelpers.ReadVector2(reader);
            dronerRole.DronePosition = NetHelpers.ReadVector2(reader);
            __instance.lastSequenceId = (ushort)(num + num2 - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckSporeTrigger))]
    class CheckSporeTriggerPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] Mushroom mushroom)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && __instance.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = __instance.GetRole() as Droner;
                if (dronerRole != null && dronerRole.ControlledDrone != null)
                    return false;
            }
            return true;
        }
    }
}