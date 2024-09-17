using System;
using TMPro;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(LobbyViewSettingsPane))]
    public static class LobbyViewPatch
    {
        [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(LobbyViewSettingsPane __instance)
        {
            RemoveRoleTab();
            string buttonName = "MGButton";
            var OverviewButton = GameObject.Find("OverviewTab"); 
            OverviewButton.transform.localScale = new Vector3(0.5f * OverviewButton.transform.localScale.x, OverviewButton.transform.localScale.y, OverviewButton.transform.localScale.z);
            OverviewButton.transform.localPosition += new Vector3(-1.1f, 0f, 0f);
            OverviewButton.transform.Find("FontPlacer").transform.localScale = new Vector3(1.35f, 1f, 1f);
            OverviewButton.transform.Find("FontPlacer").transform.localPosition = new Vector3(-0.6f, -0.1f, 0f);
            var MGButton = GameObject.Find(buttonName);
            if (MGButton == null)
            {
                MGButton = GameObject.Instantiate(OverviewButton, OverviewButton.transform.parent);
                MGButton.transform.localPosition += Vector3.right * 1.75f * 1f;
                MGButton.name = buttonName;
                var fontPlacer = MGButton.transform.Find("FontPlacer");
                if (fontPlacer != null)
                {
                    var textMeshPro = fontPlacer.GetComponentInChildren<TextMeshPro>();
                    if (textMeshPro != null)
                    {
                        __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => {
                            textMeshPro.text = "View MG Settings";
                        })));
                    }
                }
                var passiveButton = MGButton.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab((StringNames)ModGameOptionsMenu.TabIndex);
                    __instance.scrollBar.ScrollToTop();
                    Main.Instance.Log.LogMessage("MG Tab View");
                }));
            }
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.ChangeTab))]
        [HarmonyPostfix]
        public static void ChangeTabPatch(LobbyViewSettingsPane __instance, StringNames category)
        {
            if (__instance.currentTab == (StringNames)ModGameOptionsMenu.TabIndex)
            {
                DrawOptions(__instance);
               
            }
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.SetTab))]
        [HarmonyPostfix]
        public static void SetTabPatch(LobbyViewSettingsPane __instance)
        {   
            var MGButtonPassive = GameObject.Find("MGButton").GetComponent<PassiveButton>();
    
            if (__instance.currentTab == (StringNames)ModGameOptionsMenu.TabIndex)
            {   
                __instance.rolesTabButton.SelectButton(false);
                __instance.taskTabButton.SelectButton(false);
                MGButtonPassive.SelectButton(true);

                DrawOptions(__instance);         
            }
            else
            {
               MGButtonPassive.SelectButton(false);
            }
        }

        public static void RemoveRoleTab()
        {
            var roleTab = GameObject.Find("RolesTabs");
            Object.Destroy(roleTab.gameObject);
        }

        public static void DrawOptions(LobbyViewSettingsPane instance)
        {
            
            foreach (var info in instance.settingsInfo)
            {
                Object.Destroy(info.gameObject);
            }
            instance.settingsInfo.Clear();

            float num = 1.44f;
            float num2;

            instance.gameModeText.text = "MG Settings";

            foreach (var option in OptionItem.AllOptions)
            {
                if (option.IsHiddenOn(Options.CurrentGamemode)) continue;

                if (option is TextOptionItem) continue;

                var basegameSetting = GameOptionsMenuPatch.GetSetting(option);

                if (option.IsHeader)
                {
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(instance.categoryHeaderOrigin);
                    categoryHeaderMasked.SetHeader((StringNames)ModGameOptionsMenu.TabIndex, 20);
                    categoryHeaderMasked.transform.SetParent(instance.settingsContainer);
                    categoryHeaderMasked.transform.localScale = Vector3.one;
                    categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
                    categoryHeaderMasked.Title.text = option.GetName();
                    categoryHeaderMasked.Title.outlineWidth = 0.2f;
                    instance.settingsInfo.Add(categoryHeaderMasked.gameObject);  
                    num -= 0.85f;
                    continue;
                }

                num2 = instance.settingsInfo.Count % 2 == 0 ? -8.95f : -3f;

                ViewSettingsInfoPanel viewSettingsInfoPanel = Object.Instantiate(instance.infoPanelOrigin);
                viewSettingsInfoPanel.transform.SetParent(instance.settingsContainer);
                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);

                if (basegameSetting != null)
                {
                    switch (basegameSetting.Type)
                    {
                        case OptionTypes.Checkbox:
                            viewSettingsInfoPanel.SetInfoCheckbox(basegameSetting.Title, 61, option.GetBool());
                            break;

                        case OptionTypes.Int:
                            viewSettingsInfoPanel.SetInfo(basegameSetting.Title, option.GetInt().ToString(), 61);
                            break;

                        case OptionTypes.Float:
                            viewSettingsInfoPanel.SetInfo(basegameSetting.Title, option.GetFloat().ToString(), 61);
                            break;

                        case OptionTypes.String:
                            viewSettingsInfoPanel.SetInfo(basegameSetting.Title, option.GetString(), 61);
                            break;

                        default:
                            viewSettingsInfoPanel.SetInfo(basegameSetting.Title, option.GetValue().ToString(), 61);
                            break;
                    }
                }
                viewSettingsInfoPanel.titleText.text = option.GetName();
                instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

                if (instance.settingsInfo.Count % 2 == 0) num -= 0.59f;
            }
            instance.scrollBar.CalculateAndSetYBounds(instance.settingsInfo.Count + 10, 2f, 6f, 0.59f);
        }
    }
}
