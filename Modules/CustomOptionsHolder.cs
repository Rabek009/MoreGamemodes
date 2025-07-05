using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionHolder.cs
namespace MoreGamemodes
{
    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            taskOptionsLoad = Task.Run(Load);
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        public static void WaitOptionsLoad()
        {
            taskOptionsLoad.Wait();
        }
        public const int PresetId = 0;

        private static readonly string[] presets =
        {
            Main.Preset1.Value, Main.Preset2.Value, Main.Preset3.Value,
            Main.Preset4.Value, Main.Preset5.Value, Main.Preset6.Value,
            Main.Preset7.Value, Main.Preset8.Value, Main.Preset9.Value,
            Main.Preset10.Value
        };

        public static readonly string[] gameModes =
        {
            "Classic", "Hide And Seek", "Shift And Seek", "Bomb Tag", "Random Items", "Battle Royale", "Speedrun", "Paint Battle", "Kill Or Die", "Zombies", "Jailbreak", "Deathrun", "Base Wars", "Freeze Tag", "Color Wars"
        };

        public static readonly string[] speedrunBodyTypes =
        {
            "Crewmate", "Engineer", "Ghost"
        };

        public static readonly string[] trackingZombiesModes =
        {
            "None", "Nearest", "Every"
        };

        public static readonly string[] cheatingPenalties =
        {
            "Warn Host", "Warn Everyone", "Kick", "Ban"
        };

        public static Dictionary<CustomRoles, OptionItem> RolesChance;
        public static Dictionary<CustomRoles, OptionItem> RolesCount;
        public static Dictionary<AddOns, OptionItem> AddOnsChance;
        public static Dictionary<AddOns, OptionItem> AddOnsCount;

        //Main settings
        public static OptionItem Basic;
        public static OptionItem Gamemode;
        public static Gamemodes CurrentGamemode => (Gamemodes)Gamemode.GetValue();
        public static SpeedrunBodyTypes CurrentBodyType => (SpeedrunBodyTypes)BodyType.GetValue();
        public static TrackingZombiesModes CurrentTrackingZombiesMode => (TrackingZombiesModes)TrackingZombiesMode.GetValue();
        public static CheatingPenalties CurrentCheatingPenalty => (CheatingPenalties)CheatingPenalty.GetValue();
        public static OptionItem NoGameEnd;
        public static OptionItem AntiCheat;
        public static OptionItem CheatingPenalty;
        public static OptionItem Commands;
        public static OptionItem CanUseColorCommand;
        public static OptionItem EnableFortegreen;
        public static OptionItem CanUseNameCommand;
        public static OptionItem EnableNameRepeating;
        public static OptionItem MaximumNameLength;
        public static OptionItem CanUseTpoutCommand;
        public static OptionItem CanUseKcountCommand;
        public static OptionItem Amogus;

        //Hide and seek
        public static OptionItem Seekers;
        public static OptionItem HnSImpostorsBlindTime;
        public static OptionItem HnSImpostorsCanKillDuringBlind;
        public static OptionItem HnSImpostorsCanVent;
        public static OptionItem HnSImpostorsCanCloseDoors;
        public static OptionItem HnSImpostorsAreVisible;

        //Shift and seek
        public static OptionItem Shifters;
        public static OptionItem SnSImpostorsBlindTime;
        public static OptionItem SnSImpostorsCanKillDuringBlind;
        public static OptionItem SnSImpostorsCanVent;
        public static OptionItem SnSImpostorsCanCloseDoors;
        public static OptionItem SnSImpostorsAreVisible;
        public static OptionItem InstantShapeshift;

        //Bomb tag
        public static OptionItem BtGameplay;
        public static OptionItem TeleportAfterExplosion;
        public static OptionItem ExplosionDelay;
        public static OptionItem ArrowToNearestNonBombed;
        public static OptionItem BtExplosionCreatesHole;
        public static OptionItem BtHoleSpeedDecrease;
        public static OptionItem BombAssigment;
        public static OptionItem PlayersWithBomb;
        public static OptionItem MaxPlayersWithBomb;

        //Random items

        //Crewmate
        public static OptionItem TimeSlower;
        public static OptionItem EnableTimeSlower;
        public static OptionItem DiscussionTimeIncrease;
        public static OptionItem VotingTimeIncrease;
        public static OptionItem Knowledge;
        public static OptionItem EnableKnowledge;
        public static OptionItem CrewmatesSeeReveal;
        public static OptionItem ImpostorsSeeReveal;
        public static OptionItem Shield;
        public static OptionItem EnableShield;
        public static OptionItem ShieldDuration;
        public static OptionItem SeeWhoTriedKill;
        public static OptionItem Gun;
        public static OptionItem EnableGun;
        public static OptionItem CanKillCrewmate;
        public static OptionItem MisfireKillsCrewmate;
        public static OptionItem Illusion;
        public static OptionItem EnableIllusion;
        public static OptionItem RadarItem;
        public static OptionItem EnableRadar;
        public static OptionItem RadarRange;
        public static OptionItem Swap;
        public static OptionItem EnableSwap;
        public static OptionItem Medicine;
        public static OptionItem EnableMedicine;
        public static OptionItem DieOnRevive;

        //Impostor
        public static OptionItem TimeSpeeder;
        public static OptionItem EnableTimeSpeeder;
        public static OptionItem DiscussionTimeDecrease;
        public static OptionItem VotingTimeDecrease;
        public static OptionItem Flash;
        public static OptionItem EnableFlash;
        public static OptionItem FlashDuration;
        public static OptionItem ImpostorVisionInFlash;
        public static OptionItem Hack;
        public static OptionItem EnableHack;
        public static OptionItem HackDuration;
        public static OptionItem HackAffectsImpostors;
        public static OptionItem Camouflage;
        public static OptionItem EnableCamouflage;
        public static OptionItem CamouflageDuration;
        public static OptionItem MultiTeleport;
        public static OptionItem EnableMultiTeleport;
        public static OptionItem Bomb;
        public static OptionItem EnableBomb;
        public static OptionItem BombRadius;
        public static OptionItem CanKillImpostors;
        public static OptionItem RiExplosionCreatesHole;
        public static OptionItem RiHoleSpeedDecrease;
        public static OptionItem Trap;
        public static OptionItem EnableTrap;
        public static OptionItem TrapWaitTime;
        public static OptionItem TrapRadius;
        public static OptionItem CrewmatesSeeTrap;
        public static OptionItem ImpostorsSeeTrap;
        public static OptionItem TeamChanger;
        public static OptionItem EnableTeamChanger;
        public static OptionItem TargetGetsYourRole;

