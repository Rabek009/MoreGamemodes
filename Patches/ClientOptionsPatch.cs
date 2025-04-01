using UnityEngine;
using HarmonyLib;
using InnerNet;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenuBehaviourStartPatch
    {
        private static ClientOptionItem ModdedProtocol;
        private static ClientOptionItem UnlockFPS;
        private static ClientOptionItem ShowFPS;
        private static ClientOptionItem DarkTheme;
        private static ClientOptionItem DisableLobbyMusic;
        private static ClientOptionItem ApplyBanList;

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.DisableMouseMovement == null) return;

            if (ModdedProtocol == null || ModdedProtocol.ToggleButton == null)
            {
                ModdedProtocol = ClientOptionItem.Create("Modded Protocol", Main.ModdedProtocol, __instance, ModdedProtocolButtonToggle);
                static void ModdedProtocolButtonToggle()
                {
                    Main.ModdedProtocol.Value = !Main.ModdedProtocol.Value;
                    ModdedProtocol.UpdateToggle();
                }
            }
            if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
            {
                UnlockFPS = ClientOptionItem.Create("Unlock FPS", Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
                static void UnlockFPSButtonToggle()
                {
                    Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
                }
            }
            if (ShowFPS == null || ShowFPS.ToggleButton == null)
            {
                ShowFPS = ClientOptionItem.Create("Show FPS", Main.ShowFPS, __instance);
            }
            if (DarkTheme == null || DarkTheme.ToggleButton == null)
            {
                DarkTheme = ClientOptionItem.Create("Dark Theme", Main.DarkTheme, __instance, DarkThemeButtonToggle);
                static void DarkThemeButtonToggle()
                {
                    if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.NotJoined || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended) return;
                    if (Main.DarkTheme.Value)
                    {
                        Il2CppSystem.Collections.Generic.List<PoolableBehavior> activeChildren = HudManager.Instance.Chat.chatBubblePool.activeChildren;
                        for (int i = 0; i < activeChildren.Count; ++i)
                        {
                            ChatBubble chatBubble = activeChildren[i].Cast<ChatBubble>();
                            if (chatBubble.Background.color == Color.white)
                                chatBubble.Background.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                            if (chatBubble.Background.color == Palette.HalfWhite)
                                chatBubble.Background.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
                            chatBubble.TextArea.color = Color.white;
                        }
                    }
                    else
                    {
                        Il2CppSystem.Collections.Generic.List<PoolableBehavior> activeChildren = HudManager.Instance.Chat.chatBubblePool.activeChildren;
                        for (int i = 0; i < activeChildren.Count; ++i)
                        {
                            ChatBubble chatBubble = activeChildren[i].Cast<ChatBubble>();
                            if (chatBubble.Background.color == new Color(0.1f, 0.1f, 0.1f, 1f))
                                chatBubble.Background.color = Color.white;
                            if (chatBubble.Background.color == new Color(0.1f, 0.1f, 0.1f, 0.6f))
                                chatBubble.Background.color = Palette.HalfWhite;
                            chatBubble.TextArea.color = Color.black;
                        }
                        HudManager.Instance.Chat.freeChatField.background.color = Color.white;
                        HudManager.Instance.Chat.freeChatField.textArea.compoText.Color(Color.black);
                        HudManager.Instance.Chat.quickChatField.background.color = Color.white;
                        HudManager.Instance.Chat.quickChatField.text.color = Color.black;
                    }
                }
            }
            if (DisableLobbyMusic == null || DisableLobbyMusic.ToggleButton == null)
            {
                DisableLobbyMusic = ClientOptionItem.Create("Disable Lobby Music", Main.DisableLobbyMusic, __instance);
            }
            if (ApplyBanList == null || ApplyBanList.ToggleButton == null)
            {
                ApplyBanList = ClientOptionItem.Create("Apply Ban List", Main.ApplyBanList, __instance);
            }
        }
    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
    public static class OptionsMenuBehaviourClosePatch
    {
        public static void Postfix()
        {
            ClientOptionItem.CustomBackground?.gameObject.SetActive(false);
        }
    }
}