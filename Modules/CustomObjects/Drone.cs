using UnityEngine;

namespace MoreGamemodes
{
    public class Drone : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            Timer += Time.deltaTime;
            if (Timer >= 0.05f) {
                Timer -= 0.05f;
                if (Owner != null)
                {
                    var dronerRole = Owner.GetRole() as Droner;
                    if (dronerRole != null && dronerRole.DronePosition != Position)
                        RpcTeleport(dronerRole.DronePosition);
                }
            }
            base.OnFixedUpdate();
        }

        public Drone(PlayerControl owner, Vector2 position)
        {
            Timer = -0.1f;
            Owner = owner;
            CreateNetObject($"<size=3.2><line-height=97%><cspace=0.16em><mark=#000000>WWW</mark><#0000>WW</color><mark=#000000>WWW<br></mark><#0000>W</color><mark=#000000>W</mark><#0000>W</color><mark=#0080ff>W</mark><mark=#00ffff>W</mark><#0000>W</color><mark=#000000>W</mark><#0000>W<br>W</color><mark=#000000>WWWWWW</mark><#0000>W<br>WW</color><mark=#000000>W</mark><mark=#ff0000>WW</mark><mark=#000000>W</mark><#0000>WW<br>WWW</color><mark=#000000>WW</mark><#0000>WWW", position, CustomObjectTypes.Drone);
        }

        public float Timer;
        public PlayerControl Owner;
    }
}