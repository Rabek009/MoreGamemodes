using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using System;
using System.Linq;
using AmongUs.GameOptions;

using Object = UnityEngine.Object;

namespace MoreGamemodes
{
    enum CustomRPC
    {
        VersionCheck = 70,
        SyncCustomOptions,
        SetBomb,
        SetItem,
        SetHackActive,
        SetPaintActive,
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
        RemoveDeadBody,
        RequestVersionCheck,
        AddCustomSettingsChangeMessage,
        SetBaseWarsTeam,
        DestroyTurret,
        SetCanTeleport,
        SetIsDead,
        SetStandardName
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
                    SystemTypes system = (SystemTypes)subReader.ReadByte();
                    PlayerControl player = subReader.ReadNetObject<PlayerControl>();
                    byte amount = subReader.ReadByte();
                    if (AntiCheat.RpcUpdateSystemCheck(player, system, amount, subReader)) return false;
                    return true;
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
            if (AntiCheat.PlayerControlReceiveRpc(__instance, callId, reader)) return false;
            switch (rpcType)
            {
                case RpcCalls.SendChat:
                    var text = subReader.ReadString();
                    if (!SendChatPatch.OnReceiveChat(__instance, text)) return false;
                    break;
                case RpcCalls.UsePlatform:
                    if (CustomGamemode.Instance.Gamemode == Gamemodes.RandomItems && (!__instance.Data.Role.IsImpostor || Options.HackAffectsImpostors.GetBool()) && RandomItemsGamemode.instance.IsHackActive) return false;
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
                    {
                        Main.IsModded[__instance.PlayerId] = true;
                        int clientId = __instance.GetClientId();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            pc.RpcSetStandardName(Main.StandardNames.ContainsKey(pc.PlayerId) ? Main.StandardNames[pc.PlayerId] : "", clientId);
                    }
                    break;
                case CustomRPC.SetBomb:
                    __instance.SetBomb(reader.ReadBoolean());
                    break;
                case CustomRPC.SetItem:
                    __instance.SetItem((Items)reader.ReadInt32());
                    break;
                case CustomRPC.SetIsKiller:
                    __instance.SetIsKiller(reader.ReadBoolean());
                    break;
                case CustomRPC.SetZombieType:
                    __instance.SetZombieType((ZombieTypes)reader.ReadInt32());
                    break;
                case CustomRPC.SetKillsRemain:
                    __instance.SetKillsRemain(reader.ReadInt32());
                    break;
                case CustomRPC.ReactorFlash:
                    if (!__instance.AmOwner) break;
                    HudManager.Instance.ReactorFlash(reader.ReadSingle(), reader.ReadColor());
                    break;
                case CustomRPC.SetJailbreakPlayerType:
                    __instance.SetJailbreakPlayerType((JailbreakPlayerTypes)reader.ReadInt32());
                    break;
                case CustomRPC.SetItemAmount:
                    __instance.SetItemAmount((InventoryItems)reader.ReadInt32(), reader.ReadInt32());
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
                case CustomRPC.RemoveDeadBody:
                    NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
                    __instance.RemoveDeadBody(playerById);
                    break;
                case CustomRPC.RequestVersionCheck:
                    if (__instance.GetClientId() != AmongUsClient.Instance.HostId) break;
                    PlayerControl.LocalPlayer.RpcVersionCheck(Main.CurrentVersion);
                    break;
                case CustomRPC.SetBaseWarsTeam:
                    __instance.SetBaseWarsTeam((BaseWarsTeams)reader.ReadInt32());
                    break;
                case CustomRPC.SetCanTeleport:
                    __instance.SetCanTeleport(reader.ReadBoolean());
                    break;
                case CustomRPC.SetIsDead:
                    __instance.SetIsDead(reader.ReadBoolean());
                    break;
                case CustomRPC.SetStandardName:
                    __instance.SetStandardName(reader.ReadString());
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
                        if (co is TextOptionItem) continue;
                        if (co.Id == 0)
                        {
                            co.SetValue(9);
                            continue;
                        }
                        if (co.Id >= 1000 && co.IsHiddenOn(Options.CurrentGamemode)) continue;
                        co.SetValue(reader.ReadInt32());
                    }
                    break;
                case CustomRPC.SetHackActive:
                    __instance.SetHackActive(reader.ReadBoolean());
                    break;
                case CustomRPC.SetPaintActive:
                    __instance.SetPaintActive(reader.ReadBoolean());
                    break;
                case CustomRPC.SetTheme:
                    __instance.SetTheme(reader.ReadString());
                    break;
                case CustomRPC.StartGamemode:
                    __instance.StartGamemode((Gamemodes)reader.ReadInt32());
                    break;
                case CustomRPC.AddCustomSettingsChangeMessage:
                    int optionId = reader.ReadInt32();
                    var optionItem = OptionItem.AllOptions.FirstOrDefault(opt => opt.Id == optionId);
                    if (optionItem == null) break;
                    __instance.AddCustomSettingsChangeMessage(optionItem, reader.ReadString(), reader.ReadBoolean());
                    break;
                case CustomRPC.DestroyTurret:
                    __instance.DestroyTurret((SystemTypes)reader.ReadByte());
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
    class CustomNetworkTransformHandleRpc
    {
        public static bool Prefix(CustomNetworkTransform __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var rpcType = (RpcCalls)callId;
            MessageReader subReader = MessageReader.Get(reader);
            if (AntiCheat.CustomNetworkTransformReceiveRpc(__instance, callId, reader)) return false;
            switch (rpcType)
            {
                case RpcCalls.SnapTo:
                    var text = subReader.ReadString();
                    if (CoEnterVentPatch.PlayersToKick.Contains(__instance.myPlayer.PlayerId) || (AntiCheat.TimeSinceVentCancel.ContainsKey(__instance.myPlayer.PlayerId) && AntiCheat.TimeSinceVentCancel[__instance.myPlayer.PlayerId] <= 1f)) return false;
                    break;
            }
            return true;
        }
    }

