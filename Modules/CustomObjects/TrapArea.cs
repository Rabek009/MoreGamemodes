using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MoreGamemodes
{
    public class TrapArea : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            Timer += Time.deltaTime;
            if (Timer >= WaitDuration * 0.75f && State == 0)
            {
                RpcChangeSprite($"<size={Size}><font=\"VCR SDF\"><#fff70069>●");
                State = 1;
            }
            if (Timer >= WaitDuration * 0.95f && State == 1)
            {
                RpcChangeSprite($"<size={Size}><font=\"VCR SDF\"><#ff000069>●");
                State = 2;
            }
            if (Timer < WaitDuration) return;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && !p.inVent && !MeetingHud.Instance)
                {
                    dis = Vector2.Distance(Position, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            if (Vector2.Distance(Position, target.transform.position) <= Options.TrapRadius.GetFloat())
            {
                if (RandomItemsGamemode.instance != null && RandomItemsGamemode.instance.ShieldTimer[target.PlayerId] <= 0f)
                {
                    target.RpcSetDeathReason(DeathReasons.Trapped);
                    target.RpcMurderPlayer(target, true);
                    ++Main.PlayerKills[OwnerId];
                }
                Despawn();
            }
        }

        public TrapArea(float radius, float waitDuration, Vector2 position, List<byte> visibleList, byte ownerId)
        {
            Radius = radius;
            Size = radius * 25f;
            Timer = -0.1f;
            WaitDuration = waitDuration;
            State = 0;
            VisibleList = visibleList;
            OwnerId = ownerId;
            CreateNetObject($"<size={Size}><font=\"VCR SDF\"><#c7c7c769>●", position, CustomObjectTypes.TrapArea);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!VisibleList.Contains(pc.PlayerId))
                    Hide(pc);
            }
        }

        public float Radius;
        public float Size;
        public float Timer;
        public float WaitDuration;
        public int State;
        public List<byte> VisibleList;
        public byte OwnerId;
    }
}