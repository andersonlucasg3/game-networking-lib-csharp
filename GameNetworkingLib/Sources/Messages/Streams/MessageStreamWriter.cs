using System.Collections.Generic;

namespace Messages.Streams {
    using Coders;
    using Models;

    public class MessageStreamWriter : IStreamWriter {
        public byte[] Write<TMessage>(TMessage message) where TMessage : ITypedMessage {
            var buffer = new List<byte>();
            CoderHelper.WriteHeader(message.type, ref buffer);

            buffer.AddRange(Coders.Binary.Encoder.Encode(message));

            CoderHelper.InsertDelimiter(ref buffer);

            return buffer.ToArray();
        }
    }
}