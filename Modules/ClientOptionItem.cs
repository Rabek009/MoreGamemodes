using BepInEx.Configuration;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

// https://github.com/tukasa0001/TownOfHost/pull/1265
namespace MoreGamemodes
{
    public class ClientOptionItem
    {
        public ConfigEntry<bool> Config;
        public ToggleButtonBehaviour ToggleButton;
        public static SpriteRenderer CustomBackground;
        private static int numOptions = 0;

        private ClientOptionItem(
            string name,
            ConfigEntry<bool> config,
            OptionsMenuBehaviour optionsMenuBehaviour,
            Action additionalOnClickAction = null)
        {
            try
            {
                Config = config;
                var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
                if (CustomBackground == null)
                {
                    numOptions = 0;
                    CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
                    CustomBackground.name = "CustomBackground";
                    CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
                    CustomBackground.transform.localPosition += Vector3.back * 8;
                    CustomBackground.gameObject.SetActive(false);

                    var closeButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
                    closeButton.transform.localPosition = new(1.3f, -2.3f, -6f);
                    closeButton.name = "Back";
                    closeButton.Text.text = "Back";
                    closeButton.Background.color = Palette.DisabledGrey;
                    var closePassiveButton = closeButton.GetComponent<PassiveButton>();
                    closePassiveButton.OnClick = new();
                    closePassiveButton.OnClick.AddListener(new Action(() => CustomBackground.gameObject.SetActive(false)));

                    UiElement[] selectableButtons = optionsMenuBehaviour.ControllerSelectable.ToArray();
                    PassiveButton leaveButton = null;
                    PassiveButton returnButton = null;
                    foreach (var button in selectableButtons)
                    {
                        if (button == null) continue;
                        if (button.name == "LeaveGameButton")
                            leaveButton = button.GetComponent<PassiveButton>();
                        else if (button.name == "ReturnToGameButton")
                            returnButton = button.GetComponent<PassiveButton>();
                    }
                    var generalTab = mouseMoveToggle.transform.parent.parent.parent;

                    var modOptionsButton = Object.Instantiate(mouseMoveToggle, generalTab);
                    modOptionsButton.transform.localPosition = leaveButton?.transform?.localPosition ?? new(0f, -2.4f, 1f);
                    modOptionsButton.name = "MGM Options";
                    modOptionsButton.Text.text = "MGM Options";
                    modOptionsButton.Background.color = Color.cyan;
                    var modOptionsPassiveButton = modOptionsButton.GetComponent<PassiveButton>();
                    modOptionsPassiveButton.OnClick = new();
                    modOptionsPassiveButton.OnClick.AddListener(new Action(() => CustomBackground.gameObject.SetActive(true)));

                    if (leaveButton != null)
                        leaveButton.transform.localPosition = new(-1.35f, -2.411f, -1f);
                    if (returnButton != null)
                        returnButton.transform.localPosition = new(1.35f, -2.411f, -1f);
                }

                ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
                ToggleButton.transform.localPosition = new Vector3(numOptions % 2 == 0 ? -1.3f : 1.3f, 2.2f - (0.5f * (numOptions / 2)), -6f);
                ToggleButton.name = name;
                ToggleButton.Text.text = name;
                var passiveButton = ToggleButton.GetComponent<PassiveButton>();
                passiveButton.OnClick = new();
                passiveButton.OnClick.AddListener(new Action(() =>
                {
                    if (config != null) config.Value = !config.Value;
                    UpdateToggle();
                    additionalOnClickAction?.Invoke();
                }));
                UpdateToggle();
            }
            finally { numOptions++; }
        }

        public static ClientOptionItem Create(
            string name,
            ConfigEntry<bool> config,
            OptionsMenuBehaviour optionsMenuBehaviour,
            Action additionalOnClickAction = null)
        {
            return new(name, config, optionsMenuBehaviour, additionalOnClickAction);
        }

        public void UpdateToggle()
        {
            if (ToggleButton == null) return;

            var color = (Config != null && Config.Value) ? new Color32(0, 255, 0, byte.MaxValue) : new Color32(77, 77, 77, byte.MaxValue);
            ToggleButton.Background.color = color;
            ToggleButton.Rollover?.ChangeOutColor(color);
        }
    }
}