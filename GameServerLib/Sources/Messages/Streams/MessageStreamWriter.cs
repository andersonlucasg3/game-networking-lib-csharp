using System.Collections.Generic;

namespace Messages.Streams {
    using Coders;
    using Models;

    public class MessageStreamWriter: IStreamWriter {
        public byte[] Write<Message>(Message message) where Message : ITypedMessage {
            var buffer = new List<byte>();
            CoderHelper.WriteHeader(message.Type, ref buffer);

            var encoder = new Coders.Binary.Encoder();
            buffer.AddRange(encoder.Encode(message));

            CoderHelper.InsertDelimiter(ref buffer);

            return buffer.ToArray();
        }
    }
}