using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

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
            "Classic", "Hide And Seek", "Shift And Seek", "Bomb Tag", "Random Items", "Battle Royale", "Speedrun", "Paint Battle", "Kill Or Die", "Zombies", "Jailbreak", "Deathrun"
        };

        public static readonly string[] speedrunBodyTypes =
        {
            "Crewmate", "Engineer", "Ghost"
        };

        public static readonly string[] trackingZombiesModes =
        {
            "None", "Nearest", "Every"
        };

        //Main settings
        public static OptionItem Gamemode;
        public static Gamemodes CurrentGamemode => (Gamemodes)Gamemode.GetValue();
        public static SpeedrunBodyTypes CurrentBodyType => (SpeedrunBodyTypes)BodyType.GetValue();
        public static TrackingZombiesModes CurrentTrackingZombiesMode => (TrackingZombiesModes)TrackingZombiesMode.GetValue();
        public static OptionItem NoGameEnd;
        public static OptionItem CanUseColorCommand;
        public static OptionItem EnableFortegreen;
        public static OptionItem CanUseNameCommand;
        public static OptionItem EnableNameRepeating;
        public static OptionItem MaximumNameLength;
        public static OptionItem CanUseTpoutCommand;

        //Hide and seek
        public static OptionItem HnSImpostorsBlindTime;
        public static OptionItem HnSImpostorsCanKillDuringBlind;
        public static OptionItem HnSImpostorsCanVent;
        public static OptionItem HnSImpostorsCanCloseDoors;
        public static OptionItem HnSImpostorsAreVisible;

        //Shift and seek
        public static OptionItem SnSImpostorsBlindTime;
        public static OptionItem SnSImpostorsCanKillDuringBlind;
        public static OptionItem SnSImpostorsCanVent;
        public static OptionItem SnSImpostorsCanCloseDoors;
        public static OptionItem SnSImpostorsAreVisible;
        public static OptionItem InstantShapeshift;

        //Bomb tag
        public static OptionItem TeleportAfterExplosion;
        public static OptionItem ExplosionDelay;
        public static OptionItem PlayersWithBomb;
        public static OptionItem MaxPlayersWithBomb;
        public static OptionItem ArrowToNearestNonBombed;
        public static OptionItem BtShowExplosionAnimation;

        //Random items

        //Crewmate
        public static OptionItem EnableTimeSlower;
        public static OptionItem DiscussionTimeIncrease;
        public static OptionItem VotingTimeIncrease;
        public static OptionItem EnableKnowledge;
        public static OptionItem CrewmatesSeeReveal;
        public static OptionItem ImpostorsSeeReveal;
        public static OptionItem EnableShield;
        public static OptionItem ShieldDuration;
        public static OptionItem SeeWhoTriedKill;
        public static OptionItem EnableGun;
        public static OptionItem CanKillCrewmate;
        public static OptionItem MisfireKillsCrewmate;
        public static OptionItem EnableIllusion;
        public static OptionItem EnableRadar;
        public static OptionItem RadarRange;
        public static OptionItem EnableSwap;
        public static OptionItem EnableMedicine;
        public static OptionItem DieOnRevive;

        //Impostor
        public static OptionItem EnableTimeSpeeder;
        public static OptionItem DiscussionTimeDecrease;
        public static OptionItem VotingTimeDecrease;
        public static OptionItem EnableFlash;
        public static OptionItem FlashDuration;
        public static OptionItem ImpostorVisionInFlash;
        public static OptionItem EnableHack;
        public static OptionItem HackDuration;
        public static OptionItem HackAffectsImpostors;
        public static OptionItem EnableCamouflage;
        public static OptionItem CamouflageDuration;
        public static OptionItem EnableMultiTeleport;
        public static OptionItem EnableBomb;
        public static OptionItem BombRadius;
        public static OptionItem CanKillImpostors;
        public static OptionItem RiShowExplosionAnimation;
        public static OptionItem EnableTrap;
        public static OptionItem TrapWaitTime;
        public static OptionItem TrapRadius;
        public static OptionItem CrewmatesSeeTrap;
        public static OptionItem ImpostorsSeeTrap;
        public static OptionItem EnableTeamChanger;
        public static OptionItem TargetGetsYourRole;

        //Both
        public static OptionItem EnableTeleport;
        public static OptionItem EnableButton;
        public static OptionItem CanUseDuringSabotage;
        public static OptionItem EnableFinder;
        public static OptionItem EnableRope;
        public static OptionItem EnableStop;
        public static OptionItem CanBeGivenToCrewmate;
        public static OptionItem EnableNewsletter;
        public static OptionItem EnableCompass;
        public static OptionItem CompassDuration;

        //Battle royale
        public static OptionItem Lives;
        public static OptionItem LivesVisibleToOthers;
        public static OptionItem ArrowToNearestPlayer;
        public static OptionItem GracePeriod;

        //Speedrun
        public static OptionItem BodyType;
        public static OptionItem TasksVisibleToOthers;

        //Paint battle
        public static OptionItem PaintingTime;
        public static OptionItem VotingTime;

        //Kill or die
        public static OptionItem TeleportAfterRound;
        public static OptionItem KillerBlindTime;
        public static OptionItem TimeToKill;
        public static OptionItem ArrowToNearestSurvivor;

        //Zombies
        public static OptionItem ZombieKillsTurnIntoZombie;
        public static OptionItem ZombieSpeed;
        public static OptionItem ZombieVision;
        public static OptionItem CanKillZombiesAfterTasks;
        public static OptionItem NumberOfKills;
        public static OptionItem ZombieBlindTime;
        public static OptionItem TrackingZombiesMode;
        public static OptionItem EjectedPlayersAreZombies;
        public static OptionItem ZoImpostorsCanVent;
        public static OptionItem ZombiesCanVent;

        //Jailbreak
        public static OptionItem PrisonerHealth;
        public static OptionItem PrisonerRegeneration;
        public static OptionItem PrisonerDamage;
        public static OptionItem GuardHealth;
        public static OptionItem GuardRegeneration;
        public static OptionItem GuardDamage;
        public static OptionItem WeaponDamage;
        public static OptionItem ScrewdriverPrice;
        public static OptionItem PrisonerWeaponPrice;
        public static OptionItem GuardWeaponPrice;
        public static OptionItem GuardOutfitPrice;
        public static OptionItem RespawnCooldown;
        public static OptionItem SearchCooldown;
        public static OptionItem MaximumPrisonerResources;
        public static OptionItem SpaceshipPartPrice;
        public static OptionItem RequiredSpaceshipParts;
        public static OptionItem PickaxePrice;
        public static OptionItem PickaxeSpeed;
        public static OptionItem PrisonTakeoverDuration;
        public static OptionItem BreathingMaskPrice;
        public static OptionItem EnergyDrinkPrice;
        public static OptionItem EnergyDrinkDuration;
        public static OptionItem EnergyDrinkSpeedIncrease;
        public static OptionItem GameTime;
        public static OptionItem EscapistsCanHelpOthers;
        public static OptionItem HelpCooldown;
        public static OptionItem GivenResources;
        public static OptionItem PrisonerArmorPrice;
        public static OptionItem GuardArmorPrice;
        public static OptionItem ArmorProtection;

        //Deathrun
        public static OptionItem RoundCooldown;
        public static OptionItem DisableMeetings;
        public static OptionItem DrImpostorsCanVent;
        public static OptionItem AmountOfTasks;

        //Additional gamemodes
        public static OptionItem RandomSpawn;
        public static OptionItem TeleportAfterMeeting;
        public static OptionItem RandomMap;
        public static OptionItem AddTheSkeld;
        public static OptionItem AddMiraHQ;
        public static OptionItem AddPolus;
        public static OptionItem AddDleksEht;
        public static OptionItem AddTheAirship;
        public static OptionItem AddTheFungle;
        public static OptionItem DisableGapPlatform;
        public static OptionItem MidGameChat;
        public static OptionItem ProximityChat;
        public static OptionItem MessagesRadius;
        public static OptionItem ImpostorRadio;
        public static OptionItem FakeShapeshiftAppearance;
        public static OptionItem DisableDuringCommsSabotage;
        public static OptionItem DisableZipline;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;
            _ = PresetOptionItem.Create(0, TabGroup.ModSettings)
                .SetColor(new Color32(204, 204, 0, 255))
                .SetHeader(true)
                .SetGamemode(Gamemodes.All);

            //Main settings
            Gamemode = StringOptionItem.Create(1, "Gamemode", gameModes, 0, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All);
            NoGameEnd = BooleanOptionItem.Create(2, "No game end", false, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All);
            CanUseColorCommand = BooleanOptionItem.Create(3, "Can use /color command", false, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All);
            EnableFortegreen = BooleanOptionItem.Create(4, "Enable fortegreen", false, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(CanUseColorCommand);
            CanUseNameCommand = BooleanOptionItem.Create(5, "Can use /name command", false, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All);
            EnableNameRepeating = BooleanOptionItem.Create(6, "Enable name repeating", false, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(CanUseNameCommand);
            MaximumNameLength = IntegerOptionItem.Create(7, "Maximum name length", new(10, 94, 1), 25, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(CanUseNameCommand);
            CanUseTpoutCommand = BooleanOptionItem.Create(8, "Can use /tpout command", true, TabGroup.ModSettings, false)
                .SetGamemode(Gamemodes.All);

            //Hide and seek
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
            TeleportAfterExplosion = BooleanOptionItem.Create(3001, "Teleport after explosion", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);
            ExplosionDelay = IntegerOptionItem.Create(3002, "Explosion delay", new(5, 120, 1), 25, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Seconds);
            PlayersWithBomb = IntegerOptionItem.Create(3003, "Players with bomb", new(10, 95, 5), 35, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Percent);
            MaxPlayersWithBomb = IntegerOptionItem.Create(3004, "Max players with bomb", new(1, 15, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Players);
            ArrowToNearestNonBombed = BooleanOptionItem.Create(3005, "Arrow to nearest non bombed", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);
            BtShowExplosionAnimation = BooleanOptionItem.Create(3006, "Show explosion animation", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);

            //Random items

            //Crewmate
            EnableTimeSlower = BooleanOptionItem.Create(4000, "Enable time slower", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            DiscussionTimeIncrease = IntegerOptionItem.Create(4001, "Discussion time increase", new(1, 30, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            VotingTimeIncrease = IntegerOptionItem.Create(4002, "Voting time increase", new(1, 100, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            EnableKnowledge = BooleanOptionItem.Create(4003, "Enable knowledge", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            CrewmatesSeeReveal = BooleanOptionItem.Create(4004, "Crewmates see reveal", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableKnowledge);
            ImpostorsSeeReveal = BooleanOptionItem.Create(4005, "Impostors see reveal", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableKnowledge);
            EnableShield = BooleanOptionItem.Create(4006, "Enable shield", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            ShieldDuration = FloatOptionItem.Create(4007, "Shield duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableShield);
            SeeWhoTriedKill = BooleanOptionItem.Create(4008, "See who tried kill", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableShield);
            EnableGun = BooleanOptionItem.Create(4009, "Enable gun", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            CanKillCrewmate = BooleanOptionItem.Create(4010, "Can kill crewmate", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableGun);
            MisfireKillsCrewmate = BooleanOptionItem.Create(4011, "Misfire kills crewmate", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableGun);
            EnableIllusion = BooleanOptionItem.Create(4012, "Enable illusion", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableRadar = BooleanOptionItem.Create(4013, "Enable radar", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            RadarRange = FloatOptionItem.Create(4014, "Radar range", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableRadar);
            EnableSwap = BooleanOptionItem.Create(4015, "Enable swap", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableMedicine = BooleanOptionItem.Create(4016, "Enable Medicine", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            DieOnRevive = BooleanOptionItem.Create(4017, "Die on revive", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableMedicine);

            //Impostor
            EnableTimeSpeeder = BooleanOptionItem.Create(5000, "Enable time speeder", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            DiscussionTimeDecrease = IntegerOptionItem.Create(5001, "Discussion time decrease", new(1, 30, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            VotingTimeDecrease = IntegerOptionItem.Create(5002, "Voting time decrease", new(1, 100, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            EnableFlash = BooleanOptionItem.Create(5003, "Enable flash", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            FlashDuration = FloatOptionItem.Create(5004, "Flash duration", new(1f, 30f, 0.25f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableFlash);
            ImpostorVisionInFlash = FloatOptionItem.Create(5005, "Impostor vision in flash", new(0.1f, 5f, 0.05f), 0.75f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableFlash);
            EnableHack = BooleanOptionItem.Create(5006, "Enable hack", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            HackDuration = FloatOptionItem.Create(5007, "Hack duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableHack);
            HackAffectsImpostors = BooleanOptionItem.Create(5008, "Hack affects impostors", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableHack);
            EnableCamouflage = BooleanOptionItem.Create(5009, "Enable camouflage", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            CamouflageDuration = FloatOptionItem.Create(5010, "Camouflage duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableCamouflage);
            EnableMultiTeleport = BooleanOptionItem.Create(5011, "Enable multi teleport", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableBomb = BooleanOptionItem.Create(5012, "Enable bomb", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            BombRadius = FloatOptionItem.Create(5013, "Bomb radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableBomb);
            CanKillImpostors = BooleanOptionItem.Create(5014, "Can kill impostors", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableBomb);
            RiShowExplosionAnimation = BooleanOptionItem.Create(5015, "Show explosion animation", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableBomb);
            EnableTrap = BooleanOptionItem.Create(5016, "Enable trap", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            TrapWaitTime = FloatOptionItem.Create(5017, "Trap wait duration", new(3f, 30f, 0.5f), 15f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTrap);
            TrapRadius = FloatOptionItem.Create(5018, "Trap radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableTrap);
            CrewmatesSeeTrap = BooleanOptionItem.Create(5019, "Crewmates see trap", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableTrap);
            ImpostorsSeeTrap = BooleanOptionItem.Create(5020, "Impostors see trap", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableTrap);
            EnableTeamChanger = BooleanOptionItem.Create(5021, "Enable team changer", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            TargetGetsYourRole = BooleanOptionItem.Create(5022, "Target gets your role", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableTeamChanger);

            //Both
            EnableTeleport = BooleanOptionItem.Create(6000, "Enable teleport", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableButton = BooleanOptionItem.Create(6001, "Enable button", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            CanUseDuringSabotage = BooleanOptionItem.Create(6002, "Can use during sabotage", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableButton);
            EnableFinder = BooleanOptionItem.Create(6003, "Enable finder", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableRope = BooleanOptionItem.Create(6004, "Enable rope", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableStop = BooleanOptionItem.Create(6005, "Enable stop", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            CanBeGivenToCrewmate = BooleanOptionItem.Create(6006, "Can be given to crewmate", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableStop);
            EnableNewsletter = BooleanOptionItem.Create(6007, "Enable newsletter", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableCompass = BooleanOptionItem.Create(6008, "Enable compass", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            CompassDuration = FloatOptionItem.Create(6009, "Compass duration", new(1f, 10f, 0.1f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableCompass);

            //Battle royale
            Lives = IntegerOptionItem.Create(7000, "Lives", new(1, 99, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            LivesVisibleToOthers = BooleanOptionItem.Create(7001, "Lives visible to others", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            ArrowToNearestPlayer = BooleanOptionItem.Create(7002, "Arrow to nearest player", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            GracePeriod = FloatOptionItem.Create(7003, "Grace period", new(0f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale)
                .SetValueFormat(OptionFormat.Seconds);

            //Speedrun
            BodyType = StringOptionItem.Create(8000, "Body type", speedrunBodyTypes, 0, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Speedrun);
            TasksVisibleToOthers = BooleanOptionItem.Create(8001, "Tasks visible to others", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Speedrun);

            //Paint battle
            PaintingTime = IntegerOptionItem.Create(9000, "Painting time", new(30, 255, 5), 180, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.PaintBattle)
                .SetValueFormat(OptionFormat.Seconds);
            VotingTime = IntegerOptionItem.Create(9001, "Voting time", new(5, 30, 1), 10, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.PaintBattle)
                .SetValueFormat(OptionFormat.Seconds);
            
            //Kill or die
            TeleportAfterRound = BooleanOptionItem.Create(10000, "Teleport after round", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie);
            KillerBlindTime = FloatOptionItem.Create(10001, "Killer blind time", new(1f, 15f, 0.5f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie)
                .SetValueFormat(OptionFormat.Seconds);
            TimeToKill = IntegerOptionItem.Create(10002, "Time to kill", new(5, 90, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie)
                .SetValueFormat(OptionFormat.Seconds);
            ArrowToNearestSurvivor = BooleanOptionItem.Create(10003, "Arrow to nearest survivor", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.KillOrDie);

            //Zombies
            ZombieKillsTurnIntoZombie = BooleanOptionItem.Create(11000, "Zombie kills turn into zombie", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            ZombieSpeed = FloatOptionItem.Create(11001, "Zombie speed", new(0.1f, 3f, 0.05f), 0.5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetValueFormat(OptionFormat.Multiplier);
            ZombieVision = FloatOptionItem.Create(11002, "Zombie vision", new(0.05f, 5f, 0.05f), 0.25f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetValueFormat(OptionFormat.Multiplier);
            CanKillZombiesAfterTasks = BooleanOptionItem.Create(11003, "Can kill zombies after tasks", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            NumberOfKills = IntegerOptionItem.Create(11004, "Number of kills", new(1, 15, 1), 1, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetParent(CanKillZombiesAfterTasks);
            ZombieBlindTime = FloatOptionItem.Create(11005, "Zombie blind time", new(3f, 15f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies)
                .SetValueFormat(OptionFormat.Seconds);
            TrackingZombiesMode = StringOptionItem.Create(11006, "Tracking zombie mode", trackingZombiesModes, 1, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            EjectedPlayersAreZombies = BooleanOptionItem.Create(11007, "Ejected players are zombies", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            ZoImpostorsCanVent = BooleanOptionItem.Create(11008, "Impostors can vent", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);
            ZombiesCanVent = BooleanOptionItem.Create(11009, "Zombies can vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Zombies);

            //Jailbreak
            PrisonerHealth = FloatOptionItem.Create(12000, "Prisoner health", new(10f, 250f, 5f), 50f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            PrisonerRegeneration = FloatOptionItem.Create(12001, "Prisoner regeneration", new(0f, 10f, 0.5f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerSecond);
            PrisonerDamage = FloatOptionItem.Create(12002, "Prisoner damage", new(0.5f, 20f, 0.5f), 6f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            GuardHealth = FloatOptionItem.Create(12003, "Guard health", new(10f, 250f, 5f), 100f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            GuardRegeneration = FloatOptionItem.Create(12004, "Guard regeneration", new(0f, 10f, 0.5f), 2f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerSecond);
            GuardDamage = FloatOptionItem.Create(12005, "Guard damage", new(0.5f, 20f, 0.5f), 12f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            WeaponDamage = FloatOptionItem.Create(12006, "Weapon damage", new(0.5f, 10f, 0.5f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            ScrewdriverPrice = IntegerOptionItem.Create(12007, "Screwdriver price", new(5, 150, 5), 45, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            PrisonerWeaponPrice = IntegerOptionItem.Create(12008, "Prisoner weapon price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            GuardWeaponPrice = IntegerOptionItem.Create(12009, "Guard weapon price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            GuardOutfitPrice = IntegerOptionItem.Create(12010, "Guard outfit price", new(10, 250, 10), 120, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            RespawnCooldown = FloatOptionItem.Create(12011, "Respawn cooldown", new(5f, 60f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            SearchCooldown = FloatOptionItem.Create(12012, "Search cooldown", new(10f, 60f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            MaximumPrisonerResources = IntegerOptionItem.Create(12013, "Maximum prisoner resources", new(50, 2500, 50), 200, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            SpaceshipPartPrice = IntegerOptionItem.Create(12014, "Spaceship part price", new(50, 1000, 50), 100, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            RequiredSpaceshipParts = IntegerOptionItem.Create(12015, "Required spaceship parts", new(2, 15, 1), 4, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            PickaxePrice = IntegerOptionItem.Create(12016, "Pickaxe price", new(1, 50, 1), 10, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            PickaxeSpeed = FloatOptionItem.Create(12017, "Pickaxe speed", new(0.5f, 10f, 0.1f), 2f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            PrisonTakeoverDuration = FloatOptionItem.Create(12018, "Prison takeover duration", new(5f, 60f, 2.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            BreathingMaskPrice = IntegerOptionItem.Create(12019, "Breathing mask price", new(10, 250, 10), 120, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            EnergyDrinkPrice = IntegerOptionItem.Create(12020, "Energy drink price", new(5, 180, 5), 60, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            EnergyDrinkDuration = FloatOptionItem.Create(12021, "Energy drink duration", new(3f, 30f, 0.5f), 12f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            EnergyDrinkSpeedIncrease = IntegerOptionItem.Create(12022, "Energy drink speed increase", new(10, 200, 10), 50, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Percent);
            GameTime = IntegerOptionItem.Create(12023, "Game time", new(300, 3600, 30), 600, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds);
            EscapistsCanHelpOthers = BooleanOptionItem.Create(12024, "Escapists can help others", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak);
            HelpCooldown = FloatOptionItem.Create(12025, "Help cooldown", new(10f, 180f, 2.5f), 20f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EscapistsCanHelpOthers);
            GivenResources = FloatOptionItem.Create(12026, "Given resources", new(10, 400, 10), 30, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetParent(EscapistsCanHelpOthers);
            PrisonerArmorPrice = IntegerOptionItem.Create(12027, "Prisoner armor price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            GuardArmorPrice = IntegerOptionItem.Create(12028, "Guard armor price", new(1, 50, 1), 20, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            ArmorProtection = FloatOptionItem.Create(12029, "Armor protection", new(0.5f, 10f, 0.5f), 3f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Jailbreak)
                .SetValueFormat(OptionFormat.PerLevel);
            
            //Deathrun
            RoundCooldown = FloatOptionItem.Create(13000, "Round cooldown", new(5f, 60f, 2.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun)
                .SetValueFormat(OptionFormat.Seconds);
            DisableMeetings = BooleanOptionItem.Create(13001, "Disable meetings", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun);
            DrImpostorsCanVent = BooleanOptionItem.Create(13002, "Impostors can vent", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun);
            AmountOfTasks = IntegerOptionItem.Create(13003, "Amount of tasks", new(1, 3, 1), 1, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Deathrun);

            //Additional gamemodes
            RandomSpawn = BooleanOptionItem.Create(100000, "Random spawn", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All);
            TeleportAfterMeeting = BooleanOptionItem.Create(100001, "Teleport after meeting", true, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomSpawn);
            RandomMap = BooleanOptionItem.Create(100100, "Random map", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All);
            AddTheSkeld = BooleanOptionItem.Create(100101, "Add the skeld", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomMap);
            AddMiraHQ = BooleanOptionItem.Create(100102, "Add mira HQ", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomMap);
            AddPolus = BooleanOptionItem.Create(100103, "Add polus", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomMap);
            AddDleksEht = BooleanOptionItem.Create(100104, "Add dleks eht", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomMap);
            AddTheAirship = BooleanOptionItem.Create(100105, "Add the airship", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomMap);
            AddTheFungle = BooleanOptionItem.Create(100106, "Add the fungle", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomMap);
            DisableGapPlatform = BooleanOptionItem.Create(100200, "Disable gap platform", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All);
            MidGameChat = BooleanOptionItem.Create(100300, "Mid game chat", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All);
            ProximityChat = BooleanOptionItem.Create(100301, "Proximity chat", true, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(MidGameChat);
            MessagesRadius = FloatOptionItem.Create(100302, "Messages radius", new(0.5f, 5f, 0.1f), 1f, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(ProximityChat);
            ImpostorRadio = BooleanOptionItem.Create(100303, "Impostor radio", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(ProximityChat);
            FakeShapeshiftAppearance = BooleanOptionItem.Create(100304, "Fake shapeshift appearance", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(ProximityChat);
            DisableDuringCommsSabotage = BooleanOptionItem.Create(100305, "Disable during comms sabotage", true, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(MidGameChat);
            DisableZipline = BooleanOptionItem.Create(100400, "Disable zipline", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All);
            IsLoaded = true;
        }
    }
}