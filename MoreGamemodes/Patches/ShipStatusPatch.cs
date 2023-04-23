using HarmonyLib;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    class CloseDoorsPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            return !((Options.CurrentGamemode == Gamemodes.HideAndSeek && !Options.HnSImpostorsCanCloseDoors.GetBool()) || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && !Options.SnSImpostorsCanCloseDoors.GetBool()) ||
                Options.CurrentGamemode == Gamemodes.BombTag || (Options.CurrentGamemode == Gamemodes.RandomItems && Main.HackTimer > 1f && Options.HackAffectsImpostors.GetBool()) || Options.CurrentGamemode == Gamemodes.BattleRoyale);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
    class RepairSystemPatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player)
        {
            if (systemType == SystemTypes.Sabotage)
                return !(Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek || Options.CurrentGamemode == Gamemodes.BombTag || Options.CurrentGamemode == Gamemodes.BattleRoyale);
            else if (systemType == SystemTypes.MeetingRoom && (Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek || Options.CurrentGamemode == Gamemodes.BombTag || Options.CurrentGamemode == Gamemodes.BattleRoyale || Options.CurrentGamemode == Gamemodes.Speedrun))
                return false;
            else if (Options.CurrentGamemode == Gamemodes.RandomItems && Main.HackTimer > 1f && (Main.Impostors.Contains(player.PlayerId) == false || Options.HackAffectsImpostors.GetBool()))
                return false;
            else
                return true;
        }
    }
}