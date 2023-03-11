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
            "Classic", "Hide And Seek", "Shift And Seek", "Bomb Tag", "Random Items"
        };

        //Main Settings
        public static OptionItem Gamemode;
        public static Gamemodes CurrentGamemode => (Gamemodes)Gamemode.GetValue();
        public static OptionItem NoGameEnd;
        public static OptionItem CanUseColorCommand;
        public static OptionItem CanUseNameCommand;

        //HideAndSeek
        public static OptionItem HnSTeleportOnStart;
        public static OptionItem HnSImpostorsBlindTime;
        public static OptionItem HnSImpostorsCanKillDuringBlind;
        public static OptionItem HnSImpostorsCanVent;
        public static OptionItem HnSImpostorsCanCloseDoors;

        //ShiftAndSeek
        public static OptionItem SnSTeleportOnStart;
        public static OptionItem SnSImpostorsBlindTime;
        public static OptionItem SnSImpostorsCanKillDuringBlind;
        public static OptionItem SnSImpostorsCanVent;
        public static OptionItem SnSImpostorsCanCloseDoors;
        public static OptionItem ImpostorsAreVisible;

        //Bombtag
        public static OptionItem BTTeleportOnStart;
        public static OptionItem TeleportAfterExplosion;
        public static OptionItem ExplosionDelay;
        public static OptionItem PlayersWithBomb;
        public static OptionItem MaxPlayersWithBomb;

        //RandomItems
        public static OptionItem EnableTimeSlower;
        public static OptionItem DiscussionTimeIncrease;
        public static OptionItem VotingTimeIncrease;
        public static OptionItem EnableKnowlegde;
        public static OptionItem CrewmatesSeeReveal;
        public static OptionItem ImpostorsSeeReveal;
        public static OptionItem EnableShield;
        public static OptionItem ShieldDuration;
        public static OptionItem SeeWhoTriedKill;
        public static OptionItem EnableGun;
        public static OptionItem CanKillCrewmate;
        public static OptionItem MisfireKillsCrewmate;
        public static OptionItem EnableIllusion;
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
        public static OptionItem EnableTeleport;
        public static OptionItem EnableButton;
        public static OptionItem CanUseDuringSabotage;
        public static OptionItem EnableFinder;
        public static OptionItem EnableRope;
        public static OptionItem EnableStop;
        public static OptionItem CanBeGivenToCrewmate;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;
            _ = PresetOptionItem.Create(0, TabGroup.MainSettings)
                .SetColor(new Color32(204, 204, 0, 255))
                .SetHeader(true)
                .SetGameMode(Gamemodes.All);

            //Main Settings
            Gamemode = StringOptionItem.Create(1, "Gamemode", gameModes, 0, TabGroup.MainSettings, false)
                .SetGameMode(Gamemodes.All);
            NoGameEnd = BooleanOptionItem.Create(2, "No Game End", false, TabGroup.MainSettings, false);
            CanUseColorCommand = BooleanOptionItem.Create(3, "Can Use /color Command", false, TabGroup.MainSettings, false);
            CanUseNameCommand = BooleanOptionItem.Create(4, "Can Use /name Command", false, TabGroup.MainSettings, false);

            //Hide And seek
            HnSTeleportOnStart = BooleanOptionItem.Create(1000, "Teleport On Start", true, TabGroup.HideAndSeekSettings, false);
            HnSImpostorsBlindTime = FloatOptionItem.Create(1001, "Impostors Blind Time", new(0f, 30f, 0.5f), 10f, TabGroup.HideAndSeekSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            HnSImpostorsCanKillDuringBlind = BooleanOptionItem.Create(1002, "Impostors Can Kill During Blind", false, TabGroup.HideAndSeekSettings, false);
            HnSImpostorsCanVent = BooleanOptionItem.Create(1003, "Impostors Can Vent", false, TabGroup.HideAndSeekSettings, false);
            HnSImpostorsCanCloseDoors = BooleanOptionItem.Create(1004, "Impostors Can Close Doors", false, TabGroup.HideAndSeekSettings, false);

            //Shift And Seek
            SnSTeleportOnStart = BooleanOptionItem.Create(2000, "Teleport On Start", true, TabGroup.ShiftAndSeekSettings, false);
            SnSImpostorsBlindTime = FloatOptionItem.Create(2001, "Impostors Blind Time", new(0f, 30f, 0.5f), 10f, TabGroup.ShiftAndSeekSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            SnSImpostorsCanKillDuringBlind = BooleanOptionItem.Create(2002, "Impostors Can Kill During Blind", false, TabGroup.ShiftAndSeekSettings, false);
            SnSImpostorsCanVent = BooleanOptionItem.Create(2003, "Impostors Can Vent", false, TabGroup.ShiftAndSeekSettings, false);
            SnSImpostorsCanCloseDoors = BooleanOptionItem.Create(2004, "Impostors Can Close Doors", false, TabGroup.ShiftAndSeekSettings, false);
            ImpostorsAreVisible = BooleanOptionItem.Create(2005, "Impostors Are Visible", true, TabGroup.ShiftAndSeekSettings, false);

            //Bomb Tag
            BTTeleportOnStart = BooleanOptionItem.Create(3000, "Teleport On Start", false, TabGroup.BombTagSettings, false);
            TeleportAfterExplosion = BooleanOptionItem.Create(3001, "Teleport After Explosion", false, TabGroup.BombTagSettings, false);
            ExplosionDelay = IntegerOptionItem.Create(3002, "Explosion Delay", new(5, 120, 1), 25, TabGroup.BombTagSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            PlayersWithBomb = IntegerOptionItem.Create(3003, "Players with bomb", new(10, 95, 5), 35, TabGroup.BombTagSettings, false)
                .SetValueFormat(OptionFormat.Percent);
            MaxPlayersWithBomb = IntegerOptionItem.Create(3004, "Max Players With Bomb", new(1, 15, 1), 3, TabGroup.BombTagSettings, false)
                .SetValueFormat(OptionFormat.Players);

            //Random Items
            EnableTimeSlower = BooleanOptionItem.Create(4000, "Enable Time Slower", false, TabGroup.RandomItemsSettings, false);
            DiscussionTimeIncrease = IntegerOptionItem.Create(4001, "Discussion Time Increase", new(1, 30, 1), 3, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            VotingTimeIncrease = IntegerOptionItem.Create(4002, "Voting Time Increase", new(1, 100, 1), 15, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSlower);
            EnableKnowlegde = BooleanOptionItem.Create(4003, "Enable Knowlegde", false, TabGroup.RandomItemsSettings, false);
            CrewmatesSeeReveal = BooleanOptionItem.Create(4004, "Crewmates See Reveal", false, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableKnowlegde);
            ImpostorsSeeReveal = BooleanOptionItem.Create(4005, "Impostors See Reveal", true, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableKnowlegde);
            EnableShield = BooleanOptionItem.Create(4006, "Enable Shield", false, TabGroup.RandomItemsSettings, false);
            ShieldDuration = FloatOptionItem.Create(4007, "Shield Duration", new(1f, 60f, 0.5f), 10f, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableShield);
            SeeWhoTriedKill = BooleanOptionItem.Create(4008, "See Who Tried Kill", true, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableShield);
            EnableGun = BooleanOptionItem.Create(4009, "Enable Gun", false, TabGroup.RandomItemsSettings, false);
            CanKillCrewmate = BooleanOptionItem.Create(4010, "Can Kill Crewmate", false, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableGun);
            MisfireKillsCrewmate = BooleanOptionItem.Create(4011, "Misfire Kills Crewmate", false, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableGun);
            EnableIllusion = BooleanOptionItem.Create(4012, "Enable Illusion", false, TabGroup.RandomItemsSettings, false);
            EnableTimeSpeeder = BooleanOptionItem.Create(4013, "Enable Time Speeder", false, TabGroup.RandomItemsSettings, false);
            DiscussionTimeDecrease = IntegerOptionItem.Create(4014, "Discussion Time Decrease", new(1, 30, 1), 3, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            VotingTimeDecrease = IntegerOptionItem.Create(4015, "Voting Time Decrease", new(1, 100, 1), 15, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableTimeSpeeder);
            EnableFlash = BooleanOptionItem.Create(4016, "Enable Flash", false, TabGroup.RandomItemsSettings, false);
            FlashDuration = FloatOptionItem.Create(4017, "Flash Duration", new(1f, 30f, 0.25f), 5f, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableFlash);
            ImpostorVisionInFlash = FloatOptionItem.Create(4018, "Impostor Vision In Flash", new(0.1f, 5f, 0.05f), 0.75f, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Multiplier)
                .SetParent(EnableFlash);
            EnableHack = BooleanOptionItem.Create(4019, "Enable Hack", false, TabGroup.RandomItemsSettings, false);
            HackDuration = FloatOptionItem.Create(4020, "Hack Duration", new(1f, 60f, 0.5f), 10f, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableHack);
            HackAffectsImpostors = BooleanOptionItem.Create(4021, "Hack Affects Impostors", true, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableHack);
            EnableCamouflage = BooleanOptionItem.Create(4022, "Enable Camouflage", false, TabGroup.RandomItemsSettings, false);
            CamouflageDuration = FloatOptionItem.Create(4023, "Camouflage Duration", new(1f, 60f, 0.5f), 10f, TabGroup.RandomItemsSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableCamouflage);
            EnableMultiTeleport = BooleanOptionItem.Create(4024, "Enable Multi Teleport", false, TabGroup.RandomItemsSettings, false);
            EnableTeleport = BooleanOptionItem.Create(4025, "Enable Teleport", false, TabGroup.RandomItemsSettings, false);
            EnableButton = BooleanOptionItem.Create(4026, "Enable Button", false, TabGroup.RandomItemsSettings, false);
            CanUseDuringSabotage = BooleanOptionItem.Create(4027, "Can Use During Sabotage", true, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableButton);
            EnableFinder = BooleanOptionItem.Create(4028, "Enable Finder", false, TabGroup.RandomItemsSettings, false);
            EnableRope = BooleanOptionItem.Create(4029, "Enable Rope", false, TabGroup.RandomItemsSettings, false);
            EnableStop = BooleanOptionItem.Create(4030, "Enable Stop", false, TabGroup.RandomItemsSettings, false);
            CanBeGivenToCrewmate = BooleanOptionItem.Create(4031, "Can Be Given To Crewmate", true, TabGroup.RandomItemsSettings, false)
                .SetParent(EnableStop);

            IsLoaded = true;
        }
    }
}