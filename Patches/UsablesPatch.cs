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
            couldUse = GameManager.Instance.LogicUsables.CanUse(usableVent, playerControl) && pc.Role.CanUse(__instance.Cast<IUsable>()) && (!playerControl.MustCleanVent(__instance.Id) || (playerControl.inVent && Vent.currentVent == __instance)) && !playerControl.Data.IsDead && (playerControl.CanMove || playerControl.inVent);
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

    [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
    class SetButtonsPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] bool enabled)
        {
            if (!enabled) return true;
            Vent[] nearbyVents = __instance.NearbyVents;
		    Vector2 vector;
		    if (__instance.Right && __instance.Left)
		    {
			    vector = (__instance.Right.transform.position + __instance.Left.transform.position) / 2f - __instance.transform.position;
		    }
		    else
		    {
		    	vector = Vector2.zero;
		    }
		    for (int i = 0; i < __instance.Buttons.Length; i++)
		    {
			    ButtonBehavior buttonBehavior = __instance.Buttons[i];
				Vent vent = nearbyVents[i];
				if (vent)
				{
					VentilationSystem ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
					bool ventBeingCleaned = ventilationSystem != null && (ventilationSystem.IsVentCurrentlyBeingCleaned(vent.Id) || !CustomGamemode.Instance.OnEnterVent(PlayerControl.LocalPlayer, vent.Id));
					buttonBehavior.gameObject.SetActive(true);
					__instance.ToggleNeighborVentBeingCleaned(ventBeingCleaned, buttonBehavior, __instance.CleaningIndicators[i]);
				    Vector3 vector2 = vent.transform.position - __instance.transform.position;
				    Vector3 vector3 = vector2.normalized * (0.7f + __instance.spreadShift);
				    vector3.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
				    vector3.y -= 0.08f;
				    vector3.z = -10f;
				    buttonBehavior.transform.localPosition = vector3;
				    buttonBehavior.transform.LookAt2d(vent.transform);
				    vector3 = vector3.RotateZ((vector.AngleSigned(vector2) > 0f) ? __instance.spreadAmount : (-__instance.spreadAmount));
				    buttonBehavior.transform.localPosition = vector3;
				    buttonBehavior.transform.Rotate(0f, 0f, (vector.AngleSigned(vector2) > 0f) ? __instance.spreadAmount : (-__instance.spreadAmount));
			    }
			    else
			    {
				    buttonBehavior.gameObject.SetActive(false);
			    }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.UpdateArrows))]
    class UpdateArrowsPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] VentilationSystem ventSystem)
        {
            if (__instance != Vent.currentVent || ventSystem == null)
		    {
			    return false;
		    }
		    Vent[] nearbyVents = __instance.NearbyVents;
		    for (int i = 0; i < nearbyVents.Length; i++)
		    {
			    Vent vent = nearbyVents[i];
			    if (vent)
			    {
			    	bool ventBeingCleaned = ventSystem.IsVentCurrentlyBeingCleaned(vent.Id) || !CustomGamemode.Instance.OnEnterVent(PlayerControl.LocalPlayer, vent.Id);
			    	ButtonBehavior b = __instance.Buttons[i];
			    	GameObject c = __instance.CleaningIndicators[i];
			    	__instance.ToggleNeighborVentBeingCleaned(ventBeingCleaned, b, c);
			    }
		    }
            return false;
        }
    }

    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    class ConsoleCanUsePatch
    {
        public static bool Prefix(Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ((!pc.Object.GetRole().IsCrewmate() && ! __instance.AllowImpostor) || ClassicGamemode.instance.IsRoleblocked[PlayerControl.LocalPlayer.PlayerId]))
                return false;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && (PlayerControl.LocalPlayer.shouldAppearInvisible || PlayerControl.LocalPlayer.invisibilityAlpha < 1f))
                return false;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
                return false;
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && (PlayerControl.LocalPlayer.shouldAppearInvisible || PlayerControl.LocalPlayer.invisibilityAlpha < 1f))
                return false;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
                return false;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
                return false;
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
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && ClassicGamemode.instance.IsRoleblocked[PlayerControl.LocalPlayer.PlayerId])
                __instance.Close();
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