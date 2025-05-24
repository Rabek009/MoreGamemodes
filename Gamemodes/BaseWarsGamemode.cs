using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class BaseWarsGamemode : CustomGamemode
    {
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            var room = player.GetPlainShipRoom();
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            if (!IsDead[player.PlayerId])
            {
                __instance.KillButton.OverrideText("Attack");
                __instance.KillButton.ToggleVisible(true);
                if (__instance.KillButton.currentTarget != null && GetTeam(__instance.KillButton.currentTarget) == GetTeam(player))
                    __instance.KillButton.SetTarget(null);
            }
            else
            {
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
            }
            if (room != null && ((room.RoomId == SystemTypes.Reactor && GetTeam(player) == BaseWarsTeams.Blue) || (room.RoomId == SystemTypes.Nav && GetTeam(player) == BaseWarsTeams.Red) ||
                (room.RoomId == SystemTypes.LowerEngine && GetTeam(player) == BaseWarsTeams.Blue && AllTurretsPosition.Contains(SystemTypes.LowerEngine)) ||
                (room.RoomId == SystemTypes.UpperEngine && GetTeam(player) == BaseWarsTeams.Blue && AllTurretsPosition.Contains(SystemTypes.UpperEngine)) ||
                (room.RoomId == SystemTypes.Shields && GetTeam(player) == BaseWarsTeams.Red && AllTurretsPosition.Contains(SystemTypes.Shields)) ||
                (room.RoomId == SystemTypes.Weapons && GetTeam(player) == BaseWarsTeams.Red && AllTurretsPosition.Contains(SystemTypes.Weapons))))
            {
                __instance.AbilityButton.OverrideText("Attack");
                __instance.AbilityButton.ToggleVisible(true);
            }
            else
            {
                __instance.AbilityButton.SetDisabled();
                __instance.AbilityButton.ToggleVisible(false);
            }
            if (Options.CanTeleportToBase.GetBool())
            {
                if (CanTeleport[player.PlayerId])
                    __instance.PetButton.OverrideText("Teleport");
                else
                {
                    __instance.PetButton.SetDisabled();
                    __instance.PetButton.ToggleVisible(false);
                }
            }
            if (IsDead[player.PlayerId])
            {
                __instance.PetButton.SetDisabled();
                __instance.PetButton.ToggleVisible(false);
            }
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            if (GetTeam(PlayerControl.LocalPlayer) == BaseWarsTeams.Red)
                __instance.taskText.text = Utils.ColorString(Color.red, "Red team\nDestroy opponent base and protect your.");
            else
                __instance.taskText.text = Utils.ColorString(Color.blue, "Blue team\nDestroy opponent base and protect your.");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnToggleHighlight(PlayerControl __instance)
        {
            if (GetTeam(PlayerControl.LocalPlayer) == BaseWarsTeams.Red)
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.red);
            else 
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.blue);
        }

        public override Il2CppSystem.Collections.Generic.List<PlayerControl> OnBeginImpostorPrefix(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Team = new();
            Team.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (GetTeam(pc) == GetTeam(PlayerControl.LocalPlayer) && pc != PlayerControl.LocalPlayer)
                    Team.Add(pc);
            }
            return Team;
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            if (GetTeam(PlayerControl.LocalPlayer) == BaseWarsTeams.Red)
            {
                __instance.TeamTitle.text = "Red team";
                __instance.TeamTitle.color = Color.red;
                __instance.BackgroundBar.material.color = Color.red;
            }
            else
            {
                __instance.TeamTitle.text = "Blue team";
                __instance.TeamTitle.color = Color.blue;
                __instance.BackgroundBar.material.color = Color.blue;
            }
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (GetTeam(PlayerControl.LocalPlayer) == BaseWarsTeams.Red)
            {
                __instance.RoleText.text = "Red";
                __instance.RoleText.color = Color.red;
                __instance.RoleBlurbText.text = "Destroy opponent base and protect your.";
                __instance.RoleBlurbText.color = Color.red;
                __instance.YouAreText.color = Color.red;
            }
            else
            {
                __instance.RoleText.text = "Blue";
                __instance.RoleText.color = Color.blue;
                __instance.RoleBlurbText.text = "Destroy opponent base and protect your.";
                __instance.RoleBlurbText.color = Color.blue;
                __instance.YouAreText.color = Color.blue;
            }
        }

        public override bool OnSelectRolesPrefix()
        {
            Utils.RpcSetDesyncRoles(RoleTypes.Shapeshifter, RoleTypes.Crewmate);
            return false;
        }

        public override void OnSelectRolesPostfix()
        {
            var rand = new System.Random();
            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead)
                    AllPlayers.Add(pc);
            }
            int blueTeamSize = AllPlayers.Count / 2;
            int redTeamSize = AllPlayers.Count - blueTeamSize;
            while (AllPlayers.Count > blueTeamSize)
            {
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                SendRPC(player, BaseWarsTeams.Red, IsDead[player.PlayerId], CanTeleport[player.PlayerId]);
                AllPlayers.Remove(player);
            }
            foreach (var player in AllPlayers)
                SendRPC(player, BaseWarsTeams.Blue, IsDead[player.PlayerId], CanTeleport[player.PlayerId]);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (GetTeam(pc) == BaseWarsTeams.Red)
                {
                    pc.RpcSetColor(0);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.red;
                }   
                else
                {
                    pc.RpcSetColor(1);
                    foreach (var ar in PlayerControl.AllPlayerControls)
                        Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.blue;
                    if (blueTeamSize < redTeamSize && Options.SmallerTeamGetsLevel.GetBool())
                        ++PlayerLevel[pc.PlayerId];
                }
            }
        }

        public override void OnIntroDestroy()
        {
            if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0)
            {
                Utils.RpcCreateBase(BaseWarsTeams.Red, new Vector2(-20.5f, -5.5f));
                Utils.RpcCreateBase(BaseWarsTeams.Blue, new Vector2(16.5f, -4.8f));
                Utils.RpcCreateTurret(BaseWarsTeams.Red, SystemTypes.LowerEngine, new Vector2(-17.0f, -13.5f));
                Utils.RpcCreateTurret(BaseWarsTeams.Red, SystemTypes.UpperEngine, new Vector2(-17.0f, -1.3f));
                Utils.RpcCreateTurret(BaseWarsTeams.Blue, SystemTypes.Shields, new Vector2(9.3f, -12.3f));
                Utils.RpcCreateTurret(BaseWarsTeams.Blue, SystemTypes.Weapons, new Vector2(9.3f, 1.0f));
            }
            else if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3)
            {
                Utils.RpcCreateBase(BaseWarsTeams.Red, new Vector2(20.5f, -5.5f));
                Utils.RpcCreateBase(BaseWarsTeams.Blue, new Vector2(-16.5f, -4.8f));
                Utils.RpcCreateTurret(BaseWarsTeams.Red, SystemTypes.LowerEngine, new Vector2(17.0f, -13.5f));
                Utils.RpcCreateTurret(BaseWarsTeams.Red, SystemTypes.UpperEngine, new Vector2(17.0f, -1.3f));
                Utils.RpcCreateTurret(BaseWarsTeams.Blue, SystemTypes.Shields, new Vector2(-9.3f, -12.3f));
                Utils.RpcCreateTurret(BaseWarsTeams.Blue, SystemTypes.Weapons, new Vector2(-9.3f, 1.0f));
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                SpawnPlayer(pc);
                pc.RpcResetAbilityCooldown();
                PlayerHealth[pc.PlayerId] = Options.StartingHealth.GetFloat();
                if (Options.CanTeleportToBase.GetBool())
                    TeleportCooldown[pc.PlayerId] = Options.TeleportCooldown.GetFloat();
            }
            new LateTask(() => Utils.RpcSetUnshiftButton(), 0.5f);
        }

        public override void OnPet(PlayerControl pc)
        {
            if (IsDead[pc.PlayerId]) return;
            if (Options.CanTeleportToBase.GetBool() && CanTeleport[pc.PlayerId])
            {
                SpawnPlayer(pc);
                TeleportCooldown[pc.PlayerId] = Options.TeleportCooldown.GetFloat();
                SendRPC(pc, GetTeam(pc), IsDead[pc.PlayerId], false);
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < 10f || IsDead[killer.PlayerId] || IsDead[target.PlayerId] || GetTeam(killer) == GetTeam(target)) return false;
            Damage(target, Options.StartingDamage.GetFloat() + (Options.DamageIncrease.GetFloat() * GetLevel(killer)), killer);
            killer.RpcSetKillTimer();
            killer.RpcResetAbilityCooldown();
            return false;
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (IsDead[shapeshifter.PlayerId]) return false;
            var room = shapeshifter.GetPlainShipRoom();
            if (IsInTurretRange(shapeshifter))
            {
                foreach (var turret in AllTurrets)
                {
                    if (room != null && turret.Room == room.RoomId)
                        turret.ReceiveDamage(Options.StartingDamage.GetFloat() + (Options.DamageIncrease.GetFloat() * GetLevel(shapeshifter)));
                }
                shapeshifter.RpcSetKillTimer();
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            if (room != null && room.RoomId == SystemTypes.Reactor && GetTeam(shapeshifter) == BaseWarsTeams.Blue)
            {
                foreach (var Base in AllBases)
                {
                    if (Base.Team == BaseWarsTeams.Red)
                        Base.ReceiveDamage(Options.StartingDamage.GetFloat() + (Options.DamageIncrease.GetFloat() * GetLevel(shapeshifter)));
                }
                shapeshifter.RpcSetKillTimer();
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            if (room != null && room.RoomId == SystemTypes.Nav && GetTeam(shapeshifter) == BaseWarsTeams.Red)
            {
                foreach (var Base in AllBases)
                {
                    if (Base.Team == BaseWarsTeams.Blue)
                        Base.ReceiveDamage(Options.StartingDamage.GetFloat() + (Options.DamageIncrease.GetFloat() * GetLevel(shapeshifter)));
                }
                shapeshifter.RpcSetKillTimer();
                shapeshifter.RpcResetAbilityCooldown();
                return false;
            }
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (IsDead[pc.PlayerId])
                {
                    if (RespawnCooldown[pc.PlayerId] > 0f)
                    {
                        RespawnCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (RespawnCooldown[pc.PlayerId] <= 0f)
                    {
                        RespawnCooldown[pc.PlayerId] = 0f;
                        SendRPC(pc, GetTeam(pc), false, CanTeleport[pc.PlayerId]);
                        PlayerHealth[pc.PlayerId] = Options.StartingHealth.GetFloat() + (Options.HealthIncrease.GetFloat() * GetLevel(pc));
                        SpawnPlayer(pc);
                        pc.SyncPlayerSettings();
                    }   
                    continue;
                }
                var room = pc.GetPlainShipRoom();
                if (Options.CanTeleportToBase.GetBool())
                {
                    if (TeleportCooldown[pc.PlayerId] > 0)
                    {
                        TeleportCooldown[pc.PlayerId] -= Time.fixedDeltaTime;
                    }
                    if (TeleportCooldown[pc.PlayerId] < 0f)
                    {
                        TeleportCooldown[pc.PlayerId] = 0f;
                        if (!CanTeleport[pc.PlayerId])
                            SendRPC(pc, GetTeam(pc), IsDead[pc.PlayerId], true);
                    }
                }
                if (Options.TurretSlowEnemies.GetBool())
                {
                    if (IsInTurretRange(pc) && !SlowedByTurret[pc.PlayerId])
                    {
                        SlowedByTurret[pc.PlayerId] = true;
                        pc.SyncPlayerSettings();
                    }
                    else if (!IsInTurretRange(pc) && SlowedByTurret[pc.PlayerId])
                    {
                        SlowedByTurret[pc.PlayerId] = false;
                        pc.SyncPlayerSettings();
                    }
                }
                if (DesyncComms[pc.PlayerId] && !Main.IsModded[pc.PlayerId])
                {
                    if (room == null || !(room.RoomId is SystemTypes.Admin or SystemTypes.Security))
                    {
                        pc.RpcDesyncUpdateSystem(SystemTypes.Comms, 16);
                        DesyncComms[pc.PlayerId] = false;
                    }
                }
                else if (!Main.IsModded[pc.PlayerId])
                {
                    if (room != null && room.RoomId is SystemTypes.Admin or SystemTypes.Security)
                    {
                        pc.RpcDesyncUpdateSystem(SystemTypes.Comms, 128);
                        DesyncComms[pc.PlayerId] = true;
                    }
                }
                TimeSinceLastDamage[pc.PlayerId] += Time.fixedDeltaTime;
            }
            OneSecondTimer += Time.fixedDeltaTime;
            if (OneSecondTimer >= 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    var room = pc.GetPlainShipRoom();
                    if (TimeSinceLastDamage[pc.PlayerId] >= 5f)
                    {
                        if (room != null && ((GetTeam(pc) == BaseWarsTeams.Red && room.RoomId == SystemTypes.Reactor) || (GetTeam(pc) == BaseWarsTeams.Blue && room.RoomId == SystemTypes.Nav)))
                            PlayerHealth[pc.PlayerId] += Options.RegenerationInBase.GetFloat();
                        else
                            PlayerHealth[pc.PlayerId] += Options.Regeneration.GetFloat();
                        if (PlayerHealth[pc.PlayerId] > Options.StartingHealth.GetFloat() + (Options.HealthIncrease.GetFloat() * GetLevel(pc)))
                            PlayerHealth[pc.PlayerId] = Options.StartingHealth.GetFloat() + (Options.HealthIncrease.GetFloat() * GetLevel(pc));
                    }
                    if (room != null && room.RoomId is SystemTypes.Cafeteria or SystemTypes.Storage)
                        AddExp(pc, Options.ExpGainInMiddle.GetInt());
                }
                OneSecondTimer -= 1f;
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            return false;
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Comms)
                return false;
            return true;
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Tracker, 0, 0);
            opt.SetFloat(FloatOptionNames.KillCooldown, 2f);
            opt.SetFloat(FloatOptionNames.ShapeshifterCooldown, 1f);
            opt.SetFloat(FloatOptionNames.ShapeshifterDuration, 0f);
            opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
            opt.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 1f);
            opt.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
            if (SlowedByTurret[player.PlayerId])
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, opt.GetFloat(FloatOptionNames.PlayerSpeedMod) * ((100f - Options.SpeedDecrease.GetInt()) / 100f));
            if (IsDead[player.PlayerId])
            {
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
            }
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            name += Utils.ColorString(Color.red, "\nHealth: " + (int)(PlayerHealth[player.PlayerId] + 0.99f) + "/" + (Options.StartingHealth.GetFloat() + (Options.HealthIncrease.GetFloat() * GetLevel(player))));
            name += Utils.ColorString(Color.yellow, "\nLevel: " + GetLevel(player));
            if (GetLevel(player) < 15 && player == seer)
                name += Utils.ColorString(Color.magenta, " (" + GetExp(player) + "/" + RequiredExp[PlayerLevel[player.PlayerId]] + ")");
            if (Options.CanTeleportToBase.GetBool() && player == seer)
                name += Utils.ColorString(Color.cyan, "\nTeleport cooldown: " + (int)(TeleportCooldown[player.PlayerId] + 0.99f) + "s");
            return name;
        }

        public void SendRPC(PlayerControl player, BaseWarsTeams team, bool isDead, bool canTeleport)
        {
            if (PlayerTeam[player.PlayerId] == team && IsDead[player.PlayerId] == isDead && (!Options.CanTeleportToBase.GetBool() || CanTeleport[player.PlayerId] == canTeleport)) return;
            PlayerTeam[player.PlayerId] = team;
            IsDead[player.PlayerId] = isDead;
            if (Options.CanTeleportToBase.GetBool())
                CanTeleport[player.PlayerId] = canTeleport;
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write((int)team);
            writer.Write(isDead);
            if (Options.CanTeleportToBase.GetBool())
                writer.Write(canTeleport);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(PlayerControl player, MessageReader reader)
        {
            PlayerTeam[player.PlayerId] = (BaseWarsTeams)reader.ReadInt32();
            IsDead[player.PlayerId] = reader.ReadBoolean();
            if (Options.CanTeleportToBase.GetBool())
                CanTeleport[player.PlayerId] = reader.ReadBoolean();
            if (player.AmOwner)
                HudManager.Instance.TaskPanel.SetTaskText("");
        }

        public void SendRPC(GameManager manager, SystemTypes room)
        {
            if (!AllTurretsPosition.Contains(room)) return;
            AllTurretsPosition.Remove(room);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncGamemode, SendOption.Reliable, -1);
            writer.Write((byte)room);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(GameManager manager, MessageReader reader)
        {
            SystemTypes room = (SystemTypes)reader.ReadByte();
            if (AllTurretsPosition.Contains(room))
                AllTurretsPosition.Remove(room);
        }

        public BaseWarsTeams GetTeam(PlayerControl player)
        {
            if (player == null) return BaseWarsTeams.None;
            if (!PlayerTeam.ContainsKey(player.PlayerId)) return BaseWarsTeams.None;
            return PlayerTeam[player.PlayerId];
        }

        public int GetLevel(PlayerControl player)
        {
            if (player == null) return -1;
            if (!PlayerLevel.ContainsKey(player.PlayerId)) return -1;
            return PlayerLevel[player.PlayerId];
        }

        public int GetExp(PlayerControl player)
        {
            if (player == null) return -1;
            if (!PlayerExperience.ContainsKey(player.PlayerId)) return -1;
            return PlayerExperience[player.PlayerId];
        }

        public void Damage(PlayerControl player, float damage, PlayerControl attacker)
        {
            PlayerHealth[player.PlayerId] -= damage;
            TimeSinceLastDamage[player.PlayerId] = 0f;
            if (PlayerHealth[player.PlayerId] <= 0f)
            {
                PlayerHealth[player.PlayerId] = 0f;
                player.RpcTeleport(Utils.GetOutsideMapPosition());
                SendRPC(player, GetTeam(player), true, CanTeleport[player.PlayerId]);
                RespawnCooldown[player.PlayerId] = Options.BwRespawnCooldown.GetFloat();
                player.SyncPlayerSettings();
                if (attacker != null)
                {
                    AddExp(attacker, Options.ExpForKill.GetInt());
                    ++Main.PlayerKills[attacker.PlayerId];
                }   
            }
        }

        public void AddExp(PlayerControl player, int exp)
        {
            PlayerExperience[player.PlayerId] += exp;
            while (GetExp(player) >= RequiredExp[PlayerLevel[player.PlayerId]] && GetLevel(player) < 15)
            {
                PlayerExperience[player.PlayerId] -= RequiredExp[PlayerLevel[player.PlayerId]];
                ++PlayerLevel[player.PlayerId];
                PlayerHealth[player.PlayerId] += Options.HealthIncrease.GetFloat();
            }
        }

        public bool IsInTurretRange(PlayerControl player)
        {
            var room = player.GetPlainShipRoom();
            foreach (var turret in AllTurrets)
            {
                if (turret.Team != GetTeam(player) && room != null && turret.Room == room.RoomId)
                    return true;
            }
            return false;
        }

        public void SpawnPlayer(PlayerControl player)
        {
            if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 0)
            {
                if (GetTeam(player) == BaseWarsTeams.Red)
                    player.RpcTeleport(new Vector2(-20.5f, -5.5f));
                else if (GetTeam(player) == BaseWarsTeams.Blue)
                    player.RpcTeleport(new Vector2(16.5f, -4.8f));
            }
            else if (Main.RealOptions.GetByte(ByteOptionNames.MapId) == 3)
            {
                if (GetTeam(player) == BaseWarsTeams.Red)
                    player.RpcTeleport(new Vector2(20.5f, -5.5f));
                else if (GetTeam(player) == BaseWarsTeams.Blue)
                    player.RpcTeleport(new Vector2(-16.5f, -4.8f));
            }
        }

        public BaseWarsGamemode()
        {
            Gamemode = Gamemodes.BaseWars;
            PetAction = Options.CanTeleportToBase.GetBool();
            DisableTasks = true;
            PlayerHealth = new Dictionary<byte, float>();
            PlayerTeam = new Dictionary<byte, BaseWarsTeams>();
            RespawnCooldown = new Dictionary<byte, float>();
            TeleportCooldown = new Dictionary<byte, float>();
            CanTeleport = new Dictionary<byte, bool>();
            AllTurrets = new List<Turret>();
            AllTurretsPosition = new List<SystemTypes>() {SystemTypes.LowerEngine, SystemTypes.UpperEngine, SystemTypes.Shields, SystemTypes.Weapons};
            AllBases = new List<Base>();
            TimeSinceLastDamage = new Dictionary<byte, float>();
            PlayerLevel = new Dictionary<byte, int>();
            PlayerExperience = new Dictionary<byte, int>();
            IsDead = new Dictionary<byte, bool>();
            SlowedByTurret = new Dictionary<byte, bool>();
            OneSecondTimer = 0f;
            DesyncComms = new Dictionary<byte, bool>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                PlayerHealth[pc.PlayerId] = 0f;
                PlayerTeam[pc.PlayerId] = BaseWarsTeams.None;
                RespawnCooldown[pc.PlayerId] = 0f;
                TeleportCooldown[pc.PlayerId] = 0f;
                CanTeleport[pc.PlayerId] = false;
                TimeSinceLastDamage[pc.PlayerId] = 0f;
                PlayerLevel[pc.PlayerId] = 0;
                PlayerExperience[pc.PlayerId] = 0;
                IsDead[pc.PlayerId] = false;
                SlowedByTurret[pc.PlayerId] = false;
                DesyncComms[pc.PlayerId] = false;
            }
        }

        public static BaseWarsGamemode instance;
        public Dictionary<byte, float> PlayerHealth;
        public Dictionary<byte, BaseWarsTeams> PlayerTeam;
        public Dictionary<byte, float> RespawnCooldown;
        public Dictionary<byte, float> TeleportCooldown;
        public Dictionary<byte, bool> CanTeleport;
        public List<Turret> AllTurrets;
        public List<SystemTypes> AllTurretsPosition;
        public List<Base> AllBases;
        public Dictionary<byte, float> TimeSinceLastDamage;
        public Dictionary<byte, int> PlayerLevel;
        public Dictionary<byte, int> PlayerExperience;
        public Dictionary<byte, bool> IsDead;
        public Dictionary<byte, bool> SlowedByTurret;
        public float OneSecondTimer;
        public Dictionary<byte, bool> DesyncComms;

        public static List<int> RequiredExp = new() {
            50,
            100,
            150,
            200,
            250,
            300,
            350,
            425,
            500,
            575,
            650,
            725,
            800,
            900,
            1000,
            999999999,
        };
    }

    public enum BaseWarsTeams
    {
        None,
        Red,
        Blue,
    }
}