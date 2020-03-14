using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.IO;

namespace MatchMaking.Protobuf.Coders {
    using MatchMaking.Coders;
    using Messages.Coders;

    public sealed class MessageEncoder: IMessageEncoder {
        public byte[] Encode<Message>(Message message) where Message: IMessage {
            var package = new MessagePackage();
            package.Message = Any.Pack(message, CoderHelper.typePrefix);
            List<byte> buffer;
            using (var stream = new MemoryStream()) {
                package.WriteTo(stream);

                buffer = new List<byte>(stream.ToArray());
            }

            CoderHelper.InsertDelimiter(ref buffer);
            return buffer.ToArray();
        }
    }
}
