using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public ConfigEntry<string> ConfigName { get; private set; }

    public static BasePlugin Instance;
    public static ConfigEntry<string> Preset1 { get; private set; }
    public static ConfigEntry<string> Preset2 { get; private set; }
    public static ConfigEntry<string> Preset3 { get; private set; }
    public static ConfigEntry<string> Preset4 { get; private set; }
    public static ConfigEntry<string> Preset5 { get; private set; }
    public static bool GameStarted;
    public static float Timer;

    public static Dictionary<byte, int> StandardColors;
    public static Dictionary<byte, string> StandardNames;
    public static Dictionary<byte, string> StandardHats;
    public static Dictionary<byte, string> StandardSkins;
    public static Dictionary<byte, string> StandardPets;
    public static Dictionary<byte, string> StandardVisors;
    public static Dictionary<byte, string> StandardNamePlates;
    public static Dictionary<byte, byte> AllShapeshifts;
    public static List<byte> Impostors;
    public static Dictionary<(byte, byte), string> LastNotifyNames;
    public static Dictionary<byte, bool> HasBomb;
    public static IGameOptions RealOptions;
    public static Dictionary<byte, Items> AllPlayersItems;
    public static float FlashTimer;
    public static float HackTimer;
    public static float CamouflageTimer;
    public static Dictionary<byte, float> ShieldTimer;
    public static bool IsMeeting;
    public static Dictionary<byte, int> Lives;
    public static Dictionary<byte, DeathReasons> AllPlayersDeathReason;
    public static float NoBombTimer;
    public static float NoItemTimer;
    public static bool SkipMeeting;
    public static float PaintTime;
    public static byte VotingPlayerId;
    public static float PaintBattleVotingTime;
    public static Dictionary<byte, bool> HasVoted;
    public static Dictionary<byte, (int, int)> PlayerVotes;
    public static List<string> PaintBattleThemes;
    public static string Theme;
    public static bool IsCreatingBody;
    public static Dictionary<byte, float> CreateBodyCooldown;
    public static List<(string, byte, string)> MessagesToSend;
    public static bool NoItemGive;
    public static List<(Vector2, float)> Traps;
    public static Dictionary<byte, float> CompassTimer;

    public const string CurrentVersion = "0.2.0";

    public override void Load()
    {
        Instance = this;
        Preset1 = Config.Bind("Preset Name Options", "Preset1", "Preset 1");
        Preset2 = Config.Bind("Preset Name Options", "Preset2", "Preset 2");
        Preset3 = Config.Bind("Preset Name Options", "Preset3", "Preset 3");
        Preset4 = Config.Bind("Preset Name Options", "Preset4", "Preset 4");
        Preset5 = Config.Bind("Preset Name Options", "Preset5", "Preset 5");

        GameStarted = false;
        Timer = 0f;
        StandardColors = new Dictionary<byte, int>();
        StandardNames = new Dictionary<byte, string>();
        StandardHats = new Dictionary<byte, string>();
        StandardSkins = new Dictionary<byte, string>();
        StandardPets = new Dictionary<byte, string>();
        StandardVisors = new Dictionary<byte, string>();
        StandardNamePlates = new Dictionary<byte, string>();
        AllShapeshifts = new Dictionary<byte, byte>();
        Impostors = new List<byte>();
        LastNotifyNames = new Dictionary<(byte, byte), string>();
        HasBomb= new Dictionary<byte, bool>();
        RealOptions = null;
        AllPlayersItems = new Dictionary<byte, Items>();
        FlashTimer = 0f;
        HackTimer = 0f;
        CamouflageTimer = 0f;
        ShieldTimer = new Dictionary<byte, float>();
        IsMeeting = false;
        Lives = new Dictionary<byte, int>();
        AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
        NoBombTimer = 0f;
        NoItemTimer = 0f;
        SkipMeeting = false;
        PaintTime = 0f;
        VotingPlayerId = 0;
        PaintBattleVotingTime = 0f;
        HasVoted = new Dictionary<byte, bool>();
        PlayerVotes = new Dictionary<byte, (int, int)>();
        PaintBattleThemes = new List<string>()
        {
            "Crewmate", "Impostor", "Dead Body", "Cosmos", "House", "Beach", "Sky", "Love", "Jungle", "Robot", "Fruits", "Vegetables", "Lake", "Rainbow", "Portal", "Planet", "Desert", "Taiga", "Airplane", "Cave", "Island"
        };
        Theme = "";
        IsCreatingBody = false;
        CreateBodyCooldown = new Dictionary<byte, float>();
        MessagesToSend = new List<(string, byte, string)>();
        NoItemGive = false;
        Traps = new List<(Vector2, float)>();
        CompassTimer = new Dictionary<byte, float>();

        Harmony.PatchAll();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    static class OnGameCreated
    {
        public static void Postfix(PlayerControl __instance)
        {
            GameStarted = false;
            if (__instance.AmOwner && __instance.PlayerId < 15)
            {
                Timer = 0f;
                StandardColors = new Dictionary<byte, int>();
                StandardNames = new Dictionary<byte, string>();
                StandardHats = new Dictionary<byte, string>();
                StandardSkins = new Dictionary<byte, string>();
                StandardPets = new Dictionary<byte, string>();
                StandardVisors = new Dictionary<byte, string>();
                StandardNamePlates = new Dictionary<byte, string>();
                Impostors = new List<byte>();
                LastNotifyNames = new Dictionary<(byte, byte), string>();
                HasBomb = new Dictionary<byte, bool>();
                AllPlayersItems = new Dictionary<byte, Items>();
                FlashTimer = 0f;
                HackTimer = 0f;
                CamouflageTimer = 0f;
                ShieldTimer = new Dictionary<byte, float>();
                IsMeeting = false;
                Lives = new Dictionary<byte, int>();
                AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
                NoBombTimer = 0f;
                NoItemTimer = 0f;
                SkipMeeting = false;
                PaintTime = 0f;
                VotingPlayerId = 0;
                PaintBattleVotingTime = 0f;
                HasVoted = new Dictionary<byte, bool>();
                PlayerVotes = new Dictionary<byte, (int, int)>();
                Theme = "";
                IsCreatingBody = false;
                CreateBodyCooldown = new Dictionary<byte, float>();
                MessagesToSend = new List<(string, byte, string)>();
                NoItemGive = false;
                Traps = new List<(Vector2, float)>();
                CompassTimer = new Dictionary<byte, float>();
            } 
        }
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
class ModManagerLateUpdatePatch
{
    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();
        LateTask.Update(Time.fixedDeltaTime);
    }
}

public enum Gamemodes
{
    Classic,
    HideAndSeek,
    ShiftAndSeek,
    BombTag,
    RandomItems,
    BattleRoyale,
    Speedrun,
    PaintBattle,
    All = int.MaxValue,
}
public enum Items
{
    None = 0,
    //crewmate
    TimeSlower,
    Knowledge,
    Shield,
    Gun,
    Illusion,
    Radar,
    Swap,
    //impostor
    TimeSpeeder,
    Flash,
    Hack,
    Camouflage,
    MultiTeleport,
    Bomb,
    Trap,
    //both
    Teleport,
    Button,
    Finder,
    Rope,
    Stop,
    Newsletter,
    Compass,
}
public enum SpeedrunBodyTypes
{
    Crewmate,
    Engineer,
    Ghost,
    All = int.MaxValue,
}
public enum DeathReasons
{
    Alive,
    Killed,
    Exiled,
    Disconnected,
    Command,
    Bombed,
    Misfire,
    Suicide,
    Trapped,
}