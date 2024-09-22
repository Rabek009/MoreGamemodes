using HarmonyLib;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManagerUpdatePatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    class UnrestrictedNumImpostorsPatch
    {
        public static bool Prefix(ref int __result)
        {
            __result = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors);
            return false;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    class BeginGamePatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Options.EnableRandomMap.GetBool())
            {
                List<byte> maps = new();
                if (Options.AddTheSkeld.GetBool())
                    maps.Add(0);
                if (Options.AddMiraHQ.GetBool())
                    maps.Add(1);
                if (Options.AddPolus.GetBool())
                    maps.Add(2);
                if (Options.AddDleksEht.GetBool())
                    maps.Add(3);
                if (Options.AddTheAirship.GetBool())
                    maps.Add(4);
                if (Options.AddTheFungle.GetBool())
                    maps.Add(5);
                if (maps.Count == 0)
                {
                    maps.Add(0);
                    maps.Add(1);
                    maps.Add(2);
                    maps.Add(3);
                    maps.Add(4);
                    maps.Add(5);
                }
                var rand = new Random();
                byte map = maps[rand.Next(0, maps.Count)];
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, map);
                if (map == 3)
                    CreateOptionsPickerPatch.SetDleks = true;
                else
                    CreateOptionsPickerPatch.SetDleks = false;
            }
            else if (CreateOptionsPickerPatch.SetDleks)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 3);
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            if (Options.CurrentGamemode is Gamemodes.Jailbreak or Gamemodes.BaseWars && GameOptionsManager.Instance.CurrentGameOptions.MapId != 0 && GameOptionsManager.Instance.CurrentGameOptions.MapId != 3)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		    GameManager.Instance.LogicOptions.SyncOptions();
        }
    }

    // https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/Patches/LobbyBehaviourPatch.cs
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Update))]
    public class LobbyBehaviourUpdatePatch
    {
        public static void Postfix(LobbyBehaviour __instance)
        {
            Func<ISoundPlayer, bool> lobbybgm = x => x.Name.Equals("MapTheme");
            ISoundPlayer MapThemeSound = SoundManager.Instance.soundPlayers.Find(lobbybgm);
            if (Main.DisableLobbyMusic.Value)
            {
                if (MapThemeSound == null) return;
                SoundManager.Instance.StopNamedSound("MapTheme");
            }
            else
            {
                if (MapThemeSound != null) return;
                SoundManager.Instance.CrossFadeSound("MapTheme", __instance.MapTheme, 0.5f);
            }
        }
    }
}