using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using TMPro;
using AmongUs.GameOptions;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    public static class GameSettingMenuInitializeOptionsPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            __instance.GameSettingsTab.HideForOnline = new Il2CppReferenceArray<Transform>(0);
        }
        public static void Postfix(GameSettingMenu __instance)
        {
            var gamepreset = __instance.GamePresetsButton;

            var gamesettings = __instance.GameSettingsButton;
            __instance.GameSettingsButton.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            __instance.GameSettingsButton.transform.localPosition = new Vector3(gamesettings.transform.localPosition.x, gamepreset.transform.localPosition.y + 0.2f, gamesettings.transform.localPosition.z);

            var rolesettings = __instance.RoleSettingsButton;
            __instance.RoleSettingsButton.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            __instance.RoleSettingsButton.transform.localPosition = new Vector3(rolesettings.transform.localPosition.x, gamesettings.transform.localPosition.y - 0.4f, rolesettings.transform.localPosition.z);

            GameObject template = gamepreset.gameObject;
            GameObject targetBox = Object.Instantiate(template, gamepreset.transform);
            targetBox.name = "Mod Settings";
            targetBox.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            targetBox.transform.localPosition = new Vector3(targetBox.transform.localPosition.x + 2.95f, rolesettings.transform.localPosition.y - 0.1f, targetBox.transform.localPosition.z);

            _ = new LateTask(() =>
            {
                targetBox.transform.parent = null;
                gamepreset.gameObject.SetActive(false);
                targetBox.transform.parent = __instance.transform.Find("LeftPanel");
            }, 0.05f, "Remove GamePreset // Set Button 1");

            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            var label = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
            _ = new LateTask(() => label.text = "Mod Settings", 0.05f, "Set Button1 Text"); 


            GameObject template2 = targetBox.gameObject;
            GameObject targetBox2 = Object.Instantiate(template2, targetBox.transform);
            targetBox2.name = "Gamemode Settings";
            targetBox2.transform.localScale = new Vector3(1f, 1f, 1f);
            targetBox2.transform.localPosition = new Vector3(targetBox2.transform.localPosition.x, targetBox.transform.localPosition.y - 0.1f, targetBox2.transform.localPosition.z);

            PassiveButton button2 = targetBox2.GetComponent<PassiveButton>();
            button2.OnClick.RemoveAllListeners();
            var label2 = button2.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>(); 
            _ = new LateTask(() => label2.text = "Gamemode Settings", 0.05f, "Set Button2 Text"); 

            GameObject template3 = targetBox2.gameObject;
            GameObject targetBox3 = Object.Instantiate(template3, targetBox2.transform);
            targetBox3.name = "Additional Gamemodes";
            targetBox3.transform.localScale = new Vector3(1f, 1f, 1f);
            targetBox3.transform.localPosition = new Vector3(targetBox3.transform.localPosition.x, targetBox2.transform.localPosition.y, targetBox3.transform.localPosition.z);

            PassiveButton button3 = targetBox3.GetComponent<PassiveButton>();
            button3.OnClick.RemoveAllListeners();
            var label3 = button3.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>(); 
            _ = new LateTask(() => label3.text = "Additional Gamemodess", 0.05f, "Set Button3 Text");
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
    public class TabChangePatch
    {
        public static void Prefix(ref int tabNum)
        {
            if (tabNum == 0)
                tabNum = 1; 
        }
        public static void Postfix(GameSettingMenu __instance, [HarmonyArgument(0)] int tabNum)
        {
            if (tabNum == 1 && __instance.GameSettingsTab.isActiveAndEnabled)
            {
                _ = new LateTask(() => __instance.MenuDescriptionText.text = TranslationController.Instance.GetString(StringNames.GameSettingsDescription), 0.05f, "Fix Menu Description Text");
                return;
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Awake))]
    public static class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            StringOption template = Object.FindObjectOfType<StringOption>();
            GameObject gameSettings = GameObject.Find("Game Settings");
            GameSettingMenu gameSettingMenu = Object.FindObjectOfType<GameSettingMenu>();

            if (GameOptionsManager.Instance.currentGameMode == GameModes.Normal || GameOptionsManager.Instance.currentGameMode == GameModes.NormalFools)
            {
                template = Object.FindObjectOfType<StringOption>();
                if (template == null) return;

                gameSettings = GameObject.Find("Game Settings");
                if (gameSettings == null) return;

                gameSettingMenu = Object.FindObjectOfType<GameSettingMenu>();
                if (gameSettingMenu == null) return;

                GameObject.Find("Tint")?.SetActive(false);
            }
            else if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || GameOptionsManager.Instance.currentGameMode == GameModes.SeekFools)
            {
                Options.Gamemode.SetValue(0);

                gameSettingMenu = Object.FindObjectOfType<GameSettingMenu>();
                if (gameSettingMenu == null) return;

                gameSettingMenu.GameSettingsTab.gameObject.SetActive(true);
                gameSettingMenu.RoleSettingsTab.gameObject.SetActive(true);

                GameObject.Find("Tint")?.SetActive(false);

                template = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
                if (template == null) return;

                gameSettings = GameObject.Find("Game Settings");
                if (gameSettings == null) return;

                gameSettingMenu.GameSettingsTab.gameObject.SetActive(false);
                gameSettingMenu.RoleSettingsTab.gameObject.SetActive(false);
            }

            gameSettings.transform.Find("GameGroup").GetComponent<Scroller>().ScrollWheelSpeed = 1.2f;

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            List<GameObject> menus = new();
            menus.Add(gameSettingMenu.GameSettingsTab.gameObject); 
            menus.Add(gameSettingMenu.RoleSettingsTab.gameObject);
            List<GameObject> tabs = new();
            tabs.Add(gameTab);
            tabs.Add(roleTab);

            if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || GameOptionsManager.Instance.currentGameMode == GameModes.SeekFools)
            {
                menus.Remove(gameSettingMenu.RoleSettingsTab.gameObject);
                tabs.Remove(roleTab);
            }

            float delay = 0f;

            foreach (var tab in Enum.GetValues<TabGroup>().Where(tab => GameOptionsManager.Instance.currentGameMode == GameModes.Normal || GameOptionsManager.Instance.currentGameMode == GameModes.NormalFools || ((GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || GameOptionsManager.Instance.currentGameMode == GameModes.SeekFools) && (tab is TabGroup.ModSettings or TabGroup.AdditionalGamemodes))).ToArray())
            {
                var obj = gameSettings.transform.parent.Find(tab + "Tab");
                if (obj != null)
                {
                    obj.transform.Find("../../GameGroup/Text").GetComponent<TextMeshPro>().SetText(Utils.GetTabName(tab));
                    continue;
                }

                var Settings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
                Settings.name = tab + "Tab";

                var SettingsTransform = Settings.transform;
                var backPanel = SettingsTransform.Find("BackPanel");

                backPanel.transform.localScale =
                SettingsTransform.Find("Bottom Gradient").transform.localScale = new Vector3(1.6f, 1f, 1f);
                SettingsTransform.Find("Bottom Gradient").transform.localPosition += new Vector3(0.2f, 0f, 0f);
                SettingsTransform.Find("BackPanel").transform.localPosition += new Vector3(0.2f, 0f, 0f);
                SettingsTransform.Find("Background").transform.localScale = new Vector3(1.8f, 1f, 1f);
                SettingsTransform.Find("UI_Scrollbar").transform.localPosition += new Vector3(1.4f, 0f, 0f);
                SettingsTransform.Find("UI_ScrollbarTrack").transform.localPosition += new Vector3(1.4f, 0f, 0f);
                SettingsTransform.Find("GameGroup/SliderInner").transform.localPosition += new Vector3(-0.3f, 0f, 0f);

                var Menu = SettingsTransform.Find("GameGroup/SliderInner").GetComponent<GameOptionsMenu>();
                List<OptionBehaviour> scOptions = new();

                new LateTask(() =>
                {
                    Menu.GetComponentsInChildren<OptionBehaviour>().Do(x => Object.Destroy(x.gameObject));

                    foreach (var option in OptionItem.AllOptions.Where(opt => opt.Tab == tab).ToArray())
                    {

                        if (option.OptionBehaviour == null)
                        {
                            var stringOption = Object.Instantiate(template, Menu.transform);
                            scOptions.Add(stringOption);
                            stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                            stringOption.TitleText.text = option.Name;
                            stringOption.Value = stringOption.oldValue = option.CurrentValue;
                            stringOption.ValueText.text = option.GetString();
                            stringOption.name = option.Name;

                            var stringOptionTransform = stringOption.transform;
                            stringOptionTransform.Find("Background").localScale = new Vector3(1.6f, 1f, 1f);
                            stringOptionTransform.Find("Plus_TMP").localPosition += new Vector3(1.4f, 0f, 0f);
                            stringOptionTransform.Find("Minus_TMP").localPosition += new Vector3(1.0f, 0f, 0f);
                            stringOptionTransform.Find("Value_TMP").localPosition += new Vector3(1.2f, 0f, 0f);
                            stringOptionTransform.Find("Value_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(1.6f, 0.26f);
                            stringOptionTransform.Find("Title_TMP").localPosition += new Vector3(0.1f, 0f, 0f);
                            stringOptionTransform.Find("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(5.5f, 0.37f);

                            option.OptionBehaviour = stringOption;
                        }
                        option.OptionBehaviour.gameObject.SetActive(true);
                    }
                }, delay, "Settings");

                delay += 0.1f;
                
                Settings.gameObject.SetActive(false);
                menus.Add(Settings.gameObject);

                var Tab = Object.Instantiate(roleTab, roleTab.transform.parent);
                var hatButton = Tab.transform.Find("Hat Button");

                hatButton.Find("Icon").GetComponent<SpriteRenderer>().sprite = Utils.GetTabSprite(tab);
                tabs.Add(Tab);
                var TabHighlight = hatButton.Find("Tab Background").GetComponent<SpriteRenderer>();
            }

            if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || GameOptionsManager.Instance.currentGameMode == GameModes.SeekFools)
            {
                roleTab.active = false;
            }

            var tabsCount = tabs.Count;
            var menusCount = menus.Count;
            var tabsCountDividedBy323 = tabsCount / 3.23f;

            for (var i = 0; i < tabsCount; i++)
            {
                var tab = tabs[i];
                var transform = tab.transform;

                var xValue = 0.65f * (i - 1) - tabsCountDividedBy323;
                transform.localPosition = new(xValue, transform.localPosition.y, transform.localPosition.z);

                var button = tab.GetComponentInChildren<PassiveButton>();
                if (button != null)
                {
                    var copiedIndex = i;
                    button.OnClick ??= new UnityEngine.UI.Button.ButtonClickedEvent();
                    void value()
                    {
                        if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || GameOptionsManager.Instance.currentGameMode == GameModes.SeekFools)
                        {
                            gameSettingMenu.RoleSettingsTab.gameObject.SetActive(false);
                        }
                        for (var j = 0; j < menusCount; j++)
                        {
                            if ((GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || GameOptionsManager.Instance.currentGameMode == GameModes.SeekFools) && j == 0 && copiedIndex == 0) continue;
                            menus[j].SetActive(j == copiedIndex);
                        }
                    }
                    button.OnClick.AddListener((Action)value);
                }
            }
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
}