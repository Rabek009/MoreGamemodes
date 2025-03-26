using UnityEngine;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class Base : CustomNetObject
    {
        public override void OnFixedUpdate()
        {
            if (Health <= 0f)
            {
                List<byte> winners = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (BaseWarsGamemode.instance.GetTeam(pc) != Team)
                        winners.Add(pc.PlayerId);
                }
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorsBySabotage, winners);
                Bottom.Despawn();
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
                    if (!BaseWarsGamemode.instance.IsDead[pc.PlayerId] && BaseWarsGamemode.instance.GetTeam(pc) != Team && room != null && ((Team == BaseWarsTeams.Red && room.RoomId == SystemTypes.Reactor) || (Team == BaseWarsTeams.Blue && room.RoomId == SystemTypes.Nav)))
                        BaseWarsGamemode.instance.Damage(pc, Options.BaseDamage.GetFloat(), null);
                }
                if (TimeSinceLastDamage >= 5f)
                {
                    Health += Options.BaseRegeneration.GetFloat();
                    if (Health > Options.BaseHealth.GetFloat())
                        Health = Options.BaseHealth.GetFloat();
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

        public Base(BaseWarsTeams team, Vector2 position)
        {
            Team = team;
            Health = Options.BaseHealth.GetFloat();
            AttackTimer = 0f;
            TimeSinceLastDamage = 0f;
            string sprite = "";
            string sprite2 = "";
            string sprite3 = "";
            switch (team)
            {
                case BaseWarsTeams.Red:
                    sprite = "<size=3><line-height=97%><cspace=0.16em><mark=#caca00>W</mark><mark=#808080>W</mark><mark=#e8e800>W</mark><mark=#808080>WW</mark><mark=#ffff00>W</mark><mark=#808080>WW</mark><mark=#e8e800>W</mark><mark=#808080>W</mark><mark=#cccc00>W</mark>\n<mark=#caca00>W</mark><mark=#ff0000>WW</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#5d5d5d>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#cccc00>W</mark>\n<mark=#c8c8c8>WW</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark>\n<mark=#d9d900>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#5d5d5d>W</mark><mark=#c0c0c0>WWW</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#aaaaaa>W";
                    sprite2 = "<size=3><line-height=97%><cspace=0.16em>\n\n\n\n\n\n\n\n<mark=#b0b0b0>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#5d5d5d>WW</mark><mark=#b2b2b2>W</mark><mark=#808080>W</mark><mark=#b2b2b2>W</mark><mark=#5d5d5d>W</mark><mark=#aaaaaa>WW</mark>\n<mark=#b0b0b0>WW</mark><mark=#eaea00>W</mark><mark=#d5d500>WW</mark><mark=#b2b2b2>W</mark><mark=#808080>W</mark><mark=#b2b2b2>W</mark><mark=#d5d500>WW</mark><mark=#eaea00>W</mark>\n<#0000>WWW</color><mark=#eaea00>W</mark><mark=#d5d500>WW</mark><mark=#ff0000>W</mark><mark=#d5d500>WW</mark><mark=#eaea00>W</mark><#0000>W\nWWWW</color><mark=#eaea00>WW</mark><mark=#ff0000>W</mark><mark=#eaea00>WW</mark><#0000>WW";
                    sprite3 = "<size=3><line-height=97%><cspace=0.16em><#0000>WW</color><mark=#ff0000>W</mark><#0000>WW</color><mark=#ff0000>W</mark><#0000>WW</color><mark=#ff0000>W</mark><#0000>WW\nW</color><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><mark=#cc0000>W</mark><mark=#ff0000>WW</mark><mark=#c0c0c0>W</mark><mark=#ff0000>W</mark><mark=#c0c0c0>W</mark><#0000>W</color>\n<mark=#caca00>W</mark><mark=#c0c0c0>W</mark><mark=#808080>W</mark><mark=#e8e800>W</mark><mark=#cc0000>WW</mark><mark=#ff0000>W</mark><mark=#e8e800>W</mark><mark=#808080>W</mark><mark=#c0c0c0>W</mark><mark=#cccc00>W</mark>\n<mark=#caca00>W</mark><mark=#808080>WW</mark><mark=#c0c0c0>WWWWW</mark><mark=#808080>WW</mark><mark=#cccc00>W</mark>\n\n\n\n\n\n\n\n\n";
                    break;
                case BaseWarsTeams.Blue:
                    sprite = "<size=3><line-height=97%><cspace=0.16em><mark=#caca00>W</mark><mark=#808080>W</mark><mark=#e8e800>W</mark><mark=#808080>WW</mark><mark=#ffff00>W</mark><mark=#808080>WW</mark><mark=#e8e800>W</mark><mark=#808080>W</mark><mark=#cccc00>W</mark>\n<mark=#caca00>W</mark><mark=#00ffff>WW</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#00ffff>W</mark><mark=#5d5d5d>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#cccc00>W</mark>\n<mark=#c8c8c8>WW</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#00ffff>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark>\n<mark=#d9d900>W</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#5d5d5d>W</mark><mark=#c0c0c0>WWW</mark><mark=#b5b5b5>W</mark><mark=#c0c0c0>W</mark><mark=#aaaaaa>W";
                    sprite2 = "<size=3><line-height=97%><cspace=0.16em>\n\n\n\n\n\n\n\n<mark=#b0b0b0>W</mark><mark=#c0c0c0>W</mark><mark=#b5b5b5>W</mark><mark=#5d5d5d>WW</mark><mark=#b2b2b2>W</mark><mark=#808080>W</mark><mark=#b2b2b2>W</mark><mark=#5d5d5d>W</mark><mark=#aaaaaa>WW</mark>\n<mark=#b0b0b0>WW</mark><mark=#eaea00>W</mark><mark=#d5d500>WW</mark><mark=#b2b2b2>W</mark><mark=#808080>W</mark><mark=#b2b2b2>W</mark><mark=#d5d500>WW</mark><mark=#eaea00>W</mark>\n<#0000>WWW</color><mark=#eaea00>W</mark><mark=#d5d500>WW</mark><mark=#00ffff>W</mark><mark=#d5d500>WW</mark><mark=#eaea00>W</mark><#0000>W\nWWWW</color><mark=#eaea00>WW</mark><mark=#00ffff>W</mark><mark=#eaea00>WW</mark><#0000>WW";
                    sprite3 = "<size=3><line-height=97%><cspace=0.16em><#0000>WW</color><mark=#00ffff>W</mark><#0000>WW</color><mark=#00ffff>W</mark><#0000>WW</color><mark=#00ffff>W</mark><#0000>WW\nW</color><mark=#c0c0c0>W</mark><mark=#00ffff>W</mark><mark=#c0c0c0>W</mark><mark=#00d9d9>W</mark><mark=#00ffff>WW</mark><mark=#c0c0c0>W</mark><mark=#00ffff>W</mark><mark=#c0c0c0>W</mark><#0000>W</color>\n<mark=#caca00>W</mark><mark=#c0c0c0>W</mark><mark=#808080>W</mark><mark=#e8e800>W</mark><mark=#00d9d9>WW</mark><mark=#00ffff>W</mark><mark=#e8e800>W</mark><mark=#808080>W</mark><mark=#c0c0c0>W</mark><mark=#cccc00>W</mark>\n<mark=#caca00>W</mark><mark=#808080>WW</mark><mark=#c0c0c0>WWWWW</mark><mark=#808080>WW</mark><mark=#cccc00>W</mark>\n\n\n\n\n\n\n\n\n";
                    break;
            }
            BaseWarsGamemode.instance.AllBases.Add(this);
            CreateNetObject(sprite, position);
            LastNotifyHealth = (int)(Health + 0.99f);
            Bottom = Utils.RpcCreateDisplay(sprite2, Position);
            Top = Utils.RpcCreateDisplay(sprite3, Position);
            HealthDisplay = Utils.RpcCreateDisplay("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size></font><size=21><line-height=97%>\n\n<size=0>.", Position);
        }

        public BaseWarsTeams Team;
        public float Health;
        public int LastNotifyHealth;
        public float AttackTimer;
        public float TimeSinceLastDamage;
        public Display Bottom;
        public Display Top;
        public Display HealthDisplay;
    }
}