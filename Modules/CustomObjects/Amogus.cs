using UnityEngine;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class Amogus : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (MeetingHud.Instance || Main.RealOptions == null) return;
            if (FollowTarget == null || FollowTarget.Data.IsDead || FollowTarget.Data.Disconnected)
            {
                FollowTarget = GetRandomPlayer();
                TargetChangeTimer = 0f;
            }
            if (FollowTarget == null) return;
            TargetChangeTimer += Time.fixedDeltaTime;
            if (TargetChangeTimer >= 15f)
            {
                FollowTarget = GetRandomPlayer();
                TargetChangeTimer = 0f;
            }
            if (Vector2.Distance(Position, FollowTarget.transform.position) <= 0.5f) return;
            Vector2 direction = ((Vector2)FollowTarget.transform.position - Position).normalized;
            Vector2 position = Position + (direction * Main.RealOptions.GetFloat(FloatOptionNames.PlayerSpeedMod) * 2f * Time.fixedDeltaTime);
            RpcTeleport(position);
        }

        public override void OnMeeting()
        {
            base.OnMeeting();
            var rand = new System.Random();
            RpcTeleport(new Vector2((rand.NextSingle() - 0.5f) * 100f, (rand.NextSingle() - 0.5f) * 50f));
            FollowTarget = GetRandomPlayer();
            TargetChangeTimer = 0f;
        }

        public PlayerControl GetRandomPlayer()
        {
            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.IsDead && !pc.Data.Disconnected)
                    AllPlayers.Add(pc);
            }
            if (!AllPlayers.Any()) return null;
            var rand = new System.Random();
            return AllPlayers[rand.Next(0, AllPlayers.Count)];
        }

        public Amogus()
        {
            FollowTarget = GetRandomPlayer();
            TargetChangeTimer = 0f;
            var rand = new System.Random();
            CreateNetObject($"<size=0.7f><line-height=97%><cspace=0.16em><#0000>WWW</color><mark=#000000>WWWW</mark><#0000>WW\nWW</color><mark=#000000>W</mark><mark=#f3f3f3>WWWW</mark><mark=#000000>W</mark><#0000>W\nW</color><mark=#000000>W</mark><mark=#f3f3f3>WW</mark><mark=#000000>WW</mark><mark=#f3f3f3>WW</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>W</mark><mark=#000000>W</mark><mark=#72c8e0>WW</mark><mark=#000000>W</mark><mark=#f3f3f3>W</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>WW</mark><mark=#000000>WW</mark><mark=#f3f3f3>WW</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>WWWWWW</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>WWWWWW</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>WW</mark><mark=#000000>WW</mark><mark=#f3f3f3>WW</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>W</mark><mark=#000000>W</mark><#0000>WW</color><mark=#000000>W</mark><mark=#f3f3f3>W</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>W</mark><mark=#000000>W</mark><#0000>WW</color><mark=#000000>W</mark><mark=#f3f3f3>W</mark><mark=#000000>W\nW</mark><mark=#f3f3f3>WW</mark><mark=#000000>W</mark><#0000>W</color><mark=#000000>W</mark><mark=#f3f3f3>WW</mark><mark=#000000>W\n</mark><#0000>W</color><mark=#000000>WW</mark><#0000>WWW</color><mark=#000000>WW</mark><#0000>W", new Vector2((rand.NextSingle() - 0.5f) * 100f, (rand.NextSingle() - 0.5f) * 50f));
        }

        public PlayerControl FollowTarget;
        public float TargetChangeTimer;
    }
}