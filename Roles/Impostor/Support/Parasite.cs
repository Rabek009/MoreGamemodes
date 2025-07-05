using UnityEngine;
using System;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class Parasite : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = InfectCooldown.GetFloat();
            Player.RpcSetPetAbilityCooldown(true);
        }

        public override void OnHudUpate(HudManager __instance)
        {
            __instance.ReportButton.OverrideText(TranslationController.Instance.GetString(StringNames.ReportButton));
            __instance.PetButton.OverrideText("Infect");
            if (ClassicGamemode.instance.IsOnPetAbilityCooldown[Player.PlayerId])
                __instance.PetButton.SetDisabled();
            PlayerControl target = Player.GetClosestPlayer(true);
            if (target == null || Vector2.Distance(Player.transform.position, target.transform.position) > 1.8f || PhysicsHelpers.AnythingBetween(Player.Collider, Player.Collider.bounds.center, target.transform.position, Constants.ShipOnlyMask, false) || target.GetRole().IsImpostor())
                __instance.PetButton.SetDisabled();
        }

        public override void OnIntroDestroy()
        {
            Cooldown = 10f;
            Player.RpcSetPetAbilityCooldown(true);
        }

        public override void OnPet()
        {
            if (Cooldown > 0f) return;
            PlayerControl target = Player.GetClosestPlayer(true);
            if ((target == null || Vector2.Distance(Player.transform.position, target.transform.position) > 1.8f || PhysicsHelpers.AnythingBetween(Player.Collider, Player.Collider.bounds.center, target.transform.position, Constants.ShipOnlyMask, false)) && !Player.AmOwner && !Main.IsModded[Player.PlayerId])
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) No player nearby (!)"));
                return;
            }
            if (target.GetRole().IsImpostor())
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) You can't infect impostor (!)"));
                return;
            }
            if (target.GetRole().Role == CustomRoles.Romantic)
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) Target can't be infected (!)"));
                return;
            }
            switch (CurrentTurnedImpostorRole)
            {
                case TurnedImpostorRoles.Impostor:
                    ClassicGamemode.instance.ChangeRole(target, CustomRoles.Impostor);
                    break;
                case TurnedImpostorRoles.Parasite:
                    ClassicGamemode.instance.ChangeRole(target, CustomRoles.Parasite);
                    break;
                case TurnedImpostorRoles.RandomNonExisting:
                    List<CustomRoles> PotentialRoles = new();
                    foreach (var role in Enum.GetValues<CustomRoles>())
                    {
                        bool roleExists = false;
                        foreach (var player in GameData.Instance.AllPlayers)
                        {
                            if (player.GetRole().Role == role)
                            {
                                roleExists = true;
                                break;
                            }
                        }
                        if (role != CustomRoles.Impostor && CustomRolesHelper.IsImpostor(role) && CustomRolesHelper.GetRoleChance(role) > 0 && !roleExists)
                            PotentialRoles.Add(role);
                    }
                    var rand = new System.Random();
                    CustomRoles selectedRole = PotentialRoles.Count > 0 ? PotentialRoles[rand.Next(0, PotentialRoles.Count)] : CustomRoles.Impostor;
                    ClassicGamemode.instance.ChangeRole(target, selectedRole);
                    break;
                case TurnedImpostorRoles.Random:
                    List<CustomRoles> PotentialRoles2 = new();
                    foreach (var role in Enum.GetValues<CustomRoles>())
                    {
                        if (role != CustomRoles.Impostor && CustomRolesHelper.IsImpostor(role) && CustomRolesHelper.GetRoleChance(role) > 0)
                            PotentialRoles2.Add(role);
                    }
                    var rand2 = new System.Random();
                    CustomRoles selectedRole2 = PotentialRoles2.Count > 0 ? PotentialRoles2[rand2.Next(0, PotentialRoles2.Count)] : CustomRoles.Impostor;
                    ClassicGamemode.instance.ChangeRole(target, selectedRole2);
                    break;
            }
            target.Notify(Utils.ColorString(Color, "Parasite infected you!"));
            Player.RpcSetDeathReason(DeathReasons.Suicide);
            Player.RpcMurderPlayer(Player, true);
        }

        public override void OnMurderPlayer(PlayerControl target)
        {
            Cooldown = InfectCooldown.GetFloat();
            Player.RpcSetPetAbilityCooldown(true);
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            if (Cooldown > -1f)
                Cooldown -= Time.fixedDeltaTime;
            if (Cooldown <= 0f && Cooldown > -1f)
            {
                Cooldown = -1f;
                Player.RpcSetPetAbilityCooldown(false);
            }
        }

        public override string GetNamePostfix()
        {
            return Utils.ColorString(Color.red, "\n<size=1.8>Infect cooldown: " + (int)(Cooldown + 0.99f) + "s</size>");
        }

        public override void OnRevive()
        {
            Cooldown = 10f;
        }

        public Parasite(PlayerControl player)
        {
            Role = CustomRoles.Parasite;
            BaseRole = BaseRoles.Impostor;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            Cooldown = 10f;
        }
        
        public float Cooldown;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem InfectCooldown;
        public static OptionItem TurnedImpostorRole;
        public static readonly string[] turnedImpostorRoles =
        {
            "Impostor", "Parasite", "Random (non existing)", "Random"
        };
        public static TurnedImpostorRoles CurrentTurnedImpostorRole => (TurnedImpostorRoles)TurnedImpostorRole.GetValue();
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(700200, CustomRoles.Parasite, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(700201, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            InfectCooldown = FloatOptionItem.Create(700202, "Infect cooldown", new(10f, 90f, 5f), 25f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            TurnedImpostorRole = StringOptionItem.Create(700203, "Turned impostor role", turnedImpostorRoles, 0, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Parasite] = Chance;
            Options.RolesCount[CustomRoles.Parasite] = Count;
        }
    }

    public enum TurnedImpostorRoles
    {
        Impostor,
        Parasite,
        RandomNonExisting,
        Random,
    }
}