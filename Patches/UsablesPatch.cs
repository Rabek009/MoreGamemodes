using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    class CanUseVentPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse, ref float __result)
        {
            float num = float.MaxValue;
            if (pc.IsDead) return false;

            canUse = couldUse = pc.Object.CanVent();
            canUse = couldUse = (pc.Object.inVent || canUse) && (pc.Object.CanMove || pc.Object.inVent);

            if (canUse)
            {
                Vector2 truePosition = pc.Object.GetTruePosition();
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);
                canUse &= num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }

            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigameUpdatePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.HideAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.ShiftAndSeek || CustomGamemode.Instance.Gamemode == Gamemodes.BombTag ||
                (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.IsHackActive) || CustomGamemode.Instance.Gamemode == Gamemodes.BattleRoyale || CustomGamemode.Instance.Gamemode == Gamemodes.Speedrun ||
                CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle || CustomGamemode.Instance.Gamemode == Gamemodes.KillOrDie || CustomGamemode.Instance.Gamemode == Gamemodes.Jailbreak
                || (CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun && Options.DisableMeetings.GetBool()))
                __instance.Close();
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsMinigameUpdatePatch
    {
        public static void Postfix(VitalsMinigame __instance)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.IsHackActive)
                __instance.Close();
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.BombTag || CustomGamemode.Instance.Gamemode == Gamemodes.BattleRoyale || CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle || CustomGamemode.Instance.Gamemode == Gamemodes.KillOrDie || CustomGamemode.Instance.Gamemode == Gamemodes.Jailbreak)
                return false;
            return !pc.Role.IsImpostor || __instance.AllowImpostor;
        }
    }
}