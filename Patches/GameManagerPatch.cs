using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System;

namespace MoreGamemodes
{
    //https://github.com/tukasa0001/TownOfHost/blob/main/Patches/GameManagerPatch.cs
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Serialize))]
    class GameManagerSerializeFix
    {
		public static bool InitialState = true;
        public static bool Prefix(GameManager __instance, [HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] bool initialState, ref bool __result)
		{
			InitialState = initialState;
			bool flag = false;
			for (int index = 0; index < __instance.LogicComponents.Count; ++index)
			{
				GameLogicComponent logicComponent = __instance.LogicComponents[index];
				if (initialState || logicComponent.IsDirty)
				{
					flag = true;
					writer.StartMessage((byte)index);
					var hasBody = logicComponent.Serialize(writer);
					if (hasBody) writer.EndMessage();
					else writer.CancelMessage();
					logicComponent.ClearDirtyFlag();
				}
			}
			__instance.ClearDirtyBits();
			__result = flag;
			return false;
		}
    }
    [HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.Serialize))]
    class LogicOptionsSerializePatch
    {
        public static bool Prefix(LogicOptions __instance, [HarmonyArgument(0)] MessageWriter writer, ref bool __result)
        {
            if (!GameManagerSerializeFix.InitialState && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    class IsGameOverDueToDeathPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalGameOptionsV09), nameof(NormalGameOptionsV09.Serialize))]
    class NormalGameOptionsV09SerializePatch
    {
        public static bool Prefix([HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] NormalGameOptionsV09 gameOptions)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            writer.Write((byte)gameOptions.SpecialMode);
			writer.Write((byte)gameOptions.RulesPreset);
			writer.Write((byte)gameOptions.MaxPlayers);
			writer.Write((uint)gameOptions.Keywords);
			writer.Write(gameOptions.MapId == 3 ? (byte)0 : gameOptions.MapId);
			writer.Write(gameOptions.PlayerSpeedMod);
			writer.Write(gameOptions.CrewLightMod);
			writer.Write(gameOptions.ImpostorLightMod);
			writer.Write(gameOptions.KillCooldown);
			writer.Write((byte)gameOptions.NumCommonTasks);
			writer.Write((byte)gameOptions.NumLongTasks);
			writer.Write((byte)gameOptions.NumShortTasks);
			writer.Write(gameOptions.NumEmergencyMeetings);
			writer.Write((byte)gameOptions.NumImpostors);
			writer.Write((byte)gameOptions.KillDistance);
			writer.Write(gameOptions.DiscussionTime);
			writer.Write(gameOptions.VotingTime);
			writer.Write(gameOptions.IsDefaults);
			writer.Write((byte)gameOptions.EmergencyCooldown);
			writer.Write(gameOptions.ConfirmImpostor);
			writer.Write(gameOptions.VisualTasks);
			writer.Write(gameOptions.AnonymousVotes);
			writer.Write((byte)gameOptions.TaskBarMode);
			writer.Write((byte)gameOptions.Tag);
			RoleOptionsCollectionV09.Serialize(writer, gameOptions.roleOptions);
            writer.Write(gameOptions.MapId == 3);
            return false;
        }
    }
    
    [HarmonyPatch(typeof(NormalGameOptionsV09), nameof(NormalGameOptionsV09.Deserialize))]
    class NormalGameOptionsV09DeserializePatch
    {
        public static bool Prefix([HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool isAprilFoolsMode, [HarmonyArgument(2)] ILogger logger, ref NormalGameOptionsV09 __result)
        {
            try
			{
				NormalGameOptionsV09 normalGameOptionsV = new(logger);
				if (isAprilFoolsMode)
				{
					normalGameOptionsV.GameMode = normalGameOptionsV.AprilFoolsOnMode;
				}
				normalGameOptionsV.SpecialMode = (SpecialGameModes)reader.ReadByte();
				normalGameOptionsV.RulesPreset = (RulesPresets)reader.ReadByte();
				normalGameOptionsV.MaxPlayers = reader.ReadByte();
				normalGameOptionsV.Keywords = (GameKeywords)reader.ReadUInt32();
				normalGameOptionsV.MapId = reader.ReadByte();
				normalGameOptionsV.PlayerSpeedMod = reader.ReadSingle();
				normalGameOptionsV.CrewLightMod = reader.ReadSingle();
				normalGameOptionsV.ImpostorLightMod = reader.ReadSingle();
				normalGameOptionsV.KillCooldown = reader.ReadSingle();
				normalGameOptionsV.NumCommonTasks = reader.ReadByte();
				normalGameOptionsV.NumLongTasks = reader.ReadByte();
				normalGameOptionsV.NumShortTasks = reader.ReadByte();
				normalGameOptionsV.NumEmergencyMeetings = reader.ReadInt32();
				normalGameOptionsV.NumImpostors = reader.ReadByte();
				normalGameOptionsV.KillDistance = reader.ReadByte();
				normalGameOptionsV.DiscussionTime = reader.ReadInt32();
				normalGameOptionsV.VotingTime = reader.ReadInt32();
				normalGameOptionsV.IsDefaults = reader.ReadBoolean();
				normalGameOptionsV.EmergencyCooldown = reader.ReadByte();
				normalGameOptionsV.ConfirmImpostor = reader.ReadBoolean();
				normalGameOptionsV.VisualTasks = reader.ReadBoolean();
				normalGameOptionsV.AnonymousVotes = reader.ReadBoolean();
				normalGameOptionsV.TaskBarMode = (AmongUs.GameOptions.TaskBarMode)reader.ReadByte();
				normalGameOptionsV.Tag = reader.ReadByte();
				normalGameOptionsV.roleOptions.UpdateFrom(RoleOptionsCollectionV09.Deserialize(reader));
                if (reader.BytesRemaining > 0 && reader.ReadBoolean())
                    normalGameOptionsV.MapId = 3;
				__result = normalGameOptionsV;
                return false;
			}
			catch (Exception ex)
			{
				logger.WriteError("[NormalGameOptions] Deserialize() failed. More info in stack:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
			}
			__result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(HideNSeekGameOptionsV09), nameof(HideNSeekGameOptionsV09.Serialize))]
    class HideNSeekGameOptionsV09SerializePatch
    {
        public static bool Prefix([HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] HideNSeekGameOptionsV09 gameOptions)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            writer.Write((byte)gameOptions.SpecialMode);
			writer.Write((byte)gameOptions.RulesPreset);
			writer.Write((byte)gameOptions.MaxPlayers);
			writer.Write((uint)gameOptions.Keywords);
			writer.Write(gameOptions.MapId == 3 ? (byte)0 : gameOptions.MapId);
			writer.Write(gameOptions.PlayerSpeedMod);
			writer.Write(gameOptions.CrewLightMod);
			writer.Write(gameOptions.ImpostorLightMod);
			writer.Write((byte)gameOptions.NumCommonTasks);
			writer.Write((byte)gameOptions.NumLongTasks);
			writer.Write((byte)gameOptions.NumShortTasks);
			writer.Write(gameOptions.IsDefaults);
			writer.Write(gameOptions.CrewmateVentUses);
			writer.Write(gameOptions.EscapeTime);
			writer.Write(gameOptions.CrewmateFlashlightSize);
			writer.Write(gameOptions.ImpostorFlashlightSize);
			writer.Write(gameOptions.useFlashlight);
			writer.Write(gameOptions.SeekerFinalMap);
			writer.Write(gameOptions.FinalCountdownTime);
			writer.Write(gameOptions.SeekerFinalSpeed);
			writer.Write(gameOptions.SeekerPings);
			writer.Write(gameOptions.ShowCrewmateNames);
			writer.Write(gameOptions.ImpostorPlayerID);
			writer.Write(gameOptions.MaxPingTime);
			writer.Write(gameOptions.CrewmateTimeInVent);
			writer.Write((byte)gameOptions.Tag);
            writer.Write(gameOptions.MapId == 3);
            return false;
        }
    }

    [HarmonyPatch(typeof(HideNSeekGameOptionsV09), nameof(HideNSeekGameOptionsV09.Deserialize))]
    class HideNSeekGameOptionsV09DeserializePatch
    {
        public static bool Prefix([HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool isAprilFoolsMode, [HarmonyArgument(2)] ILogger logger, ref HideNSeekGameOptionsV09 __result)
        {
            try
			{
				HideNSeekGameOptionsV09 hideNSeekGameOptionsV = new(logger);
				if (isAprilFoolsMode)
				{
					hideNSeekGameOptionsV.GameMode = hideNSeekGameOptionsV.AprilFoolsOnMode;
				}
				hideNSeekGameOptionsV.SpecialMode = (SpecialGameModes)reader.ReadByte();
				hideNSeekGameOptionsV.RulesPreset = (RulesPresets)reader.ReadByte();
				hideNSeekGameOptionsV.MaxPlayers = reader.ReadByte();
				hideNSeekGameOptionsV.Keywords = (GameKeywords)reader.ReadUInt32();
				hideNSeekGameOptionsV.MapId = reader.ReadByte();
				hideNSeekGameOptionsV.PlayerSpeedMod = reader.ReadSingle();
				hideNSeekGameOptionsV.CrewLightMod = reader.ReadSingle();
				hideNSeekGameOptionsV.ImpostorLightMod = reader.ReadSingle();
				hideNSeekGameOptionsV.NumCommonTasks = reader.ReadByte();
				hideNSeekGameOptionsV.NumLongTasks = reader.ReadByte();
				hideNSeekGameOptionsV.NumShortTasks = reader.ReadByte();
				hideNSeekGameOptionsV.IsDefaults = reader.ReadBoolean();
				hideNSeekGameOptionsV.CrewmateVentUses = reader.ReadInt32();
				hideNSeekGameOptionsV.EscapeTime = reader.ReadSingle();
				hideNSeekGameOptionsV.CrewmateFlashlightSize = reader.ReadSingle();
				hideNSeekGameOptionsV.ImpostorFlashlightSize = reader.ReadSingle();
				hideNSeekGameOptionsV.useFlashlight = reader.ReadBoolean();
				hideNSeekGameOptionsV.SeekerFinalMap = reader.ReadBoolean();
				hideNSeekGameOptionsV.FinalCountdownTime = reader.ReadSingle();
				hideNSeekGameOptionsV.SeekerFinalSpeed = reader.ReadSingle();
				hideNSeekGameOptionsV.SeekerPings = reader.ReadBoolean();
				hideNSeekGameOptionsV.ShowCrewmateNames = reader.ReadBoolean();
				hideNSeekGameOptionsV.ImpostorPlayerID = reader.ReadInt32();
				hideNSeekGameOptionsV.MaxPingTime = reader.ReadSingle();
				hideNSeekGameOptionsV.CrewmateTimeInVent = reader.ReadSingle();
				hideNSeekGameOptionsV.Tag = reader.ReadByte();
                if (reader.BytesRemaining > 0 && reader.ReadBoolean())
                    hideNSeekGameOptionsV.MapId = 3;
                __result = hideNSeekGameOptionsV;
				return false;
			}
			catch (Exception ex)
			{
				logger.WriteError("[HideNSeekGameOptions] Deserialize failed. More info in stack:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
			}
			__result = null;
            return false;
        }
    }
}