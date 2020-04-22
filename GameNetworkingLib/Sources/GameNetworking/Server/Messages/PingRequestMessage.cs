using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    internal class PingRequestMessage : ITypedMessage {
        public static int Type {
            get { return (int)MessageType.ping; }
        }

        int ITypedMessage.type {
            get { return PingRequestMessage.Type; }
        }

        public void Encode(IEncoder encoder) { }

        public void Decode(IDecoder decoder) { }
    }
}