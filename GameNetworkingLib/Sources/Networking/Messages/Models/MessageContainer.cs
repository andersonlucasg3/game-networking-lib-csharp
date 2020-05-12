using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Models {
    public struct MessageContainer {
        private static IntByteArrayConverter _intConverter = new IntByteArrayConverter(0);
        private readonly byte[] _buffer;
        private readonly int _length;

        public int type { get; private set; }

        public MessageContainer(byte[] buffer, int length) {
            this._buffer = buffer;
            this._length = length;

            _intConverter.array = buffer;
            this.type = _intConverter.value;
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