using Messages.Models;
using Messages.Coders;

namespace GameNetworking.Messages.Client {
    internal class PongRequestMessage: ITypedMessage {
        public static int Type { get { return (int)MessageType.pong; } }

        int ITypedMessage.type { get { return PongRequestMessage.Type; } }

        public void Encode(IEncoder encoder) { }

        public void Decode(IDecoder decoder) { }
    }
}