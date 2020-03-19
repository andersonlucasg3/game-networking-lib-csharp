using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace MatchMaking.Models {
    public sealed class MessageContainer {
        private readonly MessagePackage package;

        public string TypeName { get { return this.package.message.TypeUrl; } }

        internal MessageContainer(MessagePackage package) {
            this.package = package;
        }

        public bool Is(MessageDescriptor descriptor) {
            return this.package.message.Is(descriptor);
        }

        public TMessage Parse<TMessage>() where TMessage: class, IMessage<TMessage>, new() {
            MessageParser<TMessage> parser = new MessageParser<TMessage>(() => { return new TMessage(); });
            return parser.ParseFrom(this.package.message.Value.ToByteArray());
        }
    }
}