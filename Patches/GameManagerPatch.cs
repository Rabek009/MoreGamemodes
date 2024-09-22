using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Serialize))]
    class GameManagerSerializeFix
    {
        public static bool Prefix(GameManager __instance, [HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] bool initialState, ref bool __result)
        {
            bool flag = false;
            for (int index = 0; index < __instance.LogicComponents.Count; ++index)
            {
                GameLogicComponent logicComponent = __instance.LogicComponents[index];
                if (initialState || logicComponent.IsDirty)
                {
                    flag = true;
                    writer.StartMessage((byte)index);
                    var hasBody = logicComponent.Serialize(writer, initialState);
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
        public static bool Prefix(LogicOptions __instance, ref bool __result, MessageWriter writer, bool initialState)
        {
            if (!initialState && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
            {
                __result = false;
                return false;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    class AntiBlackoutPatch
    {
        public static bool Prefix(LogicGameFlow __instance, ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalGameOptionsV08), nameof(NormalGameOptionsV08.Serialize))]
    class NormalGameOptionsV08SerializePatch
    {
        public static bool Prefix([HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] NormalGameOptionsV08 gameOptions)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Main.ModdedProtocol.Value || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || (!gameOptions.AreInvalid(15) && gameOptions.MaxPlayers >= 0)) return true;
            writer.Write((byte)gameOptions.SpecialMode);
			writer.Write((byte)gameOptions.RulesPreset);
            if (gameOptions.MaxPlayers < 0)
                writer.Write((byte)0);
            else if (gameOptions.MaxPlayers > 15)
			    writer.Write((byte)15);
            else
                writer.Write((byte)gameOptions.MaxPlayers);
			writer.Write((uint)gameOptions.Keywords);
			writer.Write(gameOptions.MapId);
            if (gameOptions.PlayerSpeedMod <= 0f)
                writer.Write(0.0001f);
            else if (gameOptions.PlayerSpeedMod > 3f)
                writer.Write(3f);
            else
			    writer.Write(gameOptions.PlayerSpeedMod);
			writer.Write(gameOptions.CrewLightMod);
			writer.Write(gameOptions.ImpostorLightMod);
			writer.Write(gameOptions.KillCooldown);
			writer.Write((byte)gameOptions.NumCommonTasks);
			writer.Write((byte)gameOptions.NumLongTasks);
			writer.Write((byte)gameOptions.NumShortTasks);
			writer.Write(gameOptions.NumEmergencyMeetings);
            if (gameOptions.NumImpostors < 1)
                writer.Write((byte)1);
            else if (gameOptions.NumImpostors > 3)
                writer.Write((byte)3);
            else
			    writer.Write((byte)gameOptions.NumImpostors);
            if (gameOptions.KillDistance < 0)
                writer.Write((byte)0);
            else if (gameOptions.KillDistance > 2)
                writer.Write((byte)2);
            else
			    writer.Write((byte)gameOptions.KillDistance);
			writer.Write(gameOptions.DiscussionTime);
			writer.Write(gameOptions.VotingTime);
			writer.Write(gameOptions.IsDefaults);
			writer.Write((byte)gameOptions.EmergencyCooldown);
			writer.Write(gameOptions.ConfirmImpostor);
			writer.Write(gameOptions.VisualTasks);
			writer.Write(gameOptions.AnonymousVotes);
			writer.Write((byte)gameOptions.TaskBarMode);
			RoleOptionsCollectionV08.Serialize(writer, gameOptions.roleOptions);
            return false;
        }
    }

    [HarmonyPatch(typeof(HideNSeekGameOptionsV08), nameof(HideNSeekGameOptionsV08.Serialize))]
    class HideNSeekGameOptionsV08SerializePatch
    {
        public static bool Prefix([HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] HideNSeekGameOptionsV08 gameOptions)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Main.ModdedProtocol.Value || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || (!gameOptions.AreInvalid(15) && gameOptions.MaxPlayers >= 0)) return true;
            writer.Write((byte)gameOptions.SpecialMode);
			writer.Write((byte)gameOptions.RulesPreset);
            if (gameOptions.MaxPlayers < 0)
                writer.Write((byte)0);
            else if (gameOptions.MaxPlayers > 15)
			    writer.Write((byte)15);
            else
                writer.Write((byte)gameOptions.MaxPlayers);
			writer.Write((uint)gameOptions.Keywords);
			writer.Write(gameOptions.MapId);
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
            return false;
        }
    }
}