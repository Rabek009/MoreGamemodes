using HarmonyLib;
using Hazel;

namespace MoreGamemodes
{
    //https://github.com/tukasa0001/TownOfHost/blob/main/Patches/GameManagerPatch.cs
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
    class IsGameOverDueToDeathPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}