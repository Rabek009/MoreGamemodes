using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MoreGamemodes;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    // Translator
    public static BepInEx.Logging.ManualLogSource Logger;
    public const string DebugKeyHash = "c0fd562955ba56af3ae20d7ec9e64c664f0facecef4b3e366e109306adeae29d";
    public const string DebugKeySalt = "59687b";
    public static ConfigEntry<string> DebugKeyInput { get; private set; }

    public Harmony Harmony { get; } = new(Id);

    public ConfigEntry<string> ConfigName { get; private set; }

    public static BasePlugin Instance;
    //Preset Name Options
    public static ConfigEntry<string> Preset1 { get; private set; }
    public static ConfigEntry<string> Preset2 { get; private set; }
    public static ConfigEntry<string> Preset3 { get; private set; }
    public static ConfigEntry<string> Preset4 { get; private set; }
    public static ConfigEntry<string> Preset5 { get; private set; }
    public static ConfigEntry<string> Preset6 { get; private set; }
    public static ConfigEntry<string> Preset7 { get; private set; }
    public static ConfigEntry<string> Preset8 { get; private set; }
    public static ConfigEntry<string> Preset9 { get; private set; }
    public static ConfigEntry<string> Preset10 { get; private set; }
    //Other Configs
    public static ConfigEntry<string> WebhookURL { get; private set; }
    public static HashAuth DebugKeyAuth { get; private set; }
    
    public static bool GameStarted;
    public static float Timer;

    public static Dictionary<byte, byte> StandardColors;
    public static Dictionary<byte, string> StandardNames;
    public static Dictionary<byte, string> StandardHats;
    public static Dictionary<byte, string> StandardSkins;
    public static Dictionary<byte, string> StandardPets;
    public static Dictionary<byte, string> StandardVisors;
    public static Dictionary<byte, string> StandardNamePlates;
    public static Dictionary<byte, byte> AllShapeshifts;
    public static Dictionary<(byte, byte), string> LastNotifyNames;
    public static OptionBackupData RealOptions;
    public static bool IsMeeting;
    public static Dictionary<byte, DeathReasons> AllPlayersDeathReason;
    public static List<string> PaintBattleThemes;
    public static List<(string, byte, string)> MessagesToSend;
    public static string LastResult;
    public static Dictionary<byte, RoleTypes> StandardRoles;
    public static Dictionary<byte, List<(string, float)>> ProximityMessages;
    public static Dictionary<(byte, byte), Color> NameColors;
    public static Dictionary<byte, bool> IsModded;

    public const string CurrentVersion = "1.1.0";

    public override void Load()
    {
        Instance = this;
        Preset1 = Config.Bind("Preset Name Options", "Preset1", "Preset 1");
        Preset2 = Config.Bind("Preset Name Options", "Preset2", "Preset 2");
        Preset3 = Config.Bind("Preset Name Options", "Preset3", "Preset 3");
        Preset4 = Config.Bind("Preset Name Options", "Preset4", "Preset 4");
        Preset5 = Config.Bind("Preset Name Options", "Preset5", "Preset 5");
        Preset6 = Config.Bind("Preset Name Options", "Preset6", "Preset 6");
        Preset7 = Config.Bind("Preset Name Options", "Preset7", "Preset 7");
        Preset8 = Config.Bind("Preset Name Options", "Preset8", "Preset 8");
        Preset9 = Config.Bind("Preset Name Options", "Preset9", "Preset 9");
        Preset10 = Config.Bind("Preset Name Options", "Preset10", "Preset 10");
        WebhookURL = Config.Bind("Other", "WebhookURL", "none");

        // 認証関連-初期化
        DebugKeyAuth = new HashAuth(DebugKeyHash, DebugKeySalt);

        // 認証関連-認証
        DebugModeManager.Auth(DebugKeyAuth, DebugKeyInput.Value);

        CustomGamemode.Instance = null;
        ClassicGamemode.instance = null;
        HideAndSeekGamemode.instance = null;
        ShiftAndSeekGamemode.instance = null;
        BombTagGamemode.instance = null;
        RandomItemsGamemode.instance = null;
        BattleRoyaleGamemode.instance = null;
        SpeedrunGamemode.instance = null;
        PaintBattleGamemode.instance = null;
        KillOrDieGamemode.instance = null;
        ZombiesGamemode.instance = null;

        GameStarted = false;
        Timer = 0f;
        StandardColors = new Dictionary<byte, byte>();
        StandardNames = new Dictionary<byte, string>();
        StandardHats = new Dictionary<byte, string>();
        StandardSkins = new Dictionary<byte, string>();
        StandardPets = new Dictionary<byte, string>();
        StandardVisors = new Dictionary<byte, string>();
        StandardNamePlates = new Dictionary<byte, string>();
        AllShapeshifts = new Dictionary<byte, byte>();
        LastNotifyNames = new Dictionary<(byte, byte), string>();
        RealOptions = null;
        IsMeeting = false;
        AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
        PaintBattleThemes = new List<string>()
        {
            "Crewmate", "Impostor", "Dead Body", "Cosmos", "House", "Beach", "Sky", "Love", "Jungle", "Robot", "Fruits", "Vegetables", "Lake", "Rainbow", "Portal", "Planet", "Desert", "Taiga", "Airplane", "Cave", "Island", "Animal",
        };
        MessagesToSend = new List<(string, byte, string)>();
        CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
        LastResult = "";
        StandardRoles = new Dictionary<byte, RoleTypes>();
        ProximityMessages = new Dictionary<byte, List<(string, float)>>();
        NameColors = new Dictionary<(byte, byte), Color>();
        IsModded = new Dictionary<byte, bool>();

        Harmony.PatchAll();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    static class OnGameCreated
    {
        public static void Postfix(PlayerControl __instance)
        {
            GameStarted = false;
            if (__instance.AmOwner)
            {
                Timer = 0f;
                StandardColors = new Dictionary<byte, byte>();
                StandardNames = new Dictionary<byte, string>();
                StandardHats = new Dictionary<byte, string>();
                StandardSkins = new Dictionary<byte, string>();
                StandardPets = new Dictionary<byte, string>();
                StandardVisors = new Dictionary<byte, string>();
                StandardNamePlates = new Dictionary<byte, string>();
                LastNotifyNames = new Dictionary<(byte, byte), string>();
                RealOptions = null;
                IsMeeting = false;
                AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
                MessagesToSend = new List<(string, byte, string)>();
                StandardRoles = new Dictionary<byte, RoleTypes>();
                CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
                ProximityMessages = new Dictionary<byte, List<(string, float)>>();
                NameColors = new Dictionary<(byte, byte), Color>();
                IsModded = new Dictionary<byte, bool>();
                IsModded[__instance.PlayerId] = true;
            }
            else
                IsModded[__instance.PlayerId] = false;
        }
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
class ModManagerLateUpdatePatch
{
    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();
        LateTask.Update(Time.fixedDeltaTime / 2);
        CheckMurderPatch.Update();
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
    KillOrDie,
    Zombies,
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