using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using System;

namespace MoreGamemodes
{
    enum CustomRPC
    {
        VersionCheck = 70,
        SyncCustomOptions,
        SetBomb,
        SetItem,
        SetHackTimer,
        SetPaintTime,
        SetTheme,
        SetIsKiller,
        SetZombieType,
        SetKillsRemain,
        ReactorFlash,
        SetJailbreakPlayerType,
        SetItemAmount,
        SetCurrentRecipe,
        SetKillTimer,
        PetAction,
        StartGamemode,
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
    class ShipStatusHandleRpc
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.UpdateSystem:
                    __instance.UpdateSystem((SystemTypes)subReader.ReadByte(), subReader.ReadNetObject<PlayerControl>(), subReader.ReadByte());
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class PlayerControlHandleRpc
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.SendChat:
                    var text = subReader.ReadString();
                    if (!SendChatPatch.OnReceiveChat(__instance, text)) return false;
                    break;
                case RpcCalls.UsePlatform:
                    if (Options.DisableGapPlatform.GetBool()) return false;
                    break;
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.VersionCheck:
                    if (!AmongUsClient.Instance.AmHost) break;
                    if (reader.ReadString() != Main.CurrentVersion)
                    {
                        Utils.SendChat(__instance.Data.PlayerName + " was kicked for having other version of More Gamemodes.", "AutoKick");
                        AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
                    }
                    else
                        Main.IsModded[__instance.PlayerId] = true;
                    break;
                case CustomRPC.SetBomb:
                    __instance.SetBomb(reader.ReadBoolean());
                    break;
                case CustomRPC.SetItem:
                    __instance.SetItem((Items)reader.ReadPackedInt32());
                    break;
                case CustomRPC.SetIsKiller:
                    __instance.SetIsKiller(reader.ReadBoolean());
                    break;
                case CustomRPC.SetZombieType:
                    __instance.SetZombieType((ZombieTypes)reader.ReadPackedInt32());
                    break;
                case CustomRPC.SetKillsRemain:
                    __instance.SetKillsRemain(reader.ReadInt32());
                    break;
                case CustomRPC.ReactorFlash:
                    if (!__instance.AmOwner) break;
                    HudManager.Instance.ReactorFlash(reader.ReadSingle(), reader.ReadColor());
                    break;
                case CustomRPC.SetJailbreakPlayerType:
                    __instance.SetJailbreakPlayerType((JailbreakPlayerTypes)reader.ReadPackedInt32());
                    break;
                case CustomRPC.SetItemAmount:
                    __instance.SetItemAmount((InventoryItems)reader.ReadPackedInt32(), reader.ReadInt32());
                    break;
                case CustomRPC.SetCurrentRecipe:
                    __instance.SetCurrentRecipe(reader.ReadInt32());
                    break;
                case CustomRPC.SetKillTimer:
                    if (!__instance.AmOwner) break;
                    __instance.SetKillTimer(reader.ReadSingle());
                    break;
                case CustomRPC.PetAction:
                    if (!AmongUsClient.Instance.AmHost) return;
                    ExternalRpcPetPatch.Prefix(__instance.MyPhysics, (byte)CustomRPC.PetAction, new MessageReader());
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.HandleRpc))]
    class GameManagerHandleRpc
    {
        public static void Postfix(GameManager __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.SyncCustomOptions:
                    foreach (var co in OptionItem.AllOptions)
                    {
                        co.CurrentValue = reader.ReadInt32();
                    }
                    break;
                case CustomRPC.SetHackTimer:
                    if (RandomItemsGamemode.instance == null) break;
                    RandomItemsGamemode.instance.HackTimer = reader.ReadInt32();
                    break;
                case CustomRPC.SetPaintTime:
                    if (PaintBattleGamemode.instance == null) break;
                    PaintBattleGamemode.instance.PaintTime = reader.ReadInt32();
                    break;
                case CustomRPC.SetTheme:
                    __instance.SetTheme(reader.ReadString());
                    break;
                case CustomRPC.StartGamemode:
                    __instance.StartGamemode((Gamemodes)reader.ReadPackedInt32());
                    break;
            }
        }
    }

    static class RPC
    {
        public static void SetBomb(this PlayerControl player, bool hasBomb)
        {
            if (BombTagGamemode.instance == null) return;
            BombTagGamemode.instance.HasBomb[player.PlayerId] = hasBomb;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }
        
        public static void SetItem(this PlayerControl player, Items item)
        {
            if (RandomItemsGamemode.instance == null) return;
            RandomItemsGamemode.instance.AllPlayersItems[player.PlayerId] = item;
        }

        public static void SetTheme(this GameManager manager, string theme)
        {
            if (PaintBattleGamemode.instance == null) return;
            PaintBattleGamemode.instance.Theme = theme;
            HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void SetIsKiller(this PlayerControl player, bool isKiller)
        {
            if (KillOrDieGamemode.instance == null) return;
            KillOrDieGamemode.instance.IsKiller[player.PlayerId] = isKiller;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void SetZombieType(this PlayerControl player, ZombieTypes zombieType)
        {
            if (ZombiesGamemode.instance == null) return;
            ZombiesGamemode.instance.ZombieType[player.PlayerId] = zombieType;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void SetKillsRemain(this PlayerControl player, int killsRemain)
        {
            if (ZombiesGamemode.instance == null) return;
            ZombiesGamemode.instance.KillsRemain[player.PlayerId] = killsRemain;
        }

        public static void ReactorFlash(this HudManager hud, float duration, Color color)
        {
            if (hud.FullScreen == null) return;
            var obj = hud.transform.FindChild("FlashColor_FullScreen")?.gameObject;
            if (obj == null)
            {
                obj = GameObject.Instantiate(hud.FullScreen.gameObject, hud.transform);
                obj.name = "FlashColor_FullScreen";
            }
            hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((t) =>
            {
                obj.SetActive(t != 1f);
                obj.GetComponent<SpriteRenderer>().color = new(color.r, color.g, color.b, Mathf.Clamp01((-2f * Mathf.Abs(t - 0.5f) + 1) * color.a));
            })));
        }

        public static void SetJailbreakPlayerType(this PlayerControl player, JailbreakPlayerTypes playerType)
        {
            if (JailbreakGamemode.instance == null) return;
            JailbreakGamemode.instance.PlayerType[player.PlayerId] = playerType;
        }

        public static void SetItemAmount(this PlayerControl player, InventoryItems item, int amount)
        {
            if (JailbreakGamemode.instance == null) return;
            JailbreakGamemode.instance.Inventory[(player.PlayerId, item)] = amount;
        }

        public static void SetCurrentRecipe(this PlayerControl player, int recipeId)
        {
            if (JailbreakGamemode.instance == null) return;
            JailbreakGamemode.instance.CurrentRecipe[player.PlayerId] = recipeId;
        }

        public static void StartGamemode(this GameManager manager, Gamemodes gamemode)
        {
            CustomGamemode.Instance = null;
            ClassicGamemode.instance = null;
            HideAndSeekGamemode.instance = null;
            ShiftAndSeekGamemode.instance = null;
            BombTagGamemode.instance = null;
            RandomItemsGamemode.instance = null;
            BattleRoyaleGamemode.instance = null;
            SpeedrunGamemode.instance = null;
            PaintBattleGamemode.instance = null;
            KillOrDieGamemode.instance = null;
            ZombiesGamemode.instance = null;
            JailbreakGamemode.instance = null;
            switch (gamemode)
            {
                case Gamemodes.Classic:
                    ClassicGamemode.instance = new ClassicGamemode();
                    CustomGamemode.Instance = ClassicGamemode.instance;
                    break;
                case Gamemodes.HideAndSeek:
                    HideAndSeekGamemode.instance = new HideAndSeekGamemode();
                    CustomGamemode.Instance = HideAndSeekGamemode.instance;
                    break;
                case Gamemodes.ShiftAndSeek:
                    ShiftAndSeekGamemode.instance = new ShiftAndSeekGamemode();
                    CustomGamemode.Instance = ShiftAndSeekGamemode.instance;
                    break;
                case Gamemodes.BombTag:
                    BombTagGamemode.instance = new BombTagGamemode();
                    CustomGamemode.Instance = BombTagGamemode.instance;
                    break;
                case Gamemodes.RandomItems:
                    RandomItemsGamemode.instance = new RandomItemsGamemode();
                    CustomGamemode.Instance = RandomItemsGamemode.instance;
                    break;
                case Gamemodes.BattleRoyale:
                    BattleRoyaleGamemode.instance = new BattleRoyaleGamemode();
                    CustomGamemode.Instance = BattleRoyaleGamemode.instance;
                    break;
                case Gamemodes.Speedrun:
                    SpeedrunGamemode.instance = new SpeedrunGamemode();
                    CustomGamemode.Instance = SpeedrunGamemode.instance;
                    break;
                case Gamemodes.PaintBattle:
                    PaintBattleGamemode.instance = new PaintBattleGamemode();
                    CustomGamemode.Instance = PaintBattleGamemode.instance;
                    break;
                case Gamemodes.KillOrDie:
                    KillOrDieGamemode.instance = new KillOrDieGamemode();
                    CustomGamemode.Instance = KillOrDieGamemode.instance;
                    break;
                case Gamemodes.Zombies:
                    ZombiesGamemode.instance = new ZombiesGamemode();
                    CustomGamemode.Instance = ZombiesGamemode.instance;
                    break;
                case Gamemodes.Jailbreak:
                    JailbreakGamemode.instance = new JailbreakGamemode();
                    CustomGamemode.Instance = JailbreakGamemode.instance;
                    break;
            }
        }

        public static void RpcVersionCheck(this PlayerControl player, string version)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.VersionCheck, SendOption.Reliable, AmongUsClient.Instance.HostId);
            writer.Write(version);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSyncCustomOptions(this GameManager manager)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable, -1);
            foreach (var co in OptionItem.AllOptions)
            {
                writer.Write(co.CurrentValue);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetBomb(this PlayerControl player, bool hasBomb)
        {
            player.SetBomb(hasBomb);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetBomb, SendOption.Reliable, -1);
            writer.Write(hasBomb);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetItem(this PlayerControl player, Items item)
        {
            player.SetItem(item);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetItem, SendOption.Reliable, -1);
            writer.WritePacked((int)item);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetHackTimer(this GameManager manager, int time)
        {
            if (RandomItemsGamemode.instance == null) return;
            RandomItemsGamemode.instance.HackTimer = time;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetHackTimer, SendOption.Reliable, -1);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetPaintTime(this GameManager manager, int time)
        {
            if (PaintBattleGamemode.instance == null) return;
            PaintBattleGamemode.instance.PaintTime = time;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetPaintTime, SendOption.Reliable, -1);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetTheme(this GameManager manager, string theme)
        {
            manager.SetTheme(theme);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetTheme, SendOption.Reliable, -1);
            writer.Write(theme);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetIsKiller(this PlayerControl player, bool isKiller)
        {
            player.SetIsKiller(isKiller);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetIsKiller, SendOption.Reliable, -1);
            writer.Write(isKiller);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetZombieType(this PlayerControl player, ZombieTypes zombieType)
        {
            player.SetZombieType(zombieType);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetZombieType, SendOption.Reliable, -1);
            writer.WritePacked((int)zombieType);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetKillsRemain(this PlayerControl player, int killsRemain)
        {
            player.SetKillsRemain(killsRemain);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetKillsRemain, SendOption.Reliable, -1);
            writer.Write(killsRemain);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcReactorFlash(this PlayerControl player, float duration, Color color)
        {
            if (player.AmOwner)
            {
                HudManager.Instance.ReactorFlash(duration, color);
                return;
            }
            if (Main.IsModded[player.PlayerId])
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.ReactorFlash, SendOption.Reliable, -1);
                writer.Write(duration);
                writer.WriteColor(color);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return;
            }
            player.RpcUnmoddedReactorFlash(duration);
        }

        public static void RpcSetJailbreakPlayerType(this PlayerControl player, JailbreakPlayerTypes playerType)
        {
            player.SetJailbreakPlayerType(playerType);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetJailbreakPlayerType, SendOption.Reliable, -1);
            writer.WritePacked((int)playerType);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetItemAmount(this PlayerControl player, InventoryItems item, int amount)
        {
            player.SetItemAmount(item, amount);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetItemAmount, SendOption.Reliable, -1);
            writer.WritePacked((int)item);
            writer.Write(amount);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetCurrentRecipe(this PlayerControl player, int recipeId)
        {
            player.SetCurrentRecipe(recipeId);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetCurrentRecipe, SendOption.Reliable, -1);
            writer.Write(recipeId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetKillTimer(this PlayerControl player, float time)
        {
            if (player.AmOwner)
            {
                player.SetKillTimer(time);
                return;
            }
            if (Main.IsModded[player.PlayerId])
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetKillTimer, SendOption.Reliable, -1);
                writer.Write(time);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return;
            }
            player.RpcUnmoddedSetKillTimer(time);
        }

        public static void RpcPetAction(this PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.PetAction, SendOption.Reliable, AmongUsClient.Instance.HostId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcStartGamemode(this GameManager manager, Gamemodes gamemode)
        {
            manager.StartGamemode(gamemode);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.StartGamemode, SendOption.Reliable, -1);
            writer.WritePacked((int)gamemode);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}