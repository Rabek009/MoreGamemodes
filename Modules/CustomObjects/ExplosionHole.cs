using System.Collections.Generic;
using UnityEngine;

namespace MoreGamemodes
{
    public class ExplosionHole : CustomNetObject
    {
        public static int GetSpeedDecrease(PlayerControl player)
        {
            if (player == null || player.Data.IsDead) return 0;
            int speedDecrease = 0;
            foreach (var netObject in CustomObjects)
            {
                var explosionHole = netObject as ExplosionHole;
                if (explosionHole != null)
                {
                    if (Vector2.Distance(player.GetRealPosition(), explosionHole.Position) <= explosionHole.Size / 3f && explosionHole.SpeedDecrease > speedDecrease)
                        speedDecrease = explosionHole.SpeedDecrease;
                }
            }
            return speedDecrease;
        }

        public static void FixedUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                int speedDecrease = GetSpeedDecrease(pc);
                if (speedDecrease != LastSpeedDecrease[pc.PlayerId])
                {
                    LastSpeedDecrease[pc.PlayerId] = speedDecrease;
                    pc.SyncPlayerSettings();
                }
            }
        }

        public ExplosionHole(float size, int speedDecrease, Vector2 position)
        {
            Size = size;
            SpeedDecrease = speedDecrease;
            CreateNetObject($"<size={Size}><line-height=97%><cspace=0.16em><#0000>WW</color><mark=#202020>W</mark><mark=#494949>W</mark><mark=#6c6c6c>W</mark><#0000>WW\nW</color><mark=#a8a8a8>W</mark><mark=#000000>W</mark><mark=#1e1e1e>W</mark><mark=#000000>W</mark><mark=#8d8d8d>W</mark><#0000>W</color>\n<mark=#6c6c6c>W</mark><mark=#000000>WWWWW</mark><mark=#6c6c6c>W</mark>\n<mark=#202020>W</mark><mark=#000000>WWWW</mark><mark=#1e1e1e>W</mark><mark=#494949>W\nW</mark><mark=#000000>W</mark><mark=#1e1e1e>W</mark><mark=#000000>WWW</mark><mark=#202020>W</mark>\n<#0000>W</color><mark=#8d8d8d>W</mark><mark=#000000>WW</mark><mark=#1e1e1e>W</mark><mark=#a8a8a8>W</mark><#0000>W\nWW</color><mark=#202020>W</mark><mark=#494949>W</mark><mark=#6c6c6c>W</mark><#0000>WW", position);
        }

        public float Size;
        public int SpeedDecrease;
        public static Dictionary<byte, int> LastSpeedDecrease;
    }
}