using HarmonyLib;
using Hazel;
using System.Collections.Generic;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    class CloseDoorsPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            return CustomGamemode.Instance.OnCloseDoors(__instance);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
    class MessageReaderUpdateSystemPatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            MessageReader reader2 = MessageReader.Get(reader);
            return CustomGamemode.Instance.OnUpdateSystem(__instance, systemType, player, reader2);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(byte))]
    class ByteUpdateSystemPatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] byte amount)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            MessageWriter writer = MessageWriter.Get(0);
			writer.Write(amount);
			MessageReader reader = MessageReader.Get(writer.ToByteArray(false));
            return CustomGamemode.Instance.OnUpdateSystem(__instance, systemType, player, reader);     
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
    class AddTasksFromListPatch
    {
        public static void Prefix(ShipStatus __instance,
            [HarmonyArgument(4)] Il2CppSystem.Collections.Generic.List<NormalPlayerTask> unusedTasks)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            if (CustomGamemode.Instance.Gamemode != Gamemodes.Deathrun) return;
            List<NormalPlayerTask> disabledTasks = new();
            for (var i = 0; i < unusedTasks.Count; i++)
            {
                var task = unusedTasks[i];
                if (task.TaskType == TaskTypes.UploadData && CustomGamemode.Instance.Gamemode == Gamemodes.Deathrun) disabledTasks.Add(task);
            }
            foreach (var task in disabledTasks)
                unusedTasks.Remove(task);
        }
    }

    [HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.MushroomMixUp))]
    class MushroomMixUpPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (MeetingHud.Instance) return;
            new LateTask(() =>
            {
                if (!MeetingHud.Instance)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            pc.RpcSetNamePrivate(pc.BuildPlayerName(ar, false), ar, true);
                    }
                }
            }, 1f, "Set MixUp Name");
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    class ShipStatusBeginPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (!HudManager.Instance.IsIntroDisplayed)
            {
                new LateTask(() => __instance.Begin(), 0.1f, "Delayed Task Assign");
                return false;
            }
            return true;
        }
    }
}