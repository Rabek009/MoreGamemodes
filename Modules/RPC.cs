﻿using HarmonyLib;
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
        SetStandardName,
        SetFrozen,
        SendNoisemakerAlert,
        SetColorWarsTeam,
        SetCustomRole,
        SetRoleblock,
        SyncCustomWinner,
        SetAbilityUses,
        BlockVent,
        SetPetAbilityCooldown,
        GuessPlayer,
        SetAddOn,
        MarkEscapistPosition,
        SetHitmanTarget,
        UseJudgeAbility,
        SetShamanTarget,
        SetDronerRealPosition,
        SetRomanticLover,
        SetArsonistDouseState,
        SetSoulCollectorTarget,
        MakeInvisible,
        MakeVisible,
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
                    if (text[0] == '/' && Main.IsModded[__instance.PlayerId]) return false;
                    break;
                case RpcCalls.UsePlatform:
                    if (Options.DisableGapPlatform.GetBool()) return false;
                    if (!CustomGamemode.Instance.OnUsePlatform(__instance)) return false;
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
                        bool doSend = false;
                        CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                        sender.StartMessage(__instance.GetClientId());
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!Main.StandardNames.ContainsKey(pc.PlayerId)) continue;
                            sender.StartRpc(pc.NetId, (byte)CustomRPC.SetCustomRole)
                                .Write(Main.StandardNames[pc.PlayerId])
                                .EndRpc();
                            doSend = true;
                        }
                        sender.EndMessage();
                        sender.SendMessage(doSend);
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
                case CustomRPC.SetFrozen:
                    __instance.SetFrozen(reader.ReadBoolean());
                    break;
                case CustomRPC.SendNoisemakerAlert:
                    __instance.SendNoisemakerAlert();
                    break;
                case CustomRPC.SetColorWarsTeam:
                    __instance.SetColorWarsTeam(reader.ReadByte(), reader.ReadBoolean());
                    break;
                case CustomRPC.SetCustomRole:
                    __instance.SetCustomRole((CustomRoles)reader.ReadInt32());
                    break;
                case CustomRPC.SetRoleblock:
                    __instance.SetRoleblock(reader.ReadBoolean());
                    break;
                case CustomRPC.SetAbilityUses:
                    __instance.SetAbilityUses(reader.ReadSingle());
                    break;
                case CustomRPC.SetPetAbilityCooldown:
                    __instance.SetPetAbilityCooldown(reader.ReadBoolean());
                    break;
                case CustomRPC.GuessPlayer:
                    __instance.GuessPlayer();
                    break;
                case CustomRPC.SetAddOn:
                    __instance.SetAddOn((AddOns)reader.ReadInt32());
                    break;
                case CustomRPC.MarkEscapistPosition:
                    __instance.MarkEscapistPosition(reader.ReadBoolean() ? NetHelpers.ReadVector2(reader) : null);
                    break;
                case CustomRPC.SetHitmanTarget:
                    __instance.SetHitmanTarget(reader.ReadByte());
                    break;
                case CustomRPC.UseJudgeAbility:
                    __instance.UseJudgeAbility();
                    break;
                case CustomRPC.SetShamanTarget:
                    __instance.SetShamanTarget(reader.ReadByte());
                    break;
                case CustomRPC.SetDronerRealPosition:
                    __instance.SetDronerRealPosition(reader.ReadBoolean() ? NetHelpers.ReadVector2(reader) : null);
                    break;
                case CustomRPC.SetRomanticLover:
                    __instance.SetRomanticLover(reader.ReadByte());
                    break;
                case CustomRPC.SetArsonistDouseState:
                    __instance.SetArsonistDouseState(reader.ReadNetObject<PlayerControl>(), (DouseStates)reader.ReadInt32());
                    break;
                case CustomRPC.SetSoulCollectorTarget:
                    __instance.SetSoulCollectorTarget(reader.ReadByte());
                    break;
                case CustomRPC.MakeInvisible:
                    __instance.MakeInvisible();
                    break;
                case CustomRPC.MakeVisible:
                    __instance.MakeVisible();
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
                    var viewSettingsPane = Object.FindObjectOfType<LobbyViewSettingsPane>();
                    if (viewSettingsPane != null)
                    {
                        if (viewSettingsPane.currentTab != StringNames.OverviewCategory && viewSettingsPane.currentTab != StringNames.RolesCategory)
                            viewSettingsPane.RefreshTab();
                        LobbyViewPatch.ReCreateButtons(viewSettingsPane);
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
                case CustomRPC.SyncCustomWinner:
                    if (ClassicGamemode.instance == null) break;
                    ClassicGamemode.instance.Winner = (CustomWinners)reader.ReadInt32();
                    ClassicGamemode.instance.AdditionalWinners.Clear();
                    int num = reader.ReadInt32();
                    for (int i = 0; i < num; ++i)
                        ClassicGamemode.instance.AdditionalWinners.Add((AdditionalWinners)reader.ReadInt32());
                    break;
                case CustomRPC.BlockVent:
                    __instance.BlockVent(reader.ReadInt32());
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
                    if (CoEnterVentPatch.PlayersToKick.Contains(__instance.myPlayer.PlayerId)) return false;
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
            UnmoddedGamemode.instance = null;
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
            BaseWarsGamemode.instance = null;
            FreezeTagGamemode.instance = null;
            ColorWarsGamemode.instance = null;
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
                case Gamemodes.FreezeTag:
                    FreezeTagGamemode.instance = new FreezeTagGamemode();
                    CustomGamemode.Instance = FreezeTagGamemode.instance;
                    break;
                case Gamemodes.ColorWars:
                    ColorWarsGamemode.instance = new ColorWarsGamemode();
                    CustomGamemode.Instance = ColorWarsGamemode.instance;
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
                case Gamemodes.ColorWars:
                    ColorWarsGamemode.instance.IsDead[player.PlayerId] = isDead;
                    break;
            }
        }

        public static void SetStandardName(this PlayerControl player, string name)
        {
            Main.StandardNames[player.PlayerId] = name;
        }

        public static void SetFrozen(this PlayerControl player, bool frozen)
        {
            if (FreezeTagGamemode.instance == null) return;
            FreezeTagGamemode.instance.PlayerIsFrozen[player.PlayerId] = frozen;
        }

        public static void SendNoisemakerAlert(this PlayerControl player)
        {
            if (player.Data.Role.Role != RoleTypes.Noisemaker) return;
            player.Data.Role.OnDeath(DeathReason.Kill);
        }

        public static void SetColorWarsTeam(this PlayerControl player, byte team, bool isLeader)
        {
            if (ColorWarsGamemode.instance == null) return;
            ColorWarsGamemode.instance.Team[player.PlayerId] = team;
            ColorWarsGamemode.instance.PlayerIsLeader[player.PlayerId] = isLeader;
        }

        public static void SetRoleblock(this PlayerControl player, bool roleblock)
        {
            if (ClassicGamemode.instance == null) return;
            ClassicGamemode.instance.IsRoleblocked[player.PlayerId] = roleblock;
            if (roleblock && Minigame.Instance)
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
            if (AmongUsClient.Instance.AmHost && !player.AmOwner && !Main.IsModded[player.PlayerId])
            {
                if (roleblock)
                    player.RpcDesyncUpdateSystem(SystemTypes.Comms, 128);
                else if (!Utils.IsActive(SystemTypes.Comms))
                {
                    player.RpcDesyncUpdateSystem(SystemTypes.Comms, 16);
                    if (Main.RealOptions.GetByte(ByteOptionNames.MapId) is 1 or 5)
                        player.RpcDesyncUpdateSystem(SystemTypes.Comms, 17);
                }
            }
                
        }

        public static void SetAbilityUses(this PlayerControl player, float uses)
        {
            if (ClassicGamemode.instance == null) return;
            player.GetRole().AbilityUses = uses;
        }

        public static void BlockVent(this GameManager manager, int ventId)
        {
            if (ClassicGamemode.instance == null) return;
            ClassicGamemode.instance.BlockedVents.Add(ventId);
            var ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
            if (ventilationSystem != null)
                ventilationSystem.UpdateVentArrows();
        }

        public static void SetPetAbilityCooldown(this PlayerControl player, bool onCooldown)
        {
            if (ClassicGamemode.instance == null) return;
            ClassicGamemode.instance.IsOnPetAbilityCooldown[player.PlayerId] = onCooldown;
        }

        public static void GuessPlayer(this PlayerControl player)
        {
            SoundManager.Instance.PlaySound(player.KillSfx, false, 0.8f);
            HudManager.Instance.KillOverlay.ShowKillAnimation(player.Data, player.Data);
        }

        public static void MarkEscapistPosition(this PlayerControl player, Vector2? position)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Escapist) return;
            Escapist escapistRole = player.GetRole() as Escapist;
            if (escapistRole == null) return;
            escapistRole.MarkedPosition = position;
        }

        public static void SetHitmanTarget(this PlayerControl player, byte targetId)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Hitman) return;
            Hitman hitmanRole = player.GetRole() as Hitman;
            if (hitmanRole == null) return;
            hitmanRole.Target = targetId;
        }

        public static void UseJudgeAbility(this PlayerControl player)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Judge) return;
            Judge judgeRole = player.GetRole() as Judge;
            if (judgeRole == null) return;
            judgeRole.AbilityUsed = true;
        }

        public static void SetShamanTarget(this PlayerControl player, byte targetId)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Shaman) return;
            Shaman shamanRole = player.GetRole() as Shaman;
            if (shamanRole == null) return;
            shamanRole.Target = targetId;
            if (player.AmOwner && MeetingHud.Instance != null)
                Shaman.CreateMeetingButton(MeetingHud.Instance);
        }

        public static void SetDronerRealPosition(this PlayerControl player, Vector2? position)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Droner) return;
            Droner dronerRole = player.GetRole() as Droner;
            if (dronerRole == null) return;
            dronerRole.RealPosition = position;
            if (player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
        }

        public static void SetRomanticLover(this PlayerControl player, byte targetId)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Romantic) return;
            Romantic romanticRole = player.GetRole() as Romantic;
            if (romanticRole == null) return;
            romanticRole.LoverId = targetId;
            if (player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
        }

        public static void SetArsonistDouseState(this PlayerControl player, PlayerControl target, DouseStates douseState)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.Arsonist) return;
            Arsonist arsonistRole = player.GetRole() as Arsonist;
            if (arsonistRole == null) return;
            arsonistRole.DouseState[target.PlayerId] = douseState;
        }

        public static void SetSoulCollectorTarget(this PlayerControl player, byte targetId)
        {
            if (ClassicGamemode.instance == null || player.GetRole().Role != CustomRoles.SoulCollector) return;
            SoulCollector soulCollectorRole = player.GetRole() as SoulCollector;
            if (soulCollectorRole == null) return;
            soulCollectorRole.TargetId = targetId;
            if (player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
        }

        public static void MakeInvisible(this PlayerControl player)
        {
            if (player.AmOwner)
                player.invisibilityAlpha = 0.5f;
            else
                player.invisibilityAlpha = PlayerControl.LocalPlayer.Data.Role.IsDead ? 0.5f : 0f;
            player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
            if (!player.AmOwner && !PlayerControl.LocalPlayer.Data.Role.IsDead)
            {
                player.shouldAppearInvisible = true;
                player.Visible = false;
            }
        }

        public static void MakeVisible(this PlayerControl player)
        {
            if (!player.AmOwner)
            {
                player.shouldAppearInvisible = false;
			    player.Visible = true;
            }
            player.invisibilityAlpha = 1f;
            player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
            if (!player.AmOwner)
            {
                player.shouldAppearInvisible = false;
                player.Visible = !player.inVent;
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
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable);
            foreach (var co in OptionItem.AllOptions)
            {
                if (co.Id == 0 || co is TextOptionItem) continue;
                if (co.Id >= 1000 && co.IsHiddenOn(Options.CurrentGamemode)) continue;
                writer.Write(co.CurrentValue);
            }
            writer.EndMessage();
        }

        public static void RpcSetBomb(this PlayerControl player, bool hasBomb)
        {
            player.SetBomb(hasBomb);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetBomb, SendOption.Reliable);
            writer.Write(hasBomb);
            writer.EndMessage();
        }

        public static void RpcSetItem(this PlayerControl player, Items item)
        {
            player.SetItem(item);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetItem, SendOption.Reliable);
            writer.Write((int)item);
            writer.EndMessage();
        }

        public static void RpcSetHackActive(this GameManager manager, bool active)
        {
            manager.SetHackActive(active);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.SetHackActive, SendOption.Reliable);
            writer.Write(active);
            writer.EndMessage();
        }

        public static void RpcSetPaintActive(this GameManager manager, bool active)
        {
            manager.SetPaintActive(active);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.SetPaintActive, SendOption.Reliable);
            writer.Write(active);
            writer.EndMessage();
        }

        public static void RpcSetTheme(this GameManager manager, string theme)
        {
            manager.SetTheme(theme);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.SetTheme, SendOption.Reliable);
            writer.Write(theme);
            writer.EndMessage();
        }

        public static void RpcSetIsKiller(this PlayerControl player, bool isKiller)
        {
            player.SetIsKiller(isKiller);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetIsKiller, SendOption.Reliable);
            writer.Write(isKiller);
            writer.EndMessage();
        }

        public static void RpcSetZombieType(this PlayerControl player, ZombieTypes zombieType)
        {
            player.SetZombieType(zombieType);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetZombieType, SendOption.Reliable);
            writer.Write((int)zombieType);
            writer.EndMessage();
        }

        public static void RpcSetKillsRemain(this PlayerControl player, int killsRemain)
        {
            player.SetKillsRemain(killsRemain);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetKillsRemain, SendOption.Reliable);
            writer.Write(killsRemain);
            writer.EndMessage();
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
                MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.ReactorFlash, SendOption.Reliable);
                writer.Write(duration);
                writer.WriteColor(color);
                writer.EndMessage();
                return;
            }
            player.RpcUnmoddedReactorFlash(duration);
        }

        public static void RpcSetJailbreakPlayerType(this PlayerControl player, JailbreakPlayerTypes playerType)
        {
            player.SetJailbreakPlayerType(playerType);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetJailbreakPlayerType, SendOption.Reliable);
            writer.Write((int)playerType);
            writer.EndMessage();
        }

        public static void RpcSetItemAmount(this PlayerControl player, InventoryItems item, int amount)
        {
            player.SetItemAmount(item, amount);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetItemAmount, SendOption.Reliable);
            writer.Write((int)item);
            writer.Write(amount);
            writer.EndMessage();
        }

        public static void RpcSetCurrentRecipe(this PlayerControl player, int recipeId)
        {
            player.SetCurrentRecipe(recipeId);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetCurrentRecipe, SendOption.Reliable);
            writer.Write(recipeId);
            writer.EndMessage();
        }

        public static void RpcSetKillTimer(this PlayerControl player, float time = float.MaxValue)
        {
            Main.KillCooldowns[player.PlayerId] = time != float.MaxValue ? time : Main.OptionKillCooldowns[player.PlayerId] / 2f;
            if (player.AmOwner)
            {
                player.SetKillTimer(time != float.MaxValue ? time : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                return;
            }
            if (Main.IsModded[player.PlayerId])
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetKillTimer, SendOption.Reliable);
                writer.Write(time != float.MaxValue ? time : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                writer.EndMessage();
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
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.StartGamemode, SendOption.Reliable);
            writer.Write((int)gamemode);
            writer.EndMessage();
        }

        public static void RpcRemoveDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
        {
            player.RemoveDeadBody(target);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.RemoveDeadBody, SendOption.Reliable);
            writer.Write((target != null) ? target.PlayerId : byte.MaxValue);
            writer.EndMessage();
        }

        public static void RpcRequestVersionCheck(this PlayerControl player)
        {
            AmongUsClient.Instance.SendRpc(player.NetId, (byte)CustomRPC.RequestVersionCheck, SendOption.Reliable);
        }

        public static void RpcAddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            manager.AddCustomSettingsChangeMessage(optionItem, value, playSound);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.AddCustomSettingsChangeMessage, SendOption.Reliable);
            writer.Write(optionItem.Id);
            writer.Write(value);
            writer.Write(playSound);
            writer.EndMessage();
        }

        public static void RpcSetBaseWarsTeam(this PlayerControl player, BaseWarsTeams team)
        {
            player.SetBaseWarsTeam(team);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetBaseWarsTeam, SendOption.Reliable);
            writer.Write((int)team);
            writer.EndMessage();
        }

        public static void RpcDestroyTurret(this GameManager manager, SystemTypes room)
        {
            manager.DestroyTurret(room);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.DestroyTurret, SendOption.Reliable);
            writer.Write((byte)room);
            writer.EndMessage();
        }

        public static void RpcSetCanTeleport(this PlayerControl player, bool canTeleport)
        {
            player.SetCanTeleport(canTeleport);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetCanTeleport, SendOption.Reliable);
            writer.Write(canTeleport);
            writer.EndMessage();
        }

        public static void RpcSetIsDead(this PlayerControl player, bool isDead)
        {
            player.SetIsDead(isDead);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetIsDead, SendOption.Reliable);
            writer.Write(isDead);
            writer.EndMessage();
        }

        public static void RpcSetStandardName(this PlayerControl player, string name)
        {
            player.SetStandardName(name);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetStandardName, SendOption.Reliable);
            writer.Write(name);
            writer.EndMessage();
        }

        public static void RpcSetFrozen(this PlayerControl player, bool frozen)
        {
            player.SetFrozen(frozen);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetFrozen, SendOption.Reliable);
            writer.Write(frozen);
            writer.EndMessage();
        }

        public static void RpcSendNoisemakerAlert(this PlayerControl player)
        {
            player.SendNoisemakerAlert();
            AmongUsClient.Instance.SendRpc(player.NetId, (byte)CustomRPC.SendNoisemakerAlert, SendOption.Reliable);
        }

        public static void RpcSetColorWarsTeam(this PlayerControl player, byte team, bool isLeader)
        {
            player.SetColorWarsTeam(team, isLeader);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetColorWarsTeam, SendOption.Reliable);
            writer.Write(team);
            writer.Write(isLeader);
            writer.EndMessage();
        }

        public static void RpcSetCustomRole(this PlayerControl player, CustomRoles role)
        {
            player.SetCustomRole(role);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetCustomRole, SendOption.Reliable);
            writer.Write((int)role);
            writer.EndMessage();
        }

        public static void RpcSetRoleblock(this PlayerControl player, bool roleblock)
        {
            player.SetRoleblock(roleblock);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetRoleblock, SendOption.Reliable);
            writer.Write(roleblock);
            writer.EndMessage();
        }

        public static void RpcSetAbilityUses(this PlayerControl player, float uses)
        {
            player.SetAbilityUses(uses);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetAbilityUses, SendOption.Reliable);
            writer.Write(uses);
            writer.EndMessage();
        }

        public static void RpcSyncCustomWinner(this GameManager manager)
        {
            if (ClassicGamemode.instance == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.SyncCustomWinner, SendOption.Reliable);
            writer.Write((int)ClassicGamemode.instance.Winner);
            writer.Write(ClassicGamemode.instance.AdditionalWinners.Count);
            for (int i = 0; i < ClassicGamemode.instance.AdditionalWinners.Count; ++i)
                writer.Write((int)ClassicGamemode.instance.AdditionalWinners[i]);
            writer.EndMessage();
        }

        public static void RpcBlockVent(this GameManager manager, int ventId)
        {
            manager.BlockVent(ventId);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(manager.NetId, (byte)CustomRPC.BlockVent, SendOption.Reliable);
            writer.Write(ventId);
            writer.EndMessage();
        }

        public static void RpcSetPetAbilityCooldown(this PlayerControl player, bool onCooldown)
        {
            player.SetPetAbilityCooldown(onCooldown);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetPetAbilityCooldown, SendOption.Reliable);
            writer.Write(onCooldown);
            writer.EndMessage();
        }

        public static void RpcGuessPlayer(this PlayerControl player)
        {
            player.GuessPlayer();
            AmongUsClient.Instance.SendRpc(player.NetId, (byte)CustomRPC.GuessPlayer, SendOption.Reliable);
        }

        public static void RpcSetAddOn(this PlayerControl player, AddOns addOn)
        {
            player.SetAddOn(addOn);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetAddOn, SendOption.Reliable);
            writer.Write((int)addOn);
            writer.EndMessage();
        }

        public static void RpcMarkEscapistPosition(this PlayerControl player, Vector2? position)
        {
            player.MarkEscapistPosition(position);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.MarkEscapistPosition, SendOption.Reliable);
            writer.Write(position != null);
            if (position != null)
                NetHelpers.WriteVector2((Vector2)position, writer);
            writer.EndMessage();
        }

        public static void RpcSetHitmanTarget(this PlayerControl player, byte targetId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetHitmanTarget, SendOption.Reliable);
            writer.Write(targetId);
            writer.EndMessage();
        }

        public static void RpcUseJudgeAbility(this PlayerControl player)
        {
            player.UseJudgeAbility();
            AmongUsClient.Instance.SendRpc(player.NetId, (byte)CustomRPC.UseJudgeAbility, SendOption.Reliable);
        }

        public static void RpcSetShamanTarget(this PlayerControl player, byte targetId)
        {
            if (player.AmOwner && MeetingHud.Instance != null)
                Shaman.CreateMeetingButton(MeetingHud.Instance);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetShamanTarget, SendOption.Reliable);
            writer.Write(targetId);
            writer.EndMessage();
        }

        public static void RpcSetDronerRealPosition(this PlayerControl player, Vector2? position)
        {
            player.SetDronerRealPosition(position);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetDronerRealPosition, SendOption.Reliable);
            writer.Write(position != null);
            if (position != null)
                NetHelpers.WriteVector2((Vector2)position, writer);
            writer.EndMessage();
        }

        public static void RpcSetRomanticLover(this PlayerControl player, byte targetId)
        {
            if (player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetRomanticLover, SendOption.Reliable);
            writer.Write(targetId);
            writer.EndMessage();
        }

        public static void RpcSetArsonistDouseState(this PlayerControl player, PlayerControl target, DouseStates douseState)
        {
            player.SetArsonistDouseState(target, douseState);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetArsonistDouseState, SendOption.Reliable);
            writer.WriteNetObject(target);
            writer.Write((int)douseState);
            writer.EndMessage();
        }

        public static void RpcSetSoulCollectorTarget(this PlayerControl player, byte targetId)
        {
            if (player.AmOwner)
                HudManager.Instance.SetHudActive(!MeetingHud.Instance);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(player.NetId, (byte)CustomRPC.SetSoulCollectorTarget, SendOption.Reliable);
            writer.Write(targetId);
            writer.EndMessage();
        }

        public static void RpcMakeInvisibleModded(this PlayerControl player)
        {
            player.MakeInvisible();
            AmongUsClient.Instance.SendRpc(player.NetId, (byte)CustomRPC.MakeInvisible, SendOption.Reliable);
        }

        public static void RpcMakeVisibleModded(this PlayerControl player)
        {
            player.MakeVisible();
            AmongUsClient.Instance.SendRpc(player.NetId, (byte)CustomRPC.MakeVisible, SendOption.Reliable);
        }
    }
}