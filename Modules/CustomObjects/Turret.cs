using UnityEngine;

namespace MoreGamemodes
{
    public class Turret : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            if (Health <= 0f)
            {
                BaseWarsGamemode.instance.SendRPC(GameManager.Instance, Room);
                BaseWarsGamemode.instance.AllTurrets.Remove(this);
                Top.Despawn();
                HealthDisplay.Despawn();
                Despawn();
                return;
            }
            AttackTimer += Time.fixedDeltaTime;
            TimeSinceLastDamage += Time.fixedDeltaTime;
            if (AttackTimer >= 1f)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    var room = pc.GetPlainShipRoom();
                    if (!BaseWarsGamemode.instance.IsDead[pc.PlayerId] && BaseWarsGamemode.instance.GetTeam(pc) != Team && room != null && room.RoomId == Room)
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
                HealthDisplay.RpcChangeSprite("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size></font><size=21><line-height=97%>\n\n<size=0>.");
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
            string sprite2 = "";
            switch (team)
            {
                case BaseWarsTeams.Red:
                    sprite = "<size=3><line-height=97%><cspace=0.16em>\n\n\n\n\n\n<#0000>W</color><mark=#808080>W</mark><mark=#c0c0c0>W</mark><mark=#dddd00>W</mark><mark=#c0c0c0>W</mark><mark=#808080>W</mark><#0000>W\nWW</color><mark=#c0c0c0>W</mark><mark=#dddd00>W</mark><mark=#c0c0c0>W</mark><#0000>WW\nW</color><mark=#ff8080>W</mark><mark=#969696>W</mark><mark=#c0c0c0>W</mark><mark=#969696>W</mark><mark=#ff8080>W</mark><#0000>W\nW</color><mark=#969696>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#969696>W</mark><#0000>W\nW</color><mark=#969696>W</mark><mark=#dd0000>WWW</mark><mark=#969696>W</mark><#0000>W\nWW</color><mark=#969696>WWW</mark><#0000>WW";
                    sprite2 = "<size=3><line-height=97%><cspace=0.16em><#0000>WW</color><mark=#ff8080>W</mark><mark=#ff0000>W</mark><mark=#ff8080>W</mark><#0000>WW\nW</color><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#dd0000>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><#0000>W</color>\n<mark=#e6e600>W</mark><mark=#d2d2d2>W</mark><#0000>W</color><mark=#dd0000>W</mark><#0000>W</color><mark=#d2d2d2>W</mark><mark=#e6e600>W\n</mark><mark=#d2d2d2>WW</mark><mark=#808080>W</mark><mark=#c0c0c0>W</mark><mark=#808080>W</mark><mark=#d2d2d2>WW\nW</mark><mark=#808080>W</mark><mark=#d2d2d2>W</mark><mark=#e6e600>W</mark><mark=#d2d2d2>W</mark><mark=#808080>W</mark><mark=#d2d2d2>W\nW</mark><mark=#808080>W</mark><mark=#c7c7c7>W</mark><mark=#dddd00>W</mark><mark=#c7c7c7>W</mark><mark=#e6e600>W</mark><mark=#d2d2d2>W</mark>\n\n\n\n\n\n\n";
                    break;
                case BaseWarsTeams.Blue:
                    sprite = "<size=3><line-height=97%><cspace=0.16em>\n\n\n\n\n\n<#0000>W</color><mark=#808080>W</mark><mark=#c0c0c0>W</mark><mark=#dddd00>W</mark><mark=#c0c0c0>W</mark><mark=#808080>W</mark><#0000>W\nWW</color><mark=#c0c0c0>W</mark><mark=#dddd00>W</mark><mark=#c0c0c0>W</mark><#0000>WW\nW</color><mark=#bfffff>W</mark><mark=#969696>W</mark><mark=#c0c0c0>W</mark><mark=#969696>W</mark><mark=#bfffff>W</mark><#0000>W\nW</color><mark=#969696>W</mark><mark=#00ffff>W</mark><mark=#c0c0c0>W</mark><mark=#00ffff>W</mark><mark=#969696>W</mark><#0000>W\nW</color><mark=#969696>W</mark><mark=#00e6e6>WWW</mark><mark=#969696>W</mark><#0000>W\nWW</color><mark=#969696>WWW</mark><#0000>WW";
                    sprite2 = "<size=3><line-height=97%><cspace=0.16em><#0000>WW</color><mark=#bfffff>W</mark><mark=#00f0f0>W</mark><mark=#bfffff>W</mark><#0000>WW\nW</color><mark=#c0c0c0>W</mark><mark=#00f0f0>W</mark><mark=#00e1e1>W</mark><mark=#00f0f0>W</mark><mark=#c0c0c0>W</mark><#0000>W</color>\n<mark=#e6e600>W</mark><mark=#d2d2d2>W</mark><#0000>W</color><mark=#00e1e1>W</mark><#0000>W</color><mark=#d2d2d2>W</mark><mark=#e6e600>W\n</mark><mark=#d2d2d2>WW</mark><mark=#808080>W</mark><mark=#c0c0c0>W</mark><mark=#808080>W</mark><mark=#d2d2d2>WW\nW</mark><mark=#808080>W</mark><mark=#d2d2d2>W</mark><mark=#e6e600>W</mark><mark=#d2d2d2>W</mark><mark=#808080>W</mark><mark=#d2d2d2>W\nW</mark><mark=#808080>W</mark><mark=#c7c7c7>W</mark><mark=#dddd00>W</mark><mark=#c7c7c7>W</mark><mark=#e6e600>W</mark><mark=#d2d2d2>W</mark>\n\n\n\n\n\n\n";
                    break;
            }
            BaseWarsGamemode.instance.AllTurrets.Add(this);
            CreateNetObject(sprite, position);
            Top = Utils.RpcCreateDisplay(sprite2, Position);
            LastNotifyHealth = (int)(Health + 0.99f);
            HealthDisplay = Utils.RpcCreateDisplay("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size></font><size=21><line-height=97%>\n\n<size=0>.", Position);
        }

        public BaseWarsTeams Team;
        public SystemTypes Room;
        public float Health;
        public int LastNotifyHealth;
        public float AttackTimer;
        public float TimeSinceLastDamage;
        public Display Top;
        public Display HealthDisplay;
    }
}