    static class RPC
    {
        public static void SetBomb(this PlayerControl player, bool hasBomb)
        {
            if (BombTagGamemode.instance == null) return;
            BombTagGamemode.instance.PlayerHasBomb[player.PlayerId] = hasBomb;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }
        
        public static void SetItem(this PlayerControl player, Items item)
        {
            if (RandomItemsGamemode.instance == null) return;
            RandomItemsGamemode.instance.AllPlayersItems[player.PlayerId] = item;
        }

        public static void SetHackActive(this GameManager manager, bool active)
        {
            if (RandomItemsGamemode.instance == null) return;
            RandomItemsGamemode.instance.IsHackActive = active;
            HudManager.Instance.SetHudActive(!MeetingHud.Instance);
            if (Minigame.Instance)
			{
				try
				{
					Minigame.Instance.Close();
					Minigame.Instance.Close();
				}
				catch
				{
				}
			}
        }

        public static void SetPaintActive(this GameManager manager, bool active)
        {
            if (PaintBattleGamemode.instance == null) return;
            PaintBattleGamemode.instance.IsPaintActive = active;
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
            KillOrDieGamemode.instance.IsPlayerKiller[player.PlayerId] = isKiller;
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
                obj = Object.Instantiate(hud.FullScreen.gameObject, hud.transform);
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
            DeathrunGamemode.instance = null;
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
                case Gamemodes.Deathrun:
                    DeathrunGamemode.instance = new DeathrunGamemode();
                    CustomGamemode.Instance = DeathrunGamemode.instance;
                    break;
                case Gamemodes.BaseWars:
                    BaseWarsGamemode.instance = new BaseWarsGamemode();
                    CustomGamemode.Instance = BaseWarsGamemode.instance;
                    break;
            }
        }