        //Both
        public static OptionItem Teleport;
        public static OptionItem EnableTeleport;
        public static OptionItem Button;
        public static OptionItem EnableButton;
        public static OptionItem CanUseDuringSabotage;
        public static OptionItem Finder;
        public static OptionItem EnableFinder;
        public static OptionItem Rope;
        public static OptionItem EnableRope;
        public static OptionItem Stop;
        public static OptionItem EnableStop;
        public static OptionItem CanBeGivenToCrewmate;
        public static OptionItem Newsletter;
        public static OptionItem EnableNewsletter;
        public static OptionItem Compass;
        public static OptionItem EnableCompass;
        public static OptionItem CompassDuration;
        public static OptionItem Booster;
        public static OptionItem EnableBooster;
        public static OptionItem BoosterDuration;
        public static OptionItem BoosterSpeedIncrease;

        //Battle royale
        public static OptionItem BattleRoyale;
        public static OptionItem Lives;
        public static OptionItem LivesVisibleToOthers;
        public static OptionItem ArrowToNearestPlayer;
        public static OptionItem BrGracePeriod;

        //Speedrun
        public static OptionItem Speedrun;
        public static OptionItem BodyType;
        public static OptionItem TasksVisibleToOthers;

        //Paint battle
        public static OptionItem PaintBattle;
        public static OptionItem PaintingTime;
        public static OptionItem VotingTime;

        //Kill or die
        public static OptionItem Killer;
        public static OptionItem TeleportAfterRound;
        public static OptionItem KillerBlindTime;
        public static OptionItem TimeToKill;
        public static OptionItem ArrowToNearestSurvivor;

        //Zombies
        public static OptionItem Conversion;
        public static OptionItem ZombieKillsTurnIntoZombie;
        public static OptionItem EjectedPlayersAreZombies;
        public static OptionItem Zombies;
        public static OptionItem ZombieSpeed;
        public static OptionItem ZombieVision;
        public static OptionItem ZombieBlindTime;
        public static OptionItem ZombiesCanVent;
        public static OptionItem ZoOther;
        public static OptionItem CanKillZombiesAfterTasks;
        public static OptionItem NumberOfKills;
        public static OptionItem TrackingZombiesMode;
        public static OptionItem ZoImpostorsCanVent;
        
        //Jailbreak
        public static OptionItem Prisoners;
        public static OptionItem PrisonerHealth;
        public static OptionItem PrisonerRegeneration;
        public static OptionItem PrisonerDamage;
        public static OptionItem MaximumPrisonerResources;
        public static OptionItem Guards;
        public static OptionItem GuardHealth;
        public static OptionItem GuardRegeneration;
        public static OptionItem GuardDamage;
        public static OptionItem SearchCooldown;
        public static OptionItem JbGameplay;
        public static OptionItem GameTime;
        public static OptionItem JbRespawnCooldown;
        public static OptionItem PrisonTakeoverDuration;
        public static OptionItem Weapon;
        public static OptionItem PrisonerWeaponPrice;
        public static OptionItem GuardWeaponPrice;
        public static OptionItem WeaponDamage;
        public static OptionItem Spaceship;
        public static OptionItem SpaceshipPartPrice;
        public static OptionItem RequiredSpaceshipParts;
        public static OptionItem Pickaxe;
        public static OptionItem PickaxePrice;
        public static OptionItem PickaxeSpeed;
        public static OptionItem EnergyDrink;
        public static OptionItem EnergyDrinkPrice;
        public static OptionItem EnergyDrinkDuration;
        public static OptionItem EnergyDrinkSpeedIncrease;
        public static OptionItem Armor;
        public static OptionItem PrisonerArmorPrice;
        public static OptionItem GuardArmorPrice;
        public static OptionItem ArmorProtection;
        public static OptionItem OtherItems;
        public static OptionItem ScrewdriverPrice;
        public static OptionItem GuardOutfitPrice;
        public static OptionItem BreathingMaskPrice;
        public static OptionItem Escapists;
        public static OptionItem EscapistsCanHelpOthers;
        public static OptionItem HelpCooldown;
        public static OptionItem GivenResources;
        
        //Deathrun
        public static OptionItem DrCrewmates;
        public static OptionItem DisableMeetings;
        public static OptionItem AmountOfTasks;
        public static OptionItem DrImpostors;
        public static OptionItem RoundCooldown;
        public static OptionItem DrImpostorsCanVent;

        //Base wars
        public static OptionItem PlayerStats;
        public static OptionItem StartingHealth;
        public static OptionItem StartingDamage;
        public static OptionItem Regeneration;
        public static OptionItem BwRespawnCooldown;
        public static OptionItem Turrets;
        public static OptionItem TurretHealth;
        public static OptionItem TurretDamage;
        public static OptionItem TurretRegeneration;
        public static OptionItem TurretSlowEnemies;
        public static OptionItem SpeedDecrease;
        public static OptionItem Bases;
        public static OptionItem BaseHealth;
        public static OptionItem BaseDamage;
        public static OptionItem BaseRegeneration;
        public static OptionItem RegenerationInBase;
        public static OptionItem CanTeleportToBase;
        public static OptionItem TeleportCooldown;
        public static OptionItem Levels;
        public static OptionItem ExpGainInMiddle;
        public static OptionItem ExpForKill;
        public static OptionItem HealthIncrease;
        public static OptionItem DamageIncrease;
        public static OptionItem SmallerTeamGetsLevel;

        //Freeze tag
        public static OptionItem Taggers;
        public static OptionItem FtImpostorsBlindTime;
        public static OptionItem ImpostorsCanFreezeDuringBlind;
        public static OptionItem FtImpostorsCanVent;
        public static OptionItem FtImpostorsCanCloseDoors;
        public static OptionItem Runners;
        public static OptionItem UnfreezeDuration;
        public static OptionItem UnfreezeRadius;
        public static OptionItem TaskCompleteTimeDuringFreeze;
        public static OptionItem ShowDangerMeter;

        //Color wars
        public static OptionItem Leaders;
        public static OptionItem LeadersAmount;
        public static OptionItem LeaderLives;
        public static OptionItem LivesVisibleToEnemies;
        public static OptionItem LeaderCooldown;
        public static OptionItem CwGracePeriod;
        public static OptionItem Players;
        public static OptionItem PlayerKillCooldown;
        public static OptionItem PlayerCanRespawn;
        public static OptionItem CwRespawnCooldown;
        public static OptionItem ArrowToLeader;
        public static OptionItem ArrowToNearestEnemyLeader;
        public static OptionItem NonTeamSpeed;
        public static OptionItem NonTeamVision;
        
