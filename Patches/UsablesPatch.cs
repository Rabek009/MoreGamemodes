using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    class CanUseVentPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse, ref float __result)
        {
            PlayerControl playerControl = pc.Object;
            couldUse = CustomGamemode.Instance.OnEnterVent(pc.Object, __instance.Id);
            canUse = couldUse;
            if (!canUse)
            {
                return false;
            }
            IUsable usableVent = __instance.Cast<IUsable>();
            float actualDistance = float.MaxValue;
            couldUse = GameManager.Instance.LogicUsables.CanUse(usableVent, playerControl) && (!playerControl.MustCleanVent(__instance.Id) || (playerControl.inVent && Vent.currentVent == __instance)) && !playerControl.Data.IsDead && (playerControl.CanMove || playerControl.inVent);
            if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var systemType))
            {
                VentilationSystem ventilationSystem = systemType.TryCast<VentilationSystem>();
                if (ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(__instance.Id))
                {
                    couldUse = false;
                }
            }
            canUse = couldUse;
            if (canUse)
            {
                Vector3 center = playerControl.Collider.bounds.center;
                Vector3 ventPosition = __instance.transform.position;
                actualDistance = Vector2.Distance(center, ventPosition);
                canUse &= actualDistance <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(playerControl.Collider, center, ventPosition, Constants.ShipOnlyMask, false);
            }
            __result = actualDistance;
            return false;
        }
    }

    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    class ConsoleCanUsePatch
    {
        public static bool Prefix(Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!pc.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlatformConsole), nameof(PlatformConsole.CanUse))]
    class PlatformConsoleCanUsePatch
    {
        public static bool Prefix(PlatformConsole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!pc.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (Options.EnableDisableGapPlatform.GetBool())
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(ZiplineConsole), nameof(ZiplineConsole.CanUse))]
    class ZiplineConsoleCanUsePatch
    {
        public static bool Prefix(ZiplineConsole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!pc.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive)
                return false;
            if (Options.EnableDisableZipline.GetBool())
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    class MapConsoleCanUsePatch
    {
        public static bool Prefix(MapConsole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.BaseWars)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    class SystemConsoleCanUsePatch
    {
        public static bool Prefix(SystemConsole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.HideAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.ShiftAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.BombTag ||
                (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.IsHackActive) || CustomGamemode.Instance.Gamemode == Gamemodes.BattleRoyale || CustomGamemode.Instance.Gamemode == Gamemodes.Speedrun ||
                CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle || CustomGamemode.Instance.Gamemode == Gamemodes.KillOrDie || (CustomGamemode.Instance.Gamemode == Gamemodes.Zombies && ZombiesGamemode.instance.IsZombie(pc.Object)) ||
                CustomGamemode.Instance.Gamemode == Gamemodes.Jailbreak || (CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun && Options.DisableMeetings.GetBool()) || CustomGamemode.Instance.Gamemode == Gamemodes.FreezeTag || 
                CustomGamemode.Instance.Gamemode == Gamemodes.ColorWars)
            {
                if (__instance.MinigamePrefab.TryCast<EmergencyMinigame>())
                    return false;
            }
            if (CustomGamemode.Instance.Gamemode == Gamemodes.BaseWars)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigameUpdatePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.HideAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.ShiftAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.BombTag ||
                (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.IsHackActive) || CustomGamemode.Instance.Gamemode == Gamemodes.BattleRoyale || CustomGamemode.Instance.Gamemode == Gamemodes.Speedrun ||
                CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle || CustomGamemode.Instance.Gamemode == Gamemodes.KillOrDie || (CustomGamemode.Instance.Gamemode == Gamemodes.Zombies && ZombiesGamemode.instance.IsZombie(PlayerControl.LocalPlayer)) ||
                CustomGamemode.Instance.Gamemode == Gamemodes.Jailbreak || (CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun && Options.DisableMeetings.GetBool()) || CustomGamemode.Instance.Gamemode == Gamemodes.FreezeTag || 
                CustomGamemode.Instance.Gamemode == Gamemodes.ColorWars)
            {
                __instance.Close();
            }
        }
    }
}