using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class CoStartGamePatch
    {
        public static void Prefix(AmongUsClient __instance)
        {
            if (!__instance.AmHost) return;
            Main.RealOptions = new OptionBackupData(GameOptionsManager.Instance.currentGameOptions);
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.LastNotifyNames = new Dictionary<(byte, byte), string>();
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.MessagesToSend = new List<(string, byte, string)>();
            Main.StandardRoles = new Dictionary<byte, RoleTypes>();
            CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
            CheckProtectPatch.TimeSinceLastProtect = new Dictionary<byte, float>();
            Main.ProximityMessages = new Dictionary<byte, List<(string, float)>>();
            Main.NameColors = new Dictionary<(byte, byte), Color>();
            AntiBlackout.Reset();
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
                CheckMurderPatch.TimeSinceLastKill[pc.PlayerId] = 0f;
                CheckProtectPatch.TimeSinceLastProtect[pc.PlayerId] = 0f;
                Main.ProximityMessages[pc.PlayerId] = new List<(string, float)>();
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
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CustomGamemode.Instance.OnSelectRolesPrefix();
            Utils.SyncAllSettings();
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            new LateTask(() =>{
                CustomGamemode.Instance.OnSelectRolesPostfix();
                Utils.SyncAllSettings();
            }, 0.6f);
            new LateTask(() => ShipStatus.Instance.Begin(), 1.1f);
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
            if (Options.MidGameChat.GetBool() || Options.CurrentGamemode == Gamemodes.Zombies)
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
                    GameData.Instance.RpcSetTasks(pc.PlayerId, new byte[0]);
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient._CoStartGameHost_d__30), nameof(AmongUsClient._CoStartGameHost_d__30.MoveNext))]
    public static class DleksPatch
    {
        private static bool Prefix(AmongUsClient._CoStartGameHost_d__30 __instance, ref bool __result)
        {
            if (__instance.__1__state != 0)
            {
                return true;
            }

            __instance.__1__state = -1;
            if (LobbyBehaviour.Instance)
            {
                LobbyBehaviour.Instance.Despawn();
            }

            if (ShipStatus.Instance)
            {
                __instance.__2__current = null;
                __instance.__1__state = 2;
                __result = true;
                return false;
            }

            var num2 = Mathf.Clamp(GameOptionsManager.Instance.currentGameOptions.MapId, 0, Constants.MapNames.Length - 1);
            __instance.__2__current = __instance.__4__this.ShipLoadingAsyncHandle = __instance.__4__this.ShipPrefabs[num2].InstantiateAsync();
            __instance.__1__state = 1;

            __result = true;
            return false;
        }
    }
}