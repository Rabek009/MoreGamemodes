using System;
using TMPro;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(LobbyViewSettingsPane))]
    public class LobbyViewPatch
    {
        private static Dictionary<TabGroup, GameObject> ViewSettingsButtons = new();
        private static float num;
        
        [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(LobbyViewSettingsPane __instance)
        {
            __instance.gameModeText.DestroyTranslator();
            __instance.gameModeText.text = Options.Gamemode.GetString();
            
            var OverviewButton = __instance.taskTabButton.gameObject;
            OverviewButton.transform.localScale = new Vector3(Options.CurrentGamemode == Gamemodes.Classic ? 0.4f : 0.5f, 1f, 1f);
            OverviewButton.transform.localPosition += new Vector3(-1.1f, 0f, 0f);

            var RolesButton = __instance.rolesTabButton.gameObject;
            RolesButton.transform.localScale = new Vector3(Options.CurrentGamemode == Gamemodes.Classic ? 0.4f : 0.5f, 1f, 1f);
            num = Options.CurrentGamemode == Gamemodes.Classic ? 1.4f : 1.75f;
            RolesButton.transform.localPosition = OverviewButton.transform.localPosition + Vector3.right * num * 1f;
            var label = RolesButton.transform.Find("FontPlacer").GetComponentInChildren<TextMeshPro>();
            label.DestroyTranslator();
            label.text = "Vanilla Roles";

            ViewSettingsButtons = new();
            num = Options.CurrentGamemode == Gamemodes.Classic ? 2.8f : 3.5f;
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                if (tab == TabGroup.GamemodeSettings && Options.CurrentGamemode == Gamemodes.Classic) continue;
                if ((tab is TabGroup.CrewmateRoles or TabGroup.ImpostorRoles or TabGroup.NeutralRoles or TabGroup.AddOns) && Options.CurrentGamemode != Gamemodes.Classic) continue;
                var button = Object.Instantiate(OverviewButton, OverviewButton.transform.parent);
                button.transform.localPosition += Vector3.right * num * 1f;
                button.name = Utils.GetTabName(tab);
                var label2 = button.transform.Find("FontPlacer").GetComponentInChildren<TextMeshPro>();
                __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => {
                    label2.text = Utils.GetTabName(tab);
                })));
                var passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab((StringNames)tab);
                }));
                ViewSettingsButtons.Add(tab, button);
                num += Options.CurrentGamemode == Gamemodes.Classic ? 1.4f : 1.75f;
            }
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.ChangeTab))]
        [HarmonyPrefix]
        public static bool ChangeTabPrefix(LobbyViewSettingsPane __instance, [HarmonyArgument(0)] StringNames category)
        {
            if (category == StringNames.OverviewCategory || category == StringNames.RolesCategory)
            {
                foreach (var button in ViewSettingsButtons.Values)
                    button.GetComponent<PassiveButton>().SelectButton(false);
                return true;
            }
            __instance.currentTab = category;
		    for (int i = 0; i < __instance.settingsInfo.Count; i++)
		    {
			    Object.Destroy(__instance.settingsInfo[i].gameObject);
		    }
		    __instance.settingsInfo.Clear();
            __instance.taskTabButton.SelectButton(false);
		    __instance.rolesTabButton.SelectButton(false);
            foreach (var tab in ViewSettingsButtons.Keys)
            {
                if (tab == (TabGroup)__instance.currentTab)
                    ViewSettingsButtons[tab].GetComponent<PassiveButton>().SelectButton(true);
                else
                    ViewSettingsButtons[tab].GetComponent<PassiveButton>().SelectButton(false);
            }
            DrawOptions(__instance, (TabGroup)__instance.currentTab);
            __instance.scrollBar.ScrollToTop();
            return false;
        }

        public static void DrawOptions(LobbyViewSettingsPane __instance, TabGroup tab)
        {
            float num = 1.44f;
            int settingsCount = 0;
            foreach (var option in OptionItem.AllOptions)
            {
                if (option.IsHiddenOn(Options.CurrentGamemode) || option.Tab != tab || (option.Parent != null && (option.Parent.IsHiddenOn(Options.CurrentGamemode) || !option.Parent.GetBool()))) continue;
                if (option.IsHeader || option is TextOptionItem)
                {
                    num -= 0.85f;
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin);
			        categoryHeaderMasked.SetHeader(StringNames.None, 61);
			        categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
			        categoryHeaderMasked.transform.localScale = Vector3.one;
			        categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
			        __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
			        num -= 1f;
                    categoryHeaderMasked.DestroyTranslator();
                    categoryHeaderMasked.Title.text = option.GetName();
                    settingsCount = 0;
                }
                if (option is RoleOptionItem || option is AddOnOptionItem)
                {
                    if (settingsCount % 2 == 1)
                        ++settingsCount;
                }
                if (option is TextOptionItem) continue;

                ViewSettingsInfoPanel viewSettingsInfoPanel = Object.Instantiate(__instance.infoPanelOrigin);
				viewSettingsInfoPanel.transform.SetParent(__instance.settingsContainer);
				viewSettingsInfoPanel.transform.localScale = Vector3.one;
				float num2;
				if (settingsCount % 2 == 0)
				{
					num2 = -8.95f;
					if (settingsCount > 0)
					{
						num -= 0.85f;
					}
				}
				else
				{
					num2 = -3f;
				}
				viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);

                if (option is BooleanOptionItem)
                {
                    viewSettingsInfoPanel.SetInfoCheckbox(StringNames.None, 61, option.GetBool());
                }
                else
                {
                    viewSettingsInfoPanel.SetInfo(StringNames.None, option.GetString(), 61);
                }
                viewSettingsInfoPanel.titleText.text = option.GetName();

                if (option.Parent?.Parent?.Parent != null)
                {
                    viewSettingsInfoPanel.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = new(0.7f, 0.5f, 0.5f);
                }
                else if (option.Parent?.Parent != null)
                {
                    viewSettingsInfoPanel.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = new(0.5f, 0.5f, 0.7f);
                }
                else if (option.Parent != null)
                {
                    viewSettingsInfoPanel.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = new(0.5f, 0.7f, 0.5f);
                }

                __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
                ++settingsCount;
            }
            __instance.scrollBar.SetYBoundsMax(-num - 1.44f);
        }

        public static void ReCreateButtons(LobbyViewSettingsPane __instance)
        {
            __instance.gameModeText.text = Options.Gamemode.GetString();
            foreach (var button in ViewSettingsButtons.Values)
                Object.Destroy(button.gameObject);
            ViewSettingsButtons = new();
            var OverviewButton = __instance.taskTabButton.gameObject;
            var RolesButton = __instance.rolesTabButton.gameObject;
            OverviewButton.transform.localScale = new Vector3(Options.CurrentGamemode == Gamemodes.Classic ? 0.4f : 0.5f, 1f, 1f);
            RolesButton.transform.localScale = new Vector3(Options.CurrentGamemode == Gamemodes.Classic ? 0.4f : 0.5f, 1f, 1f);
            num = Options.CurrentGamemode == Gamemodes.Classic ? 1.4f : 1.75f;
            RolesButton.transform.localPosition = OverviewButton.transform.localPosition + Vector3.right * num * 1f;
            num = Options.CurrentGamemode == Gamemodes.Classic ? 2.8f : 3.5f;
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                if (tab == TabGroup.GamemodeSettings && Options.CurrentGamemode == Gamemodes.Classic) continue;
                if ((tab is TabGroup.CrewmateRoles or TabGroup.ImpostorRoles or TabGroup.NeutralRoles or TabGroup.AddOns) && Options.CurrentGamemode != Gamemodes.Classic) continue;
                var button = Object.Instantiate(__instance.taskTabButton.gameObject, __instance.taskTabButton.gameObject.transform.parent);
                button.transform.localPosition += Vector3.right * num * 1f;
                button.name = Utils.GetTabName(tab);
                var label2 = button.transform.Find("FontPlacer").GetComponentInChildren<TextMeshPro>();
                __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => {
                    label2.text = Utils.GetTabName(tab);
                })));
                var passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                passiveButton.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab((StringNames)tab);
                }));
                if (tab == (TabGroup)__instance.currentTab)
                    passiveButton.SelectButton(true);
                else
                    passiveButton.SelectButton(false);
                ViewSettingsButtons.Add(tab, button);
                num += Options.CurrentGamemode == Gamemodes.Classic ? 1.4f : 1.75f;
            }
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnablePostfix(LobbyViewSettingsPane __instance)
        {
            ReCreateButtons(__instance);
        }
    }
}
