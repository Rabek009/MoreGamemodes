using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

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

        public override void OnShowNormalMap(MapBehaviour __instance)
        {
            __instance.taskOverlay.Hide();
        }

        public override void OnBeginImpostor(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Battle Royale";
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            __instance.RoleText.text = "Player";
            __instance.RoleBlurbText.text = "Kill everyone and survive";
            __instance.YouAreText.color = Color.clear;
        }

        public override void OnSelectRolesPrefix()
        {
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
            if (target.Lives() > 1)
            {
                --Lives[target.PlayerId];
                killer.RpcSetKillTimer(Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown));
            }
            else
            {
                Lives[target.PlayerId] = 0;
                killer.RpcMurderPlayer(target, true);
                new LateTask(() => killer.RpcSetKillTimer(Main.RealOptions.GetFloat(FloatOptionNames.KillCooldown)), 0.01f, "Set Cooldown");
            }
            return false;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, GameData.PlayerInfo target)
        {
            return false;
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            return false;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public BattleRoyaleGamemode()
        {
            Gamemode = Gamemodes.BattleRoyale;
            PetAction = false;
            Lives = new Dictionary<byte, int>();
            foreach (var pc in PlayerControl.AllPlayerControls)
                Lives[pc.PlayerId] = Options.Lives.GetInt();
        }

        public static BattleRoyaleGamemode instance;
        public Dictionary<byte, int> Lives;
    }
}