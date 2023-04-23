using HarmonyLib;
using System.Linq;
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
            Main.RealOptions = null;
            Main.RealOptions = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.Impostors = new List<byte>();
            Main.LastNotifyNames = new Dictionary<(byte, byte), string>();
            Main.AllPlayersItems = new Dictionary<byte, Items>();
            Main.ShieldTimer = new Dictionary<byte, float>();
            Main.IsMeeting = false;
            Main.HackTimer = 0f;
            Main.FlashTimer = 0f;
            Main.CamouflageTimer = 0f;
            Main.Lives = new Dictionary<byte, int>();
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.NoBombTimer = 0f;
            Main.SkipMeeting = false;
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
                pc.RpcSetBomb(false);
                pc.RpcSetItem(Items.None);
                Main.ShieldTimer[pc.PlayerId] = 0f;
                Main.Lives[pc.PlayerId] = Options.Lives.GetInt();
                pc.RpcSetDeathReason(DeathReasons.Alive);
                foreach (var ar in PlayerControl.AllPlayerControls)
                   Main.LastNotifyNames[(pc.PlayerId, ar.PlayerId)] = Main.StandardNames[pc.PlayerId];
            }    
            RPC.RpcSyncCustomOptions();
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
            {
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
            }
            else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 15, 100);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 15, 100);
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.KillCooldown, 0.001f);
                GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, Options.ExplosionDelay.GetInt() + 0.1f);
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner)
                        pc.RpcSetRole(RoleTypes.Shapeshifter);
                    else
                        pc.RpcSetRole(RoleTypes.Crewmate);
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {   
                    if (!pc.AmOwner)
                    {
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, pc.GetClientId());
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (pc != ar)
                                ar.RpcSetDesyncRole(RoleTypes.Crewmate, pc.GetClientId());
                        }
                    }
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner)
                        pc.RpcSetRole(RoleTypes.Impostor);
                    else
                        pc.RpcSetRole(RoleTypes.Crewmate);
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.AmOwner)
                    {
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, pc.GetClientId());
                        foreach (var ar in PlayerControl.AllPlayerControls)
                        {
                            if (pc != ar)
                                ar.RpcSetDesyncRole(RoleTypes.Crewmate, pc.GetClientId());
                        }
                    }
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.Speedrun)
            {
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 15, 100);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Options.CurrentBodyType == SpeedrunBodyTypes.Engineer)
                        pc.RpcSetRole(RoleTypes.Engineer);
                    else
                        pc.RpcSetRole(RoleTypes.Crewmate);
                }
            }
            Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                    {
                        pc.RpcSetColor(0);
                        pc.RpcToggleCanVent(Options.HnSImpostorsCanVent.GetBool());
                    }
                    else
                        pc.RpcSetColor(1);            
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                    {
                        if (Options.ImpostorsAreVisible.GetBool())
                            pc.RpcSetName(Utils.ColorString(Color.red, pc.Data.PlayerName));
                        pc.RpcToggleCanVent(Options.SnSImpostorsCanVent.GetBool());
                    }
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                var rand = new System.Random();
                List<PlayerControl> AllPlayers = new List<PlayerControl>();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.Data.IsDead)
                        AllPlayers.Add(pc);
                }
                var players = AllPlayers.Count();
                var bombs = System.Math.Max(System.Math.Min((players * Options.PlayersWithBomb.GetInt()) / 100, Options.MaxPlayersWithBomb.GetInt()), 1);
                if (bombs == 0)
                    bombs = 1;
                for (int i = 0; i < bombs; ++i)
                {
                    var player = AllPlayers[rand.Next(0, AllPlayers.Count())];
                    player.RpcSetBomb(true);
                    player.RpcSetColor(6);
                    player.RpcSetName(Utils.ColorString(Color.black, Main.StandardNames[player.PlayerId]));
                    AllPlayers.Remove(player);
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.HasBomb())
                    {
                        pc.RpcSetColor(6);
                        pc.RpcSetName(Utils.ColorString(Color.black, Main.StandardNames[pc.PlayerId]));
                        pc.RpcToggleCanUseKillButton(true);
                        pc.RpcToggleCanBeKilled(false);
                    }
                    else
                    {
                        pc.RpcSetColor(2);
                        pc.RpcSetName(Utils.ColorString(Color.green, Main.StandardNames[pc.PlayerId]));
                        pc.RpcToggleCanUseKillButton(false);
                        pc.RpcToggleCanBeKilled(true);
                    }
                    pc.RpcToggleCanVent(false);
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.RpcToggleCanVent(false);
                    pc.RpcSetName(Utils.ColorString(Color.white, Main.StandardNames[pc.PlayerId]));
                }              
            }
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
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.RpcAddImpostor();
                pc.RpcSetRole(RoleTypes.Shapeshifter);
            }
            Main.Timer = 0f;
            if (Options.RandomSpawn.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek || Options.CurrentGamemode == Gamemodes.ShiftAndSeek)
            {
                GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                Utils.SyncSettingsToAll(GameOptionsManager.Instance.currentGameOptions);
            }
            if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcResetAbilityCooldown();
            }
            if (Options.CurrentGamemode == Gamemodes.RandomItems)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.StandardPets[pc.PlayerId] = "pet_clank";
                new LateTask(() =>
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        pc.RpcSetOutfit(petId: "pet_clank");
                }, 0.3f, "Grant Pet For Everyone");
                Main.NoBombTimer = 11f;
            }
            if (Options.CurrentGamemode == Gamemodes.Speedrun)
            {
                if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Command);
                        pc.RpcExileV2();
                    }       
                }
            }
            Utils.SendGameData();             
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    class RpcSetRoleReplacer
    {
        public static bool doReplace = false;
        public static CustomRpcSender sender;
        public static List<(PlayerControl, RoleTypes)> StoragedData = new();
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
        {
            if (doReplace && sender != null)
            {
                StoragedData.Add((__instance, roleType));
                return false;
            }
            else return true;
        }
        public static void Release()
        {
            sender.StartMessage(-1);
            foreach (var pair in StoragedData)
            {
                pair.Item1.SetRole(pair.Item2);
                sender.StartRpc(pair.Item1.NetId, RpcCalls.SetRole)
                    .Write((ushort)pair.Item2)
                    .EndRpc();
            }
            sender.EndMessage();
            doReplace = false;
        }
        public static void StartReplace(CustomRpcSender sender)
        {
            RpcSetRoleReplacer.sender = sender;
            StoragedData = new();
            doReplace = true;
        }
    }
}