using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    internal class PongRequestMessage : ITypedMessage {
        int ITypedMessage.type { get { return (int)MessageType.pong; } }

        public void Encode(IEncoder encoder) { }

        public void Decode(IDecoder decoder) { }
    }
}