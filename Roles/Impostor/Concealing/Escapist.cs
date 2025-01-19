using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    public class Escapist : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            if (MarkedPosition == null)
                __instance.AbilityButton.OverrideText("Mark");
            else
                __instance.AbilityButton.OverrideText("Teleport");
        }

        public override void OnMeeting()
        {
            Player.RpcMarkEscapistPosition(null);
            MarkedPositionRoom = null;
        }

        public override bool OnCheckVanish()
        {
            if (MarkedPosition == null)
            {
                Player.RpcMarkEscapistPosition(Player.GetRealPosition());
                var room = Player.GetPlainShipRoom();
                if (room != null)
                    MarkedPositionRoom = room.RoomId;
                new LateTask(() => Player.RpcSetAbilityCooldown(TeleportCooldown.GetFloat()), 0.2f);
                return false;
            }
            Player.RpcTeleport((Vector2)MarkedPosition);
            Player.RpcMarkEscapistPosition(null);
            MarkedPositionRoom = null;
            return false;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.PhantomCooldown, MarkCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (MarkedPosition == null)
                return Utils.ColorString(Palette.ImpostorRed, "\n<size=1.8>Marked position: None</size>");
            if (MarkedPositionRoom == null)
                return Utils.ColorString(Palette.ImpostorRed, "\n<size=1.8>Marked position: " + (Main.RealOptions.GetByte(ByteOptionNames.MapId) is 2 or 5 ? "Outside" : "Hallway") + "</size>");
            return Utils.ColorString(Palette.ImpostorRed, "\n<size=1.8>Marked position: " + TranslationController.Instance.GetString((SystemTypes)MarkedPositionRoom) + "</size>");
        }

        public Escapist(PlayerControl player)
        {
            Role = CustomRoles.Escapist;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            MarkedPosition = null;
            MarkedPositionRoom = null;
        }

        public Vector2? MarkedPosition;
        public SystemTypes? MarkedPositionRoom;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem MarkCooldown;
        public static OptionItem TeleportCooldown;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(500200, CustomRoles.Escapist, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(500201, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            MarkCooldown = FloatOptionItem.Create(500202, "Mark cooldown", new(5f, 45f, 5f), 10f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            TeleportCooldown = FloatOptionItem.Create(500203, "Teleport cooldown", new(5f, 45f, 5f), 15f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            Options.RolesChance[CustomRoles.Escapist] = Chance;
            Options.RolesCount[CustomRoles.Escapist] = Count;
        }
    }
}