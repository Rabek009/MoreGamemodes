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
                CheckEndCriteriaNormalPatch.StartEndGame(GameOverReason.ImpostorBySabotage, winners);
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
                    if (!BaseWarsGamemode.instance.IsDead[pc.PlayerId] && BaseWarsGamemode.instance.GetTeam(pc) != Team && ((Team == BaseWarsTeams.Red && pc.GetPlainShipRoom().RoomId == SystemTypes.Reactor) || (Team == BaseWarsTeams.Blue && pc.GetPlainShipRoom().RoomId == SystemTypes.Nav)))
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
                HealthDisplay.RpcChangeSprite("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size><size=21><line-height=70%>\n\n<size=0>.");
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
            switch (team)
            {
                case BaseWarsTeams.Red:
                    sprite = "<size=3><font=\"VCR SDF\"><line-height=70%><#0000>██<#ff0000>█<#0000>██<#ff0000>█<#0000>██<#ff0000>█<#0000>██<br>█<#c0c0c0>█<#ff0000>█<#c0c0c0>█<#cc0000>█<#ff0000>██<#c0c0c0>█<#ff0000>█<#c0c0c0>█<#0000>█<br><#caca00>█<#c0c0c0>█<#808080>█<#e8e800>█<#cc0000>██<#ff0000>█<#e8e800>█<#808080>█<#c0c0c0>█<#cccc00>█<br><#caca00>█<#808080>██<#c0c0c0>█████<#808080>██<#cccc00>█<br><#caca00>█<#808080>█<#e8e800>█<#808080>██<#ffff00>█<#808080>██<#e8e800>█<#808080>█<#cccc00>█<br><#caca00>█<#ff0000>██<#b5b5b5>█<#c0c0c0>█<#ff0000>█<#5d5d5d>█<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<#cccc00>█<br><#c8c8c8>██<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<#c0c0c0>█<#ff0000>█<#c0c0c0>█<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<br><#d9d900>█<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<#5d5d5d>█<#c0c0c0>███<#b5b5b5>█<#c0c0c0>█<#aaaaaa>█<br><#b0b0b0>█<#c0c0c0>█<#b5b5b5>█<#5d5d5d>██<#b2b2b2>█<#808080>█<#b2b2b2>█<#5d5d5d>█<#aaaaaa>██<br><#b0b0b0>██<#eaea00>█<#d5d500>██<#b2b2b2>█<#808080>█<#b2b2b2>█<#d5d500>██<#eaea00>█<br><#0000>███<#eaea00>█<#d5d500>██<#ff0000>█<#d5d500>██<#eaea00>█<#0000>█<br>████<#eaea00>██<#ff0000>█<#eaea00>██<#0000>██";
                    break;
                case BaseWarsTeams.Blue:
                    sprite = "<size=3><font=\"VCR SDF\"><line-height=70%><#0000>██<#00ffff>█<#0000>██<#00ffff>█<#0000>██<#00ffff>█<#0000>██<br>█<#c0c0c0>█<#00ffff>█<#c0c0c0>█<#00d9d9>█<#00ffff>██<#c0c0c0>█<#00ffff>█<#c0c0c0>█<#0000>█<br><#caca00>█<#c0c0c0>█<#808080>█<#e8e800>█<#00d9d9>██<#00ffff>█<#e8e800>█<#808080>█<#c0c0c0>█<#cccc00>█<br><#caca00>█<#808080>██<#c0c0c0>█████<#808080>██<#cccc00>█<br><#caca00>█<#808080>█<#e8e800>█<#808080>██<#ffff00>█<#808080>██<#e8e800>█<#808080>█<#cccc00>█<br><#caca00>█<#00ffff>██<#b5b5b5>█<#c0c0c0>█<#00ffff>█<#5d5d5d>█<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<#cccc00>█<br><#c8c8c8>██<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<#c0c0c0>█<#00ffff>█<#c0c0c0>█<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<br><#d9d900>█<#b5b5b5>█<#c0c0c0>█<#b5b5b5>█<#5d5d5d>█<#c0c0c0>███<#b5b5b5>█<#c0c0c0>█<#aaaaaa>█<br><#b0b0b0>█<#c0c0c0>█<#b5b5b5>█<#5d5d5d>██<#b2b2b2>█<#808080>█<#b2b2b2>█<#5d5d5d>█<#aaaaaa>██<br><#b0b0b0>██<#eaea00>█<#d5d500>██<#b2b2b2>█<#808080>█<#b2b2b2>█<#d5d500>██<#eaea00>█<br><#0000>███<#eaea00>█<#d5d500>██<#00ffff>█<#d5d500>██<#eaea00>█<#0000>█<br>████<#eaea00>██<#00ffff>█<#eaea00>██<#0000>██";
                    break;
            }
            BaseWarsGamemode.instance.AllBases.Add(this);
            CreateNetObject(sprite, position, CustomObjectTypes.Base);
            LastNotifyHealth = (int)(Health + 0.99f);
            HealthDisplay = Utils.RpcCreateDisplay("<font=\"VCR SDF\"><color=#ff0000><size=5>Health:" + LastNotifyHealth + "</size><size=21><line-height=70%>\n\n<size=0>.", Position);
        }

        public BaseWarsTeams Team;
        public float Health;
        public int LastNotifyHealth;
        public float AttackTimer;
        public float TimeSinceLastDamage;
        public Display HealthDisplay;
    }
}