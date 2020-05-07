using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;

namespace GameNetworking.Messages.Models {
    public sealed class MessageContainer {
        private readonly byte[] _buffer;
        private readonly int _length;

        public int type { get; private set; }

        public MessageContainer(byte[] buffer, int length) {
            this._buffer = buffer;
            this._length = length;
            this.type = BitConverter.ToInt32(buffer, 0);
        }

        public bool Is(int type) {
            return type == this.type;
        }

        public TMessage Parse<TMessage>() where TMessage : struct, IDecodable {
            var message = BinaryDecoder.Decode<TMessage>(this._buffer, sizeof(int), this._length);
            ReturnBuffer(this._buffer);
            return message;
        }

        #region Buffers

        private static readonly ObjectPool<byte[]> bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
        
        public static byte[] GetBuffer() => bufferPool.Rent();
        private static void ReturnBuffer(byte[] buffer) => bufferPool.Pay(buffer);

        #endregion
    }
}