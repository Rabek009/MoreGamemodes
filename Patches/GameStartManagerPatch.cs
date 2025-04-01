using HarmonyLib;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System;
using Hazel;
using InnerNet;

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
            }
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            if (Options.CurrentGamemode is Gamemodes.Jailbreak or Gamemodes.BaseWars && GameOptionsManager.Instance.CurrentGameOptions.MapId != 0 && GameOptionsManager.Instance.CurrentGameOptions.MapId != 3)
                GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 0);
            GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		    GameManager.Instance.LogicOptions.SyncOptions();
        }
        public static void Postfix(GameStartManager __instance)
		{
			if (!AmongUsClient.Instance.AmHost || Main.ModdedProtocol.Value) return;
			MessageWriter msg = MessageWriter.Get(SendOption.Reliable);
			msg.StartMessage(6);
			msg.Write(AmongUsClient.Instance.GameId);
			msg.WritePacked(int.MaxValue);
			for (int i = 0; i < AmongUsClient.Instance.allObjects.Count; i++)
			{
				InnerNetObject innerNetObject = AmongUsClient.Instance.allObjects[i];
				msg.StartMessage(4);
				msg.WritePacked(GameStartManager.Instance.LobbyPrefab.SpawnId);
				msg.WritePacked(innerNetObject.OwnerId);
				msg.Write((byte)innerNetObject.SpawnFlags);
				msg.WritePacked(1);
				msg.WritePacked(innerNetObject.NetId);
				msg.StartMessage(1);
				msg.EndMessage();
				msg.EndMessage();
			}
			msg.EndMessage();
			AmongUsClient.Instance.SendOrDisconnect(msg);
			msg.Recycle();
			__instance.FinallyBegin();
		}
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    class GameStartManagerStartPatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            MapIconByName dleksMap = new()
            {
                Name = MapNames.Dleks,
                MapIcon = __instance.AllMapIcons[0].MapIcon,
                MapImage = __instance.AllMapIcons[0].MapImage,
                NameImage = __instance.AllMapIcons[0].NameImage,
            };
            __instance.AllMapIcons.Insert(3, dleksMap);
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.UpdateMapImage))]
    class UpdateMapImagePatch
    {
        public static void Postfix(GameStartManager __instance, [HarmonyArgument(0)] MapNames map)
        {
			__instance.MapImage.flipX = map == MapNames.Dleks;
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.RpcExtendLobbyTimer))]
    class RpcExtendLobbyTimerPatch
    {
        public static bool Prefix(LobbyBehaviour __instance)
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists) return true;
            MessageWriter writer = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.ExtendLobbyTimer, SendOption.Reliable);
		    writer.WritePacked(__instance.currentExtensionId);
		    writer.EndMessage();
            return false;
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