using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;
using System;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class CoStartGamePatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            if (!__instance.AmHost)
            {
                if (CustomGamemode.Instance == null)
                {
                    ClassicGamemode.instance = new ClassicGamemode();
                    CustomGamemode.Instance = ClassicGamemode.instance;
                }
                return;
            }
            Main.Timer = 0f;
            Main.RealOptions = new OptionBackupData(GameOptionsManager.Instance.CurrentGameOptions);
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.LastNotifyNames = new Dictionary<(byte, byte), string>();
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.MessagesToSend = new List<(string, byte, string)>();
            Main.StandardRoles = new Dictionary<byte, RoleTypes>();
            Main.DesyncRoles = new Dictionary<(byte, byte), RoleTypes>();
            Main.ProximityMessages = new Dictionary<byte, List<(string, float)>>();
            Main.NameColors = new Dictionary<(byte, byte), Color>();
            Main.RoleFakePlayer = new Dictionary<byte, uint>();
            Main.PlayerKills = new Dictionary<byte, int>();
            Main.KillCooldowns = new Dictionary<byte, float>();
            Main.OptionKillCooldowns = new Dictionary<byte, float>();
            Main.ProtectCooldowns = new Dictionary<byte, float>();
            Main.OptionProtectCooldowns = new Dictionary<byte, float>();
            RpcSetRolePatch.RoleAssigned = new Dictionary<byte, bool>();
            CoEnterVentPatch.PlayersToKick = new List<byte>();
            VentilationSystemDeterioratePatch.LastClosestVent = new Dictionary<byte, int>();
            ExplosionHole.LastSpeedDecrease = new Dictionary<byte, int>();
            PlayerTagManager.ResetPlayerTags();
            Main.StandardNames = new Dictionary<byte, string>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Main.StandardNames[pc.PlayerId] = pc.Data.PlayerName;
                Main.StandardColors[pc.PlayerId] = (byte)pc.Data.DefaultOutfit.ColorId;
                Main.StandardHats[pc.PlayerId] = pc.Data.DefaultOutfit.HatId;
                Main.StandardSkins[pc.PlayerId] = pc.Data.DefaultOutfit.SkinId;
                Main.StandardPets[pc.PlayerId] = pc.Data.DefaultOutfit.PetId;
                Main.StandardVisors[pc.PlayerId] = pc.Data.DefaultOutfit.VisorId;
                Main.StandardNamePlates[pc.PlayerId] = pc.Data.DefaultOutfit.NamePlateId;
                Main.AllShapeshifts[pc.PlayerId] = pc.PlayerId;
                pc.RpcSetDeathReason(DeathReasons.Alive);
                Main.ProximityMessages[pc.PlayerId] = new List<(string, float)>();
                RpcSetRolePatch.RoleAssigned[pc.PlayerId] = false;
                Main.RoleFakePlayer[pc.PlayerId] = pc.NetId;
                Main.PlayerKills[pc.PlayerId] = 0;
                Main.KillCooldowns[pc.PlayerId] = 0f;
                Main.OptionKillCooldowns[pc.PlayerId] = 0f;
                Main.ProtectCooldowns[pc.PlayerId] = 0f;
                Main.OptionProtectCooldowns[pc.PlayerId] = 0f;
                ExplosionHole.LastSpeedDecrease[pc.PlayerId] = 0;
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)] = Main.StandardNames[pc.PlayerId];
                    Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.clear;
                }
            }
            GameManager.Instance.RpcSyncCustomOptions();
            GameManager.Instance.RpcStartGamemode(Options.CurrentGamemode);
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static bool Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            Utils.SyncAllSettings();
            return CustomGamemode.Instance.OnSelectRolesPrefix();
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CustomGamemode.Instance.OnSelectRolesPostfix();
            new LateTask(() => {
                Utils.SyncAllSettings();
            }, 1.5f);
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
            foreach (var pc in PlayerControl.AllPlayerControls)
                Main.KillCooldowns[pc.PlayerId] = Math.Min(10f, Main.OptionKillCooldowns[pc.PlayerId]);
            if (Options.EnableRandomSpawn.GetBool() && !(CustomGamemode.Instance.Gamemode is Gamemodes.PaintBattle or Gamemodes.Jailbreak or Gamemodes.BaseWars))
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
            CustomGamemode.Instance.OnIntroDestroy();
            if (Options.EnableMidGameChat.GetBool() || CustomGamemode.Instance.Gamemode == Gamemodes.PaintBattle)
                Utils.SetChatVisible();
            Utils.SendGameData();
            if (CustomGamemode.Instance.PetAction)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetPet().Data.IsEmpty)
                    {
                        pc.RpcSetPet("pet_clank");
                        Main.StandardPets[pc.PlayerId] = "pet_clank";
                    }  
                }
            }
            if (CustomGamemode.Instance.DisableTasks)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.Data.RpcSetTasks(new byte[0]);
            }
            
            bool shouldPerformVentInteractions = false;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                VentilationSystemDeterioratePatch.LastClosestVent[pc.PlayerId] = pc.GetVentsFromClosest()[0].Id;
                if (VentilationSystemDeterioratePatch.BlockVentInteraction(pc))
                {
                    VentilationSystemDeterioratePatch.LastClosestVent[pc.PlayerId] = pc.GetVentsFromClosest()[0].Id;
                    shouldPerformVentInteractions = true;
                }
            }
            if (shouldPerformVentInteractions)
            {
                Utils.SetAllVentInteractions();
            }
        }
    }
}