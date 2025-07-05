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
                exiled.Object.SyncPlayerName(false, true);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.GetRole().OnExile(exiled);
                foreach (var addOn in pc.GetAddOns())
                    addOn.OnExile(exiled);
                pc.RpcResetAbilityCooldown();
            }
            if ((exiled == null || exiled.Object == null) && Main.RealOptions.GetBool(BoolOptionNames.ConfirmImpostor))
            {
                int impostors = 0;
                int neutralKillers = 0;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.IsDead || pc.Data.Disconnected) continue;
                    if (pc.GetRole().IsImpostor())
                        ++impostors;
                    if (pc.GetRole().IsNeutralKilling())
                        ++neutralKillers;
                }
                var text = impostors + Utils.ColorString(Palette.ImpostorRed, impostors == 1 ? " impostor" : " impostors");
                if (neutralKillers > 0)
                    text += " and " + neutralKillers + Utils.ColorString(Color.gray, neutralKillers == 1 ? " neutral killer" : " neutral killers");
                text += " remain";
                text = Utils.ColorString(Color.white, text);
			    foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.Notify(text);
            }
            if (exiled != null && exiled.Object != null && Main.RealOptions.GetBool(BoolOptionNames.ConfirmImpostor))
                exiled.PlayerName = Main.StandardNames[exiled.PlayerId];
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
            foreach (var addOn in player.GetAddOns())
                addOn.OnHudUpate(__instance);
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
            if (PlayerControl.LocalPlayer.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = PlayerControl.LocalPlayer.GetRole() as Droner;
                if (dronerRole != null && dronerRole.RealPosition != null)
                    return;
            }
            if (PlayerControl.LocalPlayer.GetRole().IsImpostor() && !MeetingHud.Instance && !IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
            {
                __instance.Close();
                __instance.ShowSabotageMap();
            }
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = PlayerControl.LocalPlayer.GetRole() as Droner;
                if (dronerRole != null && dronerRole.RealPosition != null)
                {
                    __instance.Close();
                    __instance.ShowNormalMap();
                    return;
                }
            }
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
            foreach (var addOn in PlayerControl.LocalPlayer.GetAddOns())
            {
                __instance.RoleText.text += " + " + Utils.ColorString(addOn.Color, addOn.AddOnName);
                __instance.RoleBlurbText.text += "\n" + Utils.ColorString(addOn.Color, addOn.AddOnDescription);
            }
        }

        public override void OnVotingComplete(MeetingHud __instance, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.GetRole().OnVotingComplete(__instance, states, exiled, tie);
                foreach (var addOn in pc.GetAddOns())
                    addOn.OnVotingComplete(__instance, states, exiled, tie);
            }
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
                        name += " and " + neutralKillers + Utils.ColorString(Color.gray, neutralKillers == 1 ? " neutral killer" : " neutral killers");
                    name += " remain.<size=0>";
                    exiled.PlayerName = name;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(exiled.Object.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, -1);
                    writer.Write(exiled.NetId);
                    writer.Write(name);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }, 5f, "Custom exile message");
        }

        public override bool OnCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            var player = Utils.GetPlayerById(srcPlayerId);
            if (player == null) return true;
            if (!player.GetRole().OnCastVote(__instance, suspectPlayerId))
                return false;
            bool cancel = false;
            foreach (var addOn in player.GetAddOns())
            {
                if (!addOn.OnCastVote(__instance, suspectPlayerId))
                    cancel = true;
            }
            return !cancel;
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

            List<CustomRoles> PriorityCrewmateRoles = new();
            List<CustomRoles> PriorityImpostorRoles = new();
            List<CustomRoles> PriorityBenignNeutralRoles = new();
            List<CustomRoles> PriorityEvilNeutralRoles = new();
            List<CustomRoles> PriorityKillingNeutralRoles = new();

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
                        if (CustomRolesHelper.GetRoleChance(role) > 100)
                            PriorityCrewmateRoles.Add(role);
                        else if (rand.Next(1, 101) <= CustomRolesHelper.GetRoleChance(role))
                            CrewmateRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsImpostor(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (CustomRolesHelper.GetRoleChance(role) > 100)
                            PriorityImpostorRoles.Add(role);
                        else if (rand.Next(1, 101) <= CustomRolesHelper.GetRoleChance(role))
                            ImpostorRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsNeutralBenign(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (CustomRolesHelper.GetRoleChance(role) > 100)
                            PriorityBenignNeutralRoles.Add(role);
                        else if (rand.Next(1, 101) <= CustomRolesHelper.GetRoleChance(role))
                            BenignNeutralRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsNeutralEvil(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (CustomRolesHelper.GetRoleChance(role) > 100)
                            PriorityEvilNeutralRoles.Add(role);
                        else if (rand.Next(1, 101) <= CustomRolesHelper.GetRoleChance(role))
                            EvilNeutralRoles.Add(role);
                    }
                }
                else if (CustomRolesHelper.IsNeutralKilling(role))
                {
                    for (int i = 1; i <= CustomRolesHelper.GetRoleCount(role); ++i)
                    {
                        if (CustomRolesHelper.GetRoleChance(role) > 100)
                            PriorityKillingNeutralRoles.Add(role);
                        else if (rand.Next(1, 101) <= CustomRolesHelper.GetRoleChance(role))
                            KillingNeutralRoles.Add(role);
                    }
                }
            }

            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
                AllPlayers.Add(pc);

            while (PriorityKillingNeutralRoles.Any() && killingNeutrals > 0 && ChosenKillingNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, PriorityKillingNeutralRoles.Count);
                ChosenKillingNeutralRoles.Add(PriorityKillingNeutralRoles[index]);
                PriorityKillingNeutralRoles.RemoveAt(index);
                --killingNeutrals;
            }
            while (KillingNeutralRoles.Any() && killingNeutrals > 0 && ChosenKillingNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, KillingNeutralRoles.Count);
                ChosenKillingNeutralRoles.Add(KillingNeutralRoles[index]);
                KillingNeutralRoles.RemoveAt(index);
                --killingNeutrals;
            }

            while (PriorityEvilNeutralRoles.Any() && evilNeutrals > 0 && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, PriorityEvilNeutralRoles.Count);
                ChosenEvilNeutralRoles.Add(PriorityEvilNeutralRoles[index]);
                PriorityEvilNeutralRoles.RemoveAt(index);
                --evilNeutrals;
            }
            while (EvilNeutralRoles.Any() && evilNeutrals > 0 && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, EvilNeutralRoles.Count);
                ChosenEvilNeutralRoles.Add(EvilNeutralRoles[index]);
                EvilNeutralRoles.RemoveAt(index);
                --evilNeutrals;
            }

            while (PriorityBenignNeutralRoles.Any() && benignNeutrals > 0 && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count + ChosenBenignNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, PriorityBenignNeutralRoles.Count);
                ChosenBenignNeutralRoles.Add(PriorityBenignNeutralRoles[index]);
                PriorityBenignNeutralRoles.RemoveAt(index);
                --benignNeutrals;
            }
            while (BenignNeutralRoles.Any() && benignNeutrals > 0 && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count + ChosenBenignNeutralRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, BenignNeutralRoles.Count);
                ChosenBenignNeutralRoles.Add(BenignNeutralRoles[index]);
                BenignNeutralRoles.RemoveAt(index);
                --benignNeutrals;
            }

            while (PriorityCrewmateRoles.Any() && ChosenKillingNeutralRoles.Count + ChosenEvilNeutralRoles.Count + ChosenBenignNeutralRoles.Count + ChosenCrewmateRoles.Count < AllPlayers.Count - Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, PriorityCrewmateRoles.Count);
                ChosenCrewmateRoles.Add(PriorityCrewmateRoles[index]);
                PriorityCrewmateRoles.RemoveAt(index);
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

            while (PriorityImpostorRoles.Any() && ChosenImpostorRoles.Count < Main.RealOptions.GetInt(Int32OptionNames.NumImpostors))
            {
                var index = rand.Next(0, PriorityImpostorRoles.Count);
                ChosenImpostorRoles.Add(PriorityImpostorRoles[index]);
                PriorityImpostorRoles.RemoveAt(index);
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

            List<AddOns> ChosenAddOns = new();
            List<AddOns> PriorityChosenAddOns = new();
            foreach (var addOn in Enum.GetValues<AddOns>())
            {
                for (int i = 1; i <= AddOnsHelper.GetAddOnCount(addOn); ++i)
                {
                    if (AddOnsHelper.GetAddOnChance(addOn) > 100)
                        PriorityChosenAddOns.Add(addOn);
                    if (rand.Next(1, 101) <= AddOnsHelper.GetAddOnChance(addOn))
                        ChosenAddOns.Add(addOn);
                }
            }

            while (PriorityChosenAddOns.Any())
            {
                var addOn = PriorityChosenAddOns[rand.Next(0, PriorityChosenAddOns.Count)];
                List<PlayerControl> PotentialPlayers = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if ((pc.GetRole().IsCrewmate() && !AddOnsHelper.CrewmatesCanGet(addOn)) || (pc.GetRole().IsNeutral() && !AddOnsHelper.NeutralsCanGet(addOn)) || (pc.GetRole().IsImpostor() && !AddOnsHelper.ImpostorsCanGet(addOn)))
                        continue;
                    if (pc.GetAddOns().Count >= Options.MaxAddOnsForPlayer.GetInt())
                        continue;
                    if (!pc.GetRole().IsCompatible(addOn))
                        continue;
                    bool compatible = true;
                    foreach (var addon in pc.GetAddOns())
                    {
                        if (!addon.IsCompatible(addOn))
                        {
                            compatible = false;
                            break;
                        }
                    }
                    if (!compatible)
                        continue;
                    PotentialPlayers.Add(pc);
                }
                if (PotentialPlayers.Any())
                {
                    var player = PotentialPlayers[rand.Next(0, PotentialPlayers.Count)];
                    player.RpcSetAddOn(addOn);
                }
                PriorityChosenAddOns.Remove(addOn);
            }
            while (ChosenAddOns.Any())
            {
                var addOn = ChosenAddOns[rand.Next(0, ChosenAddOns.Count)];
                List<PlayerControl> PotentialPlayers = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if ((pc.GetRole().IsCrewmate() && !AddOnsHelper.CrewmatesCanGet(addOn)) || (pc.GetRole().IsNeutral() && !AddOnsHelper.NeutralsCanGet(addOn)) || (pc.GetRole().IsImpostor() && !AddOnsHelper.ImpostorsCanGet(addOn)))
                        continue;
                    if (pc.GetAddOns().Count >= Options.MaxAddOnsForPlayer.GetInt())
                        continue;
                    if (!pc.GetRole().IsCompatible(addOn))
                        continue;
                    bool compatible = true;
                    foreach (var addon in pc.GetAddOns())
                    {
                        if (!addon.IsCompatible(addOn))
                        {
                            compatible = false;
                            break;
                        }
                    }
                    if (!compatible)
                        continue;
                    PotentialPlayers.Add(pc);
                }
                if (PotentialPlayers.Any())
                {
                    var player = PotentialPlayers[rand.Next(0, PotentialPlayers.Count)];
                    player.RpcSetAddOn(addOn);
                }
                ChosenAddOns.Remove(addOn);
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
                        List<byte> list = new();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                list.Add(ar.PlayerId);
                        }
                        Utils.SetDesyncRoleForPlayers(pc, list, RoleTypes.Crewmate, RoleTypes.Impostor);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Impostor;
                        foreach (var ar in list)
                            Main.DesyncRoles[(pc.PlayerId, ar)] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Shapeshifter:
                        List<byte> list2 = new();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                list2.Add(ar.PlayerId);
                        }
                        Utils.SetDesyncRoleForPlayers(pc, list2, RoleTypes.Crewmate, RoleTypes.Shapeshifter);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Shapeshifter;
                        foreach (var ar in list2)
                            Main.DesyncRoles[(pc.PlayerId, ar)] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Phantom:
                        List<byte> list3 = new();
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (ar.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                list3.Add(ar.PlayerId);
                        }
                        Utils.SetDesyncRoleForPlayers(pc, list3, RoleTypes.Crewmate, RoleTypes.Phantom);
                        Main.StandardRoles[pc.PlayerId] = RoleTypes.Phantom;
                        foreach (var ar in list3)
                            Main.DesyncRoles[(pc.PlayerId, ar)] = RoleTypes.Crewmate;
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
            {
                pc.GetRole().OnIntroDestroy();
                foreach (var addOn in pc.GetAddOns())
                    addOn.OnIntroDestroy();
            }  
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.GetRole().IsCrewmate())
                {
                    if (!pc.GetRole().CanUseKillButton() && !pc.GetRole().HasTasks() && !pc.AmOwner && !Main.IsModded[pc.PlayerId])
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
                pc.RpcSetAbilityCooldown(10f);
                if (!pc.AmOwner && !Main.IsModded[pc.PlayerId])
                {
                    string text = Utils.ColorString(pc.GetRole().Color, pc.GetRole().RoleDescription);
                    foreach (var addOn in pc.GetAddOns())
                        text += "\n" + Utils.ColorString(addOn.Color, addOn.AddOnDescription);
                    pc.Notify(text);
                }
            }
        }

        public override void OnPet(PlayerControl pc)
        {
            if (RoleblockTimer[pc.PlayerId] > 0f)
                return;
            pc.GetRole().OnPet();
            foreach (var addOn in pc.GetAddOns())
                addOn.OnPet();
        }

        public override bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            if (!guardian.GetRole().CanUseProtectButton())
                return false;
            if (RoleblockTimer[guardian.PlayerId] > 0f)
                return false;
            if (!guardian.GetRole().OnCheckProtect(target))
                return false;
            bool cancel = false;
            foreach (var addOn in guardian.GetAddOns())
            {
                if (!addOn.OnCheckProtect(target))
                    cancel = true;
            }
            return !cancel;
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (!killer.GetRole().CanUseKillButton() && (!Main.IsModded[killer.PlayerId] || !killer.GetRole().ForceKillButton()))
                return false;
            if (RoleblockTimer[killer.PlayerId] > 0f)
                return false;
            if (!killer.GetRole().OnCheckMurder(target))
                return false;
            bool cancel = false;
            foreach (var addOn in killer.GetAddOns())
            {
                if (!addOn.OnCheckMurder(target))
                    cancel = true;
            }
            if (cancel)
                return false;
            if (!target.GetRole().OnCheckMurderAsTarget(killer))
                return false;
            foreach (var addOn in target.GetAddOns())
            {
                if (!addOn.OnCheckMurderAsTarget(killer))
                    cancel = true;
            }
            if (cancel)
                return false;
            if (!Medic.OnGlobalCheckMurder(killer, target)) return false;
            if (!Romantic.OnGlobalCheckMurder(killer, target)) return false;
            if (!Undertaker.OnGlobalCheckMurder(killer, target)) return false;
            if (!killer.GetRole().OnCheckMurderLate(target))
                return false;
            return true;
        }

        public override void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (PlayerKiller[target.PlayerId] == byte.MaxValue)
                PlayerKiller[target.PlayerId] = killer.PlayerId;
            if (CustomRolesHelper.GetRoleChance(CustomRoles.Altruist) > 0)
                new LateTask(() => target.SetChatVisible(false), 0.2f);
            killer = Utils.GetPlayerById(PlayerKiller[target.PlayerId]);
            if (killer == null) return;
            killer.GetRole().OnMurderPlayer(target);
            foreach (var addOn in killer.GetAddOns())
                addOn.OnMurderPlayer(target);
            target.GetRole().OnMurderPlayerAsTarget(killer);
            foreach (var addOn in target.GetAddOns())
                addOn.OnMurderPlayerAsTarget(killer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.GetRole().OnGlobalMurderPlayer(killer, target);
                foreach (var addOn in pc.GetAddOns())
                    addOn.OnGlobalMurderPlayer(killer, target);
            }
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (Main.Timer < 5f) return false;
            if (!shapeshifter.GetRole().CanUseShiftButton())
                return false;
            if (!shapeshifter.GetRole().OnCheckShapeshift(target))
                return false;
            bool cancel = false;
            foreach (var addOn in shapeshifter.GetAddOns())
            {
                if (!addOn.OnCheckShapeshift(target))
                    cancel = true;
            }
            return !cancel;
        }

        public override void OnShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            shapeshifter.GetRole().OnShapeshift(target);
            foreach (var addOn in shapeshifter.GetAddOns())
                addOn.OnShapeshift(target);
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            if (!force && RoleblockTimer[__instance.PlayerId] > 0f)
            {
                return false;
            }
            if (!force && !Trapster.OnGlobalReportDeadBody(__instance, target)) return false;
            bool report = true;
            if (!force)
            {
                report = __instance.GetRole().OnReportDeadBody(target);
                if (report)
                {
                    foreach (var addOn in __instance.GetAddOns())
                    {
                        if (!addOn.OnReportDeadBody(target))
                            report = false;
                    }
                }
            }
            if (report)
            {
                Shaman.OnGlobalReportDeadBody(__instance, target);
                Arsonist.OnGlobalReportDeadBody(__instance, target);
                if (CustomRolesHelper.GetRoleChance(CustomRoles.Altruist) > 0)
                {
                    foreach (var playerId in PlayersDiedThisRound)
                    {
                        var player = Utils.GetPlayerById(playerId);
                        if (player != null && !player.Data.Disconnected)
                            DestroyableSingleton<RoleManager>.Instance.AssignRoleOnDeath(player, true);
                    }
                    PlayersDiedThisRound.Clear();
                }
                bool isDead = __instance.Data.IsDead;
                __instance.Data.IsDead = false;
                new LateTask(() => {
                    __instance.Data.IsDead = isDead;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        bool syncSettings = IsFrozen[pc.PlayerId] || IsBlinded[pc.PlayerId];
                        FreezeTimer[pc.PlayerId] = 0f;
                        BlindTimer[pc.PlayerId] = 0f;
                        RoleblockTimer[pc.PlayerId] = 0f;
                        IsFrozen[pc.PlayerId] = false;
                        IsBlinded[pc.PlayerId] = false;
                        if (IsRoleblocked[pc.PlayerId])
                            pc.RpcSetRoleblock(false);
                        pc.GetRole().OnMeeting();
                        foreach (var addOn in pc.GetAddOns())
                            addOn.OnMeeting();
                        if (syncSettings)
                            pc.SyncPlayerSettings();
                        Utils.SetAllVentInteractions();
                    }
                }, 0.01f);
                new LateTask(() => Utils.SyncAllPlayersName(true, true, true), 0.5f);
            }
            return report;
        }

        public override void OnFixedUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                foreach (var ar in PlayerControl.AllPlayerControls)
                    NameSymbols[(pc.PlayerId, ar.PlayerId)] = new Dictionary<CustomRoles, (string, Color)>();
            } 
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
                    pc.RpcSetVentInteraction();
                }
                if (syncSettings)
                    pc.SyncPlayerSettings();
                
                if (pc.Data.IsDead)
                    TimeSinceDeath[pc.PlayerId] += Time.fixedDeltaTime;
                else
                    TimeSinceDeath[pc.PlayerId] = 0f;
                
                pc.GetRole().OnFixedUpdate();
                foreach (var addOn in pc.GetAddOns())
                    addOn.OnFixedUpdate();
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            if (RoleblockTimer[player.PlayerId] > 0f)
                return false;
            if (BlockedVents.Contains(id))
                return false;
            if (Main.IsInvisible[player.PlayerId])
                return false;
            if (player.AmOwner && player.GetRole().Role == CustomRoles.Phantom && player.invisibilityAlpha < 1f)
                return false;
            if (!player.GetRole().OnEnterVent(id))
                return false;
            bool cancel = false;
            foreach (var addOn in player.GetAddOns())
            {
                if (!addOn.OnEnterVent(id))
                    cancel = true;
            }
            return !cancel;
        }

        public override void OnCompleteTask(PlayerControl __instance)
        {
            __instance.GetRole().OnCompleteTask();
            foreach (var addOn in __instance.GetAddOns())
                addOn.OnCompleteTask();
        }

        public override bool OnCheckVanish(PlayerControl phantom)
        {
            if (Main.Timer < 5f) return false;
            if (RoleblockTimer[phantom.PlayerId] > 0f)
                return false;
            if (!phantom.GetRole().OnCheckVanish())
                return false;
            bool cancel = false;
            foreach (var addOn in phantom.GetAddOns())
            {
                if (!addOn.OnCheckVanish())
                    cancel = true;
            }
            return !cancel;
        }

        public override void OnAppear(PlayerControl phantom)
        {
            phantom.GetRole().OnAppear();
            foreach (var addOn in phantom.GetAddOns())
                addOn.OnAppear();
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (RoleblockTimer[player.PlayerId] > 0f)
                return false;
            if (!player.GetRole().OnUpdateSystem(__instance, systemType, reader))
                return false;
            bool cancel = false;
            foreach (var addOn in player.GetAddOns())
            {
                if (!addOn.OnUpdateSystem(__instance, systemType, reader))
                    cancel = true;
            }
            return !cancel;
        }

        public override void OnAddVote(int srcClient, int clientId)
        {
            var player = AmongUsClient.Instance.GetClient(srcClient).Character;
            var target = AmongUsClient.Instance.GetClient(clientId).Character;
            player.GetRole().OnAddVote(target);
            foreach (var addOn in player.GetAddOns())
                addOn.OnAddVote(target);
        }

        public override bool OnClimbLadder(PlayerControl player, Ladder source, bool ladderUsed)
        {
            if (!player.GetRole().OnClimbLadder(source, ladderUsed))
                return false;
            bool cancel = false;
            foreach (var addOn in player.GetAddOns())
            {
                if (!addOn.OnClimbLadder(source, ladderUsed))
                    cancel = true;
            }
            if (!cancel && ladderUsed && (player.shouldAppearInvisible || player.invisibilityAlpha < 1f))
                player.RpcResetInvisibility();
            return !cancel;
        }

        public override bool OnUsePlatform(PlayerControl __instance)
        {
            if (IsRoleblocked[__instance.PlayerId])
                return false;
            if (__instance.shouldAppearInvisible || __instance.invisibilityAlpha < 1f)
                return false;
            if (!__instance.GetRole().OnUsePlatform())
                return false;
            bool cancel = false;
            foreach (var addOn in __instance.GetAddOns())
            {
                if (!addOn.OnUsePlatform())
                    cancel = true;
            }
            return !cancel;
        }

        public override bool OnCheckUseZipline(PlayerControl target, ZiplineBehaviour ziplineBehaviour, bool fromTop)
        {
            if (IsRoleblocked[target.PlayerId])
                return false;
            if (target.shouldAppearInvisible || target.invisibilityAlpha < 1f)
                return false;
            if (!target.GetRole().OnCheckUseZipline(ziplineBehaviour, fromTop))
                return false;
            bool cancel = false;
            foreach (var addOn in target.GetAddOns())
            {
                if (!addOn.OnCheckUseZipline(ziplineBehaviour, fromTop))
                    cancel = true;
            }
            return !cancel;
        }

        public override bool OnCheckSporeTrigger(PlayerControl __instance, Mushroom mushroom)
        {
            if (!__instance.GetRole().OnCheckSporeTrigger(mushroom))
                return false;
            bool cancel = false;
            foreach (var addOn in __instance.GetAddOns())
            {
                if (!addOn.OnCheckSporeTrigger(mushroom))
                    cancel = true;
            }
            return !cancel;
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
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
            if (player.Data.IsDead)
                opt.SetBool(BoolOptionNames.AnonymousVotes, false);
            opt.SetBool(BoolOptionNames.ConfirmImpostor, false);
            opt = player.GetRole().ApplyGameOptions(opt);
            foreach (var addOn in player.GetAddOns())
                opt = addOn.ApplyGameOptions(opt);
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            string prefix = "";
            string postfix = "";
            if (player == seer || seer.Data.Role.IsDead || (player.GetRole().IsImpostor() && seer.GetRole().IsImpostor() && Options.SeeTeammateRoles.GetBool()) || player.GetRole().IsRoleRevealed(seer) || seer.GetRole().SeePlayerRole(player))
            {
                foreach (var addOn in player.GetAddOns())
                    prefix += "<size=1.8>" + Utils.ColorString(addOn.Color, "(" + addOn.AddOnName + ")") + " </size>";
            }
            if (player == seer || seer.Data.Role.IsDead || (player.GetRole().IsImpostor() && seer.GetRole().IsImpostor() && Options.SeeTeammateRoles.GetBool()) || player.GetRole().IsRoleRevealed(seer) || seer.GetRole().SeePlayerRole(player))
                prefix += "<size=1.8>" + Utils.ColorString(player.GetRole().Color, player.GetRole().RoleName + player.GetRole().GetProgressText(false)) + "</size>\n";
            foreach (var symbol in NameSymbols[(player.PlayerId, seer.PlayerId)].Values)
                postfix += Utils.ColorString(symbol.Item2, symbol.Item1);
            if (player == seer && !player.Data.IsDead)
            {
                postfix += player.GetRole().GetNamePostfix();
                foreach (var addOn in player.GetAddOns())
                    postfix += addOn.GetNamePostfix();
            }
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

        public void ChangeRole(PlayerControl player, CustomRoles role)
        {
            player.RpcSetCustomRole(role);
            if (player.Data.IsDead)
                player.RpcSetRole(player.GetRole().IsImpostor() ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
            else
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Main.DesyncRoles.ContainsKey((player.PlayerId, pc.PlayerId)))
                        Main.DesyncRoles.Remove((player.PlayerId, pc.PlayerId));
                    if (Main.DesyncRoles.ContainsKey((pc.PlayerId, player.PlayerId)))
                        Main.DesyncRoles.Remove((pc.PlayerId, player.PlayerId));
                }
                switch (player.GetRole().BaseRole)
                {
                    case BaseRoles.Crewmate:
                        player.RpcSetRoleV2(RoleTypes.Crewmate);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        break;
                    case BaseRoles.Scientist:
                        player.RpcSetRoleV2(RoleTypes.Scientist);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Scientist;
                        break;
                    case BaseRoles.Engineer:
                        player.RpcSetRoleV2(RoleTypes.Engineer);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Engineer;
                        break;
                    case BaseRoles.Noisemaker:
                        player.RpcSetRoleV2(RoleTypes.Noisemaker);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Noisemaker;
                        break;
                    case BaseRoles.Tracker:
                        player.RpcSetRoleV2(RoleTypes.Tracker);
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Tracker;
                        break;
                    case BaseRoles.Impostor:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Impostor;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Impostor, pc);
                        }
                        break;
                    case BaseRoles.Shapeshifter:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Shapeshifter;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Shapeshifter, pc);
                        }
                        break;
                    case BaseRoles.Phantom:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Phantom;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.GetRole().BaseRole is BaseRoles.DesyncImpostor or BaseRoles.DesyncShapeshifter or BaseRoles.DesyncPhantom)
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Phantom, pc);
                        }
                        break;
                    case BaseRoles.DesyncImpostor:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player)
                                player.RpcSetDesyncRole(RoleTypes.Impostor, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                        }
                        break;
                    case BaseRoles.DesyncShapeshifter:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player)
                                player.RpcSetDesyncRole(RoleTypes.Shapeshifter, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                        }
                        break;
                    case BaseRoles.DesyncPhantom:
                        Main.StandardRoles[player.PlayerId] = RoleTypes.Crewmate;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player)
                                player.RpcSetDesyncRole(RoleTypes.Phantom, pc);
                            else
                                player.RpcSetDesyncRole(RoleTypes.Crewmate, pc);
                        }
                        break;
                }
                switch (player.GetRole().BaseRole)
                {
                    case BaseRoles.DesyncImpostor:
                    case BaseRoles.DesyncShapeshifter:
                    case BaseRoles.DesyncPhantom:
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc != player)
                            {
                                if (pc.GetRole().BaseRole is BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom)
                                    pc.RpcSetDesyncRole(RoleTypes.Crewmate, player);
                                else
                                    pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], player);
                            }
                        }
                        break;
                    default:
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc != player)
                                pc.RpcSetDesyncRole(Main.StandardRoles[pc.PlayerId], player);
                        }
                        break;
                }
                Main.KillCooldowns[player.PlayerId] = 10f;
                player.GetRole().OnIntroDestroy();
            }
            if (!player.GetRole().IsCrewmate())
            {
                if (!player.GetRole().CanUseKillButton() && !player.GetRole().HasTasks() && !player.AmOwner && !Main.IsModded[player.PlayerId])
                {
                    foreach (var task in player.Data.Tasks)
                    {
                        if (task.Complete) continue;
                        player.RpcCompleteTask(task.Id);
                    }
                }
                player.Data.Tasks.Clear();
                player.Data.MarkDirty();
            }
            else if (player.Data.Tasks.Count == 0 && DefaultTasks.ContainsKey(player.PlayerId))
            {
                player.Data.Tasks.Clear();
		        for (int i = 0; i < DefaultTasks[player.PlayerId].Count; i++)
			        player.Data.Tasks.Add(new NetworkedPlayerInfo.TaskInfo(DefaultTasks[player.PlayerId][i].TypeId, DefaultTasks[player.PlayerId][i].Id));
                player.Data.MarkDirty();
            }
            new LateTask(() => player.RpcSetKillTimer(9.8f), 0.2f);
            player.RpcResetAbilityCooldown();
            player.SyncPlayerSettings();
            player.RpcSetVentInteraction();
        }

        public ClassicGamemode()
        {
            Gamemode = Gamemodes.Classic;
            PetAction = true;
            DisableTasks = false;
            AllPlayersRole = new Dictionary<byte, CustomRole>();
            AllPlayersAddOns = new Dictionary<byte, List<AddOn>>();
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
            DefaultTasks = new Dictionary<byte, Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>();
            CompletedTasks = new Dictionary<byte, List<uint>>();
            PlayersDiedThisRound = new List<byte>();
            NameSymbols = new Dictionary<(byte, byte), Dictionary<CustomRoles, (string, Color)>>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                AllPlayersAddOns[pc.PlayerId] = new List<AddOn>();
                FreezeTimer[pc.PlayerId] = 0f;
                BlindTimer[pc.PlayerId] = 0f;
                RoleblockTimer[pc.PlayerId] = 0f;
                IsFrozen[pc.PlayerId] = false;
                IsBlinded[pc.PlayerId] = false;
                IsRoleblocked[pc.PlayerId] = false;
                IsOnPetAbilityCooldown[pc.PlayerId] = false;
                PlayerKiller[pc.PlayerId] = byte.MaxValue;
                TimeSinceDeath[pc.PlayerId] = 0f;
                foreach (var ar in PlayerControl.AllPlayerControls)
                    NameSymbols[(pc.PlayerId, ar.PlayerId)] = new Dictionary<CustomRoles, (string, Color)>();
            }
        }

        public static ClassicGamemode instance;
        public Dictionary<byte, CustomRole> AllPlayersRole;
        public Dictionary<byte, List<AddOn>> AllPlayersAddOns;
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
        public Dictionary<byte, Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>> DefaultTasks;
        public Dictionary<byte, List<uint>> CompletedTasks;
        public List<byte> PlayersDiedThisRound;
        public Dictionary<(byte, byte), Dictionary<CustomRoles, (string, Color)>> NameSymbols;
    }
}