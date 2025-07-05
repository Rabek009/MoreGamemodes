using System.Collections.Generic;
using UnityEngine;
using System;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class JailbreakGamemode : CustomGamemode
    {
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            var room = player.GetPlainShipRoom();
            if (IsDead[player.PlayerId])
            {
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
            else if (player.Data.IsDead)
            {
                __instance.AbilityButton.ToggleVisible(true);
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
            }
            else
            {
                __instance.AbilityButton.ToggleVisible(true);
                __instance.ImpostorVentButton.ToggleVisible(true);
                __instance.KillButton.ToggleVisible(true);
            }
            if (IsGuard(player))
            {
                if (__instance.KillButton.currentTarget != null && GetJailbreakPlayerType(__instance.KillButton.currentTarget) == JailbreakPlayerTypes.Wanted && __instance.KillButton.currentTarget.shapeshiftTargetPlayerId == -1)
                    __instance.KillButton.OverrideText("Attack");
                else
                    __instance.KillButton.OverrideText("Search");
                if (room != null && room.RoomId == SystemTypes.Reactor && (GameOptionsManager.Instance.CurrentGameOptions.MapId == 0 || GameOptionsManager.Instance.CurrentGameOptions.MapId == 3))
                    __instance.AbilityButton.OverrideText("Repair");
                else
                    __instance.AbilityButton.OverrideText("Buy");
                __instance.PetButton.OverrideText("Next Offer");
            }
            else if (HasEscaped(player))
            {
                __instance.AbilityButton.OverrideText("Help");
            }
            else
            {
                __instance.KillButton.OverrideText("Attack");
                if (room != null && room.RoomId == SystemTypes.Reactor && (GameOptionsManager.Instance.CurrentGameOptions.MapId == 0 || GameOptionsManager.Instance.CurrentGameOptions.MapId == 3) && HasItem(player, InventoryItems.Pickaxe))
                {
                    __instance.AbilityButton.OverrideText("Destroy");
                    __instance.AbilityButton.SetEnabled();
                }
                else if (HasItem(player, InventoryItems.GuardOutfit))
                {
                    __instance.AbilityButton.OverrideText("Craft/Change");
                    __instance.AbilityButton.SetEnabled();
                }
                else
                {
                    __instance.AbilityButton.OverrideText("Craft");
                    int price = 0;
                    InventoryItems requiredItem = InventoryItems.Resources;
                    switch ((Recipes)GetCurrentRecipe(player))
                    {
                        case Recipes.Screwdriver:
                            price = Options.ScrewdriverPrice.GetInt();
                            break;
                        case Recipes.PrisonerWeapon:
                            price = Options.PrisonerWeaponPrice.GetInt() * (GetItemAmount(player, InventoryItems.Weapon) + 1);
                            break;
                        case Recipes.Pickaxe:
                            price = Options.PickaxePrice.GetInt() * (GetItemAmount(player, InventoryItems.Pickaxe) + 1);
                            break;
                        case Recipes.SpaceshipPart:
                            price = Options.SpaceshipPartPrice.GetInt();
                            break;
                        case Recipes.Spaceship:
                            price = Options.RequiredSpaceshipParts.GetInt();
                            requiredItem = InventoryItems.SpaceshipParts;
                            break;
                        case Recipes.BreathingMask:
                            price = Options.BreathingMaskPrice.GetInt();
                            break;
                        case Recipes.GuardOutfit:
                            price = Options.GuardOutfitPrice.GetInt();
                            break;
                        case Recipes.PrisonerArmor:
                            price = Options.PrisonerArmorPrice.GetInt() * (GetItemAmount(player, InventoryItems.Armor) + 1);
                            break;
                        case Recipes.GuardWeapon:
                            price = Options.GuardWeaponPrice.GetInt() * (GetItemAmount(player, InventoryItems.Weapon) + 1);
                            break;
                        case Recipes.EnergyDrink:
                            price = Options.EnergyDrinkPrice.GetInt();
                            break;
                        case Recipes.GuardArmor:
                            price = Options.GuardArmorPrice.GetInt() * (GetItemAmount(player, InventoryItems.Armor) + 1);
                            break;
                    }
                    if (GetItemAmount(player, requiredItem) >= price)
                        __instance.AbilityButton.SetEnabled();
                    else
                        __instance.AbilityButton.SetDisabled();
                }
                __instance.PetButton.OverrideText("Next Recipe");
            }
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            if (HasEscaped(PlayerControl.LocalPlayer))
                __instance.taskText.text = Utils.ColorString(Color.green, "Escapist\nYou won! Now spectate the game.");
            else if (IsGuard(PlayerControl.LocalPlayer))
                __instance.taskText.text = Utils.ColorString(Color.blue, "Guard\nStop prisoners from escaping!");
            else
                __instance.taskText.text = Utils.ColorString(Palette.Orange, "Prisoner\nTry to escape!");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnToggleHighlight(PlayerControl __instance)
        {
            if (HasEscaped(PlayerControl.LocalPlayer))
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.green);
            else if (IsGuard(PlayerControl.LocalPlayer))
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.blue);
            else
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Palette.Orange);
        }

        public override Il2CppSystem.Collections.Generic.List<PlayerControl> OnBeginImpostorPrefix(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Team = new();
            Team.Add(PlayerControl.LocalPlayer);
            if (IsGuard(PlayerControl.LocalPlayer))
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (IsGuard(pc) && pc != PlayerControl.LocalPlayer)
                        Team.Add(pc);
                }
            }
            return Team;
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            if (IsGuard(PlayerControl.LocalPlayer))
            {
                __instance.TeamTitle.text = "Guard";
                __instance.TeamTitle.color = Color.blue;
                __instance.BackgroundBar.material.color = Color.blue;
            }
            else
            {
                __instance.TeamTitle.text = "Prisoner";
                __instance.TeamTitle.color = Palette.Orange;
                __instance.BackgroundBar.material.color = Palette.Orange;
            }
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (IsGuard(PlayerControl.LocalPlayer))
            {
                __instance.RoleText.text = "Guard";
                __instance.RoleText.color = Color.blue;
                __instance.RoleBlurbText.text = "Stop prisoners from escaping!";
                __instance.RoleBlurbText.color = Color.blue;
                __instance.YouAreText.color = Color.blue;
            }
            else
            {
                __instance.RoleText.text = "Prisoner";
                __instance.RoleText.color = Palette.Orange;
                __instance.RoleBlurbText.text = "Try to escape!";
                __instance.RoleBlurbText.color = Palette.Orange;
                __instance.YouAreText.color = Palette.Orange;
            }
        }

        public override bool OnSelectRolesPrefix()
        {
            Utils.RpcSetDesyncRoles(RoleTypes.Shapeshifter, RoleTypes.Crewmate);
            return false;
        }

        public override void OnSelectRolesPostfix()
        {
            var rand = new System.Random();
            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead)
                    AllPlayers.Add(pc);
            }
            for (int i = 0; i < Main.RealOptions.GetInt(Int32OptionNames.NumImpostors); ++i)
            {
                if (AllPlayers.Count == 0) break;
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                SendRPC_SetPlayerType(player, JailbreakPlayerTypes.Guard);
                AllPlayers.Remove(player);
            }
            foreach (var player in AllPlayers)
                SendRPC_SetPlayerType(player, JailbreakPlayerTypes.Prisoner);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsGuard(pc))
                {
                    pc.RpcSetColor(1);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.blue;
                    SendRPC_SetCurrentRecipe(pc, 1000);
                }   
                else
                {
                    pc.RpcSetColor(4);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Palette.Orange;
                    SendRPC_SetCurrentRecipe(pc, Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3 ? 1 : 0);
                }
            }
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsGuard(pc))
                {
                    SearchCooldown[pc.PlayerId] = Options.SearchCooldown.GetFloat();
                    SendRPC_SetItemAmount(pc, InventoryItems.Weapon, 3);
                    SendRPC_SetItemAmount(pc, InventoryItems.Armor, 3);
                }
                pc.RpcResetAbilityCooldown();
                PlayerHealth[pc.PlayerId] = IsGuard(pc) ? Options.GuardHealth.GetFloat() : Options.PrisonerHealth.GetFloat();
            }
        }

        public override void OnPet(PlayerControl pc)
        {
            if (ChangeRecipeCooldown[pc.PlayerId] > 0f || IsDead[pc.PlayerId]) return;
            var recipeId = GetCurrentRecipe(pc) + 1;
            if (!IsGuard(pc) && recipeId > 7)
                recipeId = 0;
            if (!IsGuard(pc) && recipeId == 0 && Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3)
                recipeId = 1;
            if (!IsGuard(pc) && recipeId == 1 && GetItemAmount(pc, InventoryItems.Weapon) >= 10)
                recipeId = 2;
            if (!IsGuard(pc) && recipeId == 2 && GetItemAmount(pc, InventoryItems.Pickaxe) >= 10)
                recipeId = 3;
            if (!IsGuard(pc) && recipeId == 7 && GetItemAmount(pc, InventoryItems.Armor) >= 10)
                recipeId = 0;
            if (IsGuard(pc) && recipeId > 1002)
                recipeId = 1000;
            if (IsGuard(pc) && recipeId == 1000 && GetItemAmount(pc, InventoryItems.Weapon) >= 10)
                recipeId = 1001;
            if (IsGuard(pc) && recipeId == 1002 && GetItemAmount(pc, InventoryItems.Armor) >= 10)
                recipeId = 1000;
            SendRPC_SetCurrentRecipe(pc, recipeId);
            ChangeRecipeCooldown[pc.PlayerId] = 0.5f;
        }

        public override bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            if (!HasEscaped(guardian)) return false;
            if (IsGuard(target) || HasEscaped(target) || IsDead[target.PlayerId]) return false;
            SendRPC_SetItemAmount(target, InventoryItems.Resources, Math.Min(GetItemAmount(target, InventoryItems.Resources) + Options.GivenResources.GetInt(), Options.MaximumPrisonerResources.GetInt()));
            guardian.RpcResetAbilityCooldown();
            return false;
        } 

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < 10f || IsDead[killer.PlayerId] || IsDead[target.PlayerId]) return false;
            if (IsGuard(killer))
            {
                if (IsGuard(target) || Main.AllShapeshifts[target.PlayerId] != target.PlayerId)
                {
                    if (SearchCooldown[killer.PlayerId] > 0f) return false;
                    SearchCooldown[killer.PlayerId] = Options.SearchCooldown.GetFloat();
                    if (!IsGuard(target))
                    {
                        target.RpcShapeshift(target, false);
                        SendRPC_SetPlayerType(target, JailbreakPlayerTypes.Wanted);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.red;
                        SendRPC_SetItemAmount(target, InventoryItems.GuardOutfit, 0);
                    }
                    killer.RpcSetKillTimer();
                }
                else if (IsWanted(target))
                {
                    var damage = Options.GuardDamage.GetFloat();
                    damage += Options.WeaponDamage.GetFloat() * GetItemAmount(killer, InventoryItems.Weapon);
                    damage -= Options.ArmorProtection.GetFloat() * GetItemAmount(target, InventoryItems.Armor);
                    if (damage <= 0.5f)
                        damage = 0.5f;
                    PlayerHealth[target.PlayerId] -= damage;
                    if (PlayerHealth[target.PlayerId] <= 0f)
                    {
                        SendRPC_SetItemAmount(target, InventoryItems.Screwdriver, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.Weapon, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.Pickaxe, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.SpaceshipParts, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.SpaceshipWithoutFuel, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.SpaceshipWithFuel, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.GuardOutfit, 0);
                        SendRPC_SetItemAmount(target, InventoryItems.Armor, 0);
                        SendRPC_SetItemAmount(killer, InventoryItems.Resources, GetItemAmount(killer, InventoryItems.Resources) + 50);
                        target.RpcShapeshift(target, false);
                        SendRPC_SetPlayerType(target, JailbreakPlayerTypes.Prisoner);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Palette.Orange;
                        RespawnCooldown[target.PlayerId] = Options.JbRespawnCooldown.GetFloat();
                        SendRPC_SetIsDead(target, true);
                        target.Data.MarkDirty();
                        target.SyncPlayerSettings();
                        target.RpcSetVentInteraction();
                        target.RpcTeleport(new Vector2(1000f, 1000f));
                        ++Main.PlayerKills[killer.PlayerId];
                    }
                    killer.RpcSetKillTimer();
                }
                else
                {
                    if (SearchCooldown[killer.PlayerId] > 0f) return false;
                    SearchCooldown[killer.PlayerId] = Options.SearchCooldown.GetFloat();
                    if (HasIllegalItem(target))
                    {
                        SendRPC_SetPlayerType(target, JailbreakPlayerTypes.Wanted);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.red;
                    }
                    killer.RpcSetKillTimer();
                }
                return false;
            }
            bool guardNearby = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsGuard(pc) && !IsDead[pc.PlayerId] && Vector2.Distance(pc.transform.position, killer.transform.position) <= Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod) * 3 
                && !PhysicsHelpers.AnythingBetween(pc.transform.position, killer.transform.position, Constants.ShadowMask, false))
                    guardNearby = true;
            }
            if (guardNearby)
            {
                SendRPC_SetPlayerType(killer, JailbreakPlayerTypes.Wanted);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(killer.PlayerId, pc.PlayerId)] = Color.red;
            }
            var damage2 = Options.PrisonerDamage.GetFloat();
            damage2 += Options.WeaponDamage.GetFloat() * GetItemAmount(killer, InventoryItems.Weapon);
            damage2 -= Options.ArmorProtection.GetFloat() * GetItemAmount(target, InventoryItems.Armor);
            if (damage2 <= 0.5f)
                damage2 = 0.5f;
            PlayerHealth[target.PlayerId] -= damage2;
            if (PlayerHealth[target.PlayerId] <= 0f)
            {
                if (IsGuard(target))
                {
                    if (GetItemAmount(target, InventoryItems.Weapon) > GetItemAmount(killer, InventoryItems.Weapon))
                        SendRPC_SetItemAmount(killer, InventoryItems.Weapon, GetItemAmount(target, InventoryItems.Weapon));
                    if (GetItemAmount(target, InventoryItems.Armor) > GetItemAmount(killer, InventoryItems.Armor))
                        SendRPC_SetItemAmount(killer, InventoryItems.Armor, GetItemAmount(target, InventoryItems.Armor));
                    SendRPC_SetItemAmount(target, InventoryItems.Weapon, 3);
                    SendRPC_SetItemAmount(target, InventoryItems.Armor, 3);
                    SendRPC_SetItemAmount(killer, InventoryItems.Resources, Math.Min(GetItemAmount(killer, InventoryItems.Resources) + 100, Options.MaximumPrisonerResources.GetInt()));
                }
                else
                {
                    SendRPC_SetItemAmount(killer, InventoryItems.Resources, Math.Min(GetItemAmount(killer, InventoryItems.Resources) + GetItemAmount(target, InventoryItems.Resources), Options.MaximumPrisonerResources.GetInt()));
                    SendRPC_SetItemAmount(killer, InventoryItems.Screwdriver, GetItemAmount(killer, InventoryItems.Screwdriver) + GetItemAmount(target, InventoryItems.Screwdriver));
                    if (GetItemAmount(target, InventoryItems.Weapon) > GetItemAmount(killer, InventoryItems.Weapon))
                        SendRPC_SetItemAmount(killer, InventoryItems.Weapon, GetItemAmount(target, InventoryItems.Weapon));
                    if (GetItemAmount(target, InventoryItems.Pickaxe) > GetItemAmount(killer, InventoryItems.Pickaxe))
                        SendRPC_SetItemAmount(killer, InventoryItems.Pickaxe, GetItemAmount(target, InventoryItems.Pickaxe));
                    SendRPC_SetItemAmount(killer, InventoryItems.SpaceshipParts, GetItemAmount(killer, InventoryItems.SpaceshipParts) + GetItemAmount(target, InventoryItems.SpaceshipParts));
                    SendRPC_SetItemAmount(killer, InventoryItems.SpaceshipWithoutFuel, GetItemAmount(killer, InventoryItems.SpaceshipWithoutFuel) + GetItemAmount(target, InventoryItems.SpaceshipWithoutFuel));
                    SendRPC_SetItemAmount(killer, InventoryItems.SpaceshipWithFuel, GetItemAmount(killer, InventoryItems.SpaceshipWithFuel) + GetItemAmount(target, InventoryItems.SpaceshipWithFuel));
                    SendRPC_SetItemAmount(killer, InventoryItems.BreathingMaskWithoutOxygen, GetItemAmount(killer, InventoryItems.BreathingMaskWithoutOxygen) + GetItemAmount(target, InventoryItems.BreathingMaskWithoutOxygen));
                    SendRPC_SetItemAmount(killer, InventoryItems.BreathingMaskWithOxygen, GetItemAmount(killer, InventoryItems.BreathingMaskWithOxygen) + GetItemAmount(target, InventoryItems.BreathingMaskWithOxygen));
                    SendRPC_SetItemAmount(killer, InventoryItems.GuardOutfit, GetItemAmount(killer, InventoryItems.GuardOutfit) + GetItemAmount(target, InventoryItems.GuardOutfit));
                    if (GetItemAmount(target, InventoryItems.Armor) > GetItemAmount(killer, InventoryItems.Armor))
                        SendRPC_SetItemAmount(killer, InventoryItems.Armor, GetItemAmount(target, InventoryItems.Armor));
                    SendRPC_SetItemAmount(target, InventoryItems.Resources, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.Screwdriver, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.Weapon, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.Pickaxe, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.SpaceshipParts, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.SpaceshipWithoutFuel, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.SpaceshipWithFuel, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.GuardOutfit, 0);
                    SendRPC_SetItemAmount(target, InventoryItems.Armor, 0);
                    target.RpcShapeshift(target, false);
                    SendRPC_SetPlayerType(target, JailbreakPlayerTypes.Prisoner);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        Main.NameColors[(killer.PlayerId, pc.PlayerId)] = Palette.Orange;
                }   
                RespawnCooldown[target.PlayerId] = Options.JbRespawnCooldown.GetFloat();
                SendRPC_SetIsDead(target, true);
                target.Data.MarkDirty();
                target.SyncPlayerSettings();
                killer.RpcSetVentInteraction();
                target.RpcSetVentInteraction();
                target.RpcTeleport(new Vector2(1000f, 1000f));
                ++Main.PlayerKills[killer.PlayerId];
            }
            killer.RpcSetKillTimer();
            return false;
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (IsDead[shapeshifter.PlayerId]) return false;
            var room = shapeshifter.GetPlainShipRoom();
            if (room != null && room.RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && IsGuard(shapeshifter))
            {
                ReactorWallHealth += 10f;
                if (ReactorWallHealth > 100f)
                    ReactorWallHealth = 100f;
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            else if (room != null && room.RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !IsGuard(shapeshifter) && HasItem(shapeshifter, InventoryItems.Pickaxe))
            {
                ReactorWallHealth -= GetItemAmount(shapeshifter, InventoryItems.Pickaxe) * Options.PickaxeSpeed.GetFloat();
                if (ReactorWallHealth < 0f)
                    ReactorWallHealth = 0f;
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            else if (!IsGuard(shapeshifter) && HasItem(shapeshifter, InventoryItems.GuardOutfit) && (IsGuard(target) || target == shapeshifter))
            {
                shapeshifter.RpcShapeshift(target, false);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (IsGuard(target))
                        Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Color.blue;
                    else if (IsWanted(shapeshifter))
                        Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Color.red;
                    else
                        Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Palette.Orange;
                }
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            var recipe = (Recipes)GetCurrentRecipe(shapeshifter);
            switch (recipe)
            {
                case Recipes.Screwdriver:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.ScrewdriverPrice.GetInt()) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.ScrewdriverPrice.GetInt());
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Screwdriver, GetItemAmount(shapeshifter, InventoryItems.Screwdriver) + 1);
                    shapeshifter.RpcSetVentInteraction();
                    break;
                case Recipes.PrisonerWeapon:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.PrisonerWeaponPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Weapon) + 1)) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.PrisonerWeaponPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Weapon) + 1));
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Weapon, GetItemAmount(shapeshifter, InventoryItems.Weapon) + 1);
                    int recipeId = 1;
                    if (GetItemAmount(shapeshifter, InventoryItems.Weapon) >= 10)
                        recipeId = 2;
                    if (GetItemAmount(shapeshifter, InventoryItems.Pickaxe) >= 10 && recipeId == 2)
                        recipeId = 3;
                    if (recipeId != 1)
                        SendRPC_SetCurrentRecipe(shapeshifter, recipeId);
                    break;
                case Recipes.Pickaxe:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.PickaxePrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Pickaxe) + 1)) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.PickaxePrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Pickaxe) + 1));
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Pickaxe, GetItemAmount(shapeshifter, InventoryItems.Pickaxe) + 1);
                    if (GetItemAmount(shapeshifter, InventoryItems.Pickaxe) >= 10)
                        SendRPC_SetCurrentRecipe(shapeshifter, 3);
                    break;
                case Recipes.SpaceshipPart:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.SpaceshipPartPrice.GetInt()) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.SpaceshipPartPrice.GetInt());
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.SpaceshipParts, GetItemAmount(shapeshifter, InventoryItems.SpaceshipParts) + 1);
                    break;
                case Recipes.Spaceship:
                    if (GetItemAmount(shapeshifter, InventoryItems.SpaceshipParts) < Options.RequiredSpaceshipParts.GetInt()) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.SpaceshipParts, GetItemAmount(shapeshifter, InventoryItems.SpaceshipParts) - Options.RequiredSpaceshipParts.GetInt());
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.SpaceshipWithoutFuel, GetItemAmount(shapeshifter, InventoryItems.SpaceshipWithoutFuel) + 1);
                    break;
                case Recipes.BreathingMask:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.BreathingMaskPrice.GetInt()) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.BreathingMaskPrice.GetInt());
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.BreathingMaskWithoutOxygen, GetItemAmount(shapeshifter, InventoryItems.BreathingMaskWithoutOxygen) + 1);
                    break;
                case Recipes.GuardOutfit:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.GuardOutfitPrice.GetInt()) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.GuardOutfitPrice.GetInt());
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.GuardOutfit, GetItemAmount(shapeshifter, InventoryItems.GuardOutfit) + 1);
                    break;
                case Recipes.PrisonerArmor:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.PrisonerArmorPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Armor) + 1)) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.PrisonerArmorPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Armor) + 1));
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Armor, GetItemAmount(shapeshifter, InventoryItems.Armor) + 1);
                    if (GetItemAmount(shapeshifter, InventoryItems.Armor) >= 10)
                        SendRPC_SetCurrentRecipe(shapeshifter, 0);
                    break;
                case Recipes.GuardWeapon:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.GuardWeaponPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Weapon) + 1)) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.GuardWeaponPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Weapon) + 1));
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Weapon, GetItemAmount(shapeshifter, InventoryItems.Weapon) + 1);
                    if (GetItemAmount(shapeshifter, InventoryItems.Weapon) >= 10)
                        SendRPC_SetCurrentRecipe(shapeshifter, 1001);
                    break;
                case Recipes.EnergyDrink:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.EnergyDrinkPrice.GetInt()) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.EnergyDrinkPrice.GetInt());
                    EnergyDrinkDuration[shapeshifter.PlayerId] = Options.EnergyDrinkDuration.GetFloat();
                    shapeshifter.SyncPlayerSettings();
                    break;
                case Recipes.GuardArmor:
                    if (GetItemAmount(shapeshifter, InventoryItems.Resources) < Options.GuardArmorPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Armor) + 1)) break;
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Resources, GetItemAmount(shapeshifter, InventoryItems.Resources) - Options.GuardArmorPrice.GetInt() * (GetItemAmount(shapeshifter, InventoryItems.Armor) + 1));
                    SendRPC_SetItemAmount(shapeshifter, InventoryItems.Armor, GetItemAmount(shapeshifter, InventoryItems.Armor) + 1);
                    if (GetItemAmount(shapeshifter, InventoryItems.Armor) >= 10)
                        SendRPC_SetCurrentRecipe(shapeshifter, 1000);
                    break;
            }
            shapeshifter.RpcResetAbilityCooldown();
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            IsGuardAlive = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsGuard(pc) && !IsDead[pc.PlayerId])
                    IsGuardAlive = true;
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (HasEscaped(pc)) continue;
                if (IsDead[pc.PlayerId])
                {
                    if (RespawnCooldown[pc.PlayerId] > 0f)
                    {
                        RespawnCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (RespawnCooldown[pc.PlayerId] <= 0f)
                    {
                        PlayerHealth[pc.PlayerId] = IsGuard(pc) ? Options.GuardHealth.GetFloat() : Options.PrisonerHealth.GetFloat();
                        SendRPC_SetIsDead(pc, false);
                        if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0)
                            pc.RpcTeleport(new Vector2(-9.0f, -4.0f));
                        else if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3)
                            pc.RpcTeleport(new Vector2(9.0f, -4.0f));
                        pc.RpcShapeshift(pc, false);
                        pc.SyncPlayerSettings();
                        RespawnCooldown[pc.PlayerId] = 0f;
                    }   
                    continue;
                }
                var room = pc.GetPlainShipRoom();
                if (ChangeRecipeCooldown[pc.PlayerId] > 0f)
                {
                    ChangeRecipeCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (ChangeRecipeCooldown[pc.PlayerId] < 0f)
                {
                    ChangeRecipeCooldown[pc.PlayerId] = 0f;
                }
                if (!IsGuard(pc) && room != null && (room.RoomId == SystemTypes.LowerEngine || room.RoomId == SystemTypes.UpperEngine) && HasItem(pc, InventoryItems.SpaceshipWithoutFuel)  && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                {
                    SendRPC_SetItemAmount(pc, InventoryItems.SpaceshipWithFuel, GetItemAmount(pc, InventoryItems.SpaceshipWithFuel) + GetItemAmount(pc, InventoryItems.SpaceshipWithoutFuel));
                    SendRPC_SetItemAmount(pc, InventoryItems.SpaceshipWithoutFuel, 0);
                }
                if (!IsGuard(pc) && room != null && room.RoomId == SystemTypes.LifeSupp && HasItem(pc, InventoryItems.BreathingMaskWithoutOxygen)  && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3))
                {
                    SendRPC_SetItemAmount(pc, InventoryItems.BreathingMaskWithOxygen, GetItemAmount(pc, InventoryItems.BreathingMaskWithOxygen) + GetItemAmount(pc, InventoryItems.BreathingMaskWithoutOxygen));
                    SendRPC_SetItemAmount(pc, InventoryItems.BreathingMaskWithoutOxygen, 0);
                }
                if (IsGuard(pc) && SearchCooldown[pc.PlayerId] > 0f)
                {
                    SearchCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (IsGuard(pc) && SearchCooldown[pc.PlayerId] < 0f)
                {
                    SearchCooldown[pc.PlayerId] = 0;
                }
                bool guardNearby = false;
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    if (IsGuard(ar) && !IsDead[ar.PlayerId] && Vector2.Distance(ar.transform.position, pc.transform.position) <= Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod) * 3 
                    && !PhysicsHelpers.AnythingBetween(ar.transform.position, pc.transform.position, Constants.ShadowMask, false))
                        guardNearby = true;
                }
                if (!IsGuard(pc) && IsDoingIllegalThing(pc) && guardNearby)
                {
                    SendRPC_SetPlayerType(pc, JailbreakPlayerTypes.Wanted);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.red;
                }
                TimeSinceLastDamage[pc.PlayerId] += Time.fixedDeltaTime;
                if (EnergyDrinkDuration[pc.PlayerId] > 0f)
                {
                    EnergyDrinkDuration[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (EnergyDrinkDuration[pc.PlayerId] < 0f)
                {
                    pc.SyncPlayerSettings();
                    EnergyDrinkDuration[pc.PlayerId] = 0f;
                }
                if (IsGuard(pc) && TakeoverTimer > 0f && room != null && room.RoomId == SystemTypes.Nav && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                    TakeoverTimer = 0f;
                if (!IsGuard(pc) && room != null && room.RoomId == SystemTypes.Storage && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && HasItem(pc, InventoryItems.SpaceshipWithFuel))
                    RpcEscape(pc);
                if (!IsGuard(pc) && room != null && room.RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && HasItem(pc, InventoryItems.BreathingMaskWithOxygen) && ReactorWallHealth <= 0f && !pc.inVent)
                    RpcEscape(pc);
            }
            bool isGuardAlive = false;
            bool isPrisonerInNav = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var room = pc.GetPlainShipRoom();
                if (IsGuard(pc) && !IsDead[pc.PlayerId])
                    isGuardAlive = true;
                if (!IsGuard(pc) && !IsDead[pc.PlayerId] && room != null && room.RoomId == SystemTypes.Nav && !pc.inVent)
                    isPrisonerInNav = true;
            }
            if (!isGuardAlive && isPrisonerInNav)
                TakeoverTimer += Time.fixedDeltaTime;
            if (TakeoverTimer >= Options.PrisonTakeoverDuration.GetFloat())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!IsGuard(pc) && !HasEscaped(pc))
                        RpcEscape(pc);
                }
            }
            OneSecondTimer += Time.fixedDeltaTime;
            if (OneSecondTimer >= 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (IsDead[pc.PlayerId]) continue;
                    var room = pc.GetPlainShipRoom();
                    if (room != null && room.RoomId == SystemTypes.MedBay && TimeSinceLastDamage[pc.PlayerId] >= 5f && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                    {
                        if (IsGuard(pc))
                        {
                            PlayerHealth[pc.PlayerId] += Options.GuardRegeneration.GetFloat() * 3f;
                            if (PlayerHealth[pc.PlayerId] > Options.GuardHealth.GetFloat())
                                PlayerHealth[pc.PlayerId] = Options.GuardHealth.GetFloat();
                        } 
                        else
                        {
                            PlayerHealth[pc.PlayerId] += Options.PrisonerRegeneration.GetFloat() * 3f;
                            if (PlayerHealth[pc.PlayerId] > Options.PrisonerHealth.GetFloat())
                                PlayerHealth[pc.PlayerId] = Options.PrisonerHealth.GetFloat();
                        } 
                    }
                    else if (TimeSinceLastDamage[pc.PlayerId] >= 5f && !pc.inVent)
                    {
                        if (IsGuard(pc))
                        {
                            PlayerHealth[pc.PlayerId] += Options.GuardRegeneration.GetFloat();
                            if (PlayerHealth[pc.PlayerId] > Options.GuardHealth.GetFloat())
                                PlayerHealth[pc.PlayerId] = Options.GuardHealth.GetFloat();
                        } 
                        else
                        {
                            PlayerHealth[pc.PlayerId] += Options.PrisonerRegeneration.GetFloat();
                            if (PlayerHealth[pc.PlayerId] > Options.PrisonerHealth.GetFloat())
                                PlayerHealth[pc.PlayerId] = Options.PrisonerHealth.GetFloat();
                        }
                    }
                    if (IsGuard(pc))
                        SendRPC_SetItemAmount(pc, InventoryItems.Resources, GetItemAmount(pc, InventoryItems.Resources) + 2);
                    if (!IsGuard(pc) && room != null && room.RoomId == SystemTypes.Electrical && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                        SendRPC_SetItemAmount(pc, InventoryItems.Resources, Math.Min(GetItemAmount(pc, InventoryItems.Resources) + 2, Options.MaximumPrisonerResources.GetInt()));
                    if (!IsGuard(pc) && room && room.RoomId == SystemTypes.Storage && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3))
                        SendRPC_SetItemAmount(pc, InventoryItems.Resources, Math.Min(GetItemAmount(pc, InventoryItems.Resources) + 5, Options.MaximumPrisonerResources.GetInt()));
                }
                OneSecondTimer -= 1f;
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            return (IsGuard(player) || HasItem(player, InventoryItems.Screwdriver)) && !Main.IsInvisible[player.PlayerId];
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage)
                return false;
            return true;
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Tracker, 0, 0);
            opt.SetFloat(FloatOptionNames.KillCooldown, 2f);
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, 1f);
            opt.SetFloat(FloatOptionNames.ShapeshifterDuration, 0f);
            opt.SetFloat(FloatOptionNames.GuardianAngelCooldown, Options.HelpCooldown.GetFloat());
            opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
            opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
            opt.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
            if (!IsGuard(player))
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, Main.RealOptions.GetFloat(FloatOptionNames.CrewLightMod));
            if (IsGuard(player) && EnergyDrinkDuration[player.PlayerId] > 0f)
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, Main.RealOptions.GetFloat(FloatOptionNames.PlayerSpeedMod) * (1f + (Options.EnergyDrinkSpeedIncrease.GetInt() / 100f)));
            if (IsDead[player.PlayerId])
            {
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
            }
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            if (IsGuard(player) && player == seer)
            {
                name += "\nHealth: " + (int)(PlayerHealth[player.PlayerId] + 0.99f) + "/" + Options.GuardHealth.GetFloat();
                name += Utils.ColorString(Color.blue, "\nSearch cooldown: " + (int)(SearchCooldown[player.PlayerId] + 0.99f) + "s");
                if (EnergyDrinkDuration[player.PlayerId] > 0f)
                    name += Utils.ColorString(Color.yellow, "\nEnergy drink: " + (int)(EnergyDrinkDuration[player.PlayerId] + 0.99f) + "s");
                name += "<" + Utils.ColorToHex(Color.green) + ">";
                name += "\nMoney: " + GetItemAmount(player, InventoryItems.Resources) + "$";
                name += "\nWeapon lvl." + GetItemAmount(player, InventoryItems.Weapon);
                name += "\nArmor lvl." + GetItemAmount(player, InventoryItems.Armor) + "</color>";
                name += Utils.ColorString(Color.white, "\nShop item:\n");
                switch ((Recipes)GetCurrentRecipe(player))
                {
                    case Recipes.GuardWeapon:
                        name += Utils.ColorString(Color.magenta, "Upgrade weapon - " + (Options.GuardWeaponPrice.GetInt() * (GetItemAmount(player, InventoryItems.Weapon) + 1)) + "$");
                        break;
                    case Recipes.EnergyDrink:
                        name += Utils.ColorString(Color.magenta, "Energy drink - " + Options.EnergyDrinkPrice.GetInt() + "$");
                        break;
                    case Recipes.GuardArmor:
                        name += Utils.ColorString(Color.magenta, "Upgrade armor - " + (Options.GuardArmorPrice.GetInt() * (GetItemAmount(player, InventoryItems.Armor) + 1)) + "$");
                        break;
                }
            }
            else if (!IsGuard(player) && !HasEscaped(player) && player == seer)
            {
                name += "\nHealth: " + (int)(PlayerHealth[player.PlayerId] + 0.99f) + "/" + Options.PrisonerHealth.GetFloat();
                name += "<" + Utils.ColorToHex(Color.green) + ">";
                name += "\nResources: " + GetItemAmount(player, InventoryItems.Resources) + "/" + Options.MaximumPrisonerResources.GetInt();
                if (HasItem(player, InventoryItems.BreathingMaskWithoutOxygen))
                    name += "\nBreathing mask without oxygen" + (GetItemAmount(player, InventoryItems.BreathingMaskWithoutOxygen) > 1 ? " x" + GetItemAmount(player, InventoryItems.BreathingMaskWithoutOxygen) : "");
                if (HasItem(player, InventoryItems.BreathingMaskWithOxygen))
                    name += "\nBreathing mask with oxygen" + (GetItemAmount(player, InventoryItems.BreathingMaskWithOxygen) > 1 ? " x" + GetItemAmount(player, InventoryItems.BreathingMaskWithOxygen) : "");
                name += "</color>";
                if (HasItem(player, InventoryItems.Screwdriver))
                    name += "\nScrewdriver";
                if (HasItem(player, InventoryItems.Weapon))
                    name += "\nWeapon lvl." + GetItemAmount(player, InventoryItems.Weapon);
                if (HasItem(player, InventoryItems.Pickaxe))
                    name += "\nPickaxe lvl." + GetItemAmount(player, InventoryItems.Pickaxe);
                if (HasItem(player, InventoryItems.SpaceshipParts))
                    name += "\nSpaceship part" + (GetItemAmount(player, InventoryItems.SpaceshipParts) > 1 ? " x" + GetItemAmount(player, InventoryItems.SpaceshipParts) : "");
                if (HasItem(player, InventoryItems.SpaceshipWithoutFuel))
                    name += "\nSpaceship without fuel" + (GetItemAmount(player, InventoryItems.SpaceshipWithoutFuel) > 1 ? " x" + GetItemAmount(player, InventoryItems.SpaceshipWithoutFuel) : "");
                if (HasItem(player, InventoryItems.SpaceshipWithFuel))
                    name += "\nSpaceship with fuel" + (GetItemAmount(player, InventoryItems.SpaceshipWithFuel) > 1 ? " x" + GetItemAmount(player, InventoryItems.SpaceshipWithFuel) : "");
                if (HasItem(player, InventoryItems.GuardOutfit))
                    name += "\nGuard outfit" + (GetItemAmount(player, InventoryItems.GuardOutfit) > 1 ? " x" + GetItemAmount(player, InventoryItems.GuardOutfit) : "");
                if (HasItem(player, InventoryItems.Armor))
                    name += "\nArmor lvl." + GetItemAmount(player, InventoryItems.Armor);
                name += Utils.ColorString(Color.white, "\nRecipe:\n");
                switch ((Recipes)GetCurrentRecipe(player))
                {
                    case Recipes.Screwdriver:
                        name += Utils.ColorString(Color.magenta, "Screwdriver - " + Options.ScrewdriverPrice.GetInt() + " res");
                        break;
                    case Recipes.PrisonerWeapon:
                        name += Utils.ColorString(Color.magenta, "Upgrade weapon - " + (Options.PrisonerWeaponPrice.GetInt() * (GetItemAmount(player, InventoryItems.Weapon) + 1)) + " res");
                        break;
                    case Recipes.Pickaxe:
                        name += Utils.ColorString(Color.magenta, "Upgrade pickaxe - " + (Options.PickaxePrice.GetInt() * (GetItemAmount(player, InventoryItems.Pickaxe) + 1)) + " res");
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
                    case Recipes.PrisonerArmor:
                        name += Utils.ColorString(Color.magenta, "Upgrade armor - " + (Options.PrisonerArmorPrice.GetInt() * (GetItemAmount(player, InventoryItems.Armor) + 1)) + " res");
                        break;
                }
            }
            if (player.GetPlainShipRoom() != null && player.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && player == seer)
                name += Utils.ColorString(Color.red, "\nWALL [" + ReactorWallHealth + "%]");
            if (ReactorWallHealth <= 0f && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && player == seer)
                name += Utils.ColorString(Color.red, "\nWALL DESTROYED!");
            if (!IsGuard(player) && !IsGuardAlive && player == seer)
                name += Utils.ColorString(Color.red, "\nNO GUARDS!");
            if (TakeoverTimer > 0f && player == seer)
                name += Utils.ColorString(Color.red, "\nTAKEOVER [" + (int)(TakeoverTimer / Options.PrisonTakeoverDuration.GetFloat() * 100f + 0.99f) + "%]");
            if (player == seer)
                name += Utils.ColorString(Color.cyan, "\nTIME: " + (int)((float)Options.GameTime.GetInt() - Main.Timer + 0.99f) + "s");
            return name;
        }

        public void SendRPC_SetPlayerType(PlayerControl player, JailbreakPlayerTypes playerType)
        {
            if (PlayerType[player.PlayerId] == playerType) return;
            PlayerType[player.PlayerId] = playerType;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write((int)JailbreakRPC.SetPlayerType);
            writer.Write((int)playerType);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public void SendRPC_SetItemAmount(PlayerControl player, InventoryItems item, int amount)
        {
            if (Inventory[(player.PlayerId, item)] == amount) return;
            Inventory[(player.PlayerId, item)] = amount;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write((int)JailbreakRPC.SetItemAmount);
            writer.Write((int)item);
            writer.Write(amount);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public void SendRPC_SetCurrentRecipe(PlayerControl player, int recipeId)
        {
            if (CurrentRecipe[player.PlayerId] == recipeId) return;
            CurrentRecipe[player.PlayerId] = recipeId;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write((int)JailbreakRPC.SetCurrentRecipe);
            writer.Write(recipeId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public void SendRPC_SetIsDead(PlayerControl player, bool isDead)
        {
            if (IsDead[player.PlayerId] == isDead) return;
            IsDead[player.PlayerId] = isDead;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write((int)JailbreakRPC.SetIsDead);
            writer.Write(isDead);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(PlayerControl player, MessageReader reader)
        {
            JailbreakRPC rpcType = (JailbreakRPC)reader.ReadInt32();
            switch(rpcType)
            {
                case JailbreakRPC.SetPlayerType:
                    PlayerType[player.PlayerId] = (JailbreakPlayerTypes)reader.ReadInt32();
                    break;
                case JailbreakRPC.SetItemAmount:
                    InventoryItems item = (InventoryItems)reader.ReadInt32();
                    Inventory[(player.PlayerId, item)] = reader.ReadInt32();
                    break;
                case JailbreakRPC.SetCurrentRecipe:
                    CurrentRecipe[player.PlayerId] = reader.ReadInt32();
                    break;
                case JailbreakRPC.SetIsDead:
                    IsDead[player.PlayerId] = reader.ReadBoolean();
                    break;
            }
        }

        public JailbreakPlayerTypes GetJailbreakPlayerType(PlayerControl player)
        {
            if (player == null) return JailbreakPlayerTypes.None;
            if (!PlayerType.ContainsKey(player.PlayerId)) return JailbreakPlayerTypes.None;
            return PlayerType[player.PlayerId];
        }

        public bool IsGuard(PlayerControl player)
        {
            return GetJailbreakPlayerType(player) == JailbreakPlayerTypes.Guard;
        }

        public bool IsWanted(PlayerControl player)
        {
            return GetJailbreakPlayerType(player) == JailbreakPlayerTypes.Wanted;
        }

        public bool HasEscaped(PlayerControl player)
        {
            return GetJailbreakPlayerType(player) == JailbreakPlayerTypes.Escapist;
        }

        public int GetItemAmount(PlayerControl player, InventoryItems itemType)
        {
            if (player == null) return 0;
            if (!Inventory.ContainsKey((player.PlayerId, itemType))) return 0;
            return Inventory[(player.PlayerId, itemType)];
        }

        public bool HasItem(PlayerControl player, InventoryItems itemType)
        {
            return GetItemAmount(player, itemType) > 0;
        }

        public int GetCurrentRecipe(PlayerControl player)
        {
            if (player == null) return -1;
            if (!CurrentRecipe.ContainsKey(player.PlayerId)) return -1;
            return CurrentRecipe[player.PlayerId];
        }

        public bool IsDoingIllegalThing(PlayerControl player)
        {
            if (IsGuard(player)) return false;
            var room = player.GetPlainShipRoom();
            if (room != null && (room.RoomId == SystemTypes.Reactor || room.RoomId == SystemTypes.Security || room.RoomId == SystemTypes.Storage || room.RoomId == SystemTypes.Admin ||
                room.RoomId == SystemTypes.Nav) && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3))
                return true;
            if (player.walkingToVent || (player.MyPhysics.Animations.IsPlayingEnterVentAnimation() && !player.inMovingPlat) || (player.MyPhysics.Animations.Animator.GetCurrentAnimation() == player.MyPhysics.Animations.group.ExitVentAnim && HasItem(player, InventoryItems.Screwdriver)))
                return true;
            return false;
        }

        public bool HasIllegalItem(PlayerControl player)
        {
            if (IsGuard(player)) return false;
            if (HasItem(player, InventoryItems.Screwdriver)) return true;
            if (HasItem(player, InventoryItems.Weapon)) return true;
            if (HasItem(player, InventoryItems.Pickaxe)) return true;
            if (HasItem(player, InventoryItems.Screwdriver)) return true;
            if (HasItem(player, InventoryItems.SpaceshipParts)) return true;
            if (HasItem(player, InventoryItems.SpaceshipWithoutFuel)) return true;
            if (HasItem(player, InventoryItems.SpaceshipWithFuel)) return true;
            if (HasItem(player, InventoryItems.GuardOutfit)) return true;
            if (HasItem(player, InventoryItems.Armor)) return true;
            return false;
        }

        public void RpcEscape(PlayerControl player)
        {
            SendRPC_SetPlayerType(player, JailbreakPlayerTypes.Escapist);
            player.RpcShapeshift(player, false);
            player.RpcSetDeathReason(DeathReasons.Escaped);
            player.RpcSetRole(RoleTypes.GuardianAngel, true);
            player.Die(DeathReason.Exile, false);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            player.SyncPlayerSettings();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Main.NameColors[(player.PlayerId, pc.PlayerId)] = Color.green;
                pc.RpcReactorFlash(0.4f, Color.blue);
            } 
        }

        public JailbreakGamemode()
        {
            Gamemode = Gamemodes.Jailbreak;
            PetAction = true;
            DisableTasks = true;
            PlayerType = new Dictionary<byte, JailbreakPlayerTypes>();
            Inventory = new Dictionary<(byte, InventoryItems), int>();
            ReactorWallHealth = 100f;
            TakeoverTimer = 0f;
            TimeSinceLastDamage = new Dictionary<byte, float>();
            RespawnCooldown = new Dictionary<byte, float>();
            SearchCooldown = new Dictionary<byte, float>();
            PlayerHealth = new Dictionary<byte, float>();
            CurrentRecipe = new Dictionary<byte, int>();
            EnergyDrinkDuration = new Dictionary<byte, float>();
            OneSecondTimer = 0f;
            ChangeRecipeCooldown = new Dictionary<byte, float>();
            IsDead = new Dictionary<byte, bool>();
            IsGuardAlive = true;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                PlayerType[pc.PlayerId] = JailbreakPlayerTypes.None;
                foreach (var item in Enum.GetValues(typeof(InventoryItems)))
                    Inventory[(pc.PlayerId, (InventoryItems)item)] = 0;
                TimeSinceLastDamage[pc.PlayerId] = 0f;
                RespawnCooldown[pc.PlayerId] = 0f;
                SearchCooldown[pc.PlayerId] = Options.SearchCooldown.GetFloat();
                PlayerHealth[pc.PlayerId] = 0f;
                CurrentRecipe[pc.PlayerId] = -1;
                EnergyDrinkDuration[pc.PlayerId] = 0f;
                ChangeRecipeCooldown[pc.PlayerId] = 0f;
                IsDead[pc.PlayerId] = false;
            }
        }

        public static JailbreakGamemode instance;
        public Dictionary<byte, JailbreakPlayerTypes> PlayerType;
        public Dictionary<(byte, InventoryItems),int> Inventory;
        public float ReactorWallHealth;
        public float TakeoverTimer;
        public Dictionary<byte, float> TimeSinceLastDamage;
        public Dictionary<byte, float> RespawnCooldown;
        public Dictionary<byte, float> SearchCooldown;
        public Dictionary<byte, float> PlayerHealth;
        public Dictionary<byte, int> CurrentRecipe;
        public Dictionary<byte, float> EnergyDrinkDuration;
        public float OneSecondTimer;
        public Dictionary<byte, float> ChangeRecipeCooldown;
        public Dictionary<byte, bool> IsDead;
        public bool IsGuardAlive;
    }

    public enum JailbreakPlayerTypes
    {
        None,
        Prisoner,
        Wanted,
        Escapist,
        Guard,
    }

    public enum InventoryItems
    {
        Resources,
        Screwdriver,
        Weapon,
        Pickaxe,
        SpaceshipParts,
        SpaceshipWithoutFuel,
        SpaceshipWithFuel,
        BreathingMaskWithoutOxygen,
        BreathingMaskWithOxygen,
        GuardOutfit,
        Armor,
    }

    public enum Recipes
    {
        None = -1,
        Screwdriver,
        PrisonerWeapon,
        Pickaxe,
        SpaceshipPart,
        Spaceship,
        BreathingMask,
        GuardOutfit,
        PrisonerArmor,
        GuardWeapon = 1000,
        EnergyDrink,
        GuardArmor,
    }

    enum JailbreakRPC
    {
        SetPlayerType,
        SetItemAmount,
        SetCurrentRecipe,
        SetIsDead,
    }
}