        public static void RemoveDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
        {
            if (target == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                AntiCheat.RemovedBodies.Add(target.PlayerId);
            }
            foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == target.PlayerId)
                    Object.Destroy(deadBody.gameObject);
            }
        }

        public static void AddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            string optionName = "";
            if (optionItem.Parent?.Parent?.Parent != null)
                optionName += optionItem.Parent.Parent.Parent.GetOptionNameSCM() + " → ";
            if (optionItem.Parent?.Parent != null)
                optionName += optionItem.Parent.Parent.GetOptionNameSCM() + " → ";
            if (optionItem.Parent != null)
                optionName += optionItem.Parent.GetOptionNameSCM() + " → ";
            optionName += optionItem.GetOptionNameSCM();
            string text = $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{optionName}</font>: <font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{value}</font>";
            HudManager.Instance.Notifier.CustomSettingsChangeMessageLogic(optionItem, text, playSound);
        }

        public static void SetBaseWarsTeam(this PlayerControl player, BaseWarsTeams team)
        {
            if (BaseWarsGamemode.instance == null) return;
            BaseWarsGamemode.instance.PlayerTeam[player.PlayerId] = team;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public static void DestroyTurret(this GameManager manager, SystemTypes room)
        {
            if (BaseWarsGamemode.instance == null) return;
            if (BaseWarsGamemode.instance.AllTurretsPosition.Contains(room))
                BaseWarsGamemode.instance.AllTurretsPosition.Remove(room);
        }

        public static void SetCanTeleport(this PlayerControl player, bool canTeleport)
        {
            if (BaseWarsGamemode.instance == null) return;
            BaseWarsGamemode.instance.CanTeleport[player.PlayerId] = canTeleport;
        }

        public static void SetIsDead(this PlayerControl player, bool isDead)
        {
            switch (CustomGamemode.Instance.Gamemode)
            {
                case Gamemodes.Jailbreak:
                    JailbreakGamemode.instance.IsDead[player.PlayerId] = isDead;
                    break;
                case Gamemodes.BaseWars:
                    BaseWarsGamemode.instance.IsDead[player.PlayerId] = isDead;
                    break;
            }
        }

        public static void SetStandardName(this PlayerControl player, string name)
        {
            Main.StandardNames[player.PlayerId] = name;
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
                if (co.Id == 0 || co is TextOptionItem) continue;
                if (co.Id >= 1000 && co.IsHiddenOn(Options.CurrentGamemode)) continue;
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
            writer.Write((int)item);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetHackActive(this GameManager manager, bool active)
        {
            manager.SetHackActive(active);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetHackActive, SendOption.Reliable, -1);
            writer.Write(active);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetPaintActive(this GameManager manager, bool active)
        {
            manager.SetPaintActive(active);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SetPaintActive, SendOption.Reliable, -1);
            writer.Write(active);
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
            writer.Write((int)zombieType);
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
            writer.Write((int)playerType);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetItemAmount(this PlayerControl player, InventoryItems item, int amount)
        {
            player.SetItemAmount(item, amount);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetItemAmount, SendOption.Reliable, -1);
            writer.Write((int)item);
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

        public static void RpcSetKillTimer(this PlayerControl player, float time = float.MaxValue)
        {
            if (player.AmOwner)
            {
                player.SetKillTimer(time != float.MaxValue ? time : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                return;
            }
            if (Main.IsModded[player.PlayerId])
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetKillTimer, SendOption.Reliable, -1);
                writer.Write(time != float.MaxValue ? time : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
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
            writer.Write((int)gamemode);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRemoveDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
        {
            player.RemoveDeadBody(target);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.RemoveDeadBody, SendOption.Reliable, -1);
            writer.Write((target != null) ? target.PlayerId : byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRequestVersionCheck(this PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.RequestVersionCheck, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcAddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            manager.AddCustomSettingsChangeMessage(optionItem, value, playSound);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.AddCustomSettingsChangeMessage, SendOption.Reliable, -1);
            writer.Write(optionItem.Id);
            writer.Write(value);
            writer.Write(playSound);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetBaseWarsTeam(this PlayerControl player, BaseWarsTeams team)
        {
            player.SetBaseWarsTeam(team);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetBaseWarsTeam, SendOption.Reliable, -1);
            writer.Write((int)team);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcDestroyTurret(this GameManager manager, SystemTypes room)
        {
            manager.DestroyTurret(room);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.DestroyTurret, SendOption.Reliable, -1);
            writer.Write((byte)room);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetCanTeleport(this PlayerControl player, bool canTeleport)
        {
            player.SetCanTeleport(canTeleport);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetCanTeleport, SendOption.Reliable, -1);
            writer.Write(canTeleport);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetIsDead(this PlayerControl player, bool isDead)
        {
            player.SetIsDead(isDead);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetIsDead, SendOption.Reliable, -1);
            writer.Write(isDead);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetStandardName(this PlayerControl player, string name, int targetClientId = -1)
        {
            if (targetClientId == -1)
                player.SetStandardName(name);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SetStandardName, SendOption.Reliable, targetClientId);
            writer.Write(name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}