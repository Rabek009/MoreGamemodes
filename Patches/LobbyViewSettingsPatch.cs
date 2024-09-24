using System;
using TMPro;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(LobbyViewSettingsPane))]
    public class LobbyViewPatch
    {
        private static GameObject ModSettingsButton = null;
        private static GameObject GamemodeSettingsButton = null;
        private static GameObject AdditionalGamemodesButton = null;
        
        [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(LobbyViewSettingsPane __instance)
        {
            if (GameManager.Instance.IsHideAndSeek()) return;
            __instance.gameModeText.DestroyTranslator();
            __instance.gameModeText.text = Options.Gamemode.GetString();
            
            var OverviewButton = __instance.taskTabButton.gameObject;
            OverviewButton.transform.localScale = new Vector3(0.5f * OverviewButton.transform.localScale.x, OverviewButton.transform.localScale.y, OverviewButton.transform.localScale.z);
            OverviewButton.transform.localPosition += new Vector3(-1.1f, 0f, 0f);

            var RolesButton = __instance.rolesTabButton.gameObject;
            RolesButton.transform.localScale = new Vector3(0.5f * RolesButton.transform.localScale.x, RolesButton.transform.localScale.y, RolesButton.transform.localScale.z);
            RolesButton.transform.localPosition = OverviewButton.transform.localPosition + Vector3.right * 1.75f * 1f;

            ModSettingsButton = GameObject.Find("ModTab");
            if (ModSettingsButton == null)
            {
                ModSettingsButton = Object.Instantiate(OverviewButton, OverviewButton.transform.parent);
                ModSettingsButton.transform.localPosition += Vector3.right * 3.5f * 1f;
                ModSettingsButton.name = "ModTab";
                var fontPlacer2 = ModSettingsButton.transform.Find("FontPlacer");
                if (fontPlacer2 != null)
                {
                    var textMeshPro = fontPlacer2.GetComponentInChildren<TextMeshPro>();
                    if (textMeshPro != null)
                    {
                        __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => {
                            textMeshPro.text = "Mod Settings";
                        })));
                    }
                }
                var passiveButton2 = ModSettingsButton.GetComponent<PassiveButton>();
                passiveButton2.OnClick.RemoveAllListeners();
                passiveButton2.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab(0);
                }));
            }

            GamemodeSettingsButton = GameObject.Find("GamemodeTab");
            if (GamemodeSettingsButton == null)
            {
                GamemodeSettingsButton = Object.Instantiate(OverviewButton, OverviewButton.transform.parent);
                GamemodeSettingsButton.transform.localPosition += Vector3.right * 5.25f * 1f;
                GamemodeSettingsButton.name = "GamemodeTab";
                var fontPlacer2 = GamemodeSettingsButton.transform.Find("FontPlacer");
                if (fontPlacer2 != null)
                {
                    var textMeshPro = fontPlacer2.GetComponentInChildren<TextMeshPro>();
                    if (textMeshPro != null)
                    {
                        __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => {
                            textMeshPro.text = "Gamemode Settings";
                        })));
                    }
                }
                var passiveButton2 = GamemodeSettingsButton.GetComponent<PassiveButton>();
                passiveButton2.OnClick.RemoveAllListeners();
                passiveButton2.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab((StringNames)1);
                }));
            }

            AdditionalGamemodesButton = GameObject.Find("AdditionalTab");
            if (AdditionalGamemodesButton == null)
            {
                AdditionalGamemodesButton = Object.Instantiate(OverviewButton, OverviewButton.transform.parent);
                AdditionalGamemodesButton.transform.localPosition += Vector3.right * 7f * 1f;
                AdditionalGamemodesButton.name = "AdditionalTab";
                var fontPlacer2 = AdditionalGamemodesButton.transform.Find("FontPlacer");
                if (fontPlacer2 != null)
                {
                    var textMeshPro = fontPlacer2.GetComponentInChildren<TextMeshPro>();
                    if (textMeshPro != null)
                    {
                        __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => {
                            textMeshPro.text = "Additional Gamemodes";
                        })));
                    }
                }
                var passiveButton2 = AdditionalGamemodesButton.GetComponent<PassiveButton>();
                passiveButton2.OnClick.RemoveAllListeners();
                passiveButton2.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab((StringNames)2);
                }));
            }
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.SetTab))]
        [HarmonyPrefix]
        public static bool SetTabPatch(LobbyViewSettingsPane __instance)
        {
            if (ModSettingsButton == null || GamemodeSettingsButton == null || AdditionalGamemodesButton == null) return true;
            var passiveButton = ModSettingsButton.GetComponent<PassiveButton>();
            var passiveButton2 = GamemodeSettingsButton.GetComponent<PassiveButton>();
            var passiveButton3 = AdditionalGamemodesButton.GetComponent<PassiveButton>();
            if (__instance.currentTab == StringNames.OverviewCategory || __instance.currentTab == StringNames.RolesCategory)
            {
                passiveButton.SelectButton(false);
                passiveButton2.SelectButton(false);
                passiveButton3.SelectButton(false);
                return true;
            }
            if (__instance.currentTab == 0)
            {
                passiveButton.SelectButton(true);
                __instance.taskTabButton.SelectButton(false);
		        __instance.rolesTabButton.SelectButton(false);
                passiveButton2.SelectButton(false);
                passiveButton3.SelectButton(false);
                DrawOptions(__instance, TabGroup.ModSettings);
            }
            else if (__instance.currentTab == (StringNames)1)
            {
                passiveButton2.SelectButton(true);
                __instance.taskTabButton.SelectButton(false);
		        __instance.rolesTabButton.SelectButton(false);
                passiveButton.SelectButton(false);
                passiveButton3.SelectButton(false);
                DrawOptions(__instance, TabGroup.GamemodeSettings);
            }
            else if (__instance.currentTab == (StringNames)2)
            {
                passiveButton3.SelectButton(true);
                __instance.taskTabButton.SelectButton(false);
		        __instance.rolesTabButton.SelectButton(false);
                passiveButton.SelectButton(false);
                passiveButton2.SelectButton(false);
                DrawOptions(__instance, TabGroup.AdditionalGamemodes);
            }
            return false;
        }

        public static void DrawOptions(LobbyViewSettingsPane __instance, TabGroup tab)
        {
            float num = 2.03f;
            int settingsCount = 0;
            foreach (var option in OptionItem.AllOptions)
            {
                if (option.IsHiddenOn(Options.CurrentGamemode) || option.Tab != tab || (option.Parent != null && (option.Parent.IsHiddenOn(Options.CurrentGamemode) || !option.Parent.GetBool()))) continue;
                if (option.IsHeader || option is TextOptionItem)
                {
                    num -= 0.59f;
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin);
			        categoryHeaderMasked.SetHeader(StringNames.None, 61);
			        categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
			        categoryHeaderMasked.transform.localScale = Vector3.one;
			        categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
			        __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
			        num -= 0.85f;
                    categoryHeaderMasked.DestroyTranslator();
                    categoryHeaderMasked.Title.text = option.GetName();
                    settingsCount = 0;
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
						num -= 0.59f;
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
                else if (option is IntegerOptionItem)
                {
                    viewSettingsInfoPanel.SetInfo(StringNames.None, option.ApplyFormat(option.GetInt().ToString()), 61);
                }
                else if (option is FloatOptionItem)
                {
                    viewSettingsInfoPanel.SetInfo(StringNames.None, option.ApplyFormat(option.GetFloat().ToString()), 61);
                }
                else if (option is StringOptionItem || option is PresetOptionItem)
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
            __instance.scrollBar.SetYBoundsMax(-num);
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnablePostfix(LobbyViewSettingsPane __instance)
        {
            if (GameManager.Instance.IsHideAndSeek()) return;
            __instance.gameModeText.text = Options.Gamemode.GetString();
        }
    }
}
