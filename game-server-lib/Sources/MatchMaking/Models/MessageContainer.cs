using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace MatchMaking.Models {
    public sealed class MessageContainer {
        private MessagePackage package;

        public string TypeName { get { return this.package.Message.TypeUrl; } }

        internal MessageContainer(MessagePackage package) {
            this.package = package;
        }

        public bool Is(MessageDescriptor descriptor) {
            return this.package.Message.Is(descriptor);
        }

        public Message Parse<Message>() where Message: class, IMessage<Message>, new() {
            MessageParser<Message> parser = new MessageParser<Message>(() => { return new Message(); });
            return parser.ParseFrom(this.package.Message.Value.ToByteArray());
        }
    }
}