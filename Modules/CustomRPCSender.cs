using System;
using Hazel;
using InnerNet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/CustomRpcSender.cs
namespace MoreGamemodes
{
    public class CustomRpcSender
    {
        public MessageWriter stream;
        public readonly SendOption sendOption;

        private State currentState = State.BeforeInit;

        private int currentRpcTarget;

        private CustomRpcSender() { }
        public CustomRpcSender(SendOption sendOption, bool isStreamed)
        {
            stream = isStreamed ? AmongUsClient.Instance.Streams[(int)sendOption] : MessageWriter.Get(sendOption);
            this.sendOption = sendOption;
            currentRpcTarget = -2;
            currentState = State.Ready;
        }
        public static CustomRpcSender Create(SendOption sendOption, bool isStreamed = false)
        {
            return new CustomRpcSender(sendOption, isStreamed);
        }

        public CustomRpcSender StartMessage(int targetClientId = -1)
        {
            if (stream.Length > 800)
            {
                AmongUsClient.Instance.SendOrDisconnect(stream);
                stream.Clear(sendOption);
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
        public CustomRpcSender EndMessage()
        {
            stream.EndMessage();
            currentRpcTarget = -2;
            currentState = State.Ready;
            return this;
        }
        public CustomRpcSender CancelMessage()
        {
            stream.CancelMessage();
            currentRpcTarget = -2;
            currentState = State.Ready;
            return this;
        }

        public CustomRpcSender StartRpc(uint targetNetId, RpcCalls rpcCall)
            => StartRpc(targetNetId, (byte)rpcCall);
        public CustomRpcSender StartRpc(uint targetNetId, byte callId)
        {
            stream.StartMessage(2);
            stream.WritePacked(targetNetId);
            stream.Write(callId);
            currentState = State.InRpc;
            return this;
        }
        public CustomRpcSender EndRpc()
        {
            stream.EndMessage();
            currentState = State.InRootMessage;
            return this;
        }

        public CustomRpcSender AutoStartRpc(uint targetNetId, byte callId, int targetClientId = -1)
        {
            if (targetClientId == -2) targetClientId = -1;
            if (currentRpcTarget != targetClientId)
            {
                if (currentState == State.InRootMessage) EndMessage();
                StartMessage(targetClientId);
            }
            StartRpc(targetNetId, callId);
            return this;
        }
        public void SendMessage(bool doSend = true)
        {
            if (currentState == State.InRootMessage) EndMessage();
            if (doSend) AmongUsClient.Instance.SendOrDisconnect(stream);
            currentState = State.Finished;
            stream.Recycle();
        }

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

        private CustomRpcSender Write(Action<MessageWriter> action)
        {
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
}