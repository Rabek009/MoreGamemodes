using HarmonyLib;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    class CanUseVentPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse, ref float __result)
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
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek || Options.CurrentGamemode == Gamemodes.BombTag ||
                (Options.CurrentGamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.HackTimer > 0f) || Options.CurrentGamemode == Gamemodes.BattleRoyale || Options.CurrentGamemode == Gamemodes.Speedrun ||
                Options.CurrentGamemode == Gamemodes.PaintBattle || Options.CurrentGamemode == Gamemodes.KillOrDie)
                __instance.Close();
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsMinigameUpdatePatch
    {
        public static void Postfix(VitalsMinigame __instance)
        {
            if (Options.CurrentGamemode == Gamemodes.RandomItems && RandomItemsGamemode.instance.HackTimer > 0f)
                __instance.Close();
        }
    }

    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    class ConsoleCanUsePatch
    {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (Options.CurrentGamemode == Gamemodes.RandomItems && (!pc.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.HackTimer > 0f)
                return false;
            if (Options.CurrentGamemode == Gamemodes.BombTag || Options.CurrentGamemode == Gamemodes.BattleRoyale || Options.CurrentGamemode == Gamemodes.PaintBattle || Options.CurrentGamemode == Gamemodes.KillOrDie)
                return false;
            return !pc.Role.IsImpostor || __instance.AllowImpostor;
        }
    }
}