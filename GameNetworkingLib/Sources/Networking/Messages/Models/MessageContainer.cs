using GameNetworking.Commons;
using GameNetworking.Messages.Coders;

namespace GameNetworking.Messages.Models {
    public sealed class MessageContainer {
        private byte[] messageBytes;

        public int type {
            get; private set;
        }

        public MessageContainer(byte[] messageBytes) {
            this.messageBytes = messageBytes;
            this.type = CoderHelper.ReadHeader(messageBytes);
        }

        public bool Is(int type) {
            return type == this.type;
        }

        /// <summary>
        /// Parses the content of <see cref="messageBytes"/> into an IDecodable instance.
        /// This method may only be called once. Calling it a second time will result in an exception.
        /// </summary>
        /// <typeparam name="TMessage">Any type that inherits from IDecodable</typeparam>
        /// <returns>The parsed <typeparamref name="TMessage"/> instance.</returns>
        public TMessage Parse<TMessage>() where TMessage : class, IDecodable, new() {
            var headerSize = sizeof(int);
            var count = this.messageBytes.Length - headerSize;
            var message = Coders.Binary.Decoder.Decode<TMessage>(messageBytes, headerSize, count);
            ReturnBuffer(this.messageBytes);
            this.messageBytes = null;
            return message;
        }

        #region Buffers

        private static readonly ObjectPool<byte[]> bufferPool = new ObjectPool<byte[]>(NewBuffer);
        private static byte[] NewBuffer() => new byte[8 * 1024];

        public static byte[] GetBuffer() => bufferPool.Rent();
        public static void ReturnBuffer(byte[] buffer) => bufferPool.Pay(buffer);

        #endregion
    }
}