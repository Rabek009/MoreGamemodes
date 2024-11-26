using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

namespace MoreGamemodes
{
    public class SecurityGuard : CustomRole
    {
        public override void OnExile(NetworkedPlayerInfo exiled)
        {
            Cooldown = BlockVentCooldown.GetFloat();
            Player.RpcSetPetAbilityCooldown(true);
        }

        public override void OnHudUpate(HudManager __instance)
        {
            __instance.ReportButton.OverrideText(TranslationController.Instance.GetString(StringNames.ReportButton));
            __instance.PetButton.OverrideText("Block vent");
            if (AbilityUses < 1f || ClassicGamemode.instance.IsOnPetAbilityCooldown[Player.PlayerId])
                __instance.PetButton.SetDisabled();
            Vent vent = Player.GetClosestVent();
            if (Vector2.Distance(Player.transform.position, vent.transform.position) > 1.8f || PhysicsHelpers.AnythingBetween(Player.Collider, Player.Collider.bounds.center, vent.transform.position, Constants.ShipOnlyMask, false) || ClassicGamemode.instance.BlockedVents.Contains(vent.Id))
            {
                __instance.PetButton.SetDisabled();
                vent.SetOutline(false, false);
            }
            else
                vent.SetOutline(true, false);
        }

        public override void OnIntroDestroy()
        {
            Cooldown = 10f;
            Player.RpcSetPetAbilityCooldown(true);
        }

