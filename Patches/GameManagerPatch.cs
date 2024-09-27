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
}