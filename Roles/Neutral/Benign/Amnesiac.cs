using UnityEngine;
using Hazel;

namespace MoreGamemodes
{
    public class Amnesiac : CustomRole
    {
        public override void OnHudUpdate(HudManager __instance)
        {
            __instance.ReportButton.OverrideText("Report");
            __instance.KillButton.OverrideText("Kill");
            __instance.PetButton.OverrideText("Change Mode");
            if (RememberMode)
                __instance.ReportButton.OverrideText("Remember");
        }

        public override void OnPet()
        {
            SendRPC(!RememberMode);
        }

        public override bool OnReportDeadBody(NetworkedPlayerInfo target)
        {
            if (!RememberMode || target == null) return true;
            ClassicGamemode.instance.ChangeRole(Player, target.GetRole().Role);
            if (target.Object == null) return false;
            if (target.GetRole().IsCrewmate())
                ClassicGamemode.instance.ChangeRole(target.Object, CustomRoles.Crewmate);
            else if (target.GetRole().IsImpostor())
                ClassicGamemode.instance.ChangeRole(target.Object, CustomRoles.Impostor);
            else if (target.GetRole().IsNeutral())
                ClassicGamemode.instance.ChangeRole(target.Object, CustomRoles.Amnesiac);
            return false;
        }

        public override void OnMeeting()
        {
            SendRPC(true);
        }

        public override string GetNamePostfix()
        {
            string postfix = "";
            if (RememberMode)
                postfix = Utils.ColorString(Color, "\n<size=1.8>Mode: Remember\n</size><size=65%>");
            else
                postfix = Utils.ColorString(Color, "\n<size=1.8>Mode: Report\n</size><size=65%>");
            postfix += Utils.ColorString(Color.magenta, "(") + Utils.ColorString(Color.cyan, "Pet to change mode") + Utils.ColorString(Color.magenta, ")</size>");
            if (SeeArrowToNearestBody.GetBool() && !Player.Data.IsDead && Player.GetClosestDeadBody() != null)
                postfix += Utils.ColorString(Color, "\n" + Utils.GetArrow(Player.transform.position, Player.GetClosestDeadBody().transform.position));
            return postfix;
        }

        public override void OnRevive()
        {
            SendRPC(true);
        }

        public override bool ShouldContinueGame()
        {
            return Object.FindObjectOfType<DeadBody>() != null;
        }

        public void SendRPC(bool rememberMode)
        {
            RememberMode = rememberMode;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(Player.NetId, (byte)CustomRPC.SyncCustomRole, SendOption.Reliable, -1);
            writer.Write(rememberMode);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public override void ReceiveRPC(MessageReader reader)
        {
            RememberMode = reader.ReadBoolean();
        }

        public Amnesiac(PlayerControl player)
        {
            Role = CustomRoles.Amnesiac;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            RememberMode = true;
        }

        public bool RememberMode;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem SeeArrowToNearestBody;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(800200, CustomRoles.Amnesiac, TabGroup.NeutralRoles, false);
            Count = IntegerOptionItem.Create(800201, "Max", new(1, 15, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            SeeArrowToNearestBody = BooleanOptionItem.Create(800202, "See arrow to nearest body", true, TabGroup.NeutralRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.Amnesiac] = Chance;
            Options.RolesCount[CustomRoles.Amnesiac] = Count;
        }
    }
}