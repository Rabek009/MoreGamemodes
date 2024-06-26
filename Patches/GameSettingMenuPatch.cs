using System;
using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreGamemodes;

[HarmonyPatch(typeof(GameSettingMenu))]
public class GameSettingMenuPatch
{

    private static GameOptionsMenu templateGameOptionsMenu;
    private static PassiveButton templateGameSettingsButton;

    static Dictionary<TabGroup, PassiveButton> ModSettingsButtons = new();
    static Dictionary<TabGroup, GameOptionsMenu> ModSettingsTabs = new();

    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void StartPostfix(GameSettingMenu __instance)
    {
        ModSettingsButtons = new();
        var gamepreset = __instance.GamePresetsButton;

        var gamesettings = __instance.GameSettingsButton;
        __instance.GameSettingsButton.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        __instance.GameSettingsButton.transform.localPosition = new Vector3(gamesettings.transform.localPosition.x, gamepreset.transform.localPosition.y + 0.1f, gamesettings.transform.localPosition.z);

        var rolesettings = __instance.RoleSettingsButton;
        __instance.RoleSettingsButton.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        __instance.RoleSettingsButton.transform.localPosition = new Vector3(rolesettings.transform.localPosition.x, gamesettings.transform.localPosition.y - 0.4f, rolesettings.transform.localPosition.z);

        //button 1
        GameObject template = gamepreset.gameObject;
        GameObject targetBox = Object.Instantiate(template, gamepreset.transform);
        targetBox.name = "Mod Settings";
        targetBox.transform.localScale = new Vector3(0.59f, 0.59f, 1f);
        targetBox.transform.localPosition = new Vector3(targetBox.transform.localPosition.x + 2.95f, rolesettings.transform.localPosition.y - 0.1f, targetBox.transform.localPosition.z);

        var ModConfButton = targetBox.GetComponent<PassiveButton>();
        ModConfButton.OnClick.RemoveAllListeners();
        ModConfButton.OnClick.AddListener((Action)(() =>
            __instance.ChangeTab(3, false)
        )); 
        var label = ModConfButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        label.DestroyTranslator();
        label.text = "Mod Settings";
        ModSettingsButtons.Add(TabGroup.ModSettings, ModConfButton);

        //button 2
        GameObject template2 = targetBox.gameObject;
        GameObject targetBox2 = Object.Instantiate(template2, targetBox.transform);
        targetBox2.name = "Gamemode Settings";
        targetBox2.transform.localScale = new Vector3(1f, 1f, 1f);
        targetBox2.transform.localPosition = new Vector3(targetBox2.transform.localPosition.x, targetBox.transform.localPosition.y, targetBox2.transform.localPosition.z);

        var GamemodeButton = targetBox2.GetComponent<PassiveButton>();
        GamemodeButton.OnClick.RemoveAllListeners();
        GamemodeButton.OnClick.AddListener((Action)(() =>
            __instance.ChangeTab(4, false)
        )); 
        var label2 = GamemodeButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        label2.DestroyTranslator();
        label2.text = "Gamemode Settings"; 
        ModSettingsButtons.Add(TabGroup.GamemodeSettings, GamemodeButton);

        //button 3
        GameObject template3 = targetBox2.gameObject;
        GameObject targetBox3 = Object.Instantiate(template3, targetBox2.transform);
        targetBox3.name = "Additional Gamemodes";
        targetBox3.transform.localScale = new Vector3(1f, 1f, 1f);
        targetBox3.transform.localPosition = new Vector3(targetBox3.transform.localPosition.x, targetBox2.transform.localPosition.y, targetBox3.transform.localPosition.z);

        var AdditionalButton = targetBox3.GetComponent<PassiveButton>();
        AdditionalButton.OnClick.RemoveAllListeners();
        AdditionalButton.OnClick.AddListener((Action)(() => 
            __instance.ChangeTab(5, false)
        )); 
        var label3 = AdditionalButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        label3.DestroyTranslator();
        label3.text = "Additional Gamemodes";
        ModSettingsButtons.Add(TabGroup.AdditionalGamemodes, AdditionalButton);

        targetBox.transform.parent = null;
        gamepreset.gameObject.SetActive(false);
        targetBox.transform.parent = __instance.transform.Find("LeftPanel");

        ModGameOptionsMenu.OptionList = new();
        ModGameOptionsMenu.BehaviourList = new();
        ModGameOptionsMenu.CategoryHeaderList = new();

        ModSettingsTabs = new();
        foreach (var tab in Enum.GetValues<TabGroup>())
        {
            var setTab = Object.Instantiate(templateGameOptionsMenu, __instance.GameSettingsTab.transform.parent);
            setTab.name = Utils.GetTabName(tab);
            setTab.gameObject.SetActive(false);

            ModSettingsTabs.Add(tab, setTab);
        }

        foreach (var tab in Enum.GetValues<TabGroup>())
        {
            if (ModSettingsButtons.TryGetValue(tab, out var button))
            {
                __instance.ControllerSelectable.Add(button);
            }
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
    public static bool ChangeTabPrefix(GameSettingMenu __instance, ref int tabNum, [HarmonyArgument(1)] bool previewOnly)
    {
        ModGameOptionsMenu.TabIndex = tabNum;

        GameOptionsMenu settingsTab;
        PassiveButton button;

        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                if (ModSettingsTabs.TryGetValue(tab, out settingsTab) &&
                    settingsTab != null)
                {
                    settingsTab.gameObject.SetActive(false);
                }
            }
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                if (ModSettingsButtons.TryGetValue(tab, out button) &&
                    button != null)
                {
                    button.SelectButton(false);
                }
            }
        }

