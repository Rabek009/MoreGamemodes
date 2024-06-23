using Hazel;

namespace MoreGamemodes
{
    public class DeathrunGamemode : CustomGamemode
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Main.Timer = 0f;
            Utils.SyncAllSettings();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.RpcSetKillTimer(Options.RoundCooldown.GetFloat());
                pc.RpcResetAbilityCooldown();
            }
            new LateTask(() => {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        pc.RpcSetKillTimer(Options.RoundCooldown.GetFloat() - 2f);
                    pc.RpcResetAbilityCooldown();
                }
            }, 2f);
        }
        
        public override void OnHudUpate(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Data.Role.IsImpostor)
            {
                if (!Options.DrImpostorsCanVent.GetBool())
                {
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                }
            }
            if (Options.DisableMeetings.GetBool())
            {
                __instance.ReportButton.SetDisabled();
                __instance.ReportButton.ToggleVisible(false);
            }
            __instance.SabotageButton.SetDisabled();
            __instance.SabotageButton.ToggleVisible(false);
        }

        public override void OnShowSabotageMap(MapBehaviour __instance)
        {
            __instance.Close();
            __instance.ShowNormalMap();
        }

        public override void OnIntroDestroy()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                    pc.RpcSetKillTimer(Options.RoundCooldown.GetFloat());
                pc.RpcResetAbilityCooldown();
            }
            new LateTask(() =>{
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.IsImpostor)
                        pc.RpcSetKillTimer(Options.RoundCooldown.GetFloat() - 2f);
                    pc.RpcResetAbilityCooldown();
                }
            }, 2f);
        }

        public override bool OnReportDeadBody(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            return !Options.DisableMeetings.GetBool();
        }

        public override void OnFixedUpdate()
        {
            if (Main.Timer >= Options.RoundCooldown.GetFloat() && Main.Timer < Options.RoundCooldown.GetFloat() + 1f && Main.GameStarted)
            {
                Utils.SyncAllSettings();
                Main.Timer += 1f;
            }
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

        public DeathrunGamemode()
        {
            Gamemode = Gamemodes.Deathrun;
            PetAction = false;
            DisableTasks = false;
        }

        public static DeathrunGamemode instance;
    }
}