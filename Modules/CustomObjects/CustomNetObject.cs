using UnityEngine;
using InnerNet;
using Hazel;
using System.Collections.Generic;
using System.Linq;

namespace MoreGamemodes
{
    public class CustomNetObject
    {
        public void RpcChangeSprite(string sprite)
        {
            Sprite = sprite;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.inVent && !x.walkingToVent).FirstOrDefault();
            if (player == null) player = PlayerControl.LocalPlayer;
            var name = player.Data.Outfits[PlayerOutfitType.Default].PlayerName;
            var colorId = player.Data.Outfits[PlayerOutfitType.Default].ColorId;
            var hatId = player.Data.Outfits[PlayerOutfitType.Default].HatId;
            var skinId = player.Data.Outfits[PlayerOutfitType.Default].SkinId;
            var petId = player.Data.Outfits[PlayerOutfitType.Default].PetId;
            var visorId = player.Data.Outfits[PlayerOutfitType.Default].VisorId;
            CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
            MessageWriter writer = sender.stream;
            sender.StartMessage(-1);
            player.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14>\n</size>" + sprite;
            player.Data.Outfits[PlayerOutfitType.Default].ColorId = 0;
            player.Data.Outfits[PlayerOutfitType.Default].HatId = "";
            player.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
            player.Data.Outfits[PlayerOutfitType.Default].PetId = "";
            player.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
            writer.StartMessage(1);
            {
                writer.WritePacked(player.Data.NetId);
                player.Data.Serialize(writer, false);
            }
            writer.EndMessage();
            playerControl.Shapeshift(player, false);
            sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                .WriteNetObject(player)
                .Write(false)
                .EndRpc();
            player.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
            player.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
            player.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
            player.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
            player.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
            player.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
            writer.StartMessage(1);
            {
                writer.WritePacked(player.Data.NetId);
                player.Data.Serialize(writer, false);
            }
            writer.EndMessage();
            sender.EndMessage();
            sender.SendMessage();
        }

        public void RpcTeleport(Vector2 position, SendOption sendOption)
        {
			playerControl.NetTransform.SnapTo(position, (ushort)(playerControl.NetTransform.lastSequenceId + 1));
		    MessageWriter writer = AmongUsClient.Instance.StartRpc(playerControl.NetTransform.NetId, (byte)RpcCalls.SnapTo, sendOption);
		    NetHelpers.WriteVector2(position, writer);
		    writer.Write(playerControl.NetTransform.lastSequenceId);
		    writer.EndMessage();
            Position = position;
        }

        public void Despawn()
        {
            playerControl.Despawn();
            CustomObjects.Remove(this);
        }
        
