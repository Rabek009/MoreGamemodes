using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;
using Hazel;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class CoStartGamePatch
    {
        public static void Prefix(AmongUsClient __instance)
        {
            switch (Options.CurrentGamemode)
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
            }
            if (!__instance.AmHost) return;
            Main.RealOptions = new OptionBackupData(GameOptionsManager.Instance.currentGameOptions);
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.LastNotifyNames = new Dictionary<(byte, byte), string>();
            Main.IsMeeting = false;
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.IsCreatingBody = false;
            Main.MessagesToSend = new List<(string, byte, string)>();
            Main.StandardRoles = new Dictionary<byte, RoleTypes>();
            CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
            Main.ProximityMessages = new Dictionary<byte, List<(string, float)>>();
            Main.NameColors = new Dictionary<(byte, byte), Color>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Main.StandardNames[pc.PlayerId] = pc.Data.PlayerName;
                Main.StandardColors[pc.PlayerId] = pc.Data.DefaultOutfit.ColorId;
                Main.StandardHats[pc.PlayerId] = pc.Data.DefaultOutfit.HatId;
                Main.StandardSkins[pc.PlayerId] = pc.Data.DefaultOutfit.SkinId;
                Main.StandardPets[pc.PlayerId] = pc.Data.DefaultOutfit.PetId;
                Main.StandardVisors[pc.PlayerId] = pc.Data.DefaultOutfit.VisorId;
                Main.StandardNamePlates[pc.PlayerId] = pc.Data.DefaultOutfit.NamePlateId;
                Main.AllShapeshifts[pc.PlayerId] = pc.PlayerId;
                pc.RpcSetDeathReason(DeathReasons.Alive);
                CheckMurderPatch.TimeSinceLastKill[pc.PlayerId] = 0f;
                Main.ProximityMessages[pc.PlayerId] = new List<(string, float)>();
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)] = Main.StandardNames[pc.PlayerId];
                    Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.clear;
                }
            }
            GameManager.Instance.RpcSyncCustomOptions();
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CustomGamemode.Instance.OnSelectRolesPrefix();
            Utils.SyncAllSettings();
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CustomGamemode.Instance.OnSelectRolesPostfix();
            Utils.SyncAllSettings();
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneDestroy
    {
        public static void Postfix()
        {
            Main.GameStarted = true;
            PlayerControl.LocalPlayer.GetComponent<CircleCollider2D>().enabled = true;
            if (!AmongUsClient.Instance.AmHost) return;
            Main.Timer = 0f;
            if (Options.RandomSpawn.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
            CustomGamemode.Instance.OnIntroDestroy();
            if (Options.MidGameChat.GetBool() && Options.CurrentGamemode != Gamemodes.PaintBattle)
                Utils.SetChatVisible();
            Utils.SendGameData();
            if (CustomGamemode.Instance.PetAction)
            {
                CustomRpcSender sender = CustomRpcSender.Create("SetPetAtSStart", SendOption.None);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetPet().Data.IsEmpty)
                    {
                        sender.RpcSetOutfit(pc, petId: "pet_clank");
                        Main.StandardPets[pc.PlayerId] = "pet_clank";
                    }  
                }
                sender.SendMessage();
            }
        }
    }
}