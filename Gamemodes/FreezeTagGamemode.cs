using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class FreezeTagGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Main.Timer = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.SyncPlayerSettings();
            }
        }
        
        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Crewmate)
                __instance.FilterText.text = "Runner";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Scientist)
                __instance.FilterText.text = "Scientist";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Engineer)
                __instance.FilterText.text = "Engineer";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Impostor)
                __instance.FilterText.text = "Tagger";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Shapeshifter)
                __instance.FilterText.text = "Shapeshifter";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Noisemaker)
                __instance.FilterText.text = "Noisemaker";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Phantom)
                __instance.FilterText.text = "Phantom";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Tracker)
                __instance.FilterText.text = "Tracker";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role.IsImpostor)
            {
                if (!Options.FtImpostorsCanVent.GetBool())
                {
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                }
                if (!Options.FtImpostorsCanCloseDoors.GetBool())
                {
                    __instance.SabotageButton.SetDisabled();
                    __instance.SabotageButton.ToggleVisible(false);
                }
                __instance.KillButton.OverrideText("Freeze");
                if (IsFrozen(__instance.KillButton.currentTarget))
                    __instance.KillButton.currentTarget = null;
            }
            if (IsFrozen(player))
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
            }
            else if (!player.Data.Role.IsSimpleRole && player.Data.Role.Role != RoleTypes.Noisemaker)
                __instance.AbilityButton.ToggleVisible(true);
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role.IsImpostor)
                __instance.taskText.text = Utils.ColorString(Color.red, "Tagger:\nFreeze everyone.");
            else if (!PlayerControl.LocalPlayer.Data.IsDead)
                __instance.taskText.text = "Runner:\nDon't get frozen and do tasks.\n\n" + str;
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (!Options.FtImpostorsCanCloseDoors.GetBool())
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }

        public override Il2CppSystem.Collections.Generic.List<PlayerControl> OnBeginCrewmatePrefix(IntroCutscene __instance)
        {
            var Team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            Team.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc != PlayerControl.LocalPlayer && !pc.Data.Role.IsImpostor)
                    Team.Add(pc);
            }
            return Team;
        }

        public override void OnBeginCrewmatePostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Runner";
            __instance.TeamTitle.color = Color.green;
            __instance.BackgroundBar.material.color = Color.green;
            __instance.ImpostorText.text = "";
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Tagger";
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Crewmate)
            {
                __instance.RoleText.text = "Runner";
                __instance.RoleBlurbText.text = "Don't get frozen and do tasks";
            }
            else if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Impostor)
            {
                __instance.RoleText.text = "Tagger";
                __instance.RoleBlurbText.text = "Freeze everyone";
            }
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                __instance.RoleText.color = Color.green;
                __instance.RoleBlurbText.color = Color.green;
                __instance.YouAreText.color = Color.green;
            }
        }

        public override void OnSelectRolesPostfix()
        {
            new LateTask(() => {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                    {
                        pc.RpcSetColor(0);
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.red;
                    }
                    else
                    {
                        pc.RpcSetColor(11);
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.green;
                    }
                }
            }, 1.2f);
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.Data.RpcSetTasks(new byte[0]);
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < Options.FtImpostorsBlindTime.GetFloat() && !Options.ImpostorsCanFreezeDuringBlind.GetBool()) return false;
            if (IsFrozen(target)) return false;
            target.RpcSetFrozen(true);
            target.RpcSetColor(10);
            foreach (var pc in PlayerControl.AllPlayerControls)
                Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.cyan;
            target.SyncPlayerSettings();
            target.RpcSetVentInteraction();
            if (target.Data.Role.Role == RoleTypes.Noisemaker)
                SendNoisemakerAlert(target);
            target.RpcSetPet("");
            target.RpcTeleport(target.transform.position);
            new LateTask(() => {
                if (!target.onLadder && !target.inMovingPlat)
                    target.RpcTeleport(target.transform.position);
            }, 0.2f);
            return false;
        }

        public override void OnShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (target.Data.Role.IsImpostor)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Color.red;
            }
            else
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = IsFrozen(target) ? Color.cyan : Color.green;
            }
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.FtImpostorsBlindTime.GetFloat() && Main.Timer < Options.FtImpostorsBlindTime.GetFloat() + 1f && Main.GameStarted)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        pc.SyncPlayerSettings();
                }
                Main.Timer += 1f;
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsFrozen(pc))
                {
                    if (!pc.AllTasksCompleted())
                    {
                        CompleteTaskTimer[pc.PlayerId] += Time.fixedDeltaTime;
                        if (CompleteTaskTimer[pc.PlayerId] >= Options.TaskCompleteTimeDuringFreeze.GetFloat())
                        {
                            CompleteTaskTimer[pc.PlayerId] -= Options.TaskCompleteTimeDuringFreeze.GetFloat();
                            List<uint> TasksRemain = new();
                            foreach (var task in pc.Data.Tasks)
                            {
                                if (!task.Complete)
                                    TasksRemain.Add(task.Id);
                            }
                            var rand = new System.Random();
                            pc.RpcCompleteTask(TasksRemain[rand.Next(0, TasksRemain.Count)]);
                        }
                    }
                    if (IsUnfrozenNearby(pc))
                    {
                        UnfreezeTimer[pc.PlayerId] += Time.fixedDeltaTime;
                        if (UnfreezeTimer[pc.PlayerId] >= Options.UnfreezeDuration.GetFloat())
                        {
                            UnfreezeTimer[pc.PlayerId] = 0f;
                            pc.RpcSetFrozen(false);
                            pc.RpcSetColor(11);
                            foreach (var ar in PlayerControl.AllPlayerControls)
                                Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.green;
                            pc.SyncPlayerSettings();
                            pc.RpcSetVentInteraction();
                            pc.RpcSetPet(Main.StandardPets[pc.PlayerId]);
                        }
                    }
                    else
                        UnfreezeTimer[pc.PlayerId] = 0f;
                }
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            if (!Options.FtImpostorsCanVent.GetBool() && player.Data.Role.IsImpostor)
                return false;
            if (IsFrozen(player))
                return false;
            return base.OnEnterVent(player, id);
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            if (!Options.FtImpostorsCanCloseDoors.GetBool()) return false;
            return true;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override bool OnClimbLadder(PlayerControl player, Ladder source, bool ladderUsed)
        {
            return !IsFrozen(player);
        }

        public override bool OnUsePlatform(PlayerControl __instance)
        {
            return !IsFrozen(__instance);
        }

        public override bool OnCheckUseZipline(PlayerControl target, ZiplineBehaviour ziplineBehaviour, bool fromTop)
        {
            return !IsFrozen(target);
        }

        public override bool OnCheckSporeTrigger(PlayerControl __instance, Mushroom mushroom)
        {
            return !IsFrozen(__instance);
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
            opt.SetFloat(FloatOptionNames.KillCooldown, 0.001f);
            if (Main.Timer < Options.FtImpostorsBlindTime.GetFloat() && player.Data.Role != null && player.Data.Role.IsImpostor)
            {
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            }
            if (IsFrozen(player) && !player.Data.Role.IsImpostor)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (Options.ShowDangerMeter.GetBool() && !player.Data.Role.IsImpostor && player == seer)
            {
                var impostor = player.GetClosestImpostor();
                if (impostor == null) return name;
                var distance = Vector2.Distance(player.GetRealPosition(), impostor.transform.position);
                if (distance <= 2f)
                    name += "\n<#ff1313>■■■■■</color>";
                else if (distance <= 4.5f)
                    name += "\n<#ff6a00>■■■■</color><#aaaaaa>■</color>";
                else if (distance <= 7f)
                    name += "\n<#ffaa00>■■■</color><#aaaaaa>■■</color>";
                else if (distance <= 10f)
                    name += "\n<#ffea00>■■</color><#aaaaaa>■■■</color>";
                else if (distance <= 13f)
                    name += "\n<#ffff00>■</color><#aaaaaa>■■■■</color>";
                else
                    name += "\n<#aaaaaa>■■■■■</color>";
            }
            return name;
        }

        public bool IsFrozen(PlayerControl player)
        {
            if (player == null) return false;
            if (!PlayerIsFrozen.ContainsKey(player.PlayerId)) return false;
            return PlayerIsFrozen[player.PlayerId];
        }

        public bool IsUnfrozenNearby(PlayerControl player)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc != player && !IsFrozen(pc) && !pc.Data.Role.IsImpostor && !pc.inVent && Vector2.Distance(pc.GetRealPosition(), player.transform.position) <= Options.UnfreezeRadius.GetFloat() / 2f)
                    return true;
            }
            return false;
        }

        public void SendNoisemakerAlert(PlayerControl player)
        {
            AntiCheat.RemovedBodies.Add(player.PlayerId);
            player.RpcSendNoisemakerAlert();
            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner || Main.IsModded[pc.PlayerId] || pc == player || (pc.Data.Role.IsImpostor && !GameManager.Instance.LogicOptions.GetNoisemakerImpostorAlert())) continue;
                sender.StartMessage(pc.GetClientId());
                sender.StartRpc(player.NetId, (byte)RpcCalls.MurderPlayer)
                    .WriteNetObject(player)
                    .Write((int)MurderResultFlags.Succeeded)
                    .EndRpc();
                sender.StartRpc(player.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)RoleTypes.Noisemaker)
                    .Write(true)
                    .EndRpc();
                sender.EndMessage();
            }
            sender.SendMessage();
        }

        public FreezeTagGamemode()
        {
            Gamemode = Gamemodes.FreezeTag;
            PetAction = false;
            DisableTasks = false;
            PlayerIsFrozen = new Dictionary<byte, bool>();
            UnfreezeTimer = new Dictionary<byte, float>();
            CompleteTaskTimer = new Dictionary<byte, float>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                PlayerIsFrozen[pc.PlayerId] = false;
                UnfreezeTimer[pc.PlayerId] = 0f;
                CompleteTaskTimer[pc.PlayerId] = 0f;
            }
        }

        public static FreezeTagGamemode instance;
        public Dictionary<byte, bool> PlayerIsFrozen;
        public Dictionary<byte, float> UnfreezeTimer;
        public Dictionary<byte, float> CompleteTaskTimer;
    }
}