        if (tabNum < 3) return true;

        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            __instance.PresetsTab.gameObject.SetActive(false);
            __instance.GameSettingsTab.gameObject.SetActive(false);
            __instance.RoleSettingsTab.gameObject.SetActive(false);
            __instance.GamePresetsButton.SelectButton(false);
            __instance.GameSettingsButton.SelectButton(false);
            __instance.RoleSettingsButton.SelectButton(false);

            if (ModSettingsTabs.TryGetValue((TabGroup)(tabNum - 3), out settingsTab) && settingsTab != null)
            {
                settingsTab.gameObject.SetActive(true);
                __instance.MenuDescriptionText.DestroyTranslator();
                switch ((TabGroup)(tabNum - 3))
                {
                    case TabGroup.ModSettings:
                        __instance.MenuDescriptionText.text = "Edit basic settings of a mod";
                        break;
                    case TabGroup.GamemodeSettings:
                        __instance.MenuDescriptionText.text = "Edit settings of current gamemode";
                        break;
                    case TabGroup.AdditionalGamemodes:
                        __instance.MenuDescriptionText.text = "Add additional gamemodes to your game";
                        break;
                }
            }
        }
        if (previewOnly)
        {
            __instance.ToggleLeftSideDarkener(false);
            __instance.ToggleRightSideDarkener(true);
            return false;
        }
        __instance.ToggleLeftSideDarkener(true);
        __instance.ToggleRightSideDarkener(false);
        if (ModSettingsButtons.TryGetValue((TabGroup)(tabNum - 3), out button) && button != null)
        {
            button.SelectButton(true);
        }

        return false;
    }

    [HarmonyPatch(nameof(GameSettingMenu.OnEnable)), HarmonyPrefix]
    private static bool OnEnablePrefix(GameSettingMenu __instance)
    {
        if (templateGameOptionsMenu == null)
        {
            templateGameOptionsMenu = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
            templateGameOptionsMenu.gameObject.SetActive(false);
        }
        if (templateGameSettingsButton == null)
        {
            templateGameSettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
            templateGameSettingsButton.gameObject.SetActive(false);
        }

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
        DestroyableSingleton<HudManager>.Instance.menuNavigationPrompts.SetActive(false);
        if (Controller.currentTouchType != Controller.TouchType.Joystick)
        {
            __instance.ChangeTab(1, Controller.currentTouchType == Controller.TouchType.Joystick);
        }
        __instance.StartCoroutine(__instance.CoSelectDefault());

        return false;
    }
    [HarmonyPatch(nameof(GameSettingMenu.Close)), HarmonyPostfix]
    private static void ClosePostfix(GameSettingMenu __instance)
    {
        foreach (var button in ModSettingsButtons.Values)
            Object.Destroy(button);
        foreach (var tab in ModSettingsTabs.Values)
            Object.Destroy(tab);
        ModSettingsButtons = new();
        ModSettingsTabs = new();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch
{
    public static void Postfix()
    {
        OptionItem.SyncAllOptions();
    }
}