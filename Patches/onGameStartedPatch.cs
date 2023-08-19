using HarmonyLib;
using System.Linq;
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
            Main.RealOptions = null;
            Main.RealOptions = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
            Main.AllShapeshifts = new Dictionary<byte, byte>();
            Main.Impostors = new List<byte>();
            Main.LastNotifyNames = new Dictionary<(byte, byte), string>();
            Main.AllPlayersItems = new Dictionary<byte, Items>();
            Main.ShieldTimer = new Dictionary<byte, float>();
            Main.IsMeeting = false;
            RPC.RpcSetHackTimer(0);
            Main.FlashTimer = 0f;
            Main.CamouflageTimer = 0f;
            Main.Lives = new Dictionary<byte, int>();
            Main.AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
            Main.NoBombTimer = 0f;
            Main.NoItemTimer = 0f;
            Main.SkipMeeting = false;
            RPC.RpcSetPaintTime(0);
            Main.VotingPlayerId = 0;
            Main.PaintBattleVotingTime = 0f;
            Main.HasVoted = new Dictionary<byte, bool>();
            Main.PlayerVotes = new Dictionary<byte, (int, int)>();
            Main.Theme = "";
            Main.IsCreatingBody = false;
            Main.CreateBodyCooldown = new Dictionary<byte, float>();
            Main.MessagesToSend = new List<(string, byte, string)>();
            Main.NoItemGive = false;
            Main.Traps = new List<(Vector2, float)>();
            Main.CompassTimer = new Dictionary<byte, float>();
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
                Main.HasVoted[pc.PlayerId] = false;
                Main.PlayerVotes[pc.PlayerId] = (0, 0);
                Main.CreateBodyCooldown[pc.PlayerId] = 0f;
                Main.CompassTimer[pc.PlayerId] = 0f;
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
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
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
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.KillCooldown, 0.001f);
                GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, Options.ExplosionDelay.GetInt() + 0.1f);
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner)
                        pc.SetRole(RoleTypes.Shapeshifter);
                    else
                        pc.SetRole(RoleTypes.Crewmate);
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
                GameOptionsManager.Instance.currentGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner)
                        pc.SetRole(RoleTypes.Impostor);
                    else
                        pc.SetRole(RoleTypes.Crewmate);
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
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (Options.CurrentBodyType == SpeedrunBodyTypes.Engineer)
                        pc.RpcSetRole(RoleTypes.Engineer);
                    else
                        pc.RpcSetRole(RoleTypes.Crewmate);
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.PaintBattle)
            {
                GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.GuardianAngelCooldown, Options.PaintingTime.GetFloat() + 1f);
                GameOptionsManager.Instance.currentGameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
                GameOptionsManager.Instance.currentGameOptions.SetByte(ByteOptionNames.MapId, 0);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcSetRole(RoleTypes.Crewmate);
            }
            GameManager.Instance.LogicOptions.SyncOptions();
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Options.CurrentGamemode == Gamemodes.HideAndSeek)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        pc.RpcSetColor(0);
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
                    }
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BombTag)
            {
                var rand = new System.Random();
                List<PlayerControl> AllPlayers = new();
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
                    }
                    else
                    {
                        pc.RpcSetColor(2);
                        pc.RpcSetName(Utils.ColorString(Color.green, Main.StandardNames[pc.PlayerId]));
                    }
                }
            }
            else if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcSetName(Utils.ColorString(Color.white, Main.StandardNames[pc.PlayerId]));          
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
                pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, -3);
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
                GameManager.Instance.LogicOptions.SyncOptions();
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
                CustomRpcSender sender = CustomRpcSender.Create("SetPetAtStart", SendOption.None);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetPet().Data.IsEmpty)
                    {
                        sender.RpcSetOutfit(pc, petId: "pet_clank");
                        Main.StandardPets[pc.PlayerId] = "pet_clank";
                    }
                    else
                        pc.RpcResetAbilityCooldown();
                }
                sender.SendMessage();
                Main.NoItemTimer = 10f;
            }
            if (Options.CurrentGamemode == Gamemodes.Speedrun)
            {
                if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Command);
                        pc.RpcSetRole(RoleTypes.GuardianAngel);
                    }       
                }
            }
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
            {
                new LateTask(() =>
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcSetDeathReason(DeathReasons.Command);
                        pc.RpcSetRole(RoleTypes.GuardianAngel);
                        pc.RpcTeleport(pc.GetPaintBattleLocation());
                    }
                }, 5f, "Set Dead");
                new LateTask(() =>
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.Data.IsDead = false;
                        pc.RpcResetAbilityCooldown();
                    }
                    Utils.SendGameData();
                }, 6f, "Alive Dead Role");
                RPC.RpcSetPaintTime(Options.PaintingTime.GetInt() + 5);
                var rand = new System.Random();
                RPC.RpcSetTheme(Main.PaintBattleThemes[rand.Next(0, Main.PaintBattleThemes.Count)]);
                Utils.SendChat("Start painting! The theme is " + Main.Theme + "! Remember to evalute less paintings that are not in theme!", "Theme");
                CustomRpcSender sender = CustomRpcSender.Create("SetPetAtSStart", SendOption.None);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetPet().Data.IsEmpty)
                        sender.RpcSetOutfit(pc, petId: "pet_clank");
                }
                sender.SendMessage();
            }
            Utils.SendGameData();
        }
    }
}