using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

namespace MoreGamemodes
{
    public class Escapist : CustomRole
    {
        public override void OnHudUpdate(HudManager __instance)
        {
            base.OnHudUpdate(__instance);
            if (Player.Data.IsDead) return;
            if (MarkedPosition == null)
                __instance.AbilityButton.OverrideText("Mark");
            else
                __instance.AbilityButton.OverrideText("Teleport");
            if (!CanUseVents.GetBool())
            {
                __instance.ImpostorVentButton.SetDisabled();
                __instance.ImpostorVentButton.ToggleVisible(false);
            }
        }

        public override void OnMeeting()
        {
            MarkedPosition = null;
            MarkedPositionRoom = null;
            SendRPC();
        }

        public override bool OnCheckVanish()
        {
            if (MarkedPosition == null)
            {
                MarkedPosition = Player.transform.position;
                var room = Player.GetPlainShipRoom();
                if (room != null)
                    MarkedPositionRoom = room.RoomId;
                SendRPC();
                new LateTask(() => Player.RpcSetAbilityCooldown(TeleportCooldown.GetFloat()), 0.2f);
                return false;
            }
            Player.RpcTeleport(MarkedPosition.Value);
            MarkedPosition = null;
            MarkedPositionRoom = null;
            SendRPC();
            return false;
        }

        public override bool OnEnterVent(int id)
        {
            return CanUseVents.GetBool();
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.PhantomCooldown, MarkCooldown.GetFloat());
            return opt;
        }

        public override string GetNamePostfix()
        {
            if (MarkedPosition == null)
                return Utils.ColorString(Color, "\n<size=1.8>Marked position: <b>None</b></size>");
            if (MarkedPositionRoom == null)
                return Utils.ColorString(Color, "\n<size=1.8>Marked position: " + (Main.RealOptions.GetByte(ByteOptionNames.MapId) is 2 or 5 ? "Outside" : "Hallway") + "</size>");
            return Utils.ColorString(Color, "\n<size=1.8>Marked position: " + TranslationController.Instance.GetString((SystemTypes)MarkedPositionRoom) + "</size>");
        }

        public override bool IsCompatible(AddOns addOn)
        {
            return addOn != AddOns.Lurker || CanUseVents.GetBool();
        }

        public override void OnRevive()
        {
            MarkedPosition = null;
            MarkedPositionRoom = null;
            SendRPC();
        }

        public void SendRPC()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable, -1);
            writer.Write(MarkedPosition != null);
            if (MarkedPosition != null)
                NetHelpers.WriteVector2(MarkedPosition.Value, writer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            MarkedPosition = reader.ReadBoolean() ? NetHelpers.ReadVector2(reader) : null;
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
        public static OptionItem CanUseVents;
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
            CanUseVents = BooleanOptionItem.Create(500204, "Can use vents", false, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Escapist] = Chance;
            Options.RolesCount[CustomRoles.Escapist] = Count;
        }
    }
}