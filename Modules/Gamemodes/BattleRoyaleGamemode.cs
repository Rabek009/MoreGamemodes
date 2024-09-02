using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class BattleRoyaleGamemode : CustomGamemode
    {
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            __instance.ImpostorVentButton.SetDisabled();
            __instance.ImpostorVentButton.ToggleVisible(false);
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
            __instance.KillButton.OverrideText("Attack");
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.IsDead)
                __instance.FilterText.text = "Ghost";
            else
                __instance.FilterText.text = "Player";
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.IsDead)
                __instance.taskText.text = Utils.ColorString(Color.red, "You're dead. Enjoy the chaos.");
            else
                __instance.taskText.text = Utils.ColorString(Color.red, "Player:\nKill everyone and survive.");
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Battle Royale";
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            __instance.RoleText.text = "Player";
            __instance.RoleBlurbText.text = "Kill everyone and survive";
            __instance.YouAreText.color = Color.clear;
        }

        public override bool OnSelectRolesPrefix()
        {
            Utils.RpcSetDesyncRoles(RoleTypes.Impostor, RoleTypes.Crewmate);
            return false;
        }

        public override void OnSelectRolesPostfix()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                foreach (var ar in PlayerControl.AllPlayerControls)
                    Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.white;
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < Options.GracePeriod.GetFloat())
                return false;         
            if (GetLives(target) > 1)
            {
                --Lives[target.PlayerId];
                killer.RpcSetKillTimer(Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown));
                return false;
            }
            else
            {
                Lives[target.PlayerId] = 0;
                return true;
            }
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return false;
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
            if (systemType == SystemTypes.Sabotage) return false;
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
            opt.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);
            return opt;
        }

        public override string BuildPlayerName(PlayerControl player, PlayerControl seer, string name)
        {
            var livesText = "";
            if (GetLives(player) <= 5)
            {
                for (int i = 1; i <= GetLives(player); i++)
                    livesText += "♥";
            }
            else
                livesText = "Lives: " + GetLives(player);
            livesText = Utils.ColorString(Color.red, livesText);
            if (Options.ArrowToNearestPlayer.GetBool() && player == seer && player.GetClosestPlayer() != null && !player.Data.IsDead)
                name += Utils.ColorString(Palette.PlayerColors[Main.StandardColors[player.GetClosestPlayer().PlayerId]], Utils.GetArrow(player.transform.position, player.GetClosestPlayer().transform.position));
            if (player == seer || Options.LivesVisibleToOthers.GetBool() || seer.Data.IsDead)
                name += "\n" + livesText;
            return name;
        }

        public int GetLives(PlayerControl player)
        {
            if (player == null) return 0;
            if (!Lives.ContainsKey(player.PlayerId)) return 0;
            return Lives[player.PlayerId];
        }

        public BattleRoyaleGamemode()
        {
            Gamemode = Gamemodes.BattleRoyale;
            PetAction = false;
            DisableTasks = true;
            Lives = new Dictionary<byte, int>();
            foreach (var pc in PlayerControl.AllPlayerControls)
                Lives[pc.PlayerId] = Options.Lives.GetInt();
        }

        public static BattleRoyaleGamemode instance;
        public Dictionary<byte, int> Lives;
    }
}