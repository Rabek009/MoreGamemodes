using Il2CppSystem.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;

namespace MoreGamemodes
{
    public class ShiftAndSeekGamemode : CustomGamemode
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
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Engineer)
                __instance.FilterText.text = "Hider";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.CrewmateGhost)
                __instance.FilterText.text = "Hider Ghost";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.GuardianAngel)
                __instance.FilterText.text = "Guardian Angel";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.Shapeshifter)
                __instance.FilterText.text = "Shifter";
            if (__instance.HauntTarget.Data.Role.Role == RoleTypes.ImpostorGhost)
                __instance.FilterText.text = "Shifter Ghost";
        }

        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role.IsImpostor)
            {
                if (!Options.SnSImpostorsCanVent.GetBool())
                {
                    __instance.ImpostorVentButton.SetDisabled();
                     __instance.ImpostorVentButton.ToggleVisible(false);
                }
                if (!Options.SnSImpostorsCanCloseDoors.GetBool())
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
                __instance.taskText.text = Utils.ColorString(Color.red, "Shifter:\nShift into your victim.");
            else if (!PlayerControl.LocalPlayer.Data.IsDead)
                __instance.taskText.text = "Hider:\nHide in vents and do tasks.\n\n" + str;
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            if (!Options.SnSImpostorsCanCloseDoors.GetBool())
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }

        public override List<PlayerControl> OnBeginCrewmatePrefix(IntroCutscene __instance)
        {
            if (Options.SnSImpostorsAreVisible.GetBool())
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
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                __instance.RoleText.text = "Shifter";
                __instance.RoleBlurbText.text = "Shift into your victim";
            }
            else
            {
                __instance.RoleText.text = "Hider";
                __instance.RoleBlurbText.text = "Hide in vents and do tasks";
            }
        }

        public override void OnSelectRolesPostfix()
        {
            if (!Options.SnSImpostorsAreVisible.GetBool()) return;
            new LateTask(() => {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                    {
                        foreach (var ar in PlayerControl.AllPlayerControls)
                            Main.NameColors[(pc.PlayerId, ar.PlayerId)] = Color.red;
                    }
                }
            }, 1.2f);
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
            if (Main.Timer < Options.SnSImpostorsBlindTime.GetFloat() && !Options.SnSImpostorsCanKillDuringBlind.GetBool()) return false;
            if (Main.AllShapeshifts[killer.PlayerId] != target.PlayerId)
                return false;
            return true;
        }

        public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            if (Options.InstantShapeshift.GetBool())
            {
                shapeshifter.RpcShapeshift(target, false);
                return false;
            }
            return true;
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target, bool force)
        {
            return false;
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.SnSImpostorsBlindTime.GetFloat() && Main.Timer < Options.SnSImpostorsBlindTime.GetFloat() + 1f && Main.GameStarted)
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
            if (!Options.SnSImpostorsCanVent.GetBool() && player.Data.Role.IsImpostor)
                return false;
            return base.OnEnterVent(player, id);
        }

        public override bool OnCloseDoors(ShipStatus __instance)
        {
            if (!Options.SnSImpostorsCanCloseDoors.GetBool()) return false;
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
            opt.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Engineer, 15, 100);
            opt.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 15, 100);
            opt.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
            opt.RoleOptions.SetRoleRate(RoleTypes.Tracker, 0, 0);
            if (Main.Timer < Options.SnSImpostorsBlindTime.GetFloat() && player.Data.Role != null && player.Data.Role.IsImpostor)
            {
                opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0f);
                opt.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            } 
            return opt;
        }

        public ShiftAndSeekGamemode()
        {
            Gamemode = Gamemodes.ShiftAndSeek;
            PetAction = false;
            DisableTasks = false;
        }

        public static ShiftAndSeekGamemode instance;
    }
}