        //Additional gamemodes
        public static OptionItem RandomSpawn;
        public static OptionItem EnableRandomSpawn;
        public static OptionItem TeleportAfterMeeting;
        public static OptionItem RandomMap;
        public static OptionItem EnableRandomMap;
        public static OptionItem AddTheSkeld;
        public static OptionItem AddMiraHQ;
        public static OptionItem AddPolus;
        public static OptionItem AddDleksEht;
        public static OptionItem AddTheAirship;
        public static OptionItem AddTheFungle;
        public static OptionItem DisableGapPlatform;
        public static OptionItem EnableDisableGapPlatform;
        public static OptionItem MidGameChat;
        public static OptionItem EnableMidGameChat;
        public static OptionItem ProximityChat;
        public static OptionItem MessagesRadius;
        public static OptionItem ImpostorRadio;
        public static OptionItem FakeShapeshiftAppearance;
        public static OptionItem DisableDuringCommsSabotage;
        public static OptionItem DisableZipline;
        public static OptionItem EnableDisableZipline;

        //Crewmates
        public static OptionItem CrewmateInvestigative;
        public static OptionItem CrewmateKilling;
        public static OptionItem CrewmateProtective;
        public static OptionItem CrewmateSupport;

        //Impostors
        public static OptionItem Impostors;
        public static OptionItem SeeTeammateRoles;
        public static OptionItem ImpostorConcealing;
        public static OptionItem ImpostorKilling;
        public static OptionItem ImpostorSupport;

        //Neutrals
        public static OptionItem Neutrals;
        public static OptionItem MinKillingNeutrals;
        public static OptionItem MaxKillingNeutrals;
        public static OptionItem MinEvilNeutrals;
        public static OptionItem MaxEvilNeutrals;
        public static OptionItem MinBenignNeutrals;
        public static OptionItem MaxBenignNeutrals;
        public static OptionItem NeutralBenign;
        public static OptionItem NeutralEvil;
        public static OptionItem NeutralKilling;

        //Add Ons
        public static OptionItem AddOns;
        public static OptionItem MaxAddOnsForPlayer;
        public static OptionItem HelpfulAddOns;
        public static OptionItem HarmfulAddOns;
        public static OptionItem ImpostorAddOns;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;
            RolesChance = new Dictionary<CustomRoles, OptionItem>();
            RolesCount = new Dictionary<CustomRoles, OptionItem>();
            AddOnsChance = new Dictionary<AddOns, OptionItem>();
            AddOnsCount = new Dictionary<AddOns, OptionItem>();

            _ = PresetOptionItem.Create(0, TabGroup.ModSettings)
                .SetColor(new Color32(204, 204, 0, 255))
                .SetHeader(true);

            //Main settings
            Basic = TextOptionItem.Create(1, "Basic", TabGroup.ModSettings)
                .SetColor(Color.yellow);
            Gamemode = StringOptionItem.Create(2, "Gamemode", gameModes, 0, TabGroup.ModSettings, false)
                .SetColor(Color.green);
            NoGameEnd = BooleanOptionItem.Create(3, "No game end", false, TabGroup.ModSettings, false)
                .SetColor(Color.red);
            AntiCheat = BooleanOptionItem.Create(4, "Anti Cheat", true, TabGroup.ModSettings, false)
                .SetColor(Color.blue);
            CheatingPenalty = StringOptionItem.Create(5, "Cheating Penalty", cheatingPenalties, 0, TabGroup.ModSettings, false)
                .SetParent(AntiCheat);
            Amogus = BooleanOptionItem.Create(6, "Amogus", false, TabGroup.ModSettings, false)
                .SetColor(Color.red);
            Commands = TextOptionItem.Create(10, "Commands", TabGroup.ModSettings)
                .SetColor(Color.cyan);
            CanUseColorCommand = BooleanOptionItem.Create(11, "Can use /color command", false, TabGroup.ModSettings, false);
            EnableFortegreen = BooleanOptionItem.Create(12, "Enable fortegreen", false, TabGroup.ModSettings, false)
                .SetParent(CanUseColorCommand);
            CanUseNameCommand = BooleanOptionItem.Create(13, "Can use /name command", false, TabGroup.ModSettings, false);
            EnableNameRepeating = BooleanOptionItem.Create(14, "Enable name repeating", false, TabGroup.ModSettings, false)
                .SetParent(CanUseNameCommand);
            MaximumNameLength = IntegerOptionItem.Create(15, "Maximum name length", new(10, 94, 1), 25, TabGroup.ModSettings, false)
                .SetParent(CanUseNameCommand);
            CanUseTpoutCommand = BooleanOptionItem.Create(16, "Can use /tpout command", true, TabGroup.ModSettings, false);
            CanUseKcountCommand = BooleanOptionItem.Create(17, "Can use /kcount command", true, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.Classic);

