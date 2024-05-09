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
            playerControl.RpcSetName(sprite);
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

        }

        public void CreateNetObject(string sprite, Vector2 position)
        {
            playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = 255;
            playerControl.isNew = false;
            playerControl.notRealPlayer = true;
            AmongUsClient.Instance.NetIdCnt += 1U;
            MessageWriter msg = MessageWriter.Get(SendOption.Reliable);
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
				msg.WritePacked(AmongUsClient.Instance.NetIdCnt + i);
				msg.StartMessage(1);
				msg.EndMessage();
				msg.EndMessage();
			}
			msg.EndMessage();
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
                if (AmongUsClient.Instance.AmClient)
		        {
			        playerControl.SetName(sprite, false);
		        }
		        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, (byte)RpcCalls.SetName, SendOption.None, -1);
		        writer.Write(sprite);
		        AmongUsClient.Instance.FinishRpcImmediately(writer);
                playerControl.NetTransform.RpcSnapTo(position);
            }, 0.1f);
            Position = position;
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
    }
}