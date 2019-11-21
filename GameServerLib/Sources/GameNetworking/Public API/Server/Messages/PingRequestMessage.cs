using Messages.Models;
using Messages.Coders;

namespace GameNetworking.Messages.Server {
    internal class PingRequestMessage : ITypedMessage {
        public static int Type {
            get { return (int)MessageType.PING; }
        }

        int ITypedMessage.Type {
            get { return PingResultRequestMessage.Type; }
        }

        public void Encode(IEncoder encoder) { }

        public void Decode(IDecoder decoder) { }
    }
}