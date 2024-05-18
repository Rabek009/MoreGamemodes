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
    public Harmony Harmony { get; } = new(Id);

    public ConfigEntry<string> ConfigName { get; private set; }

    public static BasePlugin Instance;
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
    public static Dictionary<byte, DeathReasons> AllPlayersDeathReason;
    public static List<string> PaintBattleThemes;
    public static List<(string, byte, string)> MessagesToSend;
    public static string LastResult;
    public static Dictionary<byte, RoleTypes> StandardRoles;
    public static Dictionary<byte, List<(string, float)>> ProximityMessages;
    public static Dictionary<(byte, byte), Color> NameColors;
    public static Dictionary<byte, bool> IsModded;
    public static Dictionary<byte, bool> Disconnected;

    public const string CurrentVersion = "1.3.1";

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
        JailbreakGamemode.instance = null;

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
        AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
        PaintBattleThemes = new List<string>()
        {
            "Crewmate", "Impostor", "Dead body", "Cosmos", "House", "Beach", "Sky", "Love", "Jungle", "Robot", "Fruits", "Vegetables", "Lake",
            "Rainbow", "Portal", "Planet", "Desert", "Taiga", "Airplane", "Cave", "Island", "Animal", "Anything", "Flag", "Jewellery", "Scary",
            "Shapeshifter", "Sword", "Treasure", "Your dream", "Celebrity", "Fungus"
        };
        MessagesToSend = new List<(string, byte, string)>();
        CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
        CheckProtectPatch.TimeSinceLastProtect = new Dictionary<byte, float>();
        LastResult = "";
        StandardRoles = new Dictionary<byte, RoleTypes>();
        ProximityMessages = new Dictionary<byte, List<(string, float)>>();
        NameColors = new Dictionary<(byte, byte), Color>();
        IsModded = new Dictionary<byte, bool>();
        Disconnected = new Dictionary<byte, bool>();
        CustomNetObject.CustomObjects = new List<CustomNetObject>();
        CustomNetObject.MaxId = -1;
        AntiBlackout.Reset();

        Harmony.PatchAll();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    static class OnGameCreated
    {
        public static void Prefix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost && __instance.PlayerId == 255)
            {
                __instance.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
                __instance.cosmetics.colorBlindText.color = Color.clear;
            }
        }
        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) return;
            GameStarted = false;
            if (__instance.AmOwner)
            {
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
                JailbreakGamemode.instance = null;
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
                AllPlayersDeathReason = new Dictionary<byte, DeathReasons>();
                MessagesToSend = new List<(string, byte, string)>();
                StandardRoles = new Dictionary<byte, RoleTypes>();
                CheckMurderPatch.TimeSinceLastKill = new Dictionary<byte, float>();
                CheckProtectPatch.TimeSinceLastProtect = new Dictionary<byte, float>();
                ProximityMessages = new Dictionary<byte, List<(string, float)>>();
                NameColors = new Dictionary<(byte, byte), Color>();
                IsModded = new Dictionary<byte, bool>();
                IsModded[__instance.PlayerId] = true;
                Disconnected = new Dictionary<byte, bool>();
                CustomNetObject.CustomObjects = new List<CustomNetObject>();
                CustomNetObject.MaxId = -1;
                AntiBlackout.Reset();
            }
            else if (__instance.PlayerId != 255)
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
    Jailbreak,
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
    Escaped,
}