using UnityEngine;

namespace MoreGamemodes
{
    public class Turret : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            if (Health <= 0f)
            {
                GameManager.Instance.RpcDestroyTurret(Room);
                BaseWarsGamemode.instance.AllTurrets.Remove(this);
                HealthDisplay.Despawn();
                Despawn();
                return;
            }
            base.OnFixedUpdate();
            AttackTimer += Time.fixedDeltaTime;
            TimeSinceLastDamage += Time.fixedDeltaTime;
            if (AttackTimer >= 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!BaseWarsGamemode.instance.IsDead[pc.PlayerId] && BaseWarsGamemode.instance.GetTeam(pc) != Team && pc.GetPlainShipRoom().RoomId == Room)
                        BaseWarsGamemode.instance.Damage(pc, Options.TurretDamage.GetFloat(), null);
                }
                if (TimeSinceLastDamage >= 5f)
                {
                    Health += Options.TurretRegeneration.GetFloat();
                    if (Health > Options.TurretHealth.GetFloat())
                        Health = Options.TurretHealth.GetFloat();
                }
                AttackTimer -= 1f;
            }
            if (LastNotifyHealth != (int)(Health + 0.99f))
            {
                LastNotifyHealth = (int)(Health + 0.99f);
                HealthDisplay.RpcChangeSprite("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size><size=21><line-height=70%>\n\n<size=0>.");
            }
        }

        public void ReceiveDamage(float damage)
        {
            Health -= damage;
            TimeSinceLastDamage = 0f;
        }

        public Turret(BaseWarsTeams team, SystemTypes room, Vector2 position)
        {
            Team = team;
            Room = room;
            Health = Options.TurretHealth.GetFloat();
            AttackTimer = 0f;
            TimeSinceLastDamage = 0f;
            string sprite = "";
            switch (team)
            {
                case BaseWarsTeams.Red:
                    sprite = "<size=3><font=\"VCR SDF\"><line-height=70%><#0000>██<#ff8080>█<#ff0000>█<#ff8080>█<#0000>██<br>█<#c0c0c0>█<#ff0000>█<#dd0000>█<#ff0000>█<#c0c0c0>█<#0000>█<br><#e6e600>█<#d2d2d2>█<#0000>█<#dd0000>█<#0000>█<#d2d2d2>█<#e6e600>█<br><#d2d2d2>██<#808080>█<#c0c0c0>█<#808080>█<#d2d2d2>██<br>█<#808080>█<#d2d2d2>█<#e6e600>█<#d2d2d2>█<#808080>█<#d2d2d2>█<br>█<#808080>█<#c7c7c7>█<#dddd00>█<#c7c7c7>█<#e6e600>█<#d2d2d2>█<br><#0000>█<#808080>█<#c0c0c0>█<#dddd00>█<#c0c0c0>█<#808080>█<#0000>█<br>██<#c0c0c0>█<#dddd00>█<#c0c0c0>█<#0000>██<br>█<#ff8080>█<#969696>█<#c0c0c0>█<#969696>█<#ff8080>█<#0000>█<br>█<#969696>█<#ff0000>█<#c0c0c0>█<#ff0000>█<#969696>█<#0000>█<br>█<#969696>█<#dd0000>███<#969696>█<#0000>█<br>██<#969696>███<#0000>██";
                    break;
                case BaseWarsTeams.Blue:
                    sprite = "<size=3><font=\"VCR SDF\"><line-height=70%><#0000>██<#bfffff>█<#00f0f0>█<#bfffff>█<#0000>██<br>█<#c0c0c0>█<#00f0f0>█<#00e1e1>█<#00f0f0>█<#c0c0c0>█<#0000>█<br><#e6e600>█<#d2d2d2>█<#0000>█<#00e1e1>█<#0000>█<#d2d2d2>█<#e6e600>█<br><#d2d2d2>██<#808080>█<#c0c0c0>█<#808080>█<#d2d2d2>██<br>█<#808080>█<#d2d2d2>█<#e6e600>█<#d2d2d2>█<#808080>█<#d2d2d2>█<br>█<#808080>█<#c7c7c7>█<#dddd00>█<#c7c7c7>█<#e6e600>█<#d2d2d2>█<br><#0000>█<#808080>█<#c0c0c0>█<#dddd00>█<#c0c0c0>█<#808080>█<#0000>█<br>██<#c0c0c0>█<#dddd00>█<#c0c0c0>█<#0000>██<br>█<#bfffff>█<#969696>█<#c0c0c0>█<#969696>█<#bfffff>█<#0000>█<br>█<#969696>█<#00ffff>█<#c0c0c0>█<#00ffff>█<#969696>█<#0000>█<br>█<#969696>█<#00e6e6>███<#969696>█<#0000>█<br>██<#969696>███<#0000>██";
                    break;
            }
            BaseWarsGamemode.instance.AllTurrets.Add(this);
            CreateNetObject(sprite, position, CustomObjectTypes.Turret);
            LastNotifyHealth = (int)(Health + 0.99f);
            HealthDisplay = Utils.RpcCreateDisplay("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size><size=21><line-height=70%>\n\n<size=0>.", Position);
        }

        public BaseWarsTeams Team;
        public SystemTypes Room;
        public float Health;
        public int LastNotifyHealth;
        public float AttackTimer;
        public float TimeSinceLastDamage;
        public Display HealthDisplay;
    }
}