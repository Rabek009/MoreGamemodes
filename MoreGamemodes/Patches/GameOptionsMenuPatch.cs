using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using static UnityEngine.RemoteConfigSettingsHelper;
using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.InitializeOptions))]
    public static class GameSettingMenuPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    [HarmonyPriority(Priority.First)]
    public static class GameOptionsMenuPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            var template = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;

            var gameSettings = GameObject.Find("Game Settings");
            if (gameSettings == null) return;
            gameSettings.transform.FindChild("GameGroup").GetComponent<Scroller>().ScrollWheelSpeed = 1f;

            var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            if (gameSettingMenu == null) return;
            List<GameObject> menus = new() { gameSettingMenu.RegularGameSettings, gameSettingMenu.RolesSettings.gameObject };
            List<SpriteRenderer> highlights = new() { gameSettingMenu.GameSettingsHightlight, gameSettingMenu.RolesSettingsHightlight };

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");
            List<GameObject> tabs = new() { gameTab, roleTab };

            void AddOptionTab(TabGroup tab, Sprite sprite)
            {
                var obj = gameSettings.transform.parent.Find(tab + "Tab");
                if (obj != null)
                {
                    obj.transform.FindChild("../../GameGroup/Text").GetComponent<TMPro.TextMeshPro>().SetText(tab.ToString());
                    return;
                }

                var Settings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
                Settings.name = tab + "Tab";
                Settings.transform.FindChild("BackPanel").transform.localScale =
                Settings.transform.FindChild("Bottom Gradient").transform.localScale = new Vector3(1.2f, 1f, 1f);
                Settings.transform.FindChild("Background").transform.localScale = new Vector3(1.3f, 1f, 1f);
                Settings.transform.FindChild("UI_Scrollbar").transform.localPosition += new Vector3(0.35f, 0f, 0f);
                Settings.transform.FindChild("UI_ScrollbarTrack").transform.localPosition += new Vector3(0.35f, 0f, 0f);
                Settings.transform.FindChild("GameGroup/SliderInner").transform.localPosition += new Vector3(-0.15f, 0f, 0f);
                var Menu = Settings.transform.FindChild("GameGroup/SliderInner").GetComponent<GameOptionsMenu>();

                Menu.GetComponentsInChildren<OptionBehaviour>().Do(x => Object.Destroy(x.gameObject));

                var scOptions = new List<OptionBehaviour>();
                foreach (var option in OptionItem.AllOptions)
                {
                    if (option.Tab != tab) continue;
                    if (option.OptionBehaviour == null)
                    {
                        var stringOption = Object.Instantiate(template, Menu.transform);
                        scOptions.Add(stringOption);
                        stringOption.OnValueChanged = new System.Action<OptionBehaviour>((o) => { });
                        stringOption.TitleText.text = option.Name;
                        stringOption.Value = stringOption.oldValue = option.CurrentValue;
                        stringOption.ValueText.text = option.GetString();
                        stringOption.name = option.Name;
                        stringOption.transform.FindChild("Background").localScale = new Vector3(1.2f, 1f, 1f);
                        stringOption.transform.FindChild("Plus_TMP").localPosition += new Vector3(0.3f, 0f, 0f);
                        stringOption.transform.FindChild("Minus_TMP").localPosition += new Vector3(0.3f, 0f, 0f);
                        stringOption.transform.FindChild("Value_TMP").localPosition += new Vector3(0.3f, 0f, 0f);
                        stringOption.transform.FindChild("Title_TMP").localPosition += new Vector3(0.15f, 0f, 0f);
                        stringOption.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(3.5f, 0.37f);

                        option.OptionBehaviour = stringOption;
                    }
                    option.OptionBehaviour.gameObject.SetActive(true);
                }
                Menu.Children = scOptions.ToArray();
                Settings.gameObject.SetActive(false);
                menus.Add(Settings.gameObject);

                var Tab = Object.Instantiate(roleTab, roleTab.transform.parent);
                Tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = sprite;
                tabs.Add(Tab);
                var TabHighlight = Tab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
                highlights.Add(TabHighlight);
            }

            AddOptionTab(TabGroup.MainSettings, HudManager.Instance.UseButton.graphic.sprite);

            var tab = TabGroup.MainSettings;
            switch (Options.CurrentGamemode)
            {
                case Gamemodes.HideAndSeek:
                    tab = TabGroup.HideAndSeekSettings; break;
                case Gamemodes.ShiftAndSeek:
                    tab = TabGroup.ShiftAndSeekSettings; break;
                case Gamemodes.BombTag:
                    tab = TabGroup.BombTagSettings; break;
                case Gamemodes.RandomItems:
                    tab = TabGroup.RandomItemsSettings; break;
                case Gamemodes.BattleRoyale:
                    tab = TabGroup.BattleRoyaleSettings; break;
            }

            if (Options.CurrentGamemode != Gamemodes.Classic)
                AddOptionTab(tab, HudManager.Instance.KillButton.graphic.sprite);

            AddOptionTab(TabGroup.AdditionalGamemodes, HudManager.Instance.ImpostorVentButton.graphic.sprite);

            for (var i = 0; i < tabs.Count; ++i)
            {
                tabs[i].transform.position = new(0.8f * (i - 1) - tabs.Count / 2f, tabs[i].transform.position.y, tabs[i].transform.position.z);
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                if (button == null) continue;
                var copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                Action value = () =>
                {
                    for (var j = 0; j < menus.Count; ++j)
                    {
                        menus[j].SetActive(j == copiedIndex);
                        highlights[j].enabled = j == copiedIndex;
                    }
                };
                button.OnClick.AddListener(value);
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public class GameOptionsMenuUpdatePatch
    {
        private static float _timer = 1f;

        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.transform.parent.parent.name == "Game Settings") return;
            List<TabGroup> tabs = new List<TabGroup>();
            tabs.Add(TabGroup.MainSettings);
            switch (Options.CurrentGamemode)
            {
                case Gamemodes.HideAndSeek:
                    tabs.Add(TabGroup.HideAndSeekSettings); break;
                case Gamemodes.ShiftAndSeek:
                    tabs.Add(TabGroup.ShiftAndSeekSettings); break;
                case Gamemodes.BombTag:
                    tabs.Add(TabGroup.BombTagSettings); break;
                case Gamemodes.RandomItems:
                    tabs.Add(TabGroup.RandomItemsSettings); break;
                case Gamemodes.BattleRoyale:
                    tabs.Add(TabGroup.BattleRoyaleSettings); break;
            }
            tabs.Add(TabGroup.AdditionalGamemodes);
            foreach (var tab in tabs)
            {
                if (__instance.transform.parent.parent.name != tab + "Tab") continue;
                __instance.transform.FindChild("../../GameGroup/Text").GetComponent<TMPro.TextMeshPro>().SetText(tab.ToString());

                _timer += Time.deltaTime;
                if (_timer < 0.1f) return;
                _timer = 0f;

                float numItems = __instance.Children.Length;
                var offset = 2.7f;

                foreach (var option in OptionItem.AllOptions)
                {
                    if ((TabGroup)tab != option.Tab) continue;
                    if (option?.OptionBehaviour == null || option.OptionBehaviour.gameObject == null) continue;

                    var enabled = true;
                    var parent = option.Parent;

                    enabled = AmongUsClient.Instance.AmHost;

                    var opt = option.OptionBehaviour.transform.Find("Background").GetComponent<SpriteRenderer>();
                    opt.size = new(5.0f, 0.45f);
                    while (parent != null && enabled)
                    {
                        enabled = parent.GetBool();
                        parent = parent.Parent;
                        opt.color = new(0f, 1f, 0f);
                        opt.size = new(4.8f, 0.45f);
                        opt.transform.localPosition = new Vector3(0.11f, 0f);
                        option.OptionBehaviour.transform.Find("Title_TMP").transform.localPosition = new Vector3(-0.95f, 0f);
                        option.OptionBehaviour.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(3.4f, 0.37f);
                        if (option.Parent?.Parent != null)
                        {
                            opt.color = new(0f, 0f, 1f);
                            opt.size = new(4.6f, 0.45f);
                            opt.transform.localPosition = new Vector3(0.24f, 0f);
                            option.OptionBehaviour.transform.Find("Title_TMP").transform.localPosition = new Vector3(-0.7f, 0f);
                            option.OptionBehaviour.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(3.3f, 0.37f);
                            if (option.Parent?.Parent?.Parent != null)
                            {
                                opt.color = new(1f, 0f, 0f);
                                opt.size = new(4.4f, 0.45f);
                                opt.transform.localPosition = new Vector3(0.37f, 0f);
                                option.OptionBehaviour.transform.Find("Title_TMP").transform.localPosition = new Vector3(-0.55f, 0f);
                                option.OptionBehaviour.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(3.2f, 0.37f);
                            }
                        }
                    }

                    option.OptionBehaviour.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        offset -= option.IsHeader ? 0.7f : 0.5f;
                        option.OptionBehaviour.transform.localPosition = new Vector3(
                            option.OptionBehaviour.transform.localPosition.x,
                            offset,
                            option.OptionBehaviour.transform.localPosition.z);

                        if (option.IsHeader)
                        {
                            numItems += 0.5f;
                        }
                    }
                    else
                    {
                        numItems--;
                    }
                }
                __instance.GetComponentInParent<Scroller>().ContentYBounds.max = (-offset) - 1.5f;
            }
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.GetName();
            __instance.Value = __instance.oldValue = option.CurrentValue;
            __instance.ValueText.text = option.GetString();

            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
            if (option == null) return true;

            option.SetValue(option.CurrentValue + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
            if (option == null) return true;

            option.SetValue(option.CurrentValue - 1);
            return false;
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