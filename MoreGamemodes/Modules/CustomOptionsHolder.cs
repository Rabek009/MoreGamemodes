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
            Main.Preset4.Value, Main.Preset5.Value
        };

        public static readonly string[] gameModes =
        {
            "Classic", "Hide And Seek", "Shift And Seek", "Bomb Tag", "Random Items", "Battle Royale", "Speedrun"
        };

        public static readonly string[] speedrunBodyTypes =
        {
            "Crewmate", "Engineer", "Ghost"
        };

        //Main Settings
        public static OptionItem Gamemode;
        public static Gamemodes CurrentGamemode => (Gamemodes)Gamemode.GetValue();
        public static SpeedrunBodyTypes CurrentBodyType => (SpeedrunBodyTypes)BodyType.GetValue();
        public static OptionItem NoGameEnd;
        public static OptionItem CanUseColorCommand;
        public static OptionItem CanUseNameCommand;

        //Hide And Seek
        public static OptionItem HnSImpostorsBlindTime;
        public static OptionItem HnSImpostorsCanKillDuringBlind;
        public static OptionItem HnSImpostorsCanVent;
        public static OptionItem HnSImpostorsCanCloseDoors;

        //Shift And Seek
        public static OptionItem SnSImpostorsBlindTime;
        public static OptionItem SnSImpostorsCanKillDuringBlind;
        public static OptionItem SnSImpostorsCanVent;
        public static OptionItem SnSImpostorsCanCloseDoors;
        public static OptionItem ImpostorsAreVisible;

        //Bomb Tag
        public static OptionItem TeleportAfterExplosion;
        public static OptionItem ExplosionDelay;
        public static OptionItem PlayersWithBomb;
        public static OptionItem MaxPlayersWithBomb;

        //Random Items

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

        //Both
        public static OptionItem EnableTeleport;
        public static OptionItem EnableButton;
        public static OptionItem CanUseDuringSabotage;
        public static OptionItem EnableFinder;
        public static OptionItem EnableRope;
        public static OptionItem EnableStop;
        public static OptionItem CanBeGivenToCrewmate;
        public static OptionItem EnableNewsletter;

        //Battle Royale
        public static OptionItem Lives;
        public static OptionItem LivesVisibleToOthers;
        public static OptionItem ArrowToNearestPlayer;
        public static OptionItem GracePeriod;

        //Speedrun
        public static OptionItem BodyType;
        public static OptionItem TasksVisibleToOthers;

        //Additional Gamemodes
        public static OptionItem RandomSpawn;
        public static OptionItem TeleportAfterMeeting;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;
            _ = PresetOptionItem.Create(0, TabGroup.MainSettings)
                .SetColor(new Color32(204, 204, 0, 255))
                .SetHeader(true)
                .SetGamemode(Gamemodes.All);

            //Main Settings
            Gamemode = StringOptionItem.Create(1, "Gamemode", gameModes, 0, TabGroup.MainSettings, false)
                .SetGamemode(Gamemodes.All);
            NoGameEnd = BooleanOptionItem.Create(2, "No Game End", false, TabGroup.MainSettings, false)
                .SetGamemode(Gamemodes.All);
            CanUseColorCommand = BooleanOptionItem.Create(3, "Can Use /color Command", false, TabGroup.MainSettings, false)
                .SetGamemode(Gamemodes.All);
            CanUseNameCommand = BooleanOptionItem.Create(4, "Can Use /name Command", false, TabGroup.MainSettings, false)
                .SetGamemode(Gamemodes.All);

            //Hide And seek
            HnSImpostorsBlindTime = FloatOptionItem.Create(1001, "Impostors Blind Time", new(0f, 30f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek)
                .SetValueFormat(OptionFormat.Seconds);
            HnSImpostorsCanKillDuringBlind = BooleanOptionItem.Create(1002, "Impostors Can Kill During Blind", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);
            HnSImpostorsCanVent = BooleanOptionItem.Create(1003, "Impostors Can Vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);
            HnSImpostorsCanCloseDoors = BooleanOptionItem.Create(1004, "Impostors Can Close Doors", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.HideAndSeek);

            //Shift And Seek
            SnSImpostorsBlindTime = FloatOptionItem.Create(2001, "Impostors Blind Time", new(0f, 30f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek)
                .SetValueFormat(OptionFormat.Seconds);
            SnSImpostorsCanKillDuringBlind = BooleanOptionItem.Create(2002, "Impostors Can Kill During Blind", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            SnSImpostorsCanVent = BooleanOptionItem.Create(2003, "Impostors Can Vent", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            SnSImpostorsCanCloseDoors = BooleanOptionItem.Create(2004, "Impostors Can Close Doors", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);
            ImpostorsAreVisible = BooleanOptionItem.Create(2005, "Impostors Are Visible", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.ShiftAndSeek);

            //Bomb Tag
            TeleportAfterExplosion = BooleanOptionItem.Create(3001, "Teleport After Explosion", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag);
            ExplosionDelay = IntegerOptionItem.Create(3002, "Explosion Delay", new(5, 120, 1), 25, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Seconds);
            PlayersWithBomb = IntegerOptionItem.Create(3003, "Players with bomb", new(10, 95, 5), 35, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Percent);
            MaxPlayersWithBomb = IntegerOptionItem.Create(3004, "Max Players With Bomb", new(1, 15, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BombTag)
                .SetValueFormat(OptionFormat.Players);

            //Random Items

            //Crewmate
            EnableTimeSlower = BooleanOptionItem.Create(4000, "Enable Time Slower", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            DiscussionTimeIncrease = IntegerOptionItem.Create(4001, "Discussion Time Increase", new(1, 30, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            VotingTimeIncrease = IntegerOptionItem.Create(4002, "Voting Time Increase", new(1, 100, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            EnableKnowledge = BooleanOptionItem.Create(4003, "Enable Knowledge", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            CrewmatesSeeReveal = BooleanOptionItem.Create(4004, "Crewmates See Reveal", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableKnowledge);
            ImpostorsSeeReveal = BooleanOptionItem.Create(4005, "Impostors See Reveal", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableKnowledge);
            EnableShield = BooleanOptionItem.Create(4006, "Enable Shield", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            ShieldDuration = FloatOptionItem.Create(4007, "Shield Duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableShield);
            SeeWhoTriedKill = BooleanOptionItem.Create(4008, "See Who Tried Kill", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableShield);
            EnableGun = BooleanOptionItem.Create(4009, "Enable Gun", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            CanKillCrewmate = BooleanOptionItem.Create(4010, "Can Kill Crewmate", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableGun);
            MisfireKillsCrewmate = BooleanOptionItem.Create(4011, "Misfire Kills Crewmate", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableGun);
            EnableIllusion = BooleanOptionItem.Create(4012, "Enable Illusion", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            EnableRadar = BooleanOptionItem.Create(4013, "Enable Radar", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.cyan);
            RadarRange = FloatOptionItem.Create(4014, "Radar Range", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableRadar);

            //Impostor
            EnableTimeSpeeder = BooleanOptionItem.Create(5000, "Enable Time Speeder", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            DiscussionTimeDecrease = IntegerOptionItem.Create(5001, "Discussion Time Decrease", new(1, 30, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            VotingTimeDecrease = IntegerOptionItem.Create(5002, "Voting Time Decrease", new(1, 100, 1), 15, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            EnableFlash = BooleanOptionItem.Create(5003, "Enable Flash", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            FlashDuration = FloatOptionItem.Create(5004, "Flash Duration", new(1f, 30f, 0.25f), 5f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableFlash);
            ImpostorVisionInFlash = FloatOptionItem.Create(5005, "Impostor Vision In Flash", new(0.1f, 5f, 0.05f), 0.75f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableFlash);
            EnableHack = BooleanOptionItem.Create(5006, "Enable Hack", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            HackDuration = FloatOptionItem.Create(5007, "Hack Duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableHack);
            HackAffectsImpostors = BooleanOptionItem.Create(5008, "Hack Affects Impostors", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableHack);
            EnableCamouflage = BooleanOptionItem.Create(5009, "Enable Camouflage", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            CamouflageDuration = FloatOptionItem.Create(5010, "Camouflage Duration", new(1f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableCamouflage);
            EnableMultiTeleport = BooleanOptionItem.Create(5011, "Enable Multi Teleport", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            EnableBomb = BooleanOptionItem.Create(5012, "Enable Bomb", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.red);
            BombRadius = FloatOptionItem.Create(5013, "Bomb Radius", new(0.5f, 2.5f, 0.1f), 1f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableBomb);
            CanKillImpostors = BooleanOptionItem.Create(5014, "Can Kill Impostors", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableBomb);

            //Both
            EnableTeleport = BooleanOptionItem.Create(6000, "Enable Teleport", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableButton = BooleanOptionItem.Create(6001, "Enable Button", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            CanUseDuringSabotage = BooleanOptionItem.Create(6002, "Can Use During Sabotage", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableButton);
            EnableFinder = BooleanOptionItem.Create(6003, "Enable Finder", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableRope = BooleanOptionItem.Create(6004, "Enable Rope", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            EnableStop = BooleanOptionItem.Create(6005, "Enable Stop", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);
            CanBeGivenToCrewmate = BooleanOptionItem.Create(6006, "Can Be Given To Crewmate", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetParent(EnableStop);
            EnableNewsletter = BooleanOptionItem.Create(6007, "Enable Newsletter", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.RandomItems)
                .SetColor(Color.magenta);

            //Battle Royale
            Lives = IntegerOptionItem.Create(7000, "Lives", new(1, 99, 1), 3, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            LivesVisibleToOthers = BooleanOptionItem.Create(7001, "Lives Visible To Others", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            ArrowToNearestPlayer = BooleanOptionItem.Create(7002, "Arrow To Nearest Player", true, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale);
            GracePeriod = FloatOptionItem.Create(7003, "Grace Period", new(0f, 60f, 0.5f), 10f, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.BattleRoyale)
                .SetValueFormat(OptionFormat.Seconds);

            //Speedrun
            BodyType = StringOptionItem.Create(8000, "Body Type", speedrunBodyTypes, 0, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Speedrun);
            TasksVisibleToOthers = BooleanOptionItem.Create(8001, "Tasks Visible To Others", false, TabGroup.GamemodeSettings, false)
                .SetGamemode(Gamemodes.Speedrun);

            //Additional Gamemodes
            RandomSpawn = BooleanOptionItem.Create(100000, "Random Spawn", false, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All);
            TeleportAfterMeeting = BooleanOptionItem.Create(100001, "Teleport After Meeting", true, TabGroup.AdditionalGamemodes, false)
                .SetGamemode(Gamemodes.All)
                .SetParent(RandomSpawn);

            IsLoaded = true;
        }
    }
}