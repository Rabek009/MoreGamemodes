using UnityEngine;
using InnerNet;
using Hazel;
using System.Collections.Generic;

namespace MoreGamemodes
{
    public class CustomNetObject
    {
        public void RpcChangeSprite(string sprite)
        {
            Sprite = sprite;
            new LateTask(() => {
                playerControl.RawSetName(sprite);
                var name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                var colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                var hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                var skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                var petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                var visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                CustomRpcSender sender = CustomRpcSender.Create("SetFakeData", SendOption.None);
                MessageWriter writer = sender.stream;
                sender.StartMessage(-1);
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + sprite;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                    .WriteNetObject(PlayerControl.LocalPlayer)
                    .Write(false)
                    .EndRpc();
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.EndMessage();
                sender.SendMessage();
            }, 0f);
        }

        public void RpcTeleport(Vector2 position)
        {
            playerControl.NetTransform.RpcSnapTo(position);
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
            MessageWriter writer = MessageWriter.Get(SendOption.None);
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
            PlayerControlTimer += Time.fixedDeltaTime;
            if (PlayerControlTimer > 20f)
            {
                PlayerControl oldPlayerControl = playerControl;
                playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
                playerControl.PlayerId = 255;
                playerControl.isNew = false;
                playerControl.notRealPlayer = true;
                playerControl.NetTransform.SnapTo(Position);
                AmongUsClient.Instance.NetIdCnt += 1U;
                MessageWriter msg = MessageWriter.Get(SendOption.None);
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
                    playerControl.RawSetName(Sprite);
                    var name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                    var colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                    var hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                    var skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                    var petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                    var visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                    CustomRpcSender sender = CustomRpcSender.Create("SetFakeData", SendOption.None);
                    MessageWriter writer = sender.stream;
                    sender.StartMessage(-1);
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + Sprite;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                        PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                    }
                    writer.EndMessage();
                    sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                        .WriteNetObject(PlayerControl.LocalPlayer)
                        .Write(false)
                        .EndRpc();
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                        PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                    }
                    writer.EndMessage();
                    sender.EndMessage();
                    sender.SendMessage();
                }, 0.2f);
                new LateTask(() => oldPlayerControl.Despawn(), 0.3f);
                playerControl.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
                playerControl.cosmetics.colorBlindText.color = Color.clear;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.AmOwner) continue;
                    new LateTask(() => {
                        CustomRpcSender sender = CustomRpcSender.Create("SetFakeData", SendOption.None);
                        MessageWriter writer = sender.stream;
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
                            writer.Write((byte)255);
                        }
                        writer.EndMessage();
                        sender.EndMessage();
                        sender.SendMessage();
                    }, 0.1f);
                }
                if (Type == CustomObjectTypes.TrapArea)
                {
                    TrapArea trapArea = this as TrapArea;
                    if (trapArea != null)
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!trapArea.VisibleList.Contains(pc.PlayerId))
                                Hide(pc);
                        }
                    }
                }
                PlayerControlTimer = 0f;
            }
        }

        public void CreateNetObject(string sprite, Vector2 position, CustomObjectTypes type)
        {
            playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = 255;
            playerControl.isNew = false;
            playerControl.notRealPlayer = true;
            AmongUsClient.Instance.NetIdCnt += 1U;
            MessageWriter msg = MessageWriter.Get(SendOption.None);
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
                playerControl.RawSetName(sprite);
                var name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                var colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                var hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                var skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                var petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                var visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                CustomRpcSender sender = CustomRpcSender.Create("SetFakeData", SendOption.None);
                MessageWriter writer = sender.stream;
                sender.StartMessage(-1);
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + sprite;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                    .WriteNetObject(PlayerControl.LocalPlayer)
                    .Write(false)
                    .EndRpc();
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.EndMessage();
                sender.SendMessage();
            }, 0.2f);
            Position = position;
            PlayerControlTimer = 0f;
            Type = type;
            playerControl.cosmetics.currentBodySprite.BodySprite.color = Color.clear;
            playerControl.cosmetics.colorBlindText.color = Color.clear;
            Sprite = sprite;
            ++MaxId;
            Id = MaxId;
            CustomObjects.Add(this);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                new LateTask(() => {
                    CustomRpcSender sender = CustomRpcSender.Create("SetFakeData", SendOption.None);
                    MessageWriter writer = sender.stream;
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
                        writer.Write((byte)255);
                    }
                    writer.EndMessage();
                    sender.EndMessage();
                    sender.SendMessage();
                }, 0.1f);
            }
        }

        public static List<CustomNetObject> CustomObjects;
        public static int MaxId = -1;
        public PlayerControl playerControl;
        public string Sprite;
        public int Id;
        public Vector2 Position;
        public float PlayerControlTimer;
        public CustomObjectTypes Type;
        public bool DespawnOnMeeting => Type is CustomObjectTypes.TrapArea;
    }

    public enum CustomObjectTypes
    {
        Explosion,
        TrapArea,
        Turret,
        Base,
        Display,
        ExplosionHole,
    }
}