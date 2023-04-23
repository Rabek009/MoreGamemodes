using System.Linq;
using InnerNet;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Data;
using System;

namespace MoreGamemodes
{
    static class ExtendedPlayerControl
    {
        public static void RpcTeleport(this PlayerControl player, Vector2 location)
        {
            if (player.inVent)
                player.MyPhysics.RpcBootFromVent(0);
            if (AmongUsClient.Instance.AmHost) player.NetTransform.SnapTo(location);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None);
            NetHelpers.WriteVector2(location, writer);
            writer.Write(player.NetTransform.lastSequenceId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcRandomVentTeleport(this PlayerControl player)
        {
            var vents = UnityEngine.Object.FindObjectsOfType<Vent>();
            var rand = new System.Random();
            var vent = vents[rand.Next(0, vents.Count)];
            player.RpcTeleport(new Vector2(vent.transform.position.x, vent.transform.position.y + 0.3636f));
        }

        public static void RpcSendMessage(this PlayerControl player, string message, string messageName)
        {
            var messageSender = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            var name = messageSender.Data.PlayerName;

            if (player.AmOwner)
            {
                messageSender.SetName(Utils.ColorString(Color.blue, "MG.SystemMessage." + messageName));
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(messageSender, message);
                messageSender.SetName(name);
            }
            else
            {
                CustomRpcSender sender = CustomRpcSender.Create("Send Chat Message", SendOption.None);
                sender.StartMessage(player.GetClientId());
                sender.StartRpc(messageSender.NetId, (byte)RpcCalls.SetName)
                    .Write(Utils.ColorString(Color.blue, "MG.SystemMessage." + messageName))
                    .EndRpc();
                sender.StartRpc(messageSender.NetId, (byte)RpcCalls.SendChat)
                    .Write(message)
                    .EndRpc();
                sender.StartRpc(messageSender.NetId, (byte)RpcCalls.SetName)
                    .Write(Main.GameStarted ? Main.LastNotifyNames[(messageSender.PlayerId, player.PlayerId)] : name)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
        }

        public static void RpcSendMessages(this PlayerControl player, List<string> messages, string messageName)
        {
            var messageSender = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            var name = messageSender.Data.PlayerName;

            if (player.AmOwner)
            {
                messageSender.SetName(Utils.ColorString(Color.blue, "MG.SystemMessage." + messageName));
                for (int i = 0; i < messages.Count; ++i)
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(messageSender, messages[i]);
                messageSender.SetName(name);
            }
            else
            {
                CustomRpcSender sender = CustomRpcSender.Create("Send Chat Message", SendOption.None);
                sender.StartMessage(player.GetClientId());
                sender.StartRpc(messageSender.NetId, (byte)RpcCalls.SetName)
                    .Write(Utils.ColorString(Color.blue, "MG.SystemMessage." + messageName))
                    .EndRpc();
                for (int i = 0; i < messages.Count; ++i) 
                {
                    sender.StartRpc(messageSender.NetId, (byte)RpcCalls.SendChat)
                        .Write(messages[i])
                        .EndRpc();
                }
                sender.StartRpc(messageSender.NetId, (byte)RpcCalls.SetName)
                    .Write(Main.GameStarted ? Main.LastNotifyNames[(messageSender.PlayerId, player.PlayerId)] : name)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
        }

        public static void RpcSetDesyncRole(this PlayerControl player, RoleTypes role, int clientId)
        {
            if (player == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetNamePrivate(this PlayerControl player, string name, PlayerControl seer = null, bool isRaw = false)
        {
            if (player == null || name == null || !AmongUsClient.Instance.AmHost) return;
            if (seer == null) seer = player;
            if (Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] == name && !isRaw) return;

            if (seer.AmOwner)
            {
                player.cosmetics.nameText.SetText(name);
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
                return;
            }
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, Hazel.SendOption.Reliable, clientId);
            writer.Write(name);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            if (!isRaw)
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
        }

        public static bool TryCast<T>(this Il2CppObjectBase obj, out T casted)
        where T : Il2CppObjectBase
        {
            casted = obj.TryCast<T>();
            return casted != null;
        }

        public static void RpcShapeshiftV2(this PlayerControl shifter, PlayerControl target, bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (shifter.Data.IsDead) return;
            shifter.RpcShapeshift(target, shouldAnimate);
            shifter.RpcResetAbilityCooldown();
            Main.AllShapeshifts[shifter.PlayerId] = target.PlayerId;
        }

        public static void RpcRevertShapeshiftV2(this PlayerControl shifter, bool shouldAnimate)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (shifter.Data.IsDead) return;
            shifter.RpcRevertShapeshift(shouldAnimate);
            shifter.RpcResetAbilityCooldown();
            Main.AllShapeshifts[shifter.PlayerId] = shifter.PlayerId;
        }

        public static void RpcResetAbilityCooldown(this PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.LocalPlayer == target)
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.None, target.GetClientId());
                writer.WriteNetObject(target);
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static bool HasBomb(this PlayerControl player)
        {
            var hasBomb = false;
            if (player == null)
                return hasBomb;
            var hasBombFound = Main.HasBomb.TryGetValue(player.PlayerId, out hasBomb);
            return hasBombFound ? hasBomb : false;
        }

        public static Items GetItem(this PlayerControl player)
        {
            var item = Items.None;
            if (player == null)
                return item;
            var itemFound = Main.AllPlayersItems.TryGetValue(player.PlayerId, out item);
            return itemFound ? item : Items.None;
        }

        public static int Lives(this PlayerControl player)
        {
            var lives = 0;
            if (player == null)
                return lives;
            var livesFound = Main.Lives.TryGetValue(player.PlayerId, out lives);
            return livesFound ? lives : 0;
        }

        public static bool CanVent(this PlayerControl player)
        {
            if (((Options.CurrentGamemode == Gamemodes.HideAndSeek && !Options.HnSImpostorsCanVent.GetBool()) || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && !Options.SnSImpostorsCanVent.GetBool())) && Main.Impostors.Contains(player.PlayerId))
                return false;
            if (Options.CurrentGamemode == Gamemodes.BombTag)
                return false;
            if (Options.CurrentGamemode == Gamemodes.RandomItems && Main.HackTimer > 1f && (Main.Impostors.Contains(player.PlayerId) == false || Options.HackAffectsImpostors.GetBool()))
                return false;
            if (Options.CurrentGamemode == Gamemodes.BattleRoyale)
                return false;
            return player.Data.Role.Role == RoleTypes.Engineer || player.Data.Role.IsImpostor;
        }

