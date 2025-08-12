using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MoreGamemodes
{
    public class Oracle : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = ConfessCooldown.GetFloat();
            if (exiled == null || !UnselectDeadPlayers.GetBool()) return;
            if (SelectedPlayers.Contains(exiled.PlayerId))
                SelectedPlayers.Remove(exiled.PlayerId);
        }

        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.KillButton.OverrideText("Confess");
            if (AbilityUses < 1f)
                __instance.KillButton.SetDisabled();
        }

        public override void OnIntroDestroy()
        {
            Cooldown = 10f;
        }

        public override void OnPet()
        {
            if (Main.IsModded[Player.PlayerId]) return;
            if (BaseRole == BaseRoles.Crewmate)
            {
                BaseRole = BaseRoles.DesyncImpostor;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor or BaseRoles.Shapeshifter or BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                }
                Player.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.white;
                Player.RpcSetKillTimer(Math.Max(Cooldown, 0.001f));
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                Player.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.clear;
            }
        }

        public override bool OnCheckMurder(PlayerControl target)
        {
            if (!Main.IsModded[Player.PlayerId] && Cooldown > 0f) return false;
            if (AbilityUses < 1f || SelectedPlayers.Contains(target.PlayerId)) return false;
            SelectedPlayers.Add(target.PlayerId);
            if (SelectedPlayers.Count >= NumberOfSelectedPlayers.GetInt())
            {
                List<byte> ConfessPlayers = new();
                int maxEvilness = 0;
                foreach (var playerId in SelectedPlayers)
                {
                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(playerId);
                    if (GetEvilness(playerInfo) > maxEvilness)
                    {
                        ConfessPlayers.Clear();
                        ConfessPlayers.Add(playerId);
                        maxEvilness = GetEvilness(playerInfo);
                    }
                    else if (GetEvilness(playerInfo) == maxEvilness)
                        ConfessPlayers.Add(playerId);
                }
                var rand = new System.Random();
                byte confessId = ConfessPlayers[rand.Next(0, ConfessPlayers.Count)];
                string message = "You selected " + Main.StandardNames[SelectedPlayers[0]];
                for (int i = 1; i < SelectedPlayers.Count; ++i)
                    message += ", " + Main.StandardNames[SelectedPlayers[i]];
                message += "!\n" + Main.StandardNames[confessId] + " is the most evil!";
                Player.Notify(message);
                Main.NameColors[(confessId, Player.PlayerId)] = Palette.ImpostorRed;
                SelectedPlayers.Clear();
                Player.RpcSetAbilityUses(AbilityUses - 1f);
                Player.RpcSetKillTimer(ConfessCooldown.GetFloat());
                Cooldown = ConfessCooldown.GetFloat();
            }
            return false;
        }

        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.clear;
            }
        }

        public override void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (!UnselectDeadPlayers.GetBool()) return;
            if (SelectedPlayers.Contains(target.PlayerId))
                SelectedPlayers.Remove(target.PlayerId);
        }

        public override void OnMeeting()
        {
            if (BaseRole == BaseRoles.DesyncImpostor)
            {
                BaseRole = BaseRoles.Crewmate;
                Player.RpcSetDesyncRole(RoleTypes.Crewmate, Player);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.GetRole().BaseRole is BaseRoles.Impostor && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Impostor, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Shapeshifter && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Shapeshifter, Player);
                    else if (pc.GetRole().BaseRole is BaseRoles.Phantom && !pc.Data.IsDead)
                        pc.RpcSetDesyncRole(RoleTypes.Phantom, Player);
                }
                Player.SyncPlayerSettings();
                Main.NameColors[(Player.PlayerId, Player.PlayerId)] = Color.clear;
            }
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            if (Cooldown > 0f)
                Cooldown -= Time.fixedDeltaTime;
            if (Cooldown < 0f)
                Cooldown = 0f;
            if (!SelectedPlayers.Any()) return;
            if (UnselectDeadPlayers.GetBool())
            {
                for (int i = SelectedPlayers.Count - 1; i >= 0; --i)
                {
                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(SelectedPlayers[i]);
                    if (playerInfo.IsDead || playerInfo.Disconnected)
                        SelectedPlayers.RemoveAt(i);
                }
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == Player || pc.Data.IsDead)
                {
                    foreach (var playerId in SelectedPlayers)
                        ClassicGamemode.instance.NameSymbols[(playerId, pc.PlayerId)][CustomRoles.Oracle] = ("◊", Color);
                }
            }
        }

        public override bool OnEnterVent(int id)
        {
            return false;
        }

        public override void OnCompleteTask()
        {
            if (AbilityUseGainWithEachTaskCompleted.GetFloat() <= 0f) return;
            Player.RpcSetAbilityUses(AbilityUses + AbilityUseGainWithEachTaskCompleted.GetFloat());
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.KillCooldown, ConfessCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (Main.IsModded[Player.PlayerId])
            {
                string selectedText = "";
                if (SelectedPlayers.Any())
                {
                    selectedText += Main.StandardNames[SelectedPlayers[0]];
                    for (int i = 1; i < SelectedPlayers.Count; ++i)
                        selectedText += ", " + Main.StandardNames[SelectedPlayers[i]];
                }
                else
                    selectedText += "<b>None</b>";
                return Utils.ColorString(Color, "\n<size=1.8>Selected: " + selectedText + "</size>");
            } 
            if (BaseRole == BaseRoles.Crewmate)
            {
                string selectedText = "";
                if (SelectedPlayers.Any())
                {
                    selectedText += Main.StandardNames[SelectedPlayers[0]];
                    for (int i = 1; i < SelectedPlayers.Count; ++i)
                        selectedText += ", " + Main.StandardNames[SelectedPlayers[i]];
                }
                else
                    selectedText += "<b>None</b>";
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Task\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")\n</size>") +
                    Utils.ColorString(Color.red, "<size=1.8>Confess cooldown: " + (int)(Cooldown + 0.99f) + "s\n") +
                    Utils.ColorString(Color, "Selected: " + selectedText + "</size>");
            }
            else if (BaseRole == BaseRoles.DesyncImpostor)
            {
                string selectedText = "";
                if (SelectedPlayers.Any())
                {
                    selectedText += Main.StandardNames[SelectedPlayers[0]];
                    for (int i = 1; i < SelectedPlayers.Count; ++i)
                        selectedText += ", " + Main.StandardNames[SelectedPlayers[i]];
                }
                else
                    selectedText += "<b>None</b>";
                return Utils.ColorString(Color, "\n<size=1.8>Mode: Confess\n</size><size=65%>") + Utils.ColorString(Color.magenta, "(") +
                    Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")\n</size>") +
                    Utils.ColorString(Color, "<size=1.8>Selected: " + selectedText + "</size>");
            }
            return "";
        }

        public override void OnRevive()
        {
            Cooldown = 10f;
        }

        public int GetEvilness(NetworkedPlayerInfo target)
        {
            return target.GetRole().CustomRoleType switch
            {
                CustomRoleTypes.CrewmateKilling => 1,
                CustomRoleTypes.NeutralBenign => 2,
                CustomRoleTypes.NeutralEvil => 3,
                CustomRoleTypes.NeutralKilling => 4,
                CustomRoleTypes.ImpostorVanilla => 5,
                CustomRoleTypes.ImpostorConcealing => 5,
                CustomRoleTypes.ImpostorKilling => 5,
                CustomRoleTypes.ImpostorSupport => 5,
                _ => 0,
            };
        }

        public Oracle(PlayerControl player)
        {
            Role = CustomRoles.Oracle;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = InitialAbilityUseLimit.GetInt();
            Cooldown = 10f;
            SelectedPlayers = new List<byte>();
        }

        public float Cooldown;
        public List<byte> SelectedPlayers;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem ConfessCooldown;
        public static OptionItem NumberOfSelectedPlayers;
        public static OptionItem UnselectDeadPlayers;
        public static OptionItem InitialAbilityUseLimit;
        public static OptionItem AbilityUseGainWithEachTaskCompleted;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(100500, CustomRoles.Oracle, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(100501, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            ConfessCooldown = FloatOptionItem.Create(100502, "Confess cooldown", new(5f, 60f, 2.5f), 20f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            NumberOfSelectedPlayers = IntegerOptionItem.Create(100503, "Number of selected players", new(2, 10, 1), 3, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            UnselectDeadPlayers = BooleanOptionItem.Create(100504, "Unselect dead players", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            InitialAbilityUseLimit = FloatOptionItem.Create(100505, "Initial ability use limit", new(0f, 15f, 1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(100506, "Ability use gain with each task completed", new(0f, 2f, 0.1f), 0.4f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Oracle] = Chance;
            Options.RolesCount[CustomRoles.Oracle] = Count;
        }
    }
}