            //Hide and seek
            Seekers = TextOptionItem.Create(1000, "Seekers", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.HideAndSeek)
                .SetColor(Color.red);
            HnSImpostorsBlindTime = FloatOptionItem.Create(1001, "Impostors blind time", new(0f, 30f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek)
                .SetValueFormat(OptionFormat.Seconds);
            HnSImpostorsCanKillDuringBlind = BooleanOptionItem.Create(1002, "Impostors can kill during blind", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);
            HnSImpostorsCanVent = BooleanOptionItem.Create(1003, "Impostors can vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);
            HnSImpostorsCanCloseDoors = BooleanOptionItem.Create(1004, "Impostors can close doors", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);
            HnSImpostorsAreVisible = BooleanOptionItem.Create(1005, "Impostors are visible", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);

            //Shift and seek
            Shifters = TextOptionItem.Create(2000, "Shifters", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.ShiftAndSeek)
                .SetColor(Color.red);
            SnSImpostorsBlindTime = FloatOptionItem.Create(2001, "Impostors blind time", new(0f, 30f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek)
                .SetValueFormat(OptionFormat.Seconds);
            SnSImpostorsCanKillDuringBlind = BooleanOptionItem.Create(2002, "Impostors can kill during blind", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            SnSImpostorsCanVent = BooleanOptionItem.Create(2003, "Impostors can vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            SnSImpostorsCanCloseDoors = BooleanOptionItem.Create(2004, "Impostors can close doors", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            SnSImpostorsAreVisible = BooleanOptionItem.Create(2005, "Impostors are visible", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            InstantShapeshift = BooleanOptionItem.Create(2006, "Instant shapeshift", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);

            //Bomb tag
            BtGameplay = TextOptionItem.Create(3000, "Gameplay", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BombTag)
                .SetColor(Color.green);
            TeleportAfterExplosion = BooleanOptionItem.Create(3001, "Teleport after explosion", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);
            ExplosionDelay = IntegerOptionItem.Create(3002, "Explosion delay", new(5, 120, 1), 25, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Seconds);
            ArrowToNearestNonBombed = BooleanOptionItem.Create(3003, "Arrow to nearest non bombed", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);
            BtExplosionCreatesHole = BooleanOptionItem.Create(3004, "Explosion creates hole", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);
            BtHoleSpeedDecrease = IntegerOptionItem.Create(3005, "Hole speed decrease", new(0, 95, 5), 0, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Percent)
                .SetParent(BtExplosionCreatesHole);
            BombAssigment = TextOptionItem.Create(3010, "Bomb assigment", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BombTag)
                .SetColor(Color.gray);
            PlayersWithBomb = IntegerOptionItem.Create(3011, "Players with bomb", new(10, 95, 5), 35, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Percent);
            MaxPlayersWithBomb = IntegerOptionItem.Create(3012, "Max players with bomb", new(1, 15, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Players);

            //Random items

            //Crewmate
            TimeSlower = TextOptionItem.Create(4000, "Time slower", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableTimeSlower = BooleanOptionItem.Create(4001, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            DiscussionTimeIncrease = IntegerOptionItem.Create(4002, "Discussion time increase", new(1, 30, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            VotingTimeIncrease = IntegerOptionItem.Create(4003, "Voting time increase", new(1, 100, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            Knowledge = TextOptionItem.Create(4010, "Knowledge", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableKnowledge = BooleanOptionItem.Create(4011, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            CrewmatesSeeReveal = BooleanOptionItem.Create(4012, "Crewmates see reveal", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableKnowledge);
            ImpostorsSeeReveal = BooleanOptionItem.Create(4013, "Impostors see reveal", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableKnowledge);
            Shield = TextOptionItem.Create(4020, "Shield", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableShield = BooleanOptionItem.Create(4021, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            ShieldDuration = FloatOptionItem.Create(4022, "Shield duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableShield);
            SeeWhoTriedKill = BooleanOptionItem.Create(4023, "See who tried kill", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableShield);
            Gun = TextOptionItem.Create(4030, "Gun", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableGun = BooleanOptionItem.Create(4031, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            CanKillCrewmate = BooleanOptionItem.Create(4032, "Can kill crewmate", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableGun);
            MisfireKillsCrewmate = BooleanOptionItem.Create(4033, "Misfire kills crewmate", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableGun);
            Illusion = TextOptionItem.Create(4040, "Illusion", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableIllusion = BooleanOptionItem.Create(4041, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            RadarItem = TextOptionItem.Create(4050, "Radar", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableRadar = BooleanOptionItem.Create(4051, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            RadarRange = FloatOptionItem.Create(4052, "Radar range", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableRadar);
            Swap = TextOptionItem.Create(4060, "Swap", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableSwap = BooleanOptionItem.Create(4061, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            Medicine = TextOptionItem.Create(4070, "Medicine", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableMedicine = BooleanOptionItem.Create(4071, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            DieOnRevive = BooleanOptionItem.Create(4072, "Die on revive", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableMedicine);

            //Impostor
            TimeSpeeder = TextOptionItem.Create(5000, "Time speeder", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableTimeSpeeder = BooleanOptionItem.Create(5001, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            DiscussionTimeDecrease = IntegerOptionItem.Create(5002, "Discussion time decrease", new(1, 30, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            VotingTimeDecrease = IntegerOptionItem.Create(5003, "Voting time decrease", new(1, 100, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            Flash = TextOptionItem.Create(5010, "Flash", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableFlash = BooleanOptionItem.Create(5011, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            FlashDuration = FloatOptionItem.Create(5012, "Flash duration", new(1f, 30f, 0.25f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableFlash);
            ImpostorVisionInFlash = FloatOptionItem.Create(5013, "Impostor vision in flash", new(0.1f, 5f, 0.05f), 0.75f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableFlash);
            Hack = TextOptionItem.Create(5020, "Hack", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableHack = BooleanOptionItem.Create(5021, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            HackDuration = FloatOptionItem.Create(5022, "Hack duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableHack);
            HackAffectsImpostors = BooleanOptionItem.Create(5023, "Hack affects impostors", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableHack);
            Camouflage = TextOptionItem.Create(5030, "Camouflage", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableCamouflage = BooleanOptionItem.Create(5031, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            CamouflageDuration = FloatOptionItem.Create(5032, "Camouflage duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableCamouflage);
            MultiTeleport = TextOptionItem.Create(5040, "Multi teleport", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableMultiTeleport = BooleanOptionItem.Create(5041, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            Bomb = TextOptionItem.Create(5050, "Bomb", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableBomb = BooleanOptionItem.Create(5051, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            BombRadius = FloatOptionItem.Create(5052, "Bomb radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableBomb);
            CanKillImpostors = BooleanOptionItem.Create(5053, "Can kill impostors", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableBomb);
            RiExplosionCreatesHole = BooleanOptionItem.Create(5054, "Explosion creates hole", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableBomb);
            RiHoleSpeedDecrease = IntegerOptionItem.Create(5055, "Hole speed decrease", new(0, 95, 5), 0, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Percent)
                .SetParent(RiExplosionCreatesHole);
            Trap = TextOptionItem.Create(5060, "Trap", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableTrap = BooleanOptionItem.Create(5061, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            TrapWaitTime = FloatOptionItem.Create(5062, "Trap wait duration", new(3f, 30f, 0.5f), 15f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTrap);
            TrapRadius = FloatOptionItem.Create(5063, "Trap radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableTrap);
            CrewmatesSeeTrap = BooleanOptionItem.Create(5064, "Crewmates see trap", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableTrap);
            ImpostorsSeeTrap = BooleanOptionItem.Create(5065, "Impostors see trap", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableTrap);
            TeamChanger = TextOptionItem.Create(5070, "Team changer", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableTeamChanger = BooleanOptionItem.Create(5071, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            TargetGetsYourRole = BooleanOptionItem.Create(5072, "Target gets your role", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableTeamChanger);

            //Both
            Teleport = TextOptionItem.Create(6000, "Teleport", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableTeleport = BooleanOptionItem.Create(6001, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            Button = TextOptionItem.Create(6010, "Button", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableButton = BooleanOptionItem.Create(6011, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            CanUseDuringSabotage = BooleanOptionItem.Create(6012, "Can use during sabotage", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableButton);
            Finder = TextOptionItem.Create(6020, "Finder", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableFinder = BooleanOptionItem.Create(6021, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            Rope = TextOptionItem.Create(6030, "Rope", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableRope = BooleanOptionItem.Create(6031, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            Stop = TextOptionItem.Create(6040, "Stop", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableStop = BooleanOptionItem.Create(6041, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            CanBeGivenToCrewmate = BooleanOptionItem.Create(6042, "Can be given to crewmate", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableStop);
            Newsletter = TextOptionItem.Create(6050, "Newsletter", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableNewsletter = BooleanOptionItem.Create(6051, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            Compass = TextOptionItem.Create(6060, "Compass", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableCompass = BooleanOptionItem.Create(6061, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            CompassDuration = FloatOptionItem.Create(6062, "Compass duration", new(1f, 10f, 0.1f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableCompass);
            Booster = TextOptionItem.Create(6070, "Booster", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableBooster = BooleanOptionItem.Create(6071, "Enable", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems);
            BoosterDuration = FloatOptionItem.Create(6072, "Booster duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableBooster);
            BoosterSpeedIncrease = IntegerOptionItem.Create(6073, "Booster Speed Increase", new(10, 200, 10), 50, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Percent)
                .SetParent(EnableBooster);

            //Battle royale
            BattleRoyale = TextOptionItem.Create(7000, "Battle royale", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BattleRoyale)
                .SetColor(Color.red);
            Lives = IntegerOptionItem.Create(7001, "Lives", new(1, 99, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            LivesVisibleToOthers = BooleanOptionItem.Create(7002, "Lives visible to others", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            ArrowToNearestPlayer = BooleanOptionItem.Create(7003, "Arrow to nearest player", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            BrGracePeriod = FloatOptionItem.Create(7004, "Grace period", new(0f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale)
                .SetValueFormat(OptionFormat.Seconds);

            //Speedrun
            Speedrun = TextOptionItem.Create(8000, "Speedrun", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Speedrun)
                .SetColor(Color.yellow);
            BodyType = StringOptionItem.Create(8001, "Body type", speedrunBodyTypes, 0, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Speedrun);
            TasksVisibleToOthers = BooleanOptionItem.Create(8002, "Tasks visible to others", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Speedrun);

            //Paint battle
            PaintBattle = TextOptionItem.Create(9000, "Paint battle", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.PaintBattle)
                .SetColor(Color.gray);
            PaintingTime = IntegerOptionItem.Create(9001, "Painting time", new(30, 900, 10), 180, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.PaintBattle)
                .SetValueFormat(OptionFormat.Seconds);
            VotingTime = IntegerOptionItem.Create(9002, "Voting time", new(5, 30, 1), 10, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.PaintBattle)
                .SetValueFormat(OptionFormat.Seconds);
            
            //Kill or die
            Killer = TextOptionItem.Create(10000, "Killer", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.KillOrDie)
                .SetColor(Color.red);
            TeleportAfterRound = BooleanOptionItem.Create(10001, "Teleport after round", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie);
            KillerBlindTime = FloatOptionItem.Create(10002, "Killer blind time", new(1f, 15f, 0.5f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie)
                .SetValueFormat(OptionFormat.Seconds);
            TimeToKill = IntegerOptionItem.Create(10003, "Time to kill", new(5, 90, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie)
                .SetValueFormat(OptionFormat.Seconds);
            ArrowToNearestSurvivor = BooleanOptionItem.Create(10004, "Arrow to nearest survivor", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie);

            //Zombies
            Conversion = TextOptionItem.Create(11000, "Conversion", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Zombies)
                .SetColor(Palette.Brown);
            ZombieKillsTurnIntoZombie = BooleanOptionItem.Create(11001, "Zombie kills turn into zombie", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            EjectedPlayersAreZombies = BooleanOptionItem.Create(11002, "Ejected players are zombies", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            Zombies = TextOptionItem.Create(11010, "Zombies", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Zombies)
                .SetColor(Palette.PlayerColors[2]);
            ZombieSpeed = FloatOptionItem.Create(11011, "Zombie speed", new(0.1f, 3f, 0.05f), 0.5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetValueFormat(OptionFormat.Multiplier);
            ZombieVision = FloatOptionItem.Create(11012, "Zombie vision", new(0.05f, 5f, 0.05f), 0.25f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetValueFormat(OptionFormat.Multiplier);
            ZombieBlindTime = FloatOptionItem.Create(11013, "Zombie blind time", new(3f, 15f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetValueFormat(OptionFormat.Seconds);
            ZombiesCanVent = BooleanOptionItem.Create(11014, "Zombies can vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            ZoOther = TextOptionItem.Create(11020, "Other", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Zombies)
                .SetColor(Color.gray);
            CanKillZombiesAfterTasks = BooleanOptionItem.Create(11021, "Can kill zombies after tasks", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            NumberOfKills = IntegerOptionItem.Create(11022, "Number of kills", new(1, 15, 1), 1, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetParent(CanKillZombiesAfterTasks);
            TrackingZombiesMode = StringOptionItem.Create(11023, "Tracking zombie mode", trackingZombiesModes, 1, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            ZoImpostorsCanVent = BooleanOptionItem.Create(11024, "Impostors can vent", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);

            //Jailbreak
            Prisoners = TextOptionItem.Create(12000, "Prisoners", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Palette.Orange);
            PrisonerHealth = FloatOptionItem.Create(12001, "Prisoner health", new(10f, 250f, 5f), 50f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            PrisonerRegeneration = FloatOptionItem.Create(12002, "Prisoner regeneration", new(0f, 10f, 0.5f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerSecond);
            PrisonerDamage = FloatOptionItem.Create(12003, "Prisoner damage", new(0.5f, 20f, 0.5f), 6f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            MaximumPrisonerResources = IntegerOptionItem.Create(12004, "Maximum prisoner resources", new(50, 2500, 50), 200, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            Guards = TextOptionItem.Create(12010, "Guards", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.blue);
            GuardHealth = FloatOptionItem.Create(12011, "Guard health", new(10f, 250f, 5f), 100f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            GuardRegeneration = FloatOptionItem.Create(12012, "Guard regeneration", new(0f, 10f, 0.5f), 2f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerSecond);
            GuardDamage = FloatOptionItem.Create(12013, "Guard damage", new(0.5f, 20f, 0.5f), 12f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            SearchCooldown = FloatOptionItem.Create(12014, "Search cooldown", new(10f, 60f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            JbGameplay = TextOptionItem.Create(12020, "Gameplay", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.yellow);
            GameTime = IntegerOptionItem.Create(12021, "Game time", new(300, 3600, 30), 600, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            JbRespawnCooldown = FloatOptionItem.Create(12022, "Respawn cooldown", new(5f, 60f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            PrisonTakeoverDuration = FloatOptionItem.Create(12023, "Prison takeover duration", new(5f, 60f, 2.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            Weapon = TextOptionItem.Create(12030, "Weapon", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.red);
            PrisonerWeaponPrice = IntegerOptionItem.Create(12031, "Prisoner weapon price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.ResourcesPerLevel);
            GuardWeaponPrice = IntegerOptionItem.Create(12032, "Guard weapon price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.MoneyPerLevel);
            WeaponDamage = FloatOptionItem.Create(12033, "Weapon damage", new(0.5f, 10f, 0.5f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            Spaceship = TextOptionItem.Create(12040, "Spaceship", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.gray);
            SpaceshipPartPrice = IntegerOptionItem.Create(12041, "Spaceship part price", new(50, 1000, 50), 100, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Resources);
            RequiredSpaceshipParts = IntegerOptionItem.Create(12042, "Required spaceship parts", new(2, 15, 1), 4, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            Pickaxe = TextOptionItem.Create(12050, "Pickaxe", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.cyan);
            PickaxePrice = IntegerOptionItem.Create(12051, "Pickaxe price", new(1, 50, 1), 10, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.ResourcesPerLevel);
            PickaxeSpeed = FloatOptionItem.Create(12052, "Pickaxe speed", new(0.5f, 10f, 0.1f), 2f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PercentPerLevel);
            EnergyDrink = TextOptionItem.Create(12060, "Energy drink", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.yellow);
            EnergyDrinkPrice = IntegerOptionItem.Create(12061, "Energy drink price", new(5, 180, 5), 60, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Money);
            EnergyDrinkDuration = FloatOptionItem.Create(12062, "Energy drink duration", new(3f, 30f, 0.5f), 12f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            EnergyDrinkSpeedIncrease = IntegerOptionItem.Create(12063, "Energy drink speed increase", new(10, 200, 10), 50, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Percent);
            Armor = TextOptionItem.Create(12070, "Armor", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Palette.Brown);
            PrisonerArmorPrice = IntegerOptionItem.Create(12071, "Prisoner armor price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.ResourcesPerLevel);
            GuardArmorPrice = IntegerOptionItem.Create(12072, "Guard armor price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.MoneyPerLevel);
            ArmorProtection = FloatOptionItem.Create(12073, "Armor protection", new(0.5f, 10f, 0.5f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            OtherItems = TextOptionItem.Create(12080, "Other items", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.magenta);
            ScrewdriverPrice = IntegerOptionItem.Create(12081, "Screwdriver price", new(5, 150, 5), 45, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Resources);
            GuardOutfitPrice = IntegerOptionItem.Create(12082, "Guard outfit price", new(10, 250, 10), 120, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Resources);
            BreathingMaskPrice = IntegerOptionItem.Create(12083, "Breathing mask price", new(10, 250, 10), 120, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Resources);
            Escapists = TextOptionItem.Create(12090, "Escapists", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetColor(Color.green);
            EscapistsCanHelpOthers = BooleanOptionItem.Create(12091, "Escapists can help others", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            HelpCooldown = FloatOptionItem.Create(12092, "Help cooldown", new(10f, 180f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EscapistsCanHelpOthers);
            GivenResources = FloatOptionItem.Create(12093, "Given resources", new(10, 400, 10), 30, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetParent(EscapistsCanHelpOthers);
            
            //Deathrun
            DrCrewmates = TextOptionItem.Create(13000, "Crewmates", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Deathrun)
                .SetColor(Color.cyan);
            DisableMeetings = BooleanOptionItem.Create(13001, "Disable meetings", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun);
            AmountOfTasks = IntegerOptionItem.Create(13002, "Amount of tasks", new(1, 3, 1), 1, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun);
            DrImpostors = TextOptionItem.Create(13010, "Impostors", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.Deathrun)
                .SetColor(Color.red);
            RoundCooldown = FloatOptionItem.Create(13011, "Round cooldown", new(5f, 60f, 2.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun)
                .SetValueFormat(OptionFormat.Seconds);
            DrImpostorsCanVent = BooleanOptionItem.Create(13012, "Impostors can vent", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun);
            
            //Base wars
            PlayerStats = TextOptionItem.Create(14000, "Player stats", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BaseWars)
                .SetColor(Color.magenta);
            StartingHealth = FloatOptionItem.Create(14001, "Starting health", new(10f, 250f, 5f), 100f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            StartingDamage = FloatOptionItem.Create(14002, "Starting damage", new(1f, 25f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            Regeneration = FloatOptionItem.Create(14003, "Regeneration", new(0f, 20f, 0.5f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerSecond);
            BwRespawnCooldown = FloatOptionItem.Create(14004, "Respawn cooldown", new(5f, 60f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.Seconds);
            Turrets = TextOptionItem.Create(14010, "Turrets", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BaseWars)
                .SetColor(Color.red);
            TurretHealth = FloatOptionItem.Create(14011, "Turret health", new(100f, 1500f, 25f), 500f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            TurretDamage = FloatOptionItem.Create(14012, "Turret damage", new(5f, 100f, 2.5f), 25f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerSecond);
            TurretRegeneration = FloatOptionItem.Create(14013, "Turret regeneration", new(0f, 50f, 1f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerSecond);
            TurretSlowEnemies = BooleanOptionItem.Create(14014, "Turret slow enemies", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            SpeedDecrease = IntegerOptionItem.Create(14015, "Speed decrease", new(10, 100, 5), 50, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.Percent)
                .SetParent(TurretSlowEnemies);
            Bases = TextOptionItem.Create(14020, "Bases", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BaseWars)
                .SetColor(Color.green);
            BaseHealth = FloatOptionItem.Create(14021, "Base health", new(100f, 1500f, 25f), 1000f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            BaseDamage = FloatOptionItem.Create(14022, "Base damage", new(5f, 100f, 2.5f), 35f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerSecond);
            BaseRegeneration = FloatOptionItem.Create(14023, "Base regeneration", new(0f, 50f, 1f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerSecond);
            RegenerationInBase = FloatOptionItem.Create(14024, "Regeneration in base", new(0f, 100f, 2.5f), 30f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerSecond);
            CanTeleportToBase = BooleanOptionItem.Create(14025, "Can teleport to base", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            TeleportCooldown = FloatOptionItem.Create(14026, "Teleport cooldown", new(5f, 60f, 2.5f), 30f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(CanTeleportToBase);
            Levels = TextOptionItem.Create(14030, "Levels", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.BaseWars)
                .SetColor(Color.blue);
            ExpGainInMiddle = IntegerOptionItem.Create(14031, "Exp gain in middle", new(1, 50, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.ExperiencePerSecond);
            ExpForKill = IntegerOptionItem.Create(14032, "Exp for kill", new(5, 250, 5), 90, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.Experience);
            HealthIncrease = FloatOptionItem.Create(14033, "Health increase", new(1f, 50f, 1f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerLevel);
            DamageIncrease = FloatOptionItem.Create(14034, "Damage increase", new(1f, 25f, 0.5f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars)
                .SetValueFormat(OptionFormat.PerLevel);
            SmallerTeamGetsLevel = BooleanOptionItem.Create(14035, "Smaller team gets level", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BaseWars);
            
            //Freeze tag
            Taggers = TextOptionItem.Create(15000, "Taggers", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.FreezeTag)
                .SetColor(Color.red);
            FtImpostorsBlindTime = FloatOptionItem.Create(15001, "Impostors blind time", new(0f, 30f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag)
                .SetValueFormat(OptionFormat.Seconds);
            ImpostorsCanFreezeDuringBlind = BooleanOptionItem.Create(15002, "Impostors can freeze during blind", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag);
            FtImpostorsCanVent = BooleanOptionItem.Create(15003, "Impostors can vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag);
            FtImpostorsCanCloseDoors = BooleanOptionItem.Create(15004, "Impostors can close doors", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag);
            Runners = TextOptionItem.Create(15010, "Runners", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.FreezeTag)
                .SetColor(Color.green);
            UnfreezeDuration = FloatOptionItem.Create(15011, "Unfreeze duration", new(1f, 10f, 0.25f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag)
                .SetValueFormat(OptionFormat.Seconds);
            UnfreezeRadius = FloatOptionItem.Create(15012, "Unfreeze radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag)
                .SetValueFormat(OptionFormat.Multiplier);
            TaskCompleteTimeDuringFreeze = FloatOptionItem.Create(15013, "Task complete duration during freeze", new(5f, 180f, 2.5f), 60f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag)
                .SetValueFormat(OptionFormat.Seconds);
            ShowDangerMeter = BooleanOptionItem.Create(15014, "Show danger meter", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.FreezeTag);
            
            //Color wars
            Leaders = TextOptionItem.Create(16000, "Leaders", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.ColorWars)
                .SetColor(Color.yellow);
            LeadersAmount = IntegerOptionItem.Create(16001, "Leaders amount", new(2, 15, 1), 4, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Players);
            LeaderLives = IntegerOptionItem.Create(16002, "Leader lives", new(1, 99, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars);
            LivesVisibleToEnemies = BooleanOptionItem.Create(16003, "Lives visible to enemies", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars);
            LeaderCooldown = FloatOptionItem.Create(16004, "Leader cooldown", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Seconds);
            CwGracePeriod = FloatOptionItem.Create(16005, "Grace period", new(0f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Seconds);
            Players = TextOptionItem.Create(16010, "Players", TabGroup.GamemodeSettings)
                .SetGamemode(Gamemodes.ColorWars)
                .SetColor(Color.green);
            PlayerKillCooldown = FloatOptionItem.Create(16011, "Player kill cooldown", new FloatValueRule(1f, 60f, 0.5f), 15f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Seconds);
            PlayerCanRespawn = BooleanOptionItem.Create(16012, "Player can respawn", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars);
            CwRespawnCooldown = FloatOptionItem.Create(16013, "Respawn cooldown", new(5f, 60f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(PlayerCanRespawn);
            ArrowToLeader = BooleanOptionItem.Create(16014, "Arrow to leader", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars);
            ArrowToNearestEnemyLeader = BooleanOptionItem.Create(16015, "Arrow to nearest enemy leader", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars);
            NonTeamSpeed = FloatOptionItem.Create(16016, "Non team speed", new(0.1f, 3f, 0.05f), 0.4f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Multiplier);
            NonTeamVision = FloatOptionItem.Create(16017, "Non team vision", new(0.05f, 5f, 0.05f), 0.2f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ColorWars)
                .SetValueFormat(OptionFormat.Multiplier);
            
            //Additional gamemodes
            RandomSpawn = TextOptionItem.Create(50000, "Random spawn", TabGroup.AdditionalGamemodes)
                .SetColor(Color.yellow);
            EnableRandomSpawn = BooleanOptionItem.Create(50001, "Enable", false, TabGroup.AdditionalGamemodes, false);
            TeleportAfterMeeting = BooleanOptionItem.Create(50002, "Teleport after meeting", true, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomSpawn);
            RandomMap = TextOptionItem.Create(50100, "Random map", TabGroup.AdditionalGamemodes)
                .SetColor(Color.gray);
            EnableRandomMap = BooleanOptionItem.Create(50101, "Enable", false, TabGroup.AdditionalGamemodes, false);
            AddTheSkeld = BooleanOptionItem.Create(50102, "Add the skeld", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomMap);
            AddMiraHQ = BooleanOptionItem.Create(50103, "Add mira HQ", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomMap);
            AddPolus = BooleanOptionItem.Create(50104, "Add polus", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomMap);
            AddDleksEht = BooleanOptionItem.Create(50105, "Add dleks eht", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomMap);
            AddTheAirship = BooleanOptionItem.Create(50106, "Add the airship", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomMap);
            AddTheFungle = BooleanOptionItem.Create(50107, "Add the fungle", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableRandomMap);
            DisableGapPlatform = TextOptionItem.Create(50200, "Disable gap platform", TabGroup.AdditionalGamemodes)
                .SetColor(Color.red);
            EnableDisableGapPlatform = BooleanOptionItem.Create(50201, "Enable", false, TabGroup.AdditionalGamemodes, false);
            MidGameChat = TextOptionItem.Create(50300, "Mid game chat", TabGroup.AdditionalGamemodes)
                .SetColor(Color.green);
            EnableMidGameChat = BooleanOptionItem.Create(50301, "Enable", false, TabGroup.AdditionalGamemodes, false);
            ProximityChat = BooleanOptionItem.Create(50302, "Proximity chat", true, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableMidGameChat);
            MessagesRadius = FloatOptionItem.Create(50303, "Messages radius", new(0.5f, 5f, 0.1f), 1f, TabGroup.AdditionalGamemodes, false)
                .SetParent(ProximityChat);
            ImpostorRadio = BooleanOptionItem.Create(50304, "Impostor radio", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(ProximityChat);
            FakeShapeshiftAppearance = BooleanOptionItem.Create(50305, "Fake shapeshift appearance", false, TabGroup.AdditionalGamemodes, false)
                .SetParent(ProximityChat);
            DisableDuringCommsSabotage = BooleanOptionItem.Create(50306, "Disable during comms sabotage", true, TabGroup.AdditionalGamemodes, false)
                .SetParent(EnableMidGameChat);
            DisableZipline = TextOptionItem.Create(50400, "Disable zipline", TabGroup.AdditionalGamemodes)
                .SetColor(Color.red);
            EnableDisableZipline = BooleanOptionItem.Create(50401, "Enable", false, TabGroup.AdditionalGamemodes, false);

            //Crewmate roles
            CrewmateInvestigative = TextOptionItem.Create(100000, "Crewmate investigative", TabGroup.CrewmateRoles)
                .SetColor(Palette.CrewmateBlue);
            Investigator.SetupOptionItem();
            Mortician.SetupOptionItem();
            Sniffer.SetupOptionItem();
            Snitch.SetupOptionItem();
            CrewmateKilling = TextOptionItem.Create(200000, "Crewmate killing", TabGroup.CrewmateRoles)
                .SetColor(Palette.CrewmateBlue);
            NiceGuesser.SetupOptionItem();
            Shaman.SetupOptionItem();
            Sheriff.SetupOptionItem();
            CrewmateProtective = TextOptionItem.Create(300000, "Crewmate protective", TabGroup.CrewmateRoles)
                .SetColor(Palette.CrewmateBlue);
            Altruist.SetupOptionItem();
            Immortal.SetupOptionItem();
            Medic.SetupOptionItem();
            CrewmateSupport = TextOptionItem.Create(400000, "Crewmate support", TabGroup.CrewmateRoles)
                .SetColor(Palette.CrewmateBlue);
            Judge.SetupOptionItem();
            Mayor.SetupOptionItem();
            Mutant.SetupOptionItem();
            SecurityGuard.SetupOptionItem();

            //Impostor roles
            Impostors = TextOptionItem.Create(500000, "Impostors", TabGroup.ImpostorRoles)
                .SetColor(Palette.ImpostorRed);
            SeeTeammateRoles = BooleanOptionItem.Create(500001, "See teammate roles", true, TabGroup.ImpostorRoles, false);
            ImpostorConcealing = TextOptionItem.Create(500010, "Impostor concealing", TabGroup.ImpostorRoles)
                .SetColor(Palette.ImpostorRed);
            Droner.SetupOptionItem();
            Escapist.SetupOptionItem();
            TimeFreezer.SetupOptionItem();
            ImpostorKilling = TextOptionItem.Create(600000, "Impostor killing", TabGroup.ImpostorRoles)
                .SetColor(Palette.ImpostorRed);
            Archer.SetupOptionItem();
            EvilGuesser.SetupOptionItem();
            Hitman.SetupOptionItem();
            ImpostorSupport = TextOptionItem.Create(700000, "Impostor support", TabGroup.ImpostorRoles)
                .SetColor(Palette.ImpostorRed);
            Parasite.SetupOptionItem();
            Trapster.SetupOptionItem();
            Undertaker.SetupOptionItem();

            //Neutral roles
            Neutrals = TextOptionItem.Create(800000, "Neutrals", TabGroup.NeutralRoles)
                .SetColor(Color.gray);
            MinKillingNeutrals = IntegerOptionItem.Create(800001, "Min Killing neutrals", new(0, 15, 1), 0, TabGroup.NeutralRoles, false);
            MaxKillingNeutrals = IntegerOptionItem.Create(800002, "Max Killing neutrals", new(0, 15, 1), 0, TabGroup.NeutralRoles, false);
            MinEvilNeutrals = IntegerOptionItem.Create(800003, "Min Evil neutrals", new(0, 15, 1), 0, TabGroup.NeutralRoles, false);
            MaxEvilNeutrals = IntegerOptionItem.Create(800004, "Max Evil neutrals", new(0, 15, 1), 0, TabGroup.NeutralRoles, false);
            MinBenignNeutrals = IntegerOptionItem.Create(800005, "Min Benign neutrals", new(0, 15, 1), 0, TabGroup.NeutralRoles, false);
            MaxBenignNeutrals = IntegerOptionItem.Create(800006, "Max Benign neutrals", new(0, 15, 1), 0, TabGroup.NeutralRoles, false);
            NeutralBenign = TextOptionItem.Create(800010, "Neutral benign", TabGroup.NeutralRoles)
                .SetColor(Color.gray);
            Amnesiac.SetupOptionItem();
            Opportunist.SetupOptionItem();
            Romantic.SetupOptionItem();
            NeutralEvil = TextOptionItem.Create(900000, "Neutral evil", TabGroup.NeutralRoles)
                .SetColor(Color.gray);
            Executioner.SetupOptionItem();
            Jester.SetupOptionItem();
            SoulCollector.SetupOptionItem();
            NeutralKilling = TextOptionItem.Create(1000000, "Neutral killing", TabGroup.NeutralRoles)
                .SetColor(Color.gray);
            Arsonist.SetupOptionItem();
            Ninja.SetupOptionItem();
            Pelican.SetupOptionItem();
            SerialKiller.SetupOptionItem();

            //Add ons
            AddOns = TextOptionItem.Create(1100000, "Add ons", TabGroup.AddOns)
                .SetColor(Color.yellow);
            MaxAddOnsForPlayer = IntegerOptionItem.Create(1100001, "Max add ons for player", new(0, 15, 1), 1, TabGroup.AddOns, false);
            HelpfulAddOns = TextOptionItem.Create(1100010, "Helpful add ons", TabGroup.AddOns)
                .SetColor(Color.yellow);
            Bait.SetupOptionItem();
            Radar.SetupOptionItem();
            Watcher.SetupOptionItem();
            HarmfulAddOns = TextOptionItem.Create(1200000, "Harmful add ons", TabGroup.AddOns)
                .SetColor(Color.yellow);
            Blind.SetupOptionItem();
            Oblivious.SetupOptionItem();
            ImpostorAddOns = TextOptionItem.Create(1300000, "Impostor add ons", TabGroup.AddOns)
                .SetColor(Color.yellow);
            Lurker.SetupOptionItem();

            IsLoaded = true;
        }
    }
}