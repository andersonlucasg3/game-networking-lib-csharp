using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    internal class PongRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.pong;

        public long pingRequestId { get; private set; }

        public PongRequestMessage() { }

        public PongRequestMessage(long pingRequestId) => this.pingRequestId = pingRequestId;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.pingRequestId);
        }

        public void Decode(IDecoder decoder) {
            this.pingRequestId = decoder.GetLong();
        }
    }
}