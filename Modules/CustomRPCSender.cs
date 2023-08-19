using System;
using Hazel;
using InnerNet;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace MoreGamemodes
{
    public class CustomRpcSender
    {
        public MessageWriter stream;
        public readonly string name;
        public readonly SendOption sendOption;
        public bool isUnsafe;
        public delegate void onSendDelegateType();
        public onSendDelegateType onSendDelegate;

        public State CurrentState
        {
            get { return currentState; }
            set
            {
                if (isUnsafe) currentState = value;
            }
        }
        private State currentState = State.BeforeInit;

        private int currentRpcTarget;

        private CustomRpcSender() { }
        public CustomRpcSender(string name, SendOption sendOption, bool isUnsafe)
        {
            stream = MessageWriter.Get(sendOption);

            this.name = name;
            this.sendOption = sendOption;
            this.isUnsafe = isUnsafe;
            this.currentRpcTarget = -2;

            currentState = State.Ready;
        }
        public static CustomRpcSender Create(string name = "No Name Sender", SendOption sendOption = SendOption.None, bool isUnsafe = false)
        {
            return new CustomRpcSender(name, sendOption, isUnsafe);
        }

        #region Start/End Message
        public CustomRpcSender StartMessage(int targetClientId = -1)
        {
            if (currentState != State.Ready)
            {
                string errorMsg = $"Messageを開始しようとしましたが、StateがReadyではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            if (targetClientId < 0)
            {
                stream.StartMessage(5);
                stream.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                stream.StartMessage(6);
                stream.Write(AmongUsClient.Instance.GameId);
                stream.WritePacked(targetClientId);
            }

            currentRpcTarget = targetClientId;
            currentState = State.InRootMessage;
            return this;
        }
        public CustomRpcSender EndMessage(int targetClientId = -1)
        {
            if (currentState != State.InRootMessage)
            {
                string errorMsg = $"Messageを終了しようとしましたが、StateがInRootMessageではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            stream.EndMessage();

            currentRpcTarget = -2;
            currentState = State.Ready;
            return this;
        }
        #endregion
        #region Start/End Rpc
        public CustomRpcSender StartRpc(uint targetNetId, RpcCalls rpcCall)
            => StartRpc(targetNetId, (byte)rpcCall);
        public CustomRpcSender StartRpc(
            uint targetNetId,
            byte callId)
        {
            if (currentState != State.InRootMessage)
            {
                string errorMsg = $"RPCを開始しようとしましたが、StateがInRootMessageではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            stream.StartMessage(2);
            stream.WritePacked(targetNetId);
            stream.Write(callId);

            currentState = State.InRpc;
            return this;
        }
        public CustomRpcSender EndRpc()
        {
            if (currentState != State.InRpc)
            {
                string errorMsg = $"RPCを終了しようとしましたが、StateがInRpcではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            stream.EndMessage();
            currentState = State.InRootMessage;
            return this;
        }
        #endregion
        public CustomRpcSender AutoStartRpc(
            uint targetNetId,
            byte callId,
            int targetClientId = -1)
        {
            if (targetClientId == -2) targetClientId = -1;
            if (currentState is not State.Ready and not State.InRootMessage)
            {
                string errorMsg = $"RPCを自動で開始しようとしましたが、StateがReadyまたはInRootMessageではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            if (currentRpcTarget != targetClientId)
            {
                if (currentState == State.InRootMessage) this.EndMessage();
                this.StartMessage(targetClientId);
            }
            this.StartRpc(targetNetId, callId);

            return this;
        }
        public void SendMessage()
        {
            if (currentState == State.InRootMessage) this.EndMessage();
            if (currentState != State.Ready)
            {
                string errorMsg = $"RPCを送信しようとしましたが、StateがReadyではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            AmongUsClient.Instance.SendOrDisconnect(stream);
            onSendDelegate();
            currentState = State.Finished;
            stream.Recycle();
        }
        #region PublicWriteMethods
        public CustomRpcSender Write(float val) => Write(w => w.Write(val));
        public CustomRpcSender Write(string val) => Write(w => w.Write(val));
        public CustomRpcSender Write(ulong val) => Write(w => w.Write(val));
        public CustomRpcSender Write(int val) => Write(w => w.Write(val));
        public CustomRpcSender Write(uint val) => Write(w => w.Write(val));
        public CustomRpcSender Write(ushort val) => Write(w => w.Write(val));
        public CustomRpcSender Write(byte val) => Write(w => w.Write(val));
        public CustomRpcSender Write(sbyte val) => Write(w => w.Write(val));
        public CustomRpcSender Write(bool val) => Write(w => w.Write(val));
        public CustomRpcSender Write(Il2CppStructArray<byte> bytes) => Write(w => w.Write(bytes));
        public CustomRpcSender Write(Il2CppStructArray<byte> bytes, int offset, int length) => Write(w => w.Write(bytes, offset, length));
        public CustomRpcSender WriteBytesAndSize(Il2CppStructArray<byte> bytes) => Write(w => w.WriteBytesAndSize(bytes));
        public CustomRpcSender WritePacked(int val) => Write(w => w.WritePacked(val));
        public CustomRpcSender WritePacked(uint val) => Write(w => w.WritePacked(val));
        public CustomRpcSender WriteNetObject(InnerNetObject obj) => Write(w => w.WriteNetObject(obj));
        public CustomRpcSender WriteVector2(Vector2 vec) => Write(w => NetHelpers.WriteVector2(vec, w));
        #endregion

        private CustomRpcSender Write(Action<MessageWriter> action)
        {
            if (currentState != State.InRpc)
            {
                string errorMsg = $"RPCを書き込もうとしましたが、StateがWrite(書き込み中)ではありません (in: \"{name}\")";
                if (isUnsafe)
                {
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            action(stream);

            return this;
        }

        public enum State
        {
            BeforeInit = 0,
            Ready,
            InRootMessage,
            InRpc,
            Finished,
        }
    }

    public static class CustomRpcSenderExtensions
    {
        public static void RpcSetRole(this CustomRpcSender sender, PlayerControl player, RoleTypes role, int targetClientId = -1)
        {
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetRole, targetClientId)
                .Write((ushort)role)
                .EndRpc();
        }
        public static void RpcMurderPlayer(this CustomRpcSender sender, PlayerControl player, PlayerControl target, int targetClientId = -1)
        {
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.MurderPlayer, targetClientId)
                .WriteNetObject(target)
                .EndRpc();
        }
        public static void RpcSetOutfit(this CustomRpcSender sender, PlayerControl player, int colorId = -1, string hatId = null, string skinId = null, string petId = null, string visorId = null) 
        {
            var outfit = player.Data.Outfits[PlayerOutfitType.Default];
            if (colorId == -1) colorId = outfit.ColorId;
            if (hatId == null) hatId = outfit.HatId;
            if (skinId == null) skinId = outfit.SkinId;
            if (petId == null) petId = outfit.PetId;
            if (visorId == null) visorId = outfit.VisorId;

            player.SetColor(colorId);
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetColor)
                .Write(colorId)
                .EndRpc();
            player.SetHat(hatId, colorId);
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetHatStr)
                .Write(hatId)
                .EndRpc();
            player.SetSkin(skinId, colorId);
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetSkinStr)
                .Write(skinId)
                .EndRpc();
            player.SetVisor(visorId, colorId);
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetVisorStr)
                .Write(visorId)
                .EndRpc();
            if (!player.Data.IsDead)
            {
                player.SetPet(petId);
                sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetPetStr)
                    .Write(petId)
                    .EndRpc();
            }  
        }
    }
}