        public override void OnPet()
        {
            if (Cooldown > 0f || AbilityUses < 1f) return;
            Vent vent = Player.GetClosestVent();
            if ((Vector2.Distance(Player.transform.position, vent.transform.position) > 1.8f || PhysicsHelpers.AnythingBetween(Player.Collider, Player.Collider.bounds.center, vent.transform.position, Constants.ShipOnlyMask, false)) && !Player.AmOwner && !Main.IsModded[Player.PlayerId])
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) No vent nearby (!)"));
                return;
            }
            if (ClassicGamemode.instance.BlockedVents.Contains(vent.Id))
            {
                Player.Notify(Utils.ColorString(Color.red, "(!) This vent is already blocked (!)"));
                return;
            }
            var ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
            if (ventilationSystem != null)
                ventilationSystem.BootImpostorsFromVent(vent.Id);
            Player.RpcSetAbilityUses(AbilityUses - 1f);
            Utils.RpcCreateDisplay("<size=2><line-height=97%><cspace=0.16em><mark=#8f6647>W</mark><mark=#c0c0c0>W</mark><mark=#8f6647>WW</mark><mark=#c0c0c0>W</mark><mark=#8f6647>W<br>W</mark><mark=#808080>W</mark><mark=#8f6647>WW</mark><mark=#808080>W</mark><mark=#8f6647>W<br>W</mark><mark=#c0c0c0>W</mark><mark=#8f6647>WW</mark><mark=#c0c0c0>W</mark><mark=#8f6647>W<br>W</mark><mark=#c0c0c0>W</mark><mark=#8f6647>WW</mark><mark=#c0c0c0>W</mark><mark=#8f6647>W<br>W</mark><mark=#808080>W</mark><mark=#8f6647>WW</mark><mark=#808080>W</mark><mark=#8f6647>W<br>W</mark><mark=#c0c0c0>W</mark><mark=#8f6647>WW</mark><mark=#c0c0c0>W</mark><mark=#8f6647>W", vent.transform.position);
            GameManager.Instance.RpcBlockVent(vent.Id);
            Utils.SetAllVentInteractions();
            Cooldown = BlockVentCooldown.GetFloat();
            Player.RpcSetPetAbilityCooldown(true);
        }

        public override void OnGlobalMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (Player.Data.IsDead) return;
            if (SeeKillFlash())
            {
                Player.RpcReactorFlash(0.3f, Color);
                Player.Notify(Utils.ColorString(Color.red, "Someone died!"));
            }
        }

        public override void OnMeeting()
        {
            new LateTask(() => UsingCameras = false, 1f);
        }

        public override void OnFixedUpdate()
        {
            if (Cooldown > -1f)
                Cooldown -= Time.fixedDeltaTime;
            if (Cooldown <= 0f && Cooldown > -1f)
            {
                Cooldown = -1f;
                Player.RpcSetPetAbilityCooldown(false);
            }
        }

        public override void OnCompleteTask()
        {
            if (AbilityUseGainWithEachTaskCompleted.GetFloat() <= 0f) return;
            Player.RpcSetAbilityUses(AbilityUses + AbilityUseGainWithEachTaskCompleted.GetFloat());
        }

        public override bool OnUpdateSystem(ShipStatus __instance, SystemTypes systemType, MessageReader reader)
        {
            if (systemType == SystemTypes.Security)
            {
                if (reader.ReadByte() == 1)
                    UsingCameras = true;
                else
                    UsingCameras = false;
                return !HideCameraUsage.GetBool();
            }
            return true;
        }

        public override string GetNamePostfix()
        {
            return Utils.ColorString(Color.red, "\n<size=1.8>Block vent cooldown: " + (int)(Cooldown + 0.99f) + "s</size>");
        }

        public bool SeeKillFlash()
        {
            if (Player.Data.IsDead) return false;
            var mapId = Main.RealOptions.GetByte(ByteOptionNames.MapId);
            if (Utils.IsActive(SystemTypes.Comms) && mapId != 5) return false;
            if (mapId is 0 or 2 or 3 or 4 && UsingCameras)  
                return true;
            if (mapId == 1 && Vector2.Distance(Player.GetTruePosition(), new Vector2(16.22f, 5.82f)) <= 1.5f)
                return true;
            if (mapId == 5 && Vector2.Distance(Player.GetTruePosition(), new Vector2(6.20f, 0.10f)) <= 1.8f)
                return true;
            return false;
        }

        public SecurityGuard(PlayerControl player)
        {
            Role = CustomRoles.SecurityGuard;
            BaseRole = BaseRoles.Crewmate;
            Player = player;
            Utils.SetupRoleInfo(this);
            AbilityUses = InitialAbilityUseLimit.GetInt();
            Cooldown = 10f;
            UsingCameras = false;
        }

        public float Cooldown;
        public bool UsingCameras;

        public static OptionItem Chance;
        public static OptionItem Count;
        public static OptionItem BlockVentCooldown;
        public static OptionItem InitialAbilityUseLimit;
        public static OptionItem AbilityUseGainWithEachTaskCompleted;
        public static OptionItem HideCameraUsage;
        public static OptionItem CanBeGuessed;
        public static void SetupOptionItem()
        {
            Chance = IntegerOptionItem.Create(400100, "Security guard", new(0, 100, 5), 0, TabGroup.CrewmateRoles, false)
                .SetColor(CustomRolesHelper.RoleColors[CustomRoles.SecurityGuard])
                .SetValueFormat(OptionFormat.Percent);
            Count = IntegerOptionItem.Create(400101, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            BlockVentCooldown = FloatOptionItem.Create(400102, "Block vent cooldown", new(10f, 60f, 2.5f), 15f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance)
                .SetValueFormat(OptionFormat.Seconds);
            InitialAbilityUseLimit = FloatOptionItem.Create(400103, "Initial ability use limit", new(1f, 14f, 1f), 1f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            AbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(400104, "Ability use gain with each task completed", new(0f, 2f, 0.1f), 0.5f, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            HideCameraUsage = BooleanOptionItem.Create(400105, "Hide camera usage", false, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            CanBeGuessed = BooleanOptionItem.Create(400106, "Can be guessed", true, TabGroup.CrewmateRoles, false)
                .SetParent(Chance);
            Options.RolesChance[CustomRoles.SecurityGuard] = Chance;
            Options.RolesCount[CustomRoles.SecurityGuard] = Count;
        }
    }
}