        public void Hide(PlayerControl player)
        {
            if (player.AmOwner)
            {
                playerControl.Visible = false;
                return;
            }
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(player.GetClientId());
            writer.StartMessage(5);
            writer.WritePacked(playerControl.NetId);
            writer.EndMessage();
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public virtual void OnFixedUpdate()
        {
            
        }

        public virtual void OnMeeting()
        {
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
			writer.StartMessage(5);
			writer.Write(AmongUsClient.Instance.GameId);
			writer.StartMessage(5);
			writer.WritePacked(playerControl.NetId);
			writer.EndMessage();
			writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
			writer.Recycle();
            new LateTask(() => {
                AmongUsClient.Instance.RemoveNetObject(playerControl);
                Object.Destroy(playerControl.gameObject);
                playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
                playerControl.PlayerId = 254;
                playerControl.isNew = false;
                playerControl.notRealPlayer = true;
                playerControl.NetTransform.SnapTo(new Vector2(50f, 50f));
                AmongUsClient.Instance.NetIdCnt += 1U;
                MessageWriter msg = MessageWriter.Get(SendOption.Reliable);
			    msg.StartMessage(5);
			    msg.Write(AmongUsClient.Instance.GameId);
			    AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
			    msg.EndMessage();
			    msg.StartMessage(6);
			    msg.Write(AmongUsClient.Instance.GameId);
			    msg.WritePacked(int.MaxValue);
			    for (uint i = 1; i <= 3; ++i)
			    {
			    	msg.StartMessage(4);
			    	msg.WritePacked(2U);
			    	msg.WritePacked(-2);
			    	msg.Write((byte)SpawnFlags.None);
			    	msg.WritePacked(1);
			    	msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
			    	msg.StartMessage(1);
			    	msg.EndMessage();
			    	msg.EndMessage();
			    }
			    msg.EndMessage();
			    AmongUsClient.Instance.SendOrDisconnect(msg);
			    msg.Recycle();
                if (PlayerControl.AllPlayerControls.Contains(playerControl))
                    PlayerControl.AllPlayerControls.Remove(playerControl);
                playerControl.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
                playerControl.cosmetics.colorBlindText.color = Color.clear;
            }, 5f);
            new LateTask(() => {
                bool doSend = false;
                CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                MessageWriter writer = sender.stream;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner) continue;
                    sender.StartMessage(pc.GetClientId());
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerControl.NetId);
                        writer.Write(pc.PlayerId);
                    }
                    writer.EndMessage();
                    sender.StartRpc(playerControl.NetId, (byte)RpcCalls.MurderPlayer)
                        .WriteNetObject(playerControl)
                        .Write((int)MurderResultFlags.FailedError)
                        .EndRpc();
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerControl.NetId);
                        writer.Write((byte)254);
                    }
                    writer.EndMessage();
                    sender.EndMessage();
                    doSend = true;
                }
                sender.SendMessage(doSend);
                playerControl.CachedPlayerData = PlayerControl.LocalPlayer.Data;
            }, 5.1f);
            new LateTask(() => {
                playerControl.NetTransform.RpcSnapTo(Position);
                var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.inVent && !x.walkingToVent).FirstOrDefault();
                if (player == null) player = PlayerControl.LocalPlayer;
                var name = player.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                var colorId = player.Data.Outfits[PlayerOutfitType.Default].ColorId;
                var hatId = player.Data.Outfits[PlayerOutfitType.Default].HatId;
                var skinId = player.Data.Outfits[PlayerOutfitType.Default].SkinId;
                var petId = player.Data.Outfits[PlayerOutfitType.Default].PetId;
                var visorId = player.Data.Outfits[PlayerOutfitType.Default].VisorId;
                CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                MessageWriter writer = sender.stream;
                sender.StartMessage(-1);
                player.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14>\n</size>" + Sprite;
                player.Data.Outfits[PlayerOutfitType.Default].ColorId = 0;
                player.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                player.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                player.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                player.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                writer.StartMessage(1);
                {
                    writer.WritePacked(player.Data.NetId);
                    player.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                playerControl.Shapeshift(player, false);
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                    .WriteNetObject(player)
                    .Write(false)
                    .EndRpc();
                player.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                player.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                player.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                player.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                player.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                player.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                writer.StartMessage(1);
                {
                    writer.WritePacked(player.Data.NetId);
                    player.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.EndMessage();
                sender.SendMessage();
            }, 5.2f);
        }

        public void CreateNetObject(string sprite, Vector2 position)
        {
            playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = 254;
            playerControl.isNew = false;
            playerControl.notRealPlayer = true;
            playerControl.NetTransform.SnapTo(new Vector2(50f, 50f));
            AmongUsClient.Instance.NetIdCnt += 1U;
            MessageWriter msg = MessageWriter.Get(SendOption.Reliable);
			msg.StartMessage(5);
			msg.Write(AmongUsClient.Instance.GameId);
			AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
			msg.EndMessage();
			msg.StartMessage(6);
			msg.Write(AmongUsClient.Instance.GameId);
			msg.WritePacked(int.MaxValue);
			for (uint i = 1; i <= 3; ++i)
			{
				msg.StartMessage(4);
				msg.WritePacked(2U);
				msg.WritePacked(-2);
				msg.Write((byte)SpawnFlags.None);
				msg.WritePacked(1);
				msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
				msg.StartMessage(1);
				msg.EndMessage();
				msg.EndMessage();
			}
			msg.EndMessage();
			AmongUsClient.Instance.SendOrDisconnect(msg);
			msg.Recycle();
            if (PlayerControl.AllPlayerControls.Contains(playerControl))
                PlayerControl.AllPlayerControls.Remove(playerControl);
            new LateTask(() => {
                playerControl.NetTransform.RpcSnapTo(position);
                var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.inVent && !x.walkingToVent).FirstOrDefault();
                if (player == null) player = PlayerControl.LocalPlayer;
                var name = player.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                var colorId = player.Data.Outfits[PlayerOutfitType.Default].ColorId;
                var hatId = player.Data.Outfits[PlayerOutfitType.Default].HatId;
                var skinId = player.Data.Outfits[PlayerOutfitType.Default].SkinId;
                var petId = player.Data.Outfits[PlayerOutfitType.Default].PetId;
                var visorId = player.Data.Outfits[PlayerOutfitType.Default].VisorId;
                CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                MessageWriter writer = sender.stream;
                sender.StartMessage(-1);
                player.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14>\n</size>" + sprite;
                player.Data.Outfits[PlayerOutfitType.Default].ColorId = 0;
                player.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                player.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                player.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                player.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                writer.StartMessage(1);
                {
                    writer.WritePacked(player.Data.NetId);
                    player.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                playerControl.Shapeshift(player, false);
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                    .WriteNetObject(player)
                    .Write(false)
                    .EndRpc();
                player.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                player.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                player.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                player.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                player.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                player.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                writer.StartMessage(1);
                {
                    writer.WritePacked(player.Data.NetId);
                    player.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.EndMessage();
                sender.SendMessage();
            }, 0.2f);
            Position = position;
            playerControl.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
            playerControl.cosmetics.colorBlindText.color = Color.clear;
            Sprite = sprite;
            ++MaxId;
            Id = MaxId;
            CustomObjects.Add(this);
            new LateTask(() => {
                bool doSend = false;
                CustomRpcSender sender = CustomRpcSender.Create(SendOption.Reliable);
                MessageWriter writer = sender.stream;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner) continue;
                    sender.StartMessage(pc.GetClientId());
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerControl.NetId);
                        writer.Write(pc.PlayerId);
                    }
                    writer.EndMessage();
                    sender.StartRpc(playerControl.NetId, (byte)RpcCalls.MurderPlayer)
                        .WriteNetObject(playerControl)
                        .Write((int)MurderResultFlags.FailedError)
                        .EndRpc();
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerControl.NetId);
                        writer.Write((byte)254);
                    }
                    writer.EndMessage();
                    sender.EndMessage();
                    doSend = true;
                }
                sender.SendMessage(doSend);
                playerControl.CachedPlayerData = PlayerControl.LocalPlayer.Data;
            }, 0.1f);
        }

        public static List<CustomNetObject> CustomObjects;
        public static int MaxId = -1;
        public PlayerControl playerControl;
        public string Sprite;
        public int Id;
        public Vector2 Position;
    }
}