using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using System;
using System.Linq;
using Hazel;

namespace MoreGamemodes
{
    public class ClassicGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            if (exiled != null && exiled.Object != null)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    exiled.Object.RpcSetNamePrivate(exiled.Object.BuildPlayerName(pc, false), pc, true);
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.GetRole().OnExile(exiled);
                pc.RpcResetAbilityCooldown();
            } 
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            __instance.FilterText.text = !__instance.HauntTarget.Data.Role.IsDead ? __instance.HauntTarget.GetRole().RoleName : "Ghost";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (!player.GetRole().CanUseKillButton() && player.GetRole().ForceKillButton() && !player.Data.IsDead)
                __instance.KillButton.ToggleVisible(true);
            player.GetRole().OnHudUpate(__instance);
            if (IsRoleblocked[player.PlayerId])
            {
                __instance.AbilityButton.SetDisabled();
                __instance.AdminButton.SetDisabled();
                __instance.ImpostorVentButton.SetDisabled();
                __instance.KillButton.SetDisabled();
                __instance.PetButton.SetDisabled();
                __instance.ReportButton.SetDisabled();
                __instance.SabotageButton.SetDisabled();
                __instance.UseButton.SetDisabled();
            }
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var role = PlayerControl.LocalPlayer.GetRole();
            __instance.taskText.text = Utils.ColorString(role.Color, role.RoleName + "\n" + role.RoleDescription + "\n\n");
            if (!role.IsCrewmate() && !role.CanUseKillButton())
                __instance.taskText.text += Utils.ColorString(role.Color, "Fake Tasks:\n");
            __instance.taskText.text += str;
        }

        public override void OnShowNormalMap(MapBehaviour __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole().IsImpostor() && !MeetingHud.Instance && !IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
            {
                __instance.Close();
                __instance.ShowSabotageMap();
            }
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (!PlayerControl.LocalPlayer.GetRole().IsImpostor() || MeetingHud.Instance || IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }

        public override void OnToggleHighlight(PlayerControl __instance)
        {
            var player = PlayerControl.LocalPlayer;
			__instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", player.GetRole().Color);
        }

        public override void OnSetOutline(Vent __instance, bool mainTarget)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.myRend.material.SetColor("_OutlineColor", player.GetRole().Color);
            __instance.myRend.material.SetColor("_AddColor", mainTarget ? player.GetRole().Color : Color.clear);
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole().IsNeutral())
            {
                __instance.TeamTitle.text = "Neutral";
                __instance.TeamTitle.color = Color.gray;
                __instance.BackgroundBar.material.color = Color.gray;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.color = Color.gray;
                __instance.ImpostorText.text = "You win on your own!";
            }
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            var role = PlayerControl.LocalPlayer.GetRole();
            __instance.YouAreText.color = role.Color;
            __instance.RoleText.text = role.RoleName;
            __instance.RoleText.color = role.Color;
            __instance.RoleBlurbText.color = role.Color;
            __instance.RoleBlurbText.text = role.RoleDescription;
        }

        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.GetRole().OnVotingComplete(__instance, states, exiled, tie);
            new LateTask(() => {
                if (exiled != null && exiled.Object != null && Main.RealOptions.GetBool(BoolOptionNames.ConfirmImpostor))
                {
                    int impostors = 0;
                    int neutralKillers = 0;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data.IsDead || pc.Data.Disconnected || pc == exiled.Object) continue;
                        if (pc.GetRole().IsImpostor())
                            ++impostors;
                        if (pc.GetRole().IsNeutralKilling())
                            ++neutralKillers;
                    }
                    var role = exiled.Object.GetRole();
                    string name = Main.StandardNames[exiled.PlayerId] + " was The " + Utils.ColorString(role.Color, role.RoleName) + " (";
                    if (role.IsCrewmate())
                        name += Utils.ColorString(Palette.CrewmateBlue, "Crewmate");
                    else if (role.IsImpostor())
                        name += Utils.ColorString(Palette.ImpostorRed, "Impostor");
                    else if (role.IsNeutral())
                        name += Utils.ColorString(Color.gray, "Neutral");
                    name += ")\n" + impostors + Utils.ColorString(Palette.ImpostorRed, impostors == 1 ? " impostor" : " impostors");
                    if (neutralKillers > 0)
                        name += " and " + neutralKillers + Utils.ColorString(Color.gray, impostors == 1 ? " neutral killer" : "neutral killers");
                    name += " remain.<size=0>";
			        exiled.Object.SetName(name);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(exiled.Object.NetId, (byte)RpcCalls.SetName, SendOption.None, -1);
		            writer.Write(exiled.Object.Data.NetId);
                    writer.Write(name);
		            AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }, 1f, "Custom exile message");
        }

        public override bool OnCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            var player = Utils.GetPlayerById(srcPlayerId);
            if (player == null) return true;
            return player.GetRole().OnCastVote(__instance, suspectPlayerId);
        }

        public override bool OnSelectRolesPrefix()
        {
            var rand = new System.Random();
            var killingNeutrals = rand.Next(Options.MinKillingNeutrals.GetInt(), Options.MaxKillingNeutrals.GetInt() + 1);
            var evilNeutrals = rand.Next(Options.MinEvilNeutrals.GetInt(), Options.MaxEvilNeutrals.GetInt() + 1);
            var benignNeutrals = rand.Next(Options.MinBenignNeutrals.GetInt(), Options.MaxBenignNeutrals.GetInt() + 1);

            List<CustomRoles> CrewmateRoles = new();
            List<CustomRoles> ImpostorRoles = new();
            List<CustomRoles> BenignNeutralRoles = new();
            List<CustomRoles> EvilNeutralRoles = new();
            List<CustomRoles> KillingNeutralRoles = new();

            List<CustomRoles> ChosenCrewmateRoles = new();
            List<CustomRoles> ChosenImpostorRoles = new();
            List<CustomRoles> ChosenBenignNeutralRoles = new();
            List<CustomRoles> ChosenEvilNeutralRoles = new();
            List<CustomRoles> ChosenKillingNeutralRoles = new();
            List<CustomRoles> ChosenRoles = new();

            foreach (var role in Enum.GetValues<CustomRoles>())
            {
                if (role is CustomRoles.Crewmate or CustomRoles.Impostor) continue;
                if (CustomRolesHelper.IsCrewmate(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (rand.Next(1, 100) <= CustomRolesHelper.GetRoleChance(role))
                            CrewmateRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsImpostor(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (rand.Next(1, 100) <= CustomRolesHelper.GetRoleChance(role))
                            ImpostorRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsNeutralBenign(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (rand.Next(1, 100) <= CustomRolesHelper.GetRoleChance(role))
                            BenignNeutralRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsNeutralEvil(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (rand.Next(1, 100) <= CustomRolesHelper.GetRoleChance(role))
                            EvilNeutralRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsNeutralKilling(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (rand.Next(1, 100) <= CustomRolesHelper.GetRoleChance(role))
                            KillingNeutralRoles.Add(role);
                    }
                }
            }

            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
                AllPlayers.Add(pc);

            while (KillingNeutralRoles.Any() && killingNeutrals > 0 && ChosenKillingNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, KillingNeutralRoles.Count);
                ChosenKillingNeutralRoles.Add(KillingNeutralRoles[index]);
                KillingNeutralRoles.RemoveAt(index);
                --killingNeutrals;
            }

            while (EvilNeutralRoles.Any() && evilNeutrals > 0 && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, EvilNeutralRoles.Count);
                ChosenEvilNeutralRoles.Add(EvilNeutralRoles[index]);
                EvilNeutralRoles.RemoveAt(index);
                --evilNeutrals;
            }

            while (BenignNeutralRoles.Any() && benignNeutrals > 0 && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count + ChosenBenignNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, BenignNeutralRoles.Count);
                ChosenBenignNeutralRoles.Add(BenignNeutralRoles[index]);
                BenignNeutralRoles.RemoveAt(index);
                --benignNeutrals;
            }

            while (CrewmateRoles.Any() && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count + ChosenBenignNeutralRoles.Count + ChosenCrewmateRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, CrewmateRoles.Count);
                ChosenCrewmateRoles.Add(CrewmateRoles[index]);
                CrewmateRoles.RemoveAt(index);
            }
            while (ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count + ChosenBenignNeutralRoles.Count + ChosenCrewmateRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                ChosenCrewmateRoles.Add(CustomRoles.Crewmate);
            }

            while (ImpostorRoles.Any() && ChosenImpostorRoles.Count < Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, ImpostorRoles.Count);
                ChosenImpostorRoles.Add(ImpostorRoles[index]);
                ImpostorRoles.RemoveAt(index);
            }
            while (ChosenImpostorRoles.Count < Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                ChosenImpostorRoles.Add(CustomRoles.Impostor);
            }

            foreach (var role in ChosenKillingNeutralRoles)
                ChosenRoles.Add(role);
            foreach (var role in ChosenEvilNeutralRoles)
                ChosenRoles.Add(role);
            foreach (var role in ChosenBenignNeutralRoles)
                ChosenRoles.Add(role);
            foreach (var role in ChosenCrewmateRoles)
                ChosenRoles.Add(role);
            foreach (var role in ChosenImpostorRoles)
                ChosenRoles.Add(role);
            
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var role = ChosenRoles[rand.Next(0, ChosenRoles.Count)];
                pc.RpcSetCustomRole(role);
                ChosenRoles.Remove(role);
            }

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                switch (pc.GetRole().BaseRole)
                {
                    case BaseRoles.Crewmate:
                        pc.RpcSetRole(RoleTypes.Crewmate);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Scientist:
                        pc.RpcSetRole(RoleTypes.Scientist);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Scientist;
                        break;
                    case BaseRoles.Engineer:
                        pc.RpcSetRole(RoleTypes.Engineer);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Engineer;
                        break;
                    case BaseRoles.Noisemaker:
                        pc.RpcSetRole(RoleTypes.Noisemaker);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Noisemaker;
                        break;
                    case BaseRoles.Tracker:
                        pc.RpcSetRole(RoleTypes.Tracker);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Tracker;
                        break;
                    case BaseRoles.Impostor:
                        List<PlayerControl> list = new();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                list.Add(ar);
                        }
                        Utils.SetDesyncRoleForPlayers(pc, list, RoleTypes.Crewmate, RoleTypes.Impostor);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Impostor;
                        foreach (var ar in list)
                            Main.DesyncRoles[(pc.PlayerId, ar.PlayerId)] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Shapeshifter:
                        List<PlayerControl> list2 = new();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                list2.Add(ar);
                        }
                        Utils.SetDesyncRoleForPlayers(pc, list2, RoleTypes.Crewmate, RoleTypes.Shapeshifter);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Shapeshifter;
                        foreach (var ar in list2)
                            Main.DesyncRoles[(pc.PlayerId, ar.PlayerId)] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Phantom:
                        List<PlayerControl> list3 = new();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                list3.Add(ar);
                        }
                        Utils.SetDesyncRoleForPlayers(pc, list3, RoleTypes.Crewmate, RoleTypes.Phantom);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Phantom;
                        foreach (var ar in list3)
                            Main.DesyncRoles[(pc.PlayerId, ar.PlayerId)] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.DesyncImpostor:
                        Utils.SetDesyncRoleForPlayer(pc, RoleTypes.Impostor, RoleTypes.Crewmate);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Crewmate;
                        Main.DesyncRoles[(pc.PlayerId, pc.PlayerId)] = RoleTypes.Impostor;
                        break;
                    case BaseRoles.DesyncShapeshifter:
                        Utils.SetDesyncRoleForPlayer(pc, RoleTypes.Shapeshifter, RoleTypes.Crewmate);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Crewmate;
                        Main.DesyncRoles[(pc.PlayerId, pc.PlayerId)] = RoleTypes.Shapeshifter;
                        break;
                    case BaseRoles.DesyncPhantom:
                        Utils.SetDesyncRoleForPlayer(pc, RoleTypes.Phantom, RoleTypes.Crewmate);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Crewmate;
                        Main.DesyncRoles[(pc.PlayerId, pc.PlayerId)] = RoleTypes.Phantom;
                        break;
                }
            }
            return false;
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.GetRole().OnIntroDestroy();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.GetRole().IsCrewmate())
                {
                    if (!pc.GetRole().CanUseKillButton() && !pc.AmOwner && !Main.IsModded[pc.PlayerId])
                    {
                        foreach (var task in pc.Data.Tasks)
                        {
                            if (task.Complete) continue;
                            pc.RpcCompleteTask(task.Id);
                        }
                    }
                    pc.Data.Tasks.Clear();
                    pc.Data.MarkDirty();
                }
                pc.RpcResetAbilityCooldown();
                if (!pc.AmOwner && !Main.IsModded[pc.PlayerId])
                    pc.Notify(Utils.ColorString(pc.GetRole().Color, pc.GetRole().RoleDescription));
            }
        }

        public override void OnPet(PlayerControl pc)
        {
            if (RoleblockTimer[pc.PlayerId] > 0f)
                return;
            pc.GetRole().OnPet();
        }

        public override bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            if (!guardian.GetRole().CanUseProtectButton())
                return false;
            if (RoleblockTimer[guardian.PlayerId] > 0f)
                return false;
            return guardian.GetRole().OnCheckProtect(target);
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (!killer.GetRole().CanUseKillButton() && (!Main.IsModded[killer.PlayerId] || !killer.GetRole().ForceKillButton()))
                return false;
            if (RoleblockTimer[killer.PlayerId] > 0f)
                return false;
            if (!killer.GetRole().OnCheckMurder(target) || !target.GetRole().OnCheckMurderAsTarget(killer))
                return false;
            if (!Medic.OnGlobalCheckMurder(killer, target)) return false;
            return true;
        }

        public override void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            PlayerKiller[target.PlayerId] = killer.PlayerId;
            killer.GetRole().OnMurderPlayer(target);
            target.GetRole().OnMurderPlayerAsTarget(killer);
            foreach (var pc in PlayerControl.AllPlayerControls)
                pc.GetRole().OnGlobalMurderPlayer(killer, target);
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (Main.Timer < 5f) return false;
            if (!shapeshifter.GetRole().CanUseShiftButton())
                return false;
            return shapeshifter.GetRole().OnCheckShapeshift(target);
        }

        public override void OnShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            shapeshifter.GetRole().OnShapeshift(target);
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            if (RoleblockTimer[__instance.PlayerId] > 0f)
            {
                return false;
            }
            if (!Trapster.OnGlobalReportDeadBody(__instance, target)) return false;
            bool report = __instance.GetRole().OnReportDeadBody(target);
            if (report)
            {
                new LateTask(() => {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        FreezeTimer[pc.PlayerId] = 0f;
                        BlindTimer[pc.PlayerId] = 0f;
                        RoleblockTimer[pc.PlayerId] = 0f;
                        IsFrozen[pc.PlayerId] = false;
                        IsBlinded[pc.PlayerId] = false;
                        if (IsRoleblocked[pc.PlayerId])
                            pc.RpcSetRoleblock(false);
                        pc.GetRole().OnMeeting();
                        Utils.SyncAllSettings();
                        Utils.SetAllVentInteractions();
                    }
                }, 0.01f);
                new LateTask(() => {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, true, true), ar, true);
                    }
                }, 0.5f);
            }
            return report;
        }

        public override void OnFixedUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                bool syncSettings = false;
                if (IsFrozen[pc.PlayerId] && FreezeTimer[pc.PlayerId] > 0f)
                {
                    FreezeTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (IsFrozen[pc.PlayerId] && FreezeTimer[pc.PlayerId] <= 0f)
                {
                    FreezeTimer[pc.PlayerId] = 0f;
                    IsFrozen[pc.PlayerId] = false;
                    syncSettings = true;
                }
                if (IsBlinded[pc.PlayerId] && BlindTimer[pc.PlayerId] > 0f)
                {
                    BlindTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (IsBlinded[pc.PlayerId] && BlindTimer[pc.PlayerId] <= 0f)
                {
                    BlindTimer[pc.PlayerId] = 0f;
                    IsBlinded[pc.PlayerId] = false;
                    syncSettings = true;
                }
                if (IsRoleblocked[pc.PlayerId] && RoleblockTimer[pc.PlayerId] > 0f)
                {
                    RoleblockTimer[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (IsRoleblocked[pc.PlayerId] && RoleblockTimer[pc.PlayerId] <= 0f)
                {
                    RoleblockTimer[pc.PlayerId] = 0f;
                    pc.RpcSetRoleblock(false);
                    syncSettings = true;
                    pc.RpcSetVentInteraction();
                }
                if (syncSettings)
                    pc.SyncPlayerSettings();
                
                if (pc.Data.IsDead)
                    TimeSinceDeath[pc.PlayerId] += Time.fixedDeltaTime;
                else
                    TimeSinceDeath[pc.PlayerId] = 0f;

                if (pc.Data.IsDead) continue;
                pc.GetRole().OnFixedUpdate();
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            if (RoleblockTimer[player.PlayerId] > 0f)
                return false;
            if (BlockedVents.Contains(id))
                return false;
            if (player.shouldAppearInvisible || player.invisibilityAlpha < 1f)
                return false;
            return player.GetRole().OnEnterVent(id) && GameManager.Instance.LogicOptions.MapId != 3;
        }

        public override void OnCompleteTask(PlayerControl __instance)
        {
            if (__instance.Data.IsDead) return;
            __instance.GetRole().OnCompleteTask();
        }

        public override bool OnCheckVanish(PlayerControl phantom)
        {
            if (Main.Timer < 5f) return false;
            if (RoleblockTimer[phantom.PlayerId] > 0f)
                return false;
            return phantom.GetRole().OnCheckVanish();
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (RoleblockTimer[player.PlayerId] > 0f)
                return false;
            return player.GetRole().OnUpdateSystem(__instance, systemType, reader);
        }

        public override void OnAddVote(int srcClient, int clientId)
        {
            var player = AmongUsClient.Instance.GetClient(srcClient).Character;
            var target = AmongUsClient.Instance.GetClient(clientId).Character;
            player.GetRole().OnAddVote(target);
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            if (player.GetRole() == null) return opt;
            if ((player.GetRole().IsImpostor() || player.GetRole().IsNeutralKilling()) && !player.GetRole().CanUseKillButton())
            {
                if (Utils.IsActive(SystemTypes.Electrical))
                    opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod) * 5f);
                else
                    opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod));
            }
            if ((player.GetRole().IsCrewmate() || player.GetRole().IsNeutralBenign() || player.GetRole().IsNeutralEvil()) && player.GetRole().CanUseKillButton())
            {
                if (Utils.IsActive(SystemTypes.Electrical))
                    opt.SetFloat(FloatOptionNames.ImpostorLightMod, opt.GetFloat(FloatOptionNames.CrewLightMod) / 5f);
                else
                    opt.SetFloat(FloatOptionNames.ImpostorLightMod, opt.GetFloat(FloatOptionNames.CrewLightMod));
                opt.SetBool(BoolOptionNames.NoisemakerImpostorAlert, true);
            }
            if (IsFrozen[player.PlayerId])
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            if (IsBlinded[player.PlayerId])
            {
                opt.SetFloat(FloatOptionNames.CrewLightMod, 0f);
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
            }
            opt.SetBool(BoolOptionNames.ConfirmImpostor, false);
            opt = player.GetRole().ApplyGameOptions(opt);
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            string prefix = "";
            string postfix = "";
            if (player == seer || seer.Data.IsDead || (player.GetRole().IsImpostor() && seer.GetRole().IsImpostor() && Options.SeeTeammateRoles.GetBool()))
                prefix += "<size=1.8>" + Utils.ColorString(player.GetRole().Color, player.GetRole().RoleName + player.GetRole().GetProgressText()) + "</size>\n";
            if (Medic.IsShielded(player) && (player == seer || seer.GetRole().Role == CustomRoles.Medic))
                postfix += Utils.ColorString(CustomRolesHelper.RoleColors[CustomRoles.Medic], "+");
            if (player == seer && !player.Data.IsDead)
                postfix += player.GetRole().GetNamePostfix();
            return prefix + name + postfix;
        }

        public void SetFreezeTimer(PlayerControl player, float timer)
        {
            IsFrozen[player.PlayerId] = true;
            if (timer > FreezeTimer[player.PlayerId])
                FreezeTimer[player.PlayerId] = timer;
        }

        public void SetBlindTimer(PlayerControl player, float timer)
        {
            IsBlinded[player.PlayerId] = true;
            if (timer > BlindTimer[player.PlayerId])
                BlindTimer[player.PlayerId] = timer;
        }

        public void SetRoleblockTimer(PlayerControl player, float timer)
        {
            if (!IsRoleblocked[player.PlayerId])
                player.RpcSetRoleblock(true);
            if (timer > RoleblockTimer[player.PlayerId])
                RoleblockTimer[player.PlayerId] = timer;
        }

        public ClassicGamemode()
        {
            Gamemode = Gamemodes.Classic;
            PetAction = true;
            DisableTasks = false;
            AllPlayersRole = new Dictionary<byte, CustomRole>();
            Winner = CustomWinners.None;
            AdditionalWinners = new List<AdditionalWinners>();
            FreezeTimer = new Dictionary<byte, float>();
            BlindTimer = new Dictionary<byte, float>();
            RoleblockTimer = new Dictionary<byte, float>();
            IsFrozen = new Dictionary<byte, bool>();
            IsBlinded = new Dictionary<byte, bool>();
            IsRoleblocked = new Dictionary<byte, bool>();
            BlockedVents = new List<int>();
            IsOnPetAbilityCooldown = new Dictionary<byte, bool>();
            PlayerKiller = new Dictionary<byte, byte>();
            TimeSinceDeath = new Dictionary<byte, float>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                FreezeTimer[pc.PlayerId] = 0f;
                BlindTimer[pc.PlayerId] = 0f;
                RoleblockTimer[pc.PlayerId] = 0f;
                IsFrozen[pc.PlayerId] = false;
                IsBlinded[pc.PlayerId] = false;
                IsRoleblocked[pc.PlayerId] = false;
                IsOnPetAbilityCooldown[pc.PlayerId] = false;
                PlayerKiller[pc.PlayerId] = byte.MaxValue;
                TimeSinceDeath[pc.PlayerId] = 0f;
            }
        }

        public static ClassicGamemode instance;
        public Dictionary<byte, CustomRole> AllPlayersRole;
        public CustomWinners Winner;
        public List<AdditionalWinners> AdditionalWinners;
        public Dictionary<byte, float> FreezeTimer;
        public Dictionary<byte, float> BlindTimer;
        public Dictionary<byte, float> RoleblockTimer;
        public Dictionary<byte, bool> IsFrozen;
        public Dictionary<byte, bool> IsBlinded;
        public Dictionary<byte, bool> IsRoleblocked;
        public List<int> BlockedVents;
        public Dictionary<byte, bool> IsOnPetAbilityCooldown;
        public Dictionary<byte, byte> PlayerKiller;
        public Dictionary<byte, float> TimeSinceDeath;
    }
}