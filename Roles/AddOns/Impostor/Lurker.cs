using System;
using UnityEngine;

namespace MoreGamemodes
{
    public class Lurker : AddOn
    {
        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            if ((Player.inVent || Player.walkingToVent || Player.MyPhysics.Animations.IsPlayingEnterVentAnimation() ||Player.MyPhysics.Animations.Animator.GetCurrentAnimation() == Player.MyPhysics.Animations.group.ExitVentAnim) && Main.KillCooldowns[Player.PlayerId] > 0.001f)
            {
                Timer += Time.fixedDeltaTime;
                if (Timer >= 0.2f)
                {
                    float cooldownDecrease = 0.2f * (CooldownDecreaseSpeed.GetInt() / 100f);
                    Player.RpcSetKillTimer(Math.Max(Main.KillCooldowns[Player.PlayerId] - cooldownDecrease, 0.001f));
                    Timer -= 0.2f;
                }
            }
        }

        public override void OnMeeting()
        {
            Timer = 0f;
        }

        public Lurker(PlayerControl player)
        {
            Type = AddOns.Lurker;
            Player = player;
            Utils.SetupAddOnInfo(this);
            Timer = 0f;
        }

        public float Timer;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CooldownDecreaseSpeed;
        public static void SetupOptionItem()
        {
            Chance = AddOnOptionItem.Create(1300100, AddOns.Lurker, TabGroup.AddOns, false);
            Count = IntegerOptionItem.Create(1300101, "Max", new(1, 15, 1), 1, TabGroup.AddOns, false)
                .SetParent(Chance);
            CooldownDecreaseSpeed = IntegerOptionItem.Create(1300102, "Cooldown decrease speed", new(10, 300, 5), 65, TabGroup.AddOns, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Percent);
            Options.AddOnsChance[AddOns.Lurker] = Chance;
            Options.AddOnsCount[AddOns.Lurker] = Count;
        }
    }
}