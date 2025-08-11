using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    public class Camouflager : CustomRole
    {
        public override void OnHudUpate(HudManager __instance)
        {
            base.OnHudUpate(__instance);
            if (Player.Data.IsDead) return;
            __instance.AbilityButton.OverrideText("Camouflage");
        }

        public void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            if (AbilityDuration > 0f)
            {
                AbilityDuration = -1f;
                if (ClassicGamemode.instance.IsCamouflageActive)
                {
                    Utils.RevertCamouflage();
                    ClassicGamemode.instance.IsCamouflageActive = false;
                }
            }
        }

        public override void OnFixedUpdate()
        {
            if (Player.Data.IsDead) return;
            if (AbilityDuration > -1f)
            {
                AbilityDuration -= Time.fixedDeltaTime;
            }
            if (AbilityDuration <= 0f && AbilityDuration > -1f)
            {
                AbilityDuration = -1f;
                Player.RpcResetAbilityCooldown();
            }
        }

        public override bool OnCheckVanish()
        {
            if (ClassicGamemode.instance.IsCamouflageActive)
            {
                new LateTask(() => Player.RpcSetAbilityCooldown(0.001f), 0.2f);
                return false;
            }
            Utils.Camouflage();
            ClassicGamemode.instance.IsCamouflageActive = true;
            AbilityDuration = CamouflageDuration.GetFloat();
            new LateTask(() => Player.RpcSetAbilityCooldown(CamouflageDuration.GetFloat()), 0.2f);
            new LateTask(() =>
            {
                if (ClassicGamemode.instance.IsCamouflageActive)
                {
                    Utils.RevertCamouflage();
                    ClassicGamemode.instance.IsCamouflageActive = false;
                }
            }, CamouflageDuration.GetFloat());
            return false;
        }

        public override IGameOptions ApplyGameOptions(IGameOptions opt)
        {
            opt.SetFloat(FloatOptionNames.PhantomCooldown, CamouflageCooldown.GetFloat());
            return opt;
        }

        public override void OnRevive()
        {
            AbilityDuration = -1f;
        }

        public static void OnGlobalReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.GetRole().Role == CustomRoles.Camouflager)
                {
                    Camouflager camouflagerRole = pc.GetRole() as Camouflager;
                    if (camouflagerRole == null) continue;
                    camouflagerRole.OnReportDeadBody(reporter, target);
                }
            }
        }

        public Camouflager(PlayerControl player)
        {
            Role = CustomRoles.Camouflager;
            BaseRole = BaseRoles.Phantom;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = -1f;
            AbilityDuration = -1f;
        }

        public float AbilityDuration;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem CamouflageCooldown;
        public static OptionItem CamouflageDuration;
        public static void SetupOptionItem()
        {
            Chance = RoleOptionItem.Create(500400, CustomRoles.Camouflager, TabGroup.ImpostorRoles, false);
            Count = IntegerOptionItem.Create(500401, "Max", new(1, 15, 1), 1, TabGroup.ImpostorRoles, false)
                .SetParent(Chance);
            CamouflageCooldown = FloatOptionItem.Create(500402, "Camouflage cooldown", new(10f, 60f, 5f), 25f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            CamouflageDuration = FloatOptionItem.Create(500403, "Camouflage duration", new(5f, 30f, 1f), 10f, TabGroup.ImpostorRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            Options.RolesChance[CustomRoles.Camouflager] = Chance;
            Options.RolesCount[CustomRoles.Camouflager] = Count;
        }
    }
}