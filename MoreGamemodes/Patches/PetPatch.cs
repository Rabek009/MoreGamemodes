using System;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using UnityEngine;
using System.Collections.Generic;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
    class TryPetPatch
    {
        public static void Prefix(PlayerControl __instance)
        {
            if (!(AmongUsClient.Instance.AmHost && AmongUsClient.Instance.AmClient)) return;
            var cancel = Options.CurrentGamemode == Gamemodes.RandomItems && Main.GameStarted;
            if (cancel)
                __instance.petting = true;
            ExternalRpcPetPatch.Prefix(__instance.MyPhysics, 51, new MessageReader());
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var cancel = Options.CurrentGamemode == Gamemodes.RandomItems && Main.GameStarted;
            if (cancel)
            {
                __instance.petting = false;
                if (__instance.AmOwner)
                    __instance.MyPhysics.RpcCancelPet();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    class ExternalRpcPetPatch
    {
        public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var cancel = Options.CurrentGamemode == Gamemodes.RandomItems && Main.GameStarted;
            var rpcType = callId == 51 ? RpcCalls.Pet : (RpcCalls)callId;
            if (rpcType != RpcCalls.Pet) return;

            PlayerControl pc = __instance.myPlayer;

            if (callId == 51 && cancel)
                __instance.CancelPet();
            if (callId != 51 && cancel)
            {
                __instance.CancelPet();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 50, SendOption.None, player.GetClientId()));
            }

            if (Main.HackTimer <= 1f || (Main.Impostors.Contains(pc.PlayerId) && Options.HackAffectsImpostors.GetBool() == false) && Options.CurrentGamemode == Gamemodes.RandomItems && cancel)
            {
                PlayerControl target = pc.GetClosestPlayer();
                switch (pc.GetItem())
                {
                    case Items.TimeSlower:
                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.DiscussionTime, Main.RealOptions.GetInt(Int32OptionNames.DiscussionTime) + Options.DiscussionTimeIncrease.GetInt());
                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.VotingTime, Main.RealOptions.GetInt(Int32OptionNames.VotingTime) + Options.VotingTimeIncrease.GetInt());
                        Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Knowledge:
                        if (Main.Impostors.Contains(target.PlayerId))
                        {
                            target.RpcSetNamePrivate(Utils.ColorString(Color.red, Main.StandardNames[target.PlayerId]), pc);
                            if (Options.ImpostorsSeeReveal.GetBool())
                                pc.RpcSetNamePrivate(Utils.ColorString(Color.gray, Main.StandardNames[pc.PlayerId]), target);
                        }
                        else
                        {
                            target.RpcSetNamePrivate(Utils.ColorString(Color.green, Main.StandardNames[target.PlayerId]), pc);
                            if (Options.CrewmatesSeeReveal.GetBool())
                                pc.RpcSetNamePrivate(Utils.ColorString(Color.gray, Main.StandardNames[pc.PlayerId]), target);
                        }
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Shield:
                        Main.ShieldTimer[pc.PlayerId] = Options.ShieldDuration.GetFloat() + 1f;
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Gun:
                        if (Main.Impostors.Contains(target.PlayerId))
                            pc.RpcFixedMurderPlayer(target);
                        else
                        {
                            if (Options.CanKillCrewmate.GetBool())
                                pc.RpcFixedMurderPlayer(target);
                            else
                            {
                                if (Options.MisfireKillsCrewmate.GetBool())
                                    target.RpcMurderPlayer(target);
                                pc.RpcSetDeathReason(DeathReasons.Misfire);
                                pc.RpcFixedMurderPlayer(pc);
                            }
                        }
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Illusion:
                        if (Main.Impostors.Contains(target.PlayerId))
                            target.RpcMurderPlayer(pc);
                        else
                            target.RpcSetNamePrivate(Utils.ColorString(Color.green, Main.StandardNames[target.PlayerId]), pc);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Radar:
                        bool showReactorFlash = false;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (Main.Impostors.Contains(player.PlayerId) && Vector3.Distance(pc.transform.position, player.transform.position) <= Options.RadarRange.GetFloat() * 9 && !player.Data.IsDead)
                                showReactorFlash = true;
                        }
                        if (showReactorFlash)
                            pc.RpcReactorFlash(1f, Color.red);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.TimeSpeeder:
                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.DiscussionTime, Math.Max(Main.RealOptions.GetInt(Int32OptionNames.DiscussionTime) - Options.DiscussionTimeDecrease.GetInt(), 0));
                        GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.VotingTime, Math.Max(Main.RealOptions.GetInt(Int32OptionNames.VotingTime) - Options.VotingTimeDecrease.GetInt(), 10));
                        Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Flash:
                        Main.FlashTimer = Options.FlashDuration.GetFloat() + 1f;
                        GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.CrewLightMod, 0f);
                        GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, Options.ImpostorVisionInFlash.GetFloat());
                        Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Hack:
                        Main.HackTimer = Options.HackDuration.GetFloat() + 1f;
                        if (Options.HackAffectsImpostors.GetBool())
                        {
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                            GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.001f);
                            GameOptionsManager.Instance.currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
                        }
                        GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 1f);
                        GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, 0.001f);
                        GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistBatteryCharge, 1f);
                        GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, 0.001f);
                        Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Camouflage:
                        Main.CamouflageTimer = Options.CamouflageDuration.GetFloat() + 1f;
                        Utils.Camouflage();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.MultiTeleport:
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar != pc)
                                ar.RpcTeleport(pc.transform.position);
                        }
                        Main.NoBombTimer = 11f;
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Bomb:
                        if (Main.NoBombTimer > 1f) return;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if ((!Main.Impostors.Contains(player.PlayerId) || Options.CanKillImpostors.GetBool()) && Vector3.Distance(pc.transform.position, player.transform.position) <= Options.BombRadius.GetFloat() * 2 && !player.Data.IsDead && player != pc)
                            {
                                player.RpcSetDeathReason(DeathReasons.Bombed);
                                player.RpcMurderPlayer(player);
                            }
                        }
                        pc.RpcSetDeathReason(DeathReasons.Suicide);
                        pc.RpcMurderPlayer(pc);
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
                            List<byte> winners = new List<byte>();
                            foreach (var player in PlayerControl.AllPlayerControls)
                            {
                                if (Main.Impostors.Contains(player.PlayerId))
                                    winners.Add(player.PlayerId);
                            }
                            CheckEndCriteriaPatch.StartEndGame(GameOverReason.ImpostorByKill, winners);            
                        }
                        break;
                    case Items.Teleport:
                        pc.RpcRandomVentTeleport();
                        pc.RpcSetItem(Items.None);
                        break;
                    case Items.Button:
                        if ((Utils.IsActive(SystemTypes.LifeSupp) || Utils.IsActive(SystemTypes.Reactor) || Utils.IsActive(SystemTypes.Laboratory) || Utils.IsActive(SystemTypes.Electrical) || Utils.IsActive(SystemTypes.Comms)) && !Options.CanUseDuringSabotage.GetBool())
                            break;
                        pc?.ReportDeadBody(null);
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
                    case Items.Newsletter:
                        int crewmates = 0;
                        int scientists = 0 ;
                        int engineers = 0;       
                        int impostors = 0;
                        int shapeshifters = 0;
                        int crewmateGhosts = 0;
                        int guardianAngels = 0;
                        int impostorGhosts = 0;
                        int alivePlayers = 0;
                        int deadPlayers = 0;
                        int killedPlayers = 0;
                        int exiledPlayers = 0;
                        int disconnectedPlayers = 0;
                        int misfiredPlayers = 0;
                        int bombedPlayers = 0;
                        int suicidePlayers = 0;
                        string message = "Roles in game:\n";
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            switch (player.Data.Role.Role)
                            {
                                case RoleTypes.Crewmate: ++crewmates; break;
                                case RoleTypes.Scientist: ++scientists; break;
                                case RoleTypes.Engineer: ++engineers; break;
                                case RoleTypes.Impostor: ++impostors; break;
                                case RoleTypes.Shapeshifter: ++shapeshifters; break;
                                case RoleTypes.CrewmateGhost: ++crewmateGhosts; break;
                                case RoleTypes.GuardianAngel: ++guardianAngels; break;
                                case RoleTypes.ImpostorGhost: ++impostorGhosts; break;
                            }
                        }
                        for (byte i = 0; i <= 14; ++i)
                        {
                            if (Main.AllPlayersDeathReason.ContainsKey(i))
                            {
                                var player = Utils.GetPlayerById(i);
                                if (player.GetDeathReason() == DeathReasons.Alive)
                                    ++alivePlayers;
                                else
                                {
                                    ++deadPlayers;
                                    switch (player.GetDeathReason())
                                    {
                                        case DeathReasons.Killed: ++killedPlayers; break;
                                        case DeathReasons.Exiled: ++exiledPlayers; break;
                                        case DeathReasons.Disconnected: ++disconnectedPlayers; break;
                                        case DeathReasons.Misfire: ++misfiredPlayers; break;
                                        case DeathReasons.Bombed: ++bombedPlayers; break;
                                        case DeathReasons.Suicide: ++suicidePlayers; break;
                                    }
                                }
                            }
                        }
                        message += crewmates + " crewamtes\n";
                        message += scientists + " scientists\n";
                        message += engineers + " engineers\n";
                        message += impostors + " impostors\n";
                        message += shapeshifters + " shapeshifters\n";
                        message += crewmateGhosts + " crewamte ghosts\n";
                        message += guardianAngels + " guardian angels\n";
                        message += impostorGhosts + " impostor ghosts\n\n";
                        message += alivePlayers + " players are alive\n";
                        message += deadPlayers + " players died:\n";
                        message += killedPlayers + " by getting killed\n";
                        message += exiledPlayers + " by voting\n";
                        message += disconnectedPlayers + " players disconnected\n";
                        message += misfiredPlayers + " misfired on crewmate\n";
                        message += bombedPlayers + " players got bombed\n";
                        message += suicidePlayers + " commited suicide";
                        pc.RpcSetItem(Items.None);
                        pc.RpcSendMessage(message, "Newsletter");
                        break;
                }
            }
        }
    }
}