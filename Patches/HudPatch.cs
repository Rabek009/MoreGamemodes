using HarmonyLib;
using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerPatch
    {
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) return;
            if (!Main.GameStarted || MeetingHud.Instance || !SetHudActivePatch.IsActive) return;
            CustomGamemode.Instance.OnHudUpdate(__instance);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(bool))]
    class SetHudActivePatch
    {
        public static bool IsActive = true;
        public static void Prefix(HudManager __instance, [HarmonyArgument(0)] ref bool isActive)
        {
            IsActive = isActive;
        }
    }

    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
    class SetTaskTextPatch
    {
        public static void Postfix(TaskPanelBehaviour __instance, [HarmonyArgument(0)] string str)
        {
            if (!Main.GameStarted) return;
            CustomGamemode.Instance.OnSetTaskText(__instance, str);
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    class ShowNormalMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            CustomGamemode.Instance.OnShowNormalMap(__instance);
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class ShowSabotageMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            CustomGamemode.Instance.OnShowSabotageMap(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
    class ToggleHighlightPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool active)
        {
            if (active)
                CustomGamemode.Instance.OnToggleHighlight(__instance);
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    class SetOutlinePatch
    {
        public static void Postfix(Vent __instance, [HarmonyArgument(0)] bool on, [HarmonyArgument(1)] bool mainTarget)
        {
            if (on)
                CustomGamemode.Instance.OnSetOutline(__instance, mainTarget);
        }
    }

    [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
    class ShapeshifterMinigameBeginPatch
    {
        public static bool Prefix(ShapeshifterMinigame __instance, [HarmonyArgument(0)] PlayerTask task)
        {
            if (CustomGamemode.Instance.Gamemode != Gamemodes.Classic) return true;
            if (PlayerControl.LocalPlayer.GetRole().Role == CustomRoles.Undertaker)
            {
                Minigame.Instance = __instance;
                __instance.MyTask = task;
                __instance.MyNormTask = task as NormalPlayerTask;
                __instance.timeOpened = Time.realtimeSinceStartup;
                if (PlayerControl.LocalPlayer)
                {
                    if (MapBehaviour.Instance)
                    {
                        MapBehaviour.Instance.Close();
                    }
                    PlayerControl.LocalPlayer.MyPhysics.SetNormalizedVelocity(Vector2.zero);
                }
                __instance.logger.Info("Opening minigame " + __instance.GetType().Name, null);
                __instance.StartCoroutine(__instance.CoAnimateOpen());
                DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MinigameOpened(PlayerControl.LocalPlayer.Data, __instance.TaskType);
                List<PlayerControl> list = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != PlayerControl.LocalPlayer && pc.GetRole().IsImpostor() && !pc.Data.IsDead)
                        list.Add(pc);
                }
                __instance.potentialVictims = new List<ShapeshifterPanel>();
                List<UiElement> list2 = new();
                for (int i = 0; i < list.Count; i++)
                {
                    PlayerControl player = list[i];
                    int num = i % 3;
                    int num2 = i / 3;
                    bool flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;
                    ShapeshifterPanel shapeshifterPanel = Object.Instantiate(__instance.PanelPrefab, __instance.transform);
                    shapeshifterPanel.transform.localPosition = new Vector3(__instance.XStart + num * __instance.XOffset, __instance.YStart + num2 * __instance.YOffset, -1f);
                    shapeshifterPanel.SetPlayer(i, player.Data, (Il2CppSystem.Action)(() =>
                    {
                        __instance.Shapeshift(player);
                    }));
                    shapeshifterPanel.NameText.color = flag ? player.Data.Role.NameColor : Color.white;
                    __instance.potentialVictims.Add(shapeshifterPanel);
                    list2.Add(shapeshifterPanel.Button);
                }
                ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, list2, false);
                return false;
            }
            return true;
        }
    }
}