        public static PlayerControl GetClosestPlayer(this PlayerControl player)
        {
            Vector2 playerpos = player.transform.position;
            Dictionary<PlayerControl, float> pcdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p != player)
                {
                    dis = Vector2.Distance(playerpos, p.transform.position);
                    pcdistance.Add(p, dis);
                }
            }
            var min = pcdistance.OrderBy(c => c.Value).FirstOrDefault();
            PlayerControl target = min.Key;
            return target;
        }

        public static void RpcGuardAndKill(this PlayerControl killer, PlayerControl target, int colorId = 0)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (killer.AmOwner)
            {
                killer.ProtectPlayer(target, colorId);
                killer.MurderPlayer(target);
            }
            else
            {
                CustomRpcSender sender = CustomRpcSender.Create("Guard And Kill", SendOption.None);
                sender.StartMessage(killer.GetClientId());
                sender.StartRpc(killer.NetId, (byte)RpcCalls.ProtectPlayer)
                    .WriteNetObject(target)
                    .Write(colorId)
                    .EndRpc();
                sender.StartRpc(killer.NetId, (byte)RpcCalls.MurderPlayer)
                    .WriteNetObject(target)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
        }

        public static void RpcSetKillTimer(this PlayerControl player, float timer)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (player.AmOwner)
            {
                player.SetKillTimer(timer);
                return;
            }
            var cooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.KillCooldown, timer * 2);
            Utils.SyncSettingsToAll(GameOptionsManager.Instance.CurrentGameOptions);
            player.RpcGuardAndKill(player);
            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.KillCooldown, cooldown);
            Utils.SyncSettingsToAll(GameOptionsManager.Instance.CurrentGameOptions);
        }

        public static void RpcSetOutfit(this PlayerControl player, int colorId = -1, string name = null, string hatId = null, string skinId = null, string petId = null, string visorId = null, string namePlateId = null) 
        {
            var outfit = player.Data.Outfits[PlayerOutfitType.Default];
            if (colorId == -1) colorId = outfit.ColorId;
            if (name == null) name = outfit.PlayerName;
            if (hatId == null) hatId = outfit.HatId;
            if (skinId == null) skinId = outfit.SkinId;
            if (petId == null) petId = outfit.PetId;
            if (visorId == null) visorId = outfit.VisorId;
            if (namePlateId == null) namePlateId = outfit.NamePlateId;

            outfit.ColorId = colorId;
            outfit.PlayerName = name;
            outfit.HatId = hatId;
            outfit.SkinId = skinId;
            outfit.PetId = petId;
            outfit.VisorId = visorId;
            outfit.NamePlateId = namePlateId;
            Utils.SendGameData();
            new LateTask(() =>
            {
                player.RpcShapeshiftV2(player, false);
                if (player.AmOwner)
                    player.MyPhysics.RpcCancelPet();
            }, 1f, "Set Look For Everyone");
        }

        public static void RpcExileV2(this PlayerControl player)
        {
            player.Exiled();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcFixedMurderPlayer(this PlayerControl killer, PlayerControl target)
        {
            new LateTask(() => 
            {
                killer.RpcMurderPlayer(target);
                if (killer.AmOwner)
                    killer.MyPhysics.RpcCancelPet();
            }, 0.01f, "Late Murder");
        }

        public static void RpcReactorFlash(this PlayerControl pc, float duration, Color color)
        {
            if (pc == null) return;
            if (pc.PlayerId == 0)
            {
                var hud = DestroyableSingleton<HudManager>.Instance;
                if (hud.FullScreen == null) return;
                var obj = hud.transform.FindChild("FlashColor_FullScreen")?.gameObject;
                if (obj == null)
                {
                    obj = GameObject.Instantiate(hud.FullScreen.gameObject, hud.transform);
                    obj.name = "FlashColor_FullScreen";
                }
                hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((t) =>
                {
                    obj.SetActive(t != 1f);
                    obj.GetComponent<SpriteRenderer>().color = new(color.r, color.g, color.b, Mathf.Clamp01((-2f * Mathf.Abs(t - 0.5f) + 1) * color.a));
                })));
                return;
            }
            int clientId = pc.GetClientId();
            byte reactorId = 3;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) reactorId = 21;

            MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
            SabotageWriter.Write(reactorId);
            MessageExtensions.WriteNetObject(SabotageWriter, pc);
            SabotageWriter.Write((byte)128);
            AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);

            new LateTask(() =>
            {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, duration, "Fix Desync Reactor");

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                new LateTask(() =>
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, duration, "Fix Desync Reactor 2");
        }

        public static void RpcSetDeathReason(this PlayerControl player, DeathReasons reason)
        {
            Main.AllPlayersDeathReason[player.PlayerId] = reason;
        }

        public static DeathReasons GetDeathReason(this PlayerControl player)
        {
            return Main.AllPlayersDeathReason[player.PlayerId];
        }
    }
}