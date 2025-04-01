using UnityEngine;
using Hazel;

namespace MoreGamemodes
{
    public class Drone : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            if (Owner != null)
            {
                var dronerRole = Owner.GetRole() as Droner;
                if (dronerRole != null)
                    RpcTeleport(dronerRole.DronePosition, SendOption.None);
            }
            else
            {
                Despawn();
            }
        }

        public override void OnMeeting()
        {
            Despawn();
        }

        public Drone(PlayerControl owner, Vector2 position)
        {
            Owner = owner;
            CreateNetObject($"<size=0.5><line-height=97%><cspace=0.16em><mark=#000000>WWW</mark><#0000>WW</color><mark=#000000>WWW\n</mark><#0000>W</color><mark=#000000>W</mark><#0000>W</color><mark=#0080ff>W</mark><mark=#00ffff>W</mark><#0000>W</color><mark=#000000>W</mark><#0000>W\nW</color><mark=#000000>WWWWWW</mark><#0000>W\nWW</color><mark=#000000>W</mark><mark=#ff0000>WW</mark><mark=#000000>W</mark><#0000>WW\nWWW</color><mark=#000000>WW</mark><#0000>WWW", position);
        }

        public PlayerControl Owner;
    }
}