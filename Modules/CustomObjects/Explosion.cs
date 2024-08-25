using UnityEngine;

namespace MoreGamemodes
{
    public class Explosion : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            Timer += Time.deltaTime;
            if (Timer >= Duration / 5f && Frame == 0)
            {
                RpcChangeSprite($"<size={Size}><line-height=97%><cspace=0.16em><#0000>W</color><mark=#ff0000>W</mark><#0000>W</color><mark=#ff0000>W</mark><#0000>W</color><mark=#ff0000>W</mark><#0000>W</color>\n<mark=#ff0000>W</mark><mark=#ff8000>W</mark><mark=#ff0000>W</mark><mark=#ff8000>W</mark><mark=#ff0000>W</mark><mark=#ff8000>W</mark><mark=#ff0000>W</mark>\n<mark=#ff8000>WW</mark><mark=#ffff00>W</mark><mark=#ff8000>W</mark><mark=#ffff00>W</mark><mark=#ff8000>WW</mark>\n<mark=#ffff00>WWWWWWW</mark>\n<mark=#ff8000>W</mark><mark=#ffff00>WWWWW</mark><mark=#ff8000>W</mark>\n<mark=#ff8000>WW</mark><mark=#ffff00>W</mark><mark=#ff8000>W</mark><mark=#ffff00>W</mark><mark=#ff8000>WW</mark>\n<mark=#ff0000>W</mark><#0000>W</color><mark=#ff8000>W</mark><mark=#ff0000>W</mark><mark=#ff8000>W</mark><#0000>W</color><mark=#ff0000>W");
                Frame = 1;
            }
            if (Timer >= Duration / 5f * 2f && Frame == 1)
            {
                RpcChangeSprite($"<size={Size}><line-height=97%><cspace=0.16em><#0000>W</color><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#000000>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><#0000>W</color>\n<mark=#c0c0c0>W</mark><mark=#808080>W</mark><mark=#ff0000>W</mark><mark=#ff8000>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>WW</mark>\n<mark=#ff0000>WW</mark><mark=#ff8000>W</mark><mark=#ffff00>W</mark><mark=#ff8000>W</mark><mark=#ff0000>WW</mark>\n<mark=#c0c0c0>W</mark><mark=#ff8000>W</mark><mark=#ffff00>W</mark><mark=#ffff80>W</mark><mark=#ffff00>W</mark><mark=#ff8000>W</mark><mark=#808080>W</mark>\n<mark=#ff0000>WW</mark><mark=#ff8000>W</mark><mark=#ffff00>W</mark><mark=#ff8000>W</mark><mark=#ff0000>WW</mark>\n<mark=#c0c0c0>W</mark><mark=#808080>W</mark><mark=#ff0000>W</mark><mark=#ff8000>W</mark><mark=#ff0000>W</mark><mark=#000000>W</mark><mark=#c0c0c0>W</mark>\n<#0000>W</color><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><#0000>W");
                Frame = 2;
            }
            if (Timer >= Duration / 5f * 3f && Frame == 2)
            {
                RpcChangeSprite($"<size={Size}><line-height=97%><cspace=0.16em><mark=#ff0000>W</mark><mark=#ff8000>W</mark><#0000>W</color><mark=#808080>W</mark><#0000>W</color><mark=#ff8000>W</mark><mark=#ff0000>W</mark>\n<mark=#ff8000>W</mark><#0000>W</color><mark=#ffff00>W</mark><mark=#c0c0c0>W</mark><mark=#ffff00>W</mark><#0000>W</color><mark=#ff8000>W</mark>\n<#0000>W</color><mark=#ffff00>W</mark><mark=#c0c0c0>WWW</mark><mark=#ffff00>W</mark><#0000>W</color>\n<mark=#808080>W</mark><mark=#c0c0c0>WWWWW</mark><mark=#808080>W</mark>\n<#0000>W</color><mark=#ffff00>W</mark><mark=#c0c0c0>WWW</mark><mark=#ffff00>W</mark><#0000>W</color>\n<mark=#ff8000>W</mark><#0000>W</color><mark=#ffff00>W</mark><mark=#c0c0c0>W</mark><mark=#ffff00>W</mark><#0000>W</color><mark=#ff8000>W</mark>\n<mark=#ff0000>W</mark><mark=#ff8000>W</mark><#0000>W</color><mark=#808080>W</mark><#0000>W</color><mark=#ff8000>W</mark><mark=#ff0000>W");
                Frame = 3;
            }
            if (Timer >= Duration / 5f * 4f && Frame == 3)
            {
                RpcChangeSprite($"<size={Size}><line-height=97%><cspace=0.16em><#0000>W</color><mark=#808080>W</mark><#0000>WW</color><mark=#c0c0c0>W</mark><#0000>W</color><mark=#808080>W</mark>\n<mark=#ffff00>W</mark><#0000>WW</color><mark=#c0c0c0>W</mark><#0000>W</color><mark=#808080>W</mark><#0000>W</color>\n<#0000>W</color><mark=#808080>W</mark><mark=#c0c0c0>WWWW</mark><#0000>W</color>\n<#0000>W</color><mark=#c0c0c0>WWWWWW\nW</mark><#0000>W</color><mark=#c0c0c0>WWW</mark><mark=#808080>W</mark><#0000>W</color>\n<#0000>W</color><mark=#c0c0c0>W</mark><#0000>W</color><mark=#c0c0c0>W</mark><#0000>W</color><mark=#c0c0c0>WW</mark>\n<mark=#808080>W</mark><#0000>W</color><mark=#c0c0c0>W</mark><#0000>W</color><mark=#808080>W</mark><#0000>W</color><mark=#ffff00>W");
                Frame = 4;
            }
            if (Timer >= Duration && Frame == 4)
            {
                if (CreateHole)
                    Utils.RpcCreateExplosionHole(Size, HoleSpeedDecrease, Position);
                Despawn();
            }
            base.OnFixedUpdate();
        }

        public Explosion(float size, float duration, bool createHole, int holeSpeedDecrease, Vector2 position)
        {
            Size = size;
            Duration = duration;
            Timer = -0.1f;
            Frame = 0;
            CreateHole = createHole;
            HoleSpeedDecrease = holeSpeedDecrease;
            CreateNetObject($"<size={Size}><line-height=97%><cspace=0.16em><#0000>WWW</color><mark=#ff0000>W</mark><#0000>WWW</color>\n<mark=#ff0000>W</mark><#0000>W</color><mark=#ff0000>WWW</mark><#0000>W</color><mark=#ff0000>W\nW</mark><mark=#ff8000>WW</mark><mark=#ffff00>W</mark><mark=#ff8000>WW</mark><mark=#ffff00>W\nWW</mark><mark=#ff8000>W</mark><mark=#ffff00>W</mark><mark=#ff8000>W</mark><mark=#ffff00>WW</mark>\n<mark=#ff8000>W</mark><mark=#ffff80>WW</mark><mark=#ffff00>W</mark><mark=#ffff80>WW</mark><mark=#ff8000>W</mark>\n<#0000>W</color><mark=#ff8000>W</mark><mark=#ffff80>WWW</mark><mark=#ff8000>W</mark><#0000>W\nWW</color><mark=#ff8000>WWW</mark><#0000>WW", position, CustomObjectTypes.Explosion);
        }

        public float Size;
        public float Duration;
        public float Timer;
        public int Frame;
        public bool CreateHole;
        public int HoleSpeedDecrease;
    }
}