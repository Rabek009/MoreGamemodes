using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using AmongUs.GameOptions;
using System.Reflection;
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
    public static bool CanGameEnd;
    public static float Timer;
    public static IEnumerable<PlayerControl> AllPlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(p => p != null);
    public static Dictionary<byte, int> StandardColors;
    public static Dictionary<byte, string> StandardNames;
    public static Dictionary<byte, string> StandardHats;
    public static Dictionary<byte, string> StandardSkins;
    public static Dictionary<byte, string> StandardPets;
    public static Dictionary<byte, string> StandardVisors;
    public static Dictionary<byte, byte> AllShapeshifts;
    public static List<byte> Impostors;
    public static Dictionary<byte, RoleTypes> StandardRoles;
    public static Dictionary<(byte, byte), string> LastNotifyNames;
    public static Dictionary<byte, bool> HasBomb;
    public static IGameOptions RealOptions;
    public static Dictionary<byte, Items> AllPlayersItems;
    public static float FlashTimer;
    public static float HackTimer;
    public static float CamouflageTimer;
    public static Dictionary<byte, byte> AllKills;
    public static Dictionary<byte, float> ShieldTimer;
    public static bool IsMeeting;
    public override void Load()
    {
        Instance = this;
        Preset1 = Config.Bind("Preset Name Options", "Preset1", "Preset 1");
        Preset2 = Config.Bind("Preset Name Options", "Preset2", "Preset 2");
        Preset3 = Config.Bind("Preset Name Options", "Preset3", "Preset 3");
        Preset4 = Config.Bind("Preset Name Options", "Preset4", "Preset 4");
        Preset5 = Config.Bind("Preset Name Options", "Preset5", "Preset 5");
        GameStarted = false;
        CanGameEnd = true;
        Timer = 0f;
        StandardColors = new Dictionary<byte, int>();
        StandardNames = new Dictionary<byte, string>();
        StandardHats = new Dictionary<byte, string>();
        StandardSkins = new Dictionary<byte, string>();
        StandardPets = new Dictionary<byte, string>();
        StandardVisors = new Dictionary<byte, string>();
        AllShapeshifts = new Dictionary<byte, byte>();
        Impostors = new List<byte>();
        StandardRoles = new Dictionary<byte, RoleTypes>();
        LastNotifyNames = new Dictionary<(byte, byte), string>();
        HasBomb= new Dictionary<byte, bool>();
        RealOptions = null;
        AllPlayersItems = new Dictionary<byte, Items>();
        FlashTimer = 0f;
        HackTimer = 0f;
        CamouflageTimer = 0f;
        AllKills = new Dictionary<byte, byte>();
        ShieldTimer = new Dictionary<byte, float>();
        IsMeeting = false;
        BepInEx.Logging.Logger.CreateLogSource("MoreGamemodes").LogInfo(string.Join("\n", Assembly.GetExecutingAssembly().GetManifestResourceNames()));
        Harmony.PatchAll();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    static class Start
    {
        public static void Postfix(PlayerControl __instance)
        {
            GameStarted = false;
            if (__instance.AmOwner)
            { 
                CanGameEnd = true;
                Timer = 0f;
                StandardColors = new Dictionary<byte, int>();
                StandardNames = new Dictionary<byte, string>();
                StandardHats = new Dictionary<byte, string>();
                StandardSkins = new Dictionary<byte, string>();
                StandardPets = new Dictionary<byte, string>();
                StandardVisors = new Dictionary<byte, string>();
                Impostors = new List<byte>();
                StandardRoles = new Dictionary<byte, RoleTypes>();
                LastNotifyNames = new Dictionary<(byte, byte), string>();
                HasBomb = new Dictionary<byte, bool>();
                AllPlayersItems = new Dictionary<byte, Items>();
                FlashTimer = 0f;
                HackTimer = 0f;
                CamouflageTimer = 0f;
                AllKills = new Dictionary<byte, byte>();
                ShieldTimer = new Dictionary<byte, float>();
                IsMeeting = false;
            }
            StandardNames[__instance.PlayerId] = "≋";
            StandardColors[__instance.PlayerId] = __instance.Data.DefaultOutfit.ColorId;
            StandardHats[__instance.PlayerId] = __instance.Data.DefaultOutfit.HatId;
            StandardSkins[__instance.PlayerId] = __instance.Data.DefaultOutfit.SkinId;
            StandardPets[__instance.PlayerId] = __instance.Data.DefaultOutfit.PetId;
            StandardVisors[__instance.PlayerId] = __instance.Data.DefaultOutfit.VisorId;
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
    All = int.MaxValue
}
public enum Items
{
    None = 0,
    //crewmate
    TimeSlower,
    Knowlegde,
    Shield,
    Gun,
    Illusion,
    //impostor
    TimeSpeeder,
    Flash,
    Hack,
    Camouflage,
    MultiTeleport,
    //both
    Teleport,
    Button,
    Finder,
    Rope,
    Stop,
}