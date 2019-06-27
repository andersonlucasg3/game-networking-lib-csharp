using System.Collections.Generic;

namespace Messages.Streams {
    using Coders;

    public class MessageStreamWriter: IStreamWriter {
        public byte[] Write<Message>(Message message) where Message : IEncodable {
            var buffer = new List<byte>();
            CoderHelper.WriteHeader(message.GetType(), ref buffer);

            var encoder = new Coders.Binary.Encoder();
            buffer.AddRange(encoder.Encode(message));

            CoderHelper.InsertDelimiter(ref buffer);

            return buffer.ToArray();
        }
    }
}