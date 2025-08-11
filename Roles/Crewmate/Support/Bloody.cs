using UnityEngine;
using Hazel;

namespace MoreGamemodes
{
    public class Bloody : CustomRole
    {
        public override void OnMurderPlayerAsTarget(PlayerControl killer)
        {
            if (killer == Player) return;
            if (Player.GetDeathReason() == DeathReasons.Eaten || Player.GetDeathReason() == DeathReasons.Shot || Player.GetDeathReason() == DeathReasons.Burned) return;
            if (killer.GetRole().Role == CustomRoles.Droner)
            {
                Droner dronerRole = killer.GetRole() as Droner;
                if (dronerRole != null && dronerRole.ControlledDrone != null)
                    return;
            }
            AbilityDuration = DeadBodiesSpawnDuration.GetFloat();
            TimeSinceLastBodySpawned = 0f;
        }

        public override void OnMeeting()
        {
            AbilityDuration = -1f;
            TimeSinceLastBodySpawned = 0f;
        }

        public override void OnFixedUpdate()
        {
            if (!Player.Data.IsDead || MeetingHud.Instance) return;
            if (AbilityDuration > -1f)
            {
                AbilityDuration -= Time.fixedDeltaTime;
                TimeSinceLastBodySpawned += Time.fixedDeltaTime;
                if (TimeSinceLastBodySpawned >= DeadBodiesInterval.GetFloat())
                {
                    SpawnBodyOnKiller();
                    TimeSinceLastBodySpawned -= DeadBodiesInterval.GetFloat();
                }
            }
            if (AbilityDuration <= 0f && AbilityDuration > -1f)
            {
                AbilityDuration = -1f;
                TimeSinceLastBodySpawned = 0f;
            }
        }

        public override bool IsCompatible(AddOns addOn)
        {
            return addOn != AddOns.Bait;
        }

        public override void OnRevive()
        {
            AbilityDuration = -1f;
            TimeSinceLastBodySpawned = 0f;
        }

        public void SpawnBodyOnKiller()
        {
            if (!Player.Data.IsDead || MeetingHud.Instance || ClassicGamemode.instance.PlayerKiller[Player.PlayerId] == byte.MaxValue) return;
            PlayerControl killer = Utils.GetPlayerById(ClassicGamemode.instance.PlayerKiller[Player.PlayerId]);
            if (killer == null || killer.Data.IsDead) return;
            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
            sender.StartMessage(-1);
            Player.NetTransform.SnapTo(killer.transform.position, (ushort)(Player.NetTransform.lastSequenceId + 128));
            sender.StartRpc(Player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                .WriteVector2(killer.transform.position)
                .Write((ushort)(Player.NetTransform.lastSequenceId + 2))
                .EndRpc();
            Utils.CreateDeadBody(Player.transform.position, (byte)Player.cosmetics.ColorId, Player, true);
            sender.StartRpc(Player.NetId, (byte)RpcCalls.MurderPlayer)
                .WriteNetObject(Player)
                .Write((int)MurderResultFlags.Succeeded)
                .EndRpc();
            sender.EndMessage();
            sender.SendMessage();
        }

        public Bloody(PlayerControl player)
        {
            Role = CustomRoles.Bloody;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
            TimeSinceLastBodySpawned = 0f;
        }

        public float AbilityDuration;
        public float TimeSinceLastBodySpawned;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem DeadBodiesSpawnDuration;
        public static OptionItem DeadBodiesInterval;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(400500, CustomRoles.Bloody, TabGroup.CrewmateRoles, false);
            Count = IntegerOptionItem.Create(400501, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            DeadBodiesSpawnDuration = FloatOptionItem.Create(400502, "Dead bodies spawn duration", new(3f, 30f, 1f), 10f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            DeadBodiesInterval = FloatOptionItem.Create(400503, "Dead bodies interval", new(0.2f, 3f, 0.1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            Options.RolesChance[CustomRoles.Bloody] = Chance;
            Options.RolesCount[CustomRoles.Bloody] = Count;
        }
    }
}