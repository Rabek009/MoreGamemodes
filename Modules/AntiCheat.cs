using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.QuickChat;

using Object = UnityEngine.Object;

// https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Modules/EAC.cs
namespace MoreGamemodes
{
    static class AntiCheat
    {
        public static Dictionary<byte, float> TimeSinceLastTask;
        public static List<byte> LobbyDeadBodies;
        public static Dictionary<byte, float> TimeSinceRoleChange;
        public static List<byte> ChangedTasks;
        public static List<uint> BannedPlayers;
        public static List<byte> RemovedBodies;
        public static Dictionary<byte, (byte, float)> TimeSinceLastStartCleaning;
        public static Dictionary<byte, (byte, float)> TimeSinceLastBootImpostors;
        public static Dictionary<(byte, RpcCalls), int> RpcsInCurrentSecond;
        public static float TimeSinceRateLimitReset;
        public static Dictionary<RpcCalls, int> RpcRateLimit = new()
        {
            {RpcCalls.PlayAnimation, 3},
            {RpcCalls.CompleteTask, 2},
            {RpcCalls.CheckColor, 10},
            {RpcCalls.SendChat, 2},
            {RpcCalls.SetScanner, 10},
            {RpcCalls.SetStartCounter, 15},
            {RpcCalls.EnterVent, 3},
            {RpcCalls.ExitVent, 3},
            {RpcCalls.SnapTo, 8},
            {RpcCalls.ClimbLadder, 1},
            {RpcCalls.UsePlatform, 20},
            {RpcCalls.SendQuickChat, 1},
            {RpcCalls.SetHatStr, 10},
            {RpcCalls.SetSkinStr, 10},
            {RpcCalls.SetPetStr, 10},
            {RpcCalls.SetVisorStr, 10},
            {RpcCalls.SetNamePlateStr, 10},
            {RpcCalls.CheckMurder, 25},
            {RpcCalls.CheckProtect, 25},
            {RpcCalls.Pet, 40},
            {RpcCalls.CancelPet, 40},
            {RpcCalls.CheckZipline, 1},
            {RpcCalls.CheckSpore, 5},
            {RpcCalls.CheckShapeshift, 10},
            {RpcCalls.CheckVanish, 10},
            {RpcCalls.CheckAppear, 10},
        };

        public static void Init()
        {
            TimeSinceLastTask = new Dictionary<byte, float>();
            LobbyDeadBodies = new List<byte>();
            TimeSinceRoleChange = new Dictionary<byte, float>();
            ChangedTasks = new List<byte>();
            BannedPlayers = new List<uint>();
            RemovedBodies = new List<byte>();
            TimeSinceLastStartCleaning = new Dictionary<byte, (byte, float)>();
            TimeSinceLastBootImpostors = new Dictionary<byte, (byte, float)>();
            RpcsInCurrentSecond = new Dictionary<(byte, RpcCalls), int>();
            TimeSinceRateLimitReset = 0f;
        }

        public static bool PlayerControlReceiveRpc(PlayerControl pc, byte callId, MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost || !Options.AntiCheat.GetBool()) return false;
            if (pc == null || reader == null) return false;
            if (pc.AmOwner) return false;
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;
            bool gameStarted = AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended;
            switch (callId)
            {
                case 101:
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();
                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);
                        if (!flag)
                        {
                            HandleCheat(pc, "AUM Chat");
                            return true;
                        }
                    }
                    catch
                    {
                    }
                    break;
                case unchecked((byte)42069):
                    try
                    {
                        var aumid = sr.ReadByte();
                        if (aumid == pc.PlayerId)
                        {
                            HandleCheat(pc, "AUM");
                            return true;
                        }
                    }
                    catch
                    {
                    }
                    break;
                case 119:
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();
                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);
                        if (!flag)
                        {
                            HandleCheat(pc, "KN Chat");
                            return true;
                        }
                    }
                    catch
                    {
                    }
                    break;
                case 250:
                    if (sr.BytesRemaining == 0)
                    {
                        HandleCheat(pc, "KN");
                        return true;
                    }
                    break;
                case unchecked((byte)420):
                    if (sr.BytesRemaining == 0)
                    {
                        HandleCheat(pc, "Sicko");
                        return true;
                    }
                    break;
            }
            if (callId <= 65)
            {
                if (!RpcsInCurrentSecond.ContainsKey((pc.PlayerId, rpc)))
                    RpcsInCurrentSecond[(pc.PlayerId, rpc)] = 0;
                ++RpcsInCurrentSecond[(pc.PlayerId, rpc)];
                if (RpcRateLimit.ContainsKey(rpc) && RpcsInCurrentSecond[(pc.PlayerId, rpc)] > RpcRateLimit[rpc])
                {
                    HandleCheat(pc, "Sending too many packets");
                    return true;
                }
            }
            else
                return false;
            switch (rpc)
            {
                case RpcCalls.PlayAnimation:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "PlayAnimation Rpc in lobby");
                        return true;
                    }
                    if (!GameManager.Instance.LogicOptions.GetVisualTasks())
                    {
                        HandleCheat(pc, "PlayAnimation Rpc with visuals off");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "PlayAnimation Rpc during meeting");
                        return true;
                    }
                    if (pc.GetSelfRole().IsImpostor() && !TimeSinceRoleChange.ContainsKey(pc.PlayerId))
                    {
                        HandleCheat(pc, "PlayAnimation Rpc as impostor");
                        return true;
                    }
                    var animation = sr.ReadByte();
                    if (!pc.HasTask((TaskTypes)animation) && !ChangedTasks.Contains(pc.PlayerId))
                    {
                        HandleCheat(pc, "Hack sent animation");
                        return true;
                    }
                    break;
                case RpcCalls.CompleteTask:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "CompleteTask Rpc in lobby");
                        return true;
                    }
                    if (TimeSinceLastTask.ContainsKey(pc.PlayerId) && TimeSinceLastTask[pc.PlayerId] < 0.1f)
                    {
                        HandleCheat(pc, "Auto complete tasks");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Doing task during meeting");
                        return true;
                    }
                    if (pc.GetSelfRole().IsImpostor() && !TimeSinceRoleChange.ContainsKey(pc.PlayerId))
                    {
                        HandleCheat(pc, "Doing task as impostor");
                        return true;
                    }
                    TimeSinceLastTask[pc.PlayerId] = 0f;
                    break;
                case RpcCalls.Exiled:
                case RpcCalls.SetName:
                case RpcCalls.SetColor:
                case RpcCalls.StartMeeting:
                case RpcCalls.SendChatNote:
                case RpcCalls.SetRole:
                case RpcCalls.ProtectPlayer:
                case RpcCalls.Shapeshift:
                case RpcCalls.UseZipline:
                case RpcCalls.TriggerSpores:
                case RpcCalls.RejectShapeshift:
                case RpcCalls.StartVanish:
                case RpcCalls.StartAppear:
                    HandleCheat(pc, "Invalid Rpc");
                    return true;
                case RpcCalls.CheckName:
                    if (Main.GameStarted)
                    {
                        HandleCheat(pc, "Changing name mid game");
                        return true;
                    }
                    var name = sr.ReadString();
                    if (name.Length > 10 || name.Contains("<") || name.Contains(">"))
                    {
                        HandleCheat(pc, "Invalid name");
                        return true;
                    }
                    break;
                case RpcCalls.CheckColor:
                    if (Main.GameStarted)
                    {
                        HandleCheat(pc, "Changing color mid game");
                        return true;
                    }
                    var colorId = sr.ReadByte();
                    if (colorId > 17)
                    {
                        HandleCheat(pc, "Invalid color");
                        return true;
                    }
                    break;
                case RpcCalls.ReportDeadBody:
                    var targetId = sr.ReadByte();
                    if (!gameStarted && !LobbyDeadBodies.Contains(targetId))
                    {
                        HandleCheat(pc, "Reporting in lobby");
                        return true;
                    }
                    if (GameManager.Instance.TryCast<HideAndSeekManager>())
                    {
                        HandleCheat(pc, "Reporting in hide n seek");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Reporting during meeting");
                        return true;
                    }
                    if (targetId != byte.MaxValue)
                    {
                        bool bodyExists = false;
                        foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
                        {
                            if (deadBody.ParentId == targetId)
                            {
                                bodyExists = true;
                                break;
                            }
                        }
                        if (!bodyExists && !LobbyDeadBodies.Contains(targetId) && !RemovedBodies.Contains(targetId) && targetId != pc.PlayerId && (!MeetingHud.Instance || MeetingHud.Instance.state != MeetingHud.VoteStates.Animating))
                        {
                            HandleCheat(pc, "Reporting non existing body");
                            return true;
                        }
                    }
                    break;
                case RpcCalls.MurderPlayer:
                    PlayerControl target = sr.ReadNetObject<PlayerControl>();
                    MurderResultFlags resultFlags = (MurderResultFlags)sr.ReadInt32();
                    if (!resultFlags.HasFlag(MurderResultFlags.FailedError) && !resultFlags.HasFlag(MurderResultFlags.FailedProtected) && target != null)
                    {
                        new LateTask(() => target.RpcRevive(), 0.1f);
                    }
                    HandleCheat(pc, "Invalid Rpc");
                    return true;
                case RpcCalls.SendChat:
                    var text = sr.ReadString();
                    if (text.Length > 300)
                    {
                        HandleCheat(pc, "Too long chat message");
                        return true;
                    }
                    break;
                case RpcCalls.SetScanner:
                    if (!GameManager.Instance.LogicOptions.GetVisualTasks())
                    {
                        HandleCheat(pc, "SetScanner Rpc with visuals off");
                        return true;
                    }
                    if (!sr.ReadBoolean()) break;
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "SetScanner Rpc in lobby");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "SetScanner Rpc during meeting");
                        return true;
                    }
                    if (pc.GetSelfRole().IsImpostor() && !TimeSinceRoleChange.ContainsKey(pc.PlayerId))
                    {
                        HandleCheat(pc, "SetScanner Rpc as impostor");
                        return true;
                    }
                    if (!pc.HasTask(TaskTypes.SubmitScan) && !ChangedTasks.Contains(pc.PlayerId))
                    {
                        HandleCheat(pc, "Hack sent scan");
                        return true;
                    }
                    break;
                case RpcCalls.SetStartCounter:
                    if (Main.GameStarted)
                    {
                        HandleCheat(pc, "SetStartCounter Rpc mid game");
                        return true;
                    }
                    sr.ReadPackedInt32();
                    sbyte startCounter = sr.ReadSByte();
                    if (startCounter != -1)
                    {
                        HandleCheat(pc, "Invalid SetStartCounter Rpc");
                        return true;
                    }
                    break;
                case RpcCalls.UsePlatform:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using platform in lobby");
                        return true;
                    }
                    if (GameManager.Instance.LogicOptions.MapId != 4)
                    {
                        HandleCheat(pc, "Using platform on wrong map");
                        return true;
                    }
                    if (GameManager.Instance.TryCast<HideAndSeekManager>())
                    {
                        HandleCheat(pc, "Using platform in hide n seek");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Using platform during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.SendQuickChat:
                    QuickChatPhraseType quickChatPhraseType = (QuickChatPhraseType)sr.ReadByte();
                    if (quickChatPhraseType == QuickChatPhraseType.Empty)
                    {
                        HandleCheat(pc, "Empty message in quick chat");
                        return true;
                    }
                    if (quickChatPhraseType == QuickChatPhraseType.PlayerId)
                    {
                        byte playerID = sr.ReadByte();
                        if (playerID == 255)
                        {
                            HandleCheat(pc, "Sending invalid player in quick chat");
                            return true;
                        }
                        if (gameStarted && GameData.Instance.GetPlayerById(playerID) == null)
                        {
                            HandleCheat(pc, "Sending non existing player in quick chat");
                            return true;
                        }
                    }
                    if (quickChatPhraseType != QuickChatPhraseType.ComplexPhrase) break;
                    sr.ReadUInt16();
                    int num = sr.ReadByte();
                    if (num == 0)
                    {
                        HandleCheat(pc, "Complex phrase without arguments");
                        return true;
                    }
                    if (num > 3)
                    {
                        HandleCheat(pc, "Trying to crash or lag other players");
                        return true;
                    }
                    break;
                case RpcCalls.SetLevel:
                case RpcCalls.SetHatStr:
                case RpcCalls.SetSkinStr:
                case RpcCalls.SetPetStr:
                case RpcCalls.SetVisorStr:
                case RpcCalls.SetNamePlateStr:
                    if (Main.GameStarted)
                    {
                        HandleCheat(pc, "Set outfit/level mid game");
                        return true;
                    }
                    break;
                case RpcCalls.CheckMurder:
                    PlayerControl target2 = sr.ReadNetObject<PlayerControl>();
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using kill button in lobby");
                        return true;
                    }
                    if (target2 != null && target2 == pc)
                    {
                        HandleCheat(pc, "Using kill button on self");
                        return true;
                    }
                    if (target2 == null) break;
                    if (CustomGamemode.Instance.Gamemode is Gamemodes.BombTag or Gamemodes.BattleRoyale or Gamemodes.PaintBattle or Gamemodes.KillOrDie or Gamemodes.Jailbreak or Gamemodes.BaseWars or Gamemodes.ColorWars) break;
                    var targetRole = Main.DesyncRoles.ContainsKey((target2.PlayerId, pc.PlayerId)) ? Main.DesyncRoles[(target2.PlayerId, pc.PlayerId)] : Main.StandardRoles[target2.PlayerId];
                    if (!pc.GetSelfRole().IsImpostor() && !Main.IsModded[pc.PlayerId])
                    {
                        if (!TimeSinceRoleChange.ContainsKey(pc.PlayerId) || TimeSinceRoleChange[pc.PlayerId] > 5f)
                            HandleCheat(pc, "Trying to kill as crewmate");
                        pc.RpcMurderPlayer(target2, false);
                        return true;
                    }
                    if (targetRole.IsImpostor() && !Main.IsModded[pc.PlayerId])
                    {
                        if (!TimeSinceRoleChange.ContainsKey(target2.PlayerId) || TimeSinceRoleChange[target2.PlayerId] > 5f)
                            HandleCheat(pc, "Trying to kill impostor");
                        return true;
                    }
                    if (pc.Data.Role is PhantomRole phantomRole && phantomRole.IsInvisible)
                    {
                        HandleCheat(pc, "Trying to kill while invisible");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Trying to kill during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.CheckProtect:
                    PlayerControl target3 = sr.ReadNetObject<PlayerControl>();
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using protect button in lobby");
                        return true;
                    }
                    if (target3 != null && target3 == pc)
                    {
                        HandleCheat(pc, "Using protect button on self");
                        return true;
                    }
                    if (target3 == null) break;
                    if (pc.Data.Role.Role != RoleTypes.GuardianAngel)
                    {
                        if (!TimeSinceRoleChange.ContainsKey(pc.PlayerId) || TimeSinceRoleChange[pc.PlayerId] > 5f)
                            HandleCheat(pc, "Trying to protect as not guardian angel");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Trying to protect during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.CheckZipline:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using zipline in lobby");
                        return true;
                    }
                    if (GameManager.Instance.LogicOptions.MapId < 5)
                    {
                        HandleCheat(pc, "Using zipline on wrong map");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Using zipline during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.CheckSpore:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Triggering spore in lobby");
                        return true;
                    }
                    if (GameManager.Instance.LogicOptions.MapId < 5)
                    {
                        HandleCheat(pc, "Triggering spore on wrong map");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Triggering spore during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.CheckShapeshift:
                    PlayerControl target4 = sr.ReadNetObject<PlayerControl>();
                    bool animate = sr.ReadBoolean();
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using shift button in lobby");
                        return true;
                    }
                    if (target4 != null && target4 != pc && !animate)
                    {
                        HandleCheat(pc, "No shapeshift animation");
                        return true;
                    }
                    if (pc.shapeshiftTargetPlayerId != -1 && target4 != null && target4 != pc)
                    {
                        HandleCheat(pc, "Shapeshifting while shapeshifted");
                        return true;
                    }
                    if (pc.GetSelfRole() != RoleTypes.Shapeshifter)
                    {
                        if (!TimeSinceRoleChange.ContainsKey(pc.PlayerId) || TimeSinceRoleChange[pc.PlayerId] > 5f)
                            HandleCheat(pc, "Trying to shift as not shapeshifter");
                        pc.RpcRejectShapeshift();
                        return true;
                    }
                    if (((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance) && target4 != pc)
                    {
                        HandleCheat(pc, "Trying to shift during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.CheckVanish:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using vanish button in lobby");
                        return true;
                    }
                    if (pc.GetSelfRole() != RoleTypes.Phantom)
                    {
                        if (!TimeSinceRoleChange.ContainsKey(pc.PlayerId) || TimeSinceRoleChange[pc.PlayerId] > 5f)
                            HandleCheat(pc, "Trying to vanish as not phantom");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Trying to vanish during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.CheckAppear:
                    if (!gameStarted)
                    {
                        HandleCheat(pc, "Using appear button in lobby");
                        return true;
                    }
                    if (pc.GetSelfRole() != RoleTypes.Phantom)
                    {
                        if (!TimeSinceRoleChange.ContainsKey(pc.PlayerId) || TimeSinceRoleChange[pc.PlayerId] > 5f)
                            HandleCheat(pc, "Trying to appear as not phantom");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(pc, "Trying to appear during meeting");
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static bool PlayerPhysicsReceiveRpc(PlayerPhysics physics, byte callId, MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost || !Options.AntiCheat.GetBool()) return false;
            if (physics == null || reader == null) return false;
            if (physics.AmOwner) return false;
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;
            bool gameStarted = AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended;
            switch (rpc)
            {
                case RpcCalls.EnterVent:
                case RpcCalls.ExitVent:
                    if (!gameStarted)
                    {
                        HandleCheat(physics.myPlayer, "Venting in lobby");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(physics.myPlayer, "Venting during meeting");
                        return true;
                    }
                    if (!physics.myPlayer.GetSelfRole().IsImpostor() && physics.myPlayer.GetSelfRole() != RoleTypes.Engineer && (!TimeSinceRoleChange.ContainsKey(physics.myPlayer.PlayerId) || TimeSinceRoleChange[physics.myPlayer.PlayerId] > 5f))
                    {
                        HandleCheat(physics.myPlayer, "Trying to vent as non venting role");
                        return true;
                    }
                    break;
                case RpcCalls.ClimbLadder:
                    if (!gameStarted)
                    {
                        HandleCheat(physics.myPlayer, "Climbing ladder in lobby");
                        return true;
                    }
                    if (GameManager.Instance.LogicOptions.MapId < 4)
                    {
                        HandleCheat(physics.myPlayer, "Climbing ladder on wrong map");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(physics.myPlayer, "Climbing during meeting");
                        return true;
                    }
                    break;
                case RpcCalls.Pet:
                case RpcCalls.CancelPet:
                    if (physics.myPlayer.inVent)
                    {
                        HandleCheat(physics.myPlayer, "Petting in vent");
                        return true;
                    }
                    if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
                    {
                        HandleCheat(physics.myPlayer, "Petting during meeting");
                        return true;
                    }
                    break;
            }
            if (callId <= 65)
            {
                if (!RpcsInCurrentSecond.ContainsKey((physics.myPlayer.PlayerId, rpc)))
                    RpcsInCurrentSecond[(physics.myPlayer.PlayerId, rpc)] = 0;
                ++RpcsInCurrentSecond[(physics.myPlayer.PlayerId, rpc)];
                if (RpcRateLimit.ContainsKey(rpc) && RpcsInCurrentSecond[(physics.myPlayer.PlayerId, rpc)] > RpcRateLimit[rpc])
                {
                    HandleCheat(physics.myPlayer, "Sending too many packets");
                    return true;
                }
            }
            return false;
        }

        public static bool CustomNetworkTransformReceiveRpc(CustomNetworkTransform netTransform, byte callId, MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost || !Options.AntiCheat.GetBool()) return false;
            if (netTransform == null || reader == null) return false;
            if (netTransform.AmOwner) return false;
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;
            bool gameStarted = AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended;
            switch (rpc)
            {
                case RpcCalls.SnapTo:
                    if (!gameStarted)
                    {
                        HandleCheat(netTransform.myPlayer, "Teleportation in lobby");
                        return true;
                    }
                    if (MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating)
                    {
                        HandleCheat(netTransform.myPlayer, "Teleportation during meeting");
                        return true;
                    }
                    break;
            }
            if (callId <= 65)
            {
                if (!RpcsInCurrentSecond.ContainsKey((netTransform.myPlayer.PlayerId, rpc)))
                    RpcsInCurrentSecond[(netTransform.myPlayer.PlayerId, rpc)] = 0;
                ++RpcsInCurrentSecond[(netTransform.myPlayer.PlayerId, rpc)];
                if (RpcRateLimit.ContainsKey(rpc) && RpcsInCurrentSecond[(netTransform.myPlayer.PlayerId, rpc)] > RpcRateLimit[rpc])
                {
                    HandleCheat(netTransform.myPlayer, "Sending too many packets");
                    return true;
                }
            }
            return false;
        }

        public static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, byte amount, MessageReader reader)
        {
            if (!AmongUsClient.Instance.AmHost || !Options.AntiCheat.GetBool()) return false;
            if (player == null || reader == null) return false;
            if (player.AmOwner) return false;
            MessageReader sr = MessageReader.Get(reader);
            var mapId = GameManager.Instance.LogicOptions.MapId;
            var selfRole = player.GetSelfRole();
            switch (systemType)
            {
                case SystemTypes.Reactor:
                    if (mapId == 2 || mapId == 4)
                    {
                        HandleCheat(player, "Sabotage doesn't exist on map");
                        return true;
                    }
                    if (amount != 64 && amount != 65 && amount != 32 && amount != 33)
                    {
                        HandleCheat(player, "Hack sent sabotage");
                        return true;
                    }
                    break;
                case SystemTypes.Electrical:
                    if (mapId >= 5)
                    {
                        HandleCheat(player, "Sabotage doesn't exist on map");
                        return true;
                    }
                    if (amount >= 5)
                    {
                        HandleCheat(player, "Hack sent sabotage");
                        return true;
                    }
                    break;
                case SystemTypes.LifeSupp:
                    if (mapId == 2 || mapId >= 4)
                    {
                        HandleCheat(player, "Sabotage doesn't exist on map");
                        return true;
                    }
                    if (amount != 64 && amount != 65)
                    {
                        HandleCheat(player, "Hack sent sabotage");
                        return true;
                    }
                    break;
                case SystemTypes.Comms:
                    if (amount == 0)
                    {
                        if (mapId == 1 || mapId >= 5)
                        {
                            HandleCheat(player, "Hack sent sabotage");
                            return true;
                        }
                    }
                    else if (amount == 64 || amount == 65 || amount == 32 || amount == 33 || amount == 16 || amount == 17)
                    {
                        if (mapId != 1 && mapId < 5)
                        {
                            HandleCheat(player, "Hack sent sabotage");
                            return true;
                        }
                    }
                    else
                    {
                        HandleCheat(player, "Hack sent sabotage");
                        return true;
                    }
                    break;
                case SystemTypes.Sabotage:
                    if (!selfRole.IsImpostor())
                    {   
                        if (!TimeSinceRoleChange.ContainsKey(player.PlayerId) || TimeSinceRoleChange[player.PlayerId] > 5f)
                            HandleCheat(player, "Sabotaging as crewmate");
                        return true;
                    }
                    if (amount != 3 && amount != 7 && amount != 8 && amount != 14 && amount != 21 && amount != 57 && amount != 58)
                    {
                        HandleCheat(player, "Invalid sabotage");
                        return true;
                    }
                    break;
                case SystemTypes.Laboratory:
                    if (mapId != 2)
                    {
                        HandleCheat(player, "Sabotage doesn't exist on map");
                        return true;
                    }
                    if (amount != 64 && amount != 65 && amount != 32 && amount != 33)
                    {
                        HandleCheat(player, "Hack sent sabotage");
                        return true;
                    }
                    break;
                case SystemTypes.Ventilation:
                    VentilationSystem ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
                    if (ventilationSystem == null) break;
                    --reader.Position;
                    ushort num = reader.ReadUInt16();
		            VentilationSystem.Operation operation = (VentilationSystem.Operation)reader.ReadByte();
		            byte ventId = reader.ReadByte();
                    Vent vent = ShipStatus.Instance.AllVents.FirstOrDefault((Vent v) => v.Id == ventId);
                    if (vent == null)
                    {
                        HandleCheat(player, "Hack sent vent");
                        return true;
                    }
                    switch (operation)
                    {
                        case VentilationSystem.Operation.StartCleaning:
                            if (ventilationSystem.PlayersCleaningVents.ContainsKey(player.PlayerId))
                            {
                                HandleCheat(player, "Hack sent clean vent");
                                return true;
                            }
                            if (selfRole.IsImpostor() && !TimeSinceRoleChange.ContainsKey(player.PlayerId))
                            {
                                HandleCheat(player, "Cleaning vent as impostor");
                                return true;
                            }
                            if (!player.HasTask(TaskTypes.VentCleaning) && !ChangedTasks.Contains(player.PlayerId))
                            {
                                HandleCheat(player, "Hack sent clean vent");
                                return true;
                            }
                            if (TimeSinceLastStartCleaning.ContainsKey(player.PlayerId) && TimeSinceLastStartCleaning[player.PlayerId].Item2 < 0.1f && TimeSinceLastStartCleaning[player.PlayerId].Item1 != ventId)
                            {
                                HandleCheat(player, "Hack sent clean vent");
                                return true;
                            }
                            TimeSinceLastStartCleaning[player.PlayerId] = (ventId, 0f);
                            break;
                        case VentilationSystem.Operation.BootImpostors:
                            if (selfRole.IsImpostor() && !TimeSinceRoleChange.ContainsKey(player.PlayerId))
                            {
                                HandleCheat(player, "Boot from vent hack");
                                return true;
                            }
                            if (!player.HasTask(TaskTypes.VentCleaning) && !ChangedTasks.Contains(player.PlayerId))
                            {
                                HandleCheat(player, "Boot from vent hack");
                                return true;
                            }
                            if (TimeSinceLastBootImpostors.ContainsKey(player.PlayerId) && TimeSinceLastBootImpostors[player.PlayerId].Item2 < 0.1f && TimeSinceLastBootImpostors[player.PlayerId].Item1 != ventId)
                            {
                                HandleCheat(player, "Boot from vent hack");
                                return true;
                            }
                            TimeSinceLastBootImpostors[player.PlayerId] = (ventId, 0f);
                            break;
                    }
                    break;
                case SystemTypes.MushroomMixupSabotage:
                    HandleCheat(player, "Hack sent sabotage");
                    return true;
                case SystemTypes.HeliSabotage:
                    if (mapId != 4)
                    {
                        HandleCheat(player, "Sabotage doesn't exist on map");
                        return true;
                    }
                    if (amount != 64 && amount != 65 && amount != 16 && amount != 17 && amount != 32 && amount != 33)
                    {
                        HandleCheat(player, "Hack sent sabotage");
                        return true;
                    }
                    break;
            }
            if ((MeetingHud.Instance && MeetingHud.Instance.state != MeetingHud.VoteStates.Animating) || ExileController.Instance)
            {
                if (CustomGamemode.Instance.Gamemode == Gamemodes.Zombies && systemType == SystemTypes.Ventilation) return false;
                HandleCheat(player, "Sent UpdateSystem Rpc during meeting");
                return true;
            }
            return false;
        }

        public static void OnUpdate()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (TimeSinceLastTask.ContainsKey(pc.PlayerId))
                    TimeSinceLastTask[pc.PlayerId] += Time.fixedDeltaTime;
                if (TimeSinceRoleChange.ContainsKey(pc.PlayerId))
                    TimeSinceRoleChange[pc.PlayerId] += Time.fixedDeltaTime;
                if (TimeSinceLastStartCleaning.ContainsKey(pc.PlayerId))
                    TimeSinceLastStartCleaning[pc.PlayerId] = (TimeSinceLastStartCleaning[pc.PlayerId].Item1, TimeSinceLastStartCleaning[pc.PlayerId].Item2 + Time.fixedDeltaTime);
                if (TimeSinceLastBootImpostors.ContainsKey(pc.PlayerId))
                    TimeSinceLastBootImpostors[pc.PlayerId] = (TimeSinceLastBootImpostors[pc.PlayerId].Item1, TimeSinceLastBootImpostors[pc.PlayerId].Item2 + Time.fixedDeltaTime);
            }
            TimeSinceRateLimitReset += Time.fixedDeltaTime;
            if (TimeSinceRateLimitReset >= 1f)
            {
                RpcsInCurrentSecond.Clear();
                TimeSinceRateLimitReset -= 1f;
            }
        }

        public static void OnMeeting()
        {
            new LateTask(() => {
                LobbyDeadBodies = new List<byte>();
                TimeSinceRoleChange = new Dictionary<byte, float>();
                ChangedTasks = new List<byte>();
                RemovedBodies = new List<byte>();
            }, 5f);
        }
        
        public static void HandleCheat(PlayerControl pc, string reason)
        {
            if (!Options.AntiCheat.GetBool()) return;
            if (reason != "Sending too many packets")
                Main.Instance.Log.LogInfo("AntiCheat: " + pc.Data.PlayerName + " is hacking!\nReason: " + reason);
            switch (Options.CurrentCheatingPenalty)
            {
                case CheatingPenalties.WarnHost:
                    if (HudManager.Instance.Notifier.activeMessages.Count < HudManager.Instance.Notifier.maxMessages)
                        HudManager.Instance.Notifier.AddDisconnectMessage("AntiCheat: " + pc.Data.PlayerName + " is hacking!\nReason: " + reason);
                    break;
                case CheatingPenalties.WarnEveryone:
                    if (HudManager.Instance.Notifier.activeMessages.Count < HudManager.Instance.Notifier.maxMessages)
                    {
                        Utils.SendChat(pc.Data.PlayerName + " is hacking!\nReason: " + reason, "AntiCheat");
                        HudManager.Instance.Notifier.AddDisconnectMessage("AntiCheat: " + pc.Data.PlayerName + " is hacking!\nReason: " + reason);
                    }
                    break;
                case CheatingPenalties.Kick:
                    if (!BannedPlayers.Contains(pc.NetId))
                    {
                        BannedPlayers.Add(pc.NetId);
                        pc.RpcSetName(pc.Data.PlayerName + " was kicked for hacking.\nReason: " + reason + "<size=0>");
                        AmongUsClient.Instance.KickPlayer(pc.GetClientId(), false);
                    }
                    break;
                case CheatingPenalties.Ban:
                    if (!BannedPlayers.Contains(pc.NetId))
                    {
                        BannedPlayers.Add(pc.NetId);
                        pc.RpcSetName(pc.Data.PlayerName + " was banned for hacking.\nReason: " + reason + "<size=0>");
                        AmongUsClient.Instance.KickPlayer(pc.GetClientId(), true);
                    }
                    break;
            }
        }
    }

    public enum CheatingPenalties
    {
        WarnHost,
        WarnEveryone,
        Kick,
        Ban,
    }
}