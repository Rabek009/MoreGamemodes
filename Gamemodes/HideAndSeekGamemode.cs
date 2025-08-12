using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class HideAndSeekGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Main.Timer = 0f;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.SyncPlayerSettings();
            }
        }

        public override void OnSetFilterText(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Crewmate)
                __instance.FilterText.text = "Hider";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Scientist)
                __instance.FilterText.text = "Scientist";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Engineer)
                __instance.FilterText.text = "Engineer";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.CrewmateGhost)
                __instance.FilterText.text = "Hider Ghost";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.GuardianAngel)
                __instance.FilterText.text = "Guardian Angel";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Impostor)
                __instance.FilterText.text = "Seeker";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Shapeshifter)
                __instance.FilterText.text = "Shapeshifter";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.ImpostorGhost)
                __instance.FilterText.text = "Seeker Ghost";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Noisemaker)
                __instance.FilterText.text = "Noisemaker";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Phantom)
                __instance.FilterText.text = "Phantom";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Tracker)
                __instance.FilterText.text = "Tracker";
        }
        
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role.IsImpostor)
            {
                if (!Options.HnSImpostorsCanVent.GetBool())
                {
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                }
                if (!Options.HnSImpostorsCanCloseDoors.GetBool())
                {
                    __instance.SabotageButton.SetDisabled();
                    __instance.SabotageButton.ToggleVisible(false);
                }
            }
            __instance.ReportButton.SetDisabled();
            __instance.ReportButton.ToggleVisible(false);
        }

        public override void OnSetTaskText(TaskPanelBehaviour __instance, string str)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role.IsImpostor)
                __instance.taskText.text = Utils.ColorString(Color.red, "Seeker:\nKill all hiders.");
            else if (!PlayerControl.LocalPlayer.Data.IsDead)
                __instance.taskText.text = "Hider:\nDo your tasks and survive.\n\n" + str;
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (!Options.HnSImpostorsCanCloseDoors.GetBool())
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }

        public override List<PlayerControl> OnBeginCrewmatePrefix(IntroCutscene __instance)
        {
            if (Options.HnSImpostorsAreVisible.GetBool())
            {
                var Team = new List<PlayerControl>();
                Team.Add(PlayerControl.LocalPlayer);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != PlayerControl.LocalPlayer && !pc.Data.Role.IsImpostor)
                        Team.Add(pc);
                }
                return Team;
            }
            return base.OnBeginCrewmatePrefix(__instance);
        }

        public override void OnBeginCrewmatePostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Hider";
            __instance.ImpostorText.text = "";
        }

        public override void OnBeginImpostorPostfix(IntroCutscene __instance)
        {
            __instance.TeamTitle.text = "Seeker";
        }

        public override void OnShowRole(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Crewmate)
            {
                __instance.RoleText.text = "Hider";
                __instance.RoleBlurbText.text = "Do your tasks and survive";
            }
            else if (PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.Impostor)
            {
                __instance.RoleText.text = "Seeker";
                __instance.RoleBlurbText.text = "Kill all hiders";
            }
        }

        public override void OnSelectRolesPostfix()
        {
            if (!Options.HnSImpostorsAreVisible.GetBool()) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
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
                }
            }
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.Data.RpcSetTasks(new byte[0]);
            }
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (Main.Timer < Options.HnSImpostorsBlindTime.GetFloat() && !Options.HnSImpostorsCanKillDuringBlind.GetBool()) return false;
            return true;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.HnSImpostorsBlindTime.GetFloat() && Main.Timer < Options.HnSImpostorsBlindTime.GetFloat() + 1f && Main.GameStarted)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        pc.SyncPlayerSettings();
                }
                Main.Timer += 1f;
            }
        }

        public override bool OnEnterVent(PlayerControl player, int id)
        {
            if (!Options.HnSImpostorsCanVent.GetBool() && player.Data.Role.IsImpostor)
                return false;
            return base.OnEnterVent(player, id);
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            if (!Options.HnSImpostorsCanCloseDoors.GetBool()) return false;
            return true;
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, MessageReader reader)
        {
            if (systemType == SystemTypes.Sabotage) return false;
            return true;
        }

        public override IGameOptions BuildGameOptions(PlayerControl player, IGameOptions opt)
        {
            opt.SetInt(Int32OptionNames.NumEmergencyMeetings, 0);
            if (Main.Timer < Options.HnSImpostorsBlindTime.GetFloat() && player.Data.Role != null && player.Data.Role.IsImpostor)
            {
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            }
            return opt;
        }

        public HideAndSeekGamemode()
        {
            Gamemode = Gamemodes.HideAndSeek;
            PetAction = false;
            DisableTasks = false;
        }

        public static HideAndSeekGamemode instance;
    }
}