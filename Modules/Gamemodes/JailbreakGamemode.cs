using Il2CppSystem.Collections.Generic;
using UnityEngine;
using System;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class JailbreakGamemode : CustomGamemode
    {
        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.HasEscaped())
                __instance.FilterText.text = "Escapist";
            else if (__instance.HauntTarget.IsGuard())
                __instance.FilterText.text = "Guard";
            else
                __instance.FilterText.text = "Prisoner";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.IsGuard())
            {
                if (__instance.KillButton.currentTarget != null && __instance.KillButton.currentTarget.GetJailbreakPlayerType() == JailbreakPlayerTypes.Wanted && __instance.KillButton.currentTarget.shapeshiftTargetPlayerId == -1)
                    __instance.KillButton.OverrideText("Attack");
                else
                    __instance.KillButton.OverrideText("Search");
                if (player.GetPlainShipRoom() != null && player.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (GameOptionsManager.Instance.CurrentGameOptions.MapId == 0 || GameOptionsManager.Instance.CurrentGameOptions.MapId == 3))
                    __instance.AbilityButton.OverrideText("Repair");
                else
                    __instance.AbilityButton.OverrideText("Buy");
                __instance.PetButton.OverrideText("Next Offer");
            }
            else if (player.HasEscaped())
            {
                __instance.AbilityButton.OverrideText("Help");
            }
            else
            {
                __instance.KillButton.OverrideText("Attack");
                if (player.GetPlainShipRoom() != null && player.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (GameOptionsManager.Instance.CurrentGameOptions.MapId == 0 || GameOptionsManager.Instance.CurrentGameOptions.MapId == 3) && player.HasItem(InventoryItems.Pickaxe))
                {
                    __instance.AbilityButton.OverrideText("Destroy");
                    __instance.AbilityButton.SetEnabled();
                }
                else if (player.HasItem(InventoryItems.GuardOutfit))
                {
                    __instance.AbilityButton.OverrideText("Craft/Change");
                    __instance.AbilityButton.SetEnabled();
                }
                else
                {
                    __instance.AbilityButton.OverrideText("Craft");
                    int price = 0;
                    InventoryItems requiredItem = InventoryItems.Resources;
                    switch ((Recipes)player.GetCurrentRecipe())
                    {
                        case Recipes.Screwdriver:
                            price = Options.ScrewdriverPrice.GetInt();
                            break;
                        case Recipes.PrisonerWeapon:
                            price = Options.PrisonerWeaponPrice.GetInt() * (player.GetItemAmount(InventoryItems.Weapon) + 1);
                            break;
                        case Recipes.Pickaxe:
                            price = Options.PickaxePrice.GetInt() * (player.GetItemAmount(InventoryItems.Pickaxe) + 1);
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
                            price = Options.PrisonerArmorPrice.GetInt() * (player.GetItemAmount(InventoryItems.Armor) + 1);
                            break;
                        case Recipes.GuardWeapon:
                            price = Options.GuardWeaponPrice.GetInt() * (player.GetItemAmount(InventoryItems.Weapon) + 1);
                            break;
                        case Recipes.EnergyDrink:
                            price = Options.EnergyDrinkPrice.GetInt();
                            break;
                        case Recipes.GuardArmor:
                            price = Options.GuardArmorPrice.GetInt() * (player.GetItemAmount(InventoryItems.Armor) + 1);
                            break;
                    }
                    if (player.GetItemAmount(requiredItem) >= price)
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
            if (PlayerControl.LocalPlayer.HasEscaped())
                __instance.taskText.text = Utils.ColorString(Color.green, "Escapist\nYou won! Now spectate the game.");
            else if (PlayerControl.LocalPlayer.IsGuard())
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
            if (PlayerControl.LocalPlayer.HasEscaped())
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.green);
            else if (PlayerControl.LocalPlayer.IsGuard())
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.blue);
            else
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Palette.Orange);
        }

        public override List<PlayerControl> OnBeginImpostorPrefix(IntroCutscene __instance)
        {
            List<PlayerControl> Team = new();
            Team.Add(PlayerControl.LocalPlayer);
            if (PlayerControl.LocalPlayer.IsGuard())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsGuard() && pc != PlayerControl.LocalPlayer)
                        Team.Add(pc);
                }
            }
            return Team;
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.IsGuard())
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
            if (PlayerControl.LocalPlayer.IsGuard())
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
                player.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Guard);
                AllPlayers.Remove(player);
            }
            foreach (var player in AllPlayers)
                player.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Prisoner);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsGuard())
                {
                     pc.RpcSetColor(1);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.blue;
                    pc.RpcSetCurrentRecipe(1000);
                }   
                else
                {
                    pc.RpcSetColor(4);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Palette.Orange;
                    pc.RpcSetCurrentRecipe(Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3 ? 1 : 0);
                }
            }
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsGuard())
                {
                    SearchCooldown[pc.PlayerId] = Options.SearchCooldown.GetFloat();
                    pc.RpcSetItemAmount(InventoryItems.Weapon, 3);
                    pc.RpcSetItemAmount(InventoryItems.Armor, 3);
                }
                pc.RpcResetAbilityCooldown();
                PlayerHealth[pc.PlayerId] = pc.IsGuard() ? Options.GuardHealth.GetFloat() : Options.PrisonerHealth.GetFloat();
            }
        }

        public override void OnPet(PlayerControl pc)
        {
            if (ChangeRecipeCooldown[pc.PlayerId] > 0f) return;
            var recipeId = pc.GetCurrentRecipe() + 1;
            if (!pc.IsGuard() && recipeId > 7)
                recipeId = 0;
            if (!pc.IsGuard() && recipeId == 0 && Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3)
                recipeId = 1;
            if (!pc.IsGuard() && recipeId == 1 && pc.GetItemAmount(InventoryItems.Weapon) >= 10)
                recipeId = 2;
            if (!pc.IsGuard() && recipeId == 2 && pc.GetItemAmount(InventoryItems.Pickaxe) >= 10)
                recipeId = 3;
            if (!pc.IsGuard() && recipeId == 7 && pc.GetItemAmount(InventoryItems.Armor) >= 10)
                recipeId = 0;
            if (pc.IsGuard() && recipeId > 1002)
                recipeId = 1000;
            if (pc.IsGuard() && recipeId == 1000 && pc.GetItemAmount(InventoryItems.Weapon) >= 10)
                recipeId = 1001;
            if (pc.IsGuard() && recipeId == 1002 && pc.GetItemAmount(InventoryItems.Armor) >= 10)
                recipeId = 1000;
            pc.RpcSetCurrentRecipe(recipeId);
            pc.RpcSetNamePrivate(pc.BuildPlayerName(pc, false), pc, true);
            ChangeRecipeCooldown[pc.PlayerId] = 0.5f;
        }

        public override bool OnCheckProtect(PlayerControl guardian, PlayerControl target)
        {
            if (!guardian.HasEscaped()) return false;
            if (target.IsGuard() || target.HasEscaped() || target.Data.IsDead) return false;
            target.RpcSetItemAmount(InventoryItems.Resources, Math.Min(target.GetItemAmount(InventoryItems.Resources) + Options.GivenResources.GetInt(), Options.MaximumPrisonerResources.GetInt()));
            guardian.RpcResetAbilityCooldown();
            return false;
        } 

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < 10f) return false;
            if (killer.IsGuard())
            {
                if (target.IsGuard() || Main.AllShapeshifts[target.PlayerId] != target.PlayerId)
                {
                    if (SearchCooldown[killer.PlayerId] > 0f) return false;
                    SearchCooldown[killer.PlayerId] = Options.SearchCooldown.GetFloat();
                    if (!target.IsGuard())
                    {
                        target.RpcShapeshift(target, false);
                        target.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Wanted);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.red;
                        target.RpcSetItemAmount(InventoryItems.GuardOutfit, 0);
                    }
                    killer.RpcSetKillTimer(1f);
                }
                else if (target.IsWanted())
                {
                    var damage = Options.GuardDamage.GetFloat();
                    damage += Options.WeaponDamage.GetFloat() * killer.GetItemAmount(InventoryItems.Weapon);
                    damage -= Options.ArmorProtection.GetFloat() * target.GetItemAmount(InventoryItems.Armor);
                    if (damage <= 0.5f)
                        damage = 0.5f;
                    PlayerHealth[target.PlayerId] -= damage;
                    if (PlayerHealth[target.PlayerId] <= 0f)
                    {
                        target.RpcSetItemAmount(InventoryItems.Screwdriver, 0);
                        target.RpcSetItemAmount(InventoryItems.Weapon, 0);
                        target.RpcSetItemAmount(InventoryItems.Pickaxe, 0);
                        target.RpcSetItemAmount(InventoryItems.SpaceshipParts, 0);
                        target.RpcSetItemAmount(InventoryItems.SpaceshipWithoutFuel, 0);
                        target.RpcSetItemAmount(InventoryItems.SpaceshipWithFuel, 0);
                        target.RpcSetItemAmount(InventoryItems.GuardOutfit, 0);
                        target.RpcSetItemAmount(InventoryItems.Armor, 0);
                        killer.RpcSetItemAmount(InventoryItems.Resources, killer.GetItemAmount(InventoryItems.Resources) + 50);
                        target.RpcShapeshift(target, false);
                        target.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Prisoner);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Palette.Orange;
                        RespawnCooldown[target.PlayerId] = Options.RespawnCooldown.GetFloat();
                        target.Data.IsDead = true;
                        new LateTask(() => {
                            AntiCheat.IsDead[target.PlayerId] = true;
                        }, Mathf.Max(0.02f, AmongUsClient.Instance.Ping / 1000f * 12f));
                        Utils.SendGameData();
                        target.SyncPlayerSettings();
                        target.RpcTeleport(new Vector2(1000f, 1000f));
                    }
                    killer.RpcSetKillTimer(1f);
                }
                else
                {
                    if (SearchCooldown[killer.PlayerId] > 0f) return false;
                    SearchCooldown[killer.PlayerId] = Options.SearchCooldown.GetFloat();
                    if (target.HasIllegalItem())
                    {
                        target.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Wanted);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            Main.NameColors[(target.PlayerId, pc.PlayerId)] = Color.red;
                    }
                    killer.RpcSetKillTimer(1f);
                }
                return false;
            }
            bool guardNearby = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsGuard() && !pc.Data.IsDead && Vector2.Distance(pc.transform.position, killer.transform.position) <= Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod) * 3 
                && !PhysicsHelpers.AnythingBetween(pc.transform.position, killer.transform.position, Constants.ShadowMask, false))
                    guardNearby = true;
            }
            if (guardNearby)
            {
                killer.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Wanted);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.NameColors[(killer.PlayerId, pc.PlayerId)] = Color.red;
            }
            var damage2 = Options.PrisonerDamage.GetFloat();
            damage2 += Options.WeaponDamage.GetFloat() * killer.GetItemAmount(InventoryItems.Weapon);
            damage2 -= Options.ArmorProtection.GetFloat() * target.GetItemAmount(InventoryItems.Armor);
            if (damage2 <= 0.5f)
                damage2 = 0.5f;
            PlayerHealth[target.PlayerId] -= damage2;
            if (PlayerHealth[target.PlayerId] <= 0f)
            {
                if (target.IsGuard())
                {
                    if (target.GetItemAmount(InventoryItems.Weapon) > killer.GetItemAmount(InventoryItems.Weapon))
                        killer.RpcSetItemAmount(InventoryItems.Weapon, target.GetItemAmount(InventoryItems.Weapon));
                    if (target.GetItemAmount(InventoryItems.Armor) > killer.GetItemAmount(InventoryItems.Armor))
                        killer.RpcSetItemAmount(InventoryItems.Armor, target.GetItemAmount(InventoryItems.Armor));
                    target.RpcSetItemAmount(InventoryItems.Weapon, 3);
                    target.RpcSetItemAmount(InventoryItems.Armor, 3);
                    killer.RpcSetItemAmount(InventoryItems.Resources, Math.Min(killer.GetItemAmount(InventoryItems.Resources) + 100, Options.MaximumPrisonerResources.GetInt()));
                }
                else
                {
                    killer.RpcSetItemAmount(InventoryItems.Resources, Math.Min(killer.GetItemAmount(InventoryItems.Resources) + target.GetItemAmount(InventoryItems.Resources), Options.MaximumPrisonerResources.GetInt()));
                    killer.RpcSetItemAmount(InventoryItems.Screwdriver, killer.GetItemAmount(InventoryItems.Screwdriver) + target.GetItemAmount(InventoryItems.Screwdriver));
                    if (target.GetItemAmount(InventoryItems.Weapon) > killer.GetItemAmount(InventoryItems.Weapon))
                        killer.RpcSetItemAmount(InventoryItems.Weapon, target.GetItemAmount(InventoryItems.Weapon));
                    if (target.GetItemAmount(InventoryItems.Pickaxe) > killer.GetItemAmount(InventoryItems.Pickaxe))
                        killer.RpcSetItemAmount(InventoryItems.Pickaxe, target.GetItemAmount(InventoryItems.Pickaxe));
                    killer.RpcSetItemAmount(InventoryItems.SpaceshipParts, killer.GetItemAmount(InventoryItems.SpaceshipParts) + target.GetItemAmount(InventoryItems.SpaceshipParts));
                    killer.RpcSetItemAmount(InventoryItems.SpaceshipWithoutFuel, killer.GetItemAmount(InventoryItems.SpaceshipWithoutFuel) + target.GetItemAmount(InventoryItems.SpaceshipWithoutFuel));
                    killer.RpcSetItemAmount(InventoryItems.SpaceshipWithFuel, killer.GetItemAmount(InventoryItems.SpaceshipWithFuel) + target.GetItemAmount(InventoryItems.SpaceshipWithFuel));
                    killer.RpcSetItemAmount(InventoryItems.BreathingMaskWithoutOxygen, killer.GetItemAmount(InventoryItems.BreathingMaskWithoutOxygen) + target.GetItemAmount(InventoryItems.BreathingMaskWithoutOxygen));
                    killer.RpcSetItemAmount(InventoryItems.BreathingMaskWithOxygen, killer.GetItemAmount(InventoryItems.BreathingMaskWithOxygen) + target.GetItemAmount(InventoryItems.BreathingMaskWithOxygen));
                    killer.RpcSetItemAmount(InventoryItems.GuardOutfit, killer.GetItemAmount(InventoryItems.GuardOutfit) + target.GetItemAmount(InventoryItems.GuardOutfit));
                    if (target.GetItemAmount(InventoryItems.Armor) > killer.GetItemAmount(InventoryItems.Armor))
                        killer.RpcSetItemAmount(InventoryItems.Armor, target.GetItemAmount(InventoryItems.Armor));
                    target.RpcSetItemAmount(InventoryItems.Resources, 0);
                    target.RpcSetItemAmount(InventoryItems.Screwdriver, 0);
                    target.RpcSetItemAmount(InventoryItems.Weapon, 0);
                    target.RpcSetItemAmount(InventoryItems.Pickaxe, 0);
                    target.RpcSetItemAmount(InventoryItems.SpaceshipParts, 0);
                    target.RpcSetItemAmount(InventoryItems.SpaceshipWithoutFuel, 0);
                    target.RpcSetItemAmount(InventoryItems.SpaceshipWithFuel, 0);
                    target.RpcSetItemAmount(InventoryItems.GuardOutfit, 0);
                    target.RpcSetItemAmount(InventoryItems.Armor, 0);
                    target.RpcShapeshift(target, false);
                    target.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Prisoner);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        Main.NameColors[(killer.PlayerId, pc.PlayerId)] = Palette.Orange;
                }   
                RespawnCooldown[target.PlayerId] = Options.RespawnCooldown.GetFloat();
                target.Data.IsDead = true;
                new LateTask(() => {
                    AntiCheat.IsDead[target.PlayerId] = true;
                }, Mathf.Max(0.02f, AmongUsClient.Instance.Ping / 1000f * 12f));
                Utils.SendGameData();
                target.SyncPlayerSettings();
                target.RpcTeleport(new Vector2(1000f, 1000f));
            }
            killer.RpcSetKillTimer(1f);
            return false;
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (shapeshifter.GetPlainShipRoom() != null && shapeshifter.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && shapeshifter.IsGuard())
            {
                ReactorWallHealth = 100f;
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            else if (shapeshifter.GetPlainShipRoom() != null && shapeshifter.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !shapeshifter.IsGuard() && shapeshifter.HasItem(InventoryItems.Pickaxe))
            {
                ReactorWallHealth -= shapeshifter.GetItemAmount(InventoryItems.Pickaxe) * Options.PickaxeSpeed.GetFloat();
                if (ReactorWallHealth < 0f)
                    ReactorWallHealth = 0f;
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            else if (!shapeshifter.IsGuard() && shapeshifter.HasItem(InventoryItems.GuardOutfit) && (target.IsGuard() || target == shapeshifter))
            {
                shapeshifter.RpcShapeshift(target, false);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (target.IsGuard())
                        Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Color.blue;
                    else if (shapeshifter.IsWanted())
                        Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Color.red;
                    else
                        Main.NameColors[(shapeshifter.PlayerId, pc.PlayerId)] = Palette.Orange;
                }
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            var recipe = (Recipes)shapeshifter.GetCurrentRecipe();
            switch (recipe)
            {
                case Recipes.Screwdriver:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.ScrewdriverPrice.GetInt()) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.ScrewdriverPrice.GetInt());
                    shapeshifter.RpcSetItemAmount(InventoryItems.Screwdriver, shapeshifter.GetItemAmount(InventoryItems.Screwdriver) + 1);
                    break;
                case Recipes.PrisonerWeapon:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.PrisonerWeaponPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Weapon) + 1)) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.PrisonerWeaponPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Weapon) + 1));
                    shapeshifter.RpcSetItemAmount(InventoryItems.Weapon, shapeshifter.GetItemAmount(InventoryItems.Weapon) + 1);
                    int recipeId = 1;
                    if (shapeshifter.GetItemAmount(InventoryItems.Weapon) >= 10)
                        recipeId = 2;
                    if (shapeshifter.GetItemAmount(InventoryItems.Pickaxe) >= 10 && recipeId == 2)
                        recipeId = 3;
                    if (recipeId != 1)
                        shapeshifter.RpcSetCurrentRecipe(recipeId);
                    break;
                case Recipes.Pickaxe:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.PickaxePrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Pickaxe) + 1)) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.PickaxePrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Pickaxe) + 1));
                    shapeshifter.RpcSetItemAmount(InventoryItems.Pickaxe, shapeshifter.GetItemAmount(InventoryItems.Pickaxe) + 1);
                    if (shapeshifter.GetItemAmount(InventoryItems.Pickaxe) >= 10)
                        shapeshifter.RpcSetCurrentRecipe(3);
                    break;
                case Recipes.SpaceshipPart:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.SpaceshipPartPrice.GetInt()) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.SpaceshipPartPrice.GetInt());
                    shapeshifter.RpcSetItemAmount(InventoryItems.SpaceshipParts, shapeshifter.GetItemAmount(InventoryItems.SpaceshipParts) + 1);
                    break;
                case Recipes.Spaceship:
                    if (shapeshifter.GetItemAmount(InventoryItems.SpaceshipParts) < Options.RequiredSpaceshipParts.GetInt()) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.SpaceshipParts, shapeshifter.GetItemAmount(InventoryItems.SpaceshipParts) - Options.RequiredSpaceshipParts.GetInt());
                    shapeshifter.RpcSetItemAmount(InventoryItems.SpaceshipWithoutFuel, shapeshifter.GetItemAmount(InventoryItems.SpaceshipWithoutFuel) + 1);
                    break;
                case Recipes.BreathingMask:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.BreathingMaskPrice.GetInt()) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.BreathingMaskPrice.GetInt());
                    shapeshifter.RpcSetItemAmount(InventoryItems.BreathingMaskWithoutOxygen, shapeshifter.GetItemAmount(InventoryItems.BreathingMaskWithoutOxygen) + 1);
                    break;
                case Recipes.GuardOutfit:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.GuardOutfitPrice.GetInt()) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.GuardOutfitPrice.GetInt());
                    shapeshifter.RpcSetItemAmount(InventoryItems.GuardOutfit, shapeshifter.GetItemAmount(InventoryItems.GuardOutfit) + 1);
                    break;
                case Recipes.PrisonerArmor:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.PrisonerArmorPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Armor) + 1)) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.PrisonerArmorPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Armor) + 1));
                    shapeshifter.RpcSetItemAmount(InventoryItems.Armor, shapeshifter.GetItemAmount(InventoryItems.Armor) + 1);
                    if (shapeshifter.GetItemAmount(InventoryItems.Armor) >= 10)
                        shapeshifter.RpcSetCurrentRecipe(0);
                    break;
                case Recipes.GuardWeapon:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.GuardWeaponPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Weapon) + 1)) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.GuardWeaponPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Weapon) + 1));
                    shapeshifter.RpcSetItemAmount(InventoryItems.Weapon, shapeshifter.GetItemAmount(InventoryItems.Weapon) + 1);
                    if (shapeshifter.GetItemAmount(InventoryItems.Weapon) >= 10)
                        shapeshifter.RpcSetCurrentRecipe(1001);
                    break;
                case Recipes.EnergyDrink:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.EnergyDrinkPrice.GetInt()) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.EnergyDrinkPrice.GetInt());
                    EnergyDrinkDuration[shapeshifter.PlayerId] = Options.EnergyDrinkDuration.GetFloat();
                    shapeshifter.SyncPlayerSettings();
                    break;
                case Recipes.GuardArmor:
                    if (shapeshifter.GetItemAmount(InventoryItems.Resources) < Options.GuardArmorPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Armor) + 1)) break;
                    shapeshifter.RpcSetItemAmount(InventoryItems.Resources, shapeshifter.GetItemAmount(InventoryItems.Resources) - Options.GuardArmorPrice.GetInt() * (shapeshifter.GetItemAmount(InventoryItems.Armor) + 1));
                    shapeshifter.RpcSetItemAmount(InventoryItems.Armor, shapeshifter.GetItemAmount(InventoryItems.Armor) + 1);
                    if (shapeshifter.GetItemAmount(InventoryItems.Armor) >= 10)
                        shapeshifter.RpcSetCurrentRecipe(1000);
                    break;
            }
            shapeshifter.RpcResetAbilityCooldown();
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.IsDead)
                {
                    if (pc.HasEscaped()) continue;
                    if (RespawnCooldown[pc.PlayerId] > 0f)
                    {
                        RespawnCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (RespawnCooldown[pc.PlayerId] <= 0f)
                    {
                        PlayerHealth[pc.PlayerId] = pc.IsGuard() ? Options.GuardHealth.GetFloat() : Options.PrisonerHealth.GetFloat();
                        pc.Data.IsDead = false;
                        AntiCheat.IsDead[pc.PlayerId] = false;
                        Utils.SendGameData();
                        if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3)
                            pc.MyPhysics.RpcBootFromVent(6);
                        pc.RpcShapeshift(pc, false);
                        pc.SyncPlayerSettings();
                        RespawnCooldown[pc.PlayerId] = 0f;
                    }   
                    continue;
                }
                if (ChangeRecipeCooldown[pc.PlayerId] > 0f)
                {
                    ChangeRecipeCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (ChangeRecipeCooldown[pc.PlayerId] < 0f)
                {
                    ChangeRecipeCooldown[pc.PlayerId] = 0f;
                }
                if (!pc.IsGuard() && pc.GetPlainShipRoom() != null && (pc.GetPlainShipRoom().RoomId == SystemTypes.LowerEngine || pc.GetPlainShipRoom().RoomId == SystemTypes.UpperEngine) && pc.HasItem(InventoryItems.SpaceshipWithoutFuel)  && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                {
                    pc.RpcSetItemAmount(InventoryItems.SpaceshipWithFuel, pc.GetItemAmount(InventoryItems.SpaceshipWithFuel) + pc.GetItemAmount(InventoryItems.SpaceshipWithoutFuel));
                    pc.RpcSetItemAmount(InventoryItems.SpaceshipWithoutFuel, 0);
                }
                if (!pc.IsGuard() && pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.LifeSupp && pc.HasItem(InventoryItems.BreathingMaskWithoutOxygen)  && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3))
                {
                    pc.RpcSetItemAmount(InventoryItems.BreathingMaskWithOxygen, pc.GetItemAmount(InventoryItems.BreathingMaskWithOxygen) + pc.GetItemAmount(InventoryItems.BreathingMaskWithoutOxygen));
                    pc.RpcSetItemAmount(InventoryItems.BreathingMaskWithoutOxygen, 0);
                }
                if (pc.IsGuard() && SearchCooldown[pc.PlayerId] > 0f)
                {
                    SearchCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                }
                if (pc.IsGuard() && SearchCooldown[pc.PlayerId] < 0f)
                {
                    SearchCooldown[pc.PlayerId] = 0;
                }
                bool guardNearby = false;
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    if (ar.IsGuard() && !ar.Data.IsDead && Vector2.Distance(ar.transform.position, pc.transform.position) <= Main.RealOptions.GetFloat(FloatOptionNames.ImpostorLightMod) * 3 
                    && !PhysicsHelpers.AnythingBetween(ar.transform.position, pc.transform.position, Constants.ShadowMask, false))
                        guardNearby = true;
                }
                if (!pc.IsGuard() && pc.IsDoingIllegalThing() && guardNearby)
                {
                    pc.RpcSetJailbreakPlayerType(JailbreakPlayerTypes.Wanted);
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
                if (pc.IsGuard() && TakeoverTimer > 0f && pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.Nav && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                    TakeoverTimer = 0f;
                if (!pc.IsGuard() && pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.Storage && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && pc.HasItem(InventoryItems.SpaceshipWithFuel))
                    pc.RpcEscape();
                if (!pc.IsGuard() && pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.Reactor && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && pc.HasItem(InventoryItems.BreathingMaskWithOxygen) && ReactorWallHealth <= 0f && !pc.inVent)
                    pc.RpcEscape();
            }
            bool isGuardAlive = false;
            bool isPrisonerInNav = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsGuard() && !pc.Data.IsDead)
                    isGuardAlive = true;
                if (!pc.IsGuard() && !pc.Data.IsDead && pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.Nav && !pc.inVent)
                    isPrisonerInNav = true;
            }
            if (!isGuardAlive && isPrisonerInNav)
                TakeoverTimer += Time.fixedDeltaTime;
            if (TakeoverTimer >= Options.PrisonTakeoverDuration.GetFloat())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.IsGuard() && !pc.HasEscaped())
                        pc.RpcEscape();
                }
            }
            OneSecondTimer += Time.fixedDeltaTime;
            if (OneSecondTimer >= 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.IsDead) continue;
                    if (pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.MedBay && TimeSinceLastDamage[pc.PlayerId] >= 5f  && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                    {
                        if (pc.IsGuard())
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
                    else
                    {
                        if (pc.IsGuard())
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
                    if (pc.IsGuard())
                        pc.RpcSetItemAmount(InventoryItems.Resources, pc.GetItemAmount(InventoryItems.Resources) + 2);
                    if (!pc.IsGuard() && pc.GetPlainShipRoom() != null && pc.GetPlainShipRoom().RoomId == SystemTypes.Electrical && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3) && !pc.inVent)
                        pc.RpcSetItemAmount(InventoryItems.Resources, Math.Min(pc.GetItemAmount(InventoryItems.Resources) + 2, Options.MaximumPrisonerResources.GetInt()));
                    if (!pc.IsGuard() && pc.GetPlainShipRoom() && pc.GetPlainShipRoom().RoomId == SystemTypes.Storage && (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0 || Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3))
                        pc.RpcSetItemAmount(InventoryItems.Resources, Math.Min(pc.GetItemAmount(InventoryItems.Resources) + 5, Options.MaximumPrisonerResources.GetInt()));
                }
                OneSecondTimer -= 1f;
            }
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

        public JailbreakGamemode()
        {
            Gamemode = Gamemodes.Jailbreak;
            PetAction = true;
            DisableTasks = true;
            PlayerType = new System.Collections.Generic.Dictionary<byte, JailbreakPlayerTypes>();
            Inventory = new System.Collections.Generic.Dictionary<(byte, InventoryItems), int>();
            ReactorWallHealth = 100f;
            TakeoverTimer = 0f;
            TimeSinceLastDamage = new System.Collections.Generic.Dictionary<byte, float>();
            RespawnCooldown = new System.Collections.Generic.Dictionary<byte, float>();
            SearchCooldown = new System.Collections.Generic.Dictionary<byte, float>();
            PlayerHealth = new System.Collections.Generic.Dictionary<byte, float>();
            CurrentRecipe = new System.Collections.Generic.Dictionary<byte, int>();
            EnergyDrinkDuration = new System.Collections.Generic.Dictionary<byte, float>();
            OneSecondTimer = 0f;
            TimeSinceNameUpdate = new System.Collections.Generic.Dictionary<byte, float>();
            ChangeRecipeCooldown = new System.Collections.Generic.Dictionary<byte, float>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                PlayerType[pc.PlayerId] = JailbreakPlayerTypes.None;
                foreach (var item in Enum.GetValues(typeof(InventoryItems)))
                    Inventory[(pc.PlayerId, (InventoryItems)item)] = 0;
                TimeSinceLastDamage[pc.PlayerId] = 0f;
                RespawnCooldown[pc.PlayerId] = 0f;
                SearchCooldown[pc.PlayerId] = Options.SearchCooldown.GetFloat();
                PlayerHealth[pc.PlayerId] = 0;
                CurrentRecipe[pc.PlayerId] = -1;
                EnergyDrinkDuration[pc.PlayerId] = 0f;
                TimeSinceNameUpdate[pc.PlayerId] = 1f / GameData.Instance.PlayerCount * pc.PlayerId;
                ChangeRecipeCooldown[pc.PlayerId] = 0f;
            }
        }

        public static JailbreakGamemode instance;
        public System.Collections.Generic.Dictionary<byte, JailbreakPlayerTypes> PlayerType;
        public System.Collections.Generic.Dictionary<(byte, InventoryItems),int> Inventory;
        public float ReactorWallHealth;
        public float TakeoverTimer;
        public System.Collections.Generic.Dictionary<byte, float> TimeSinceLastDamage;
        public System.Collections.Generic.Dictionary<byte, float> RespawnCooldown;
        public System.Collections.Generic.Dictionary<byte, float> SearchCooldown;
        public System.Collections.Generic.Dictionary<byte, float> PlayerHealth;
        public System.Collections.Generic.Dictionary<byte, int> CurrentRecipe;
        public System.Collections.Generic.Dictionary<byte, float> EnergyDrinkDuration;
        public float OneSecondTimer;
        public System.Collections.Generic.Dictionary<byte, float> TimeSinceNameUpdate;
        public System.Collections.Generic.Dictionary<byte, float> ChangeRecipeCooldown;
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
}