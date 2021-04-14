using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    struct PongRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.pong;

        public long pingRequestId { get; private set; }

        public PongRequestMessage(long pingRequestId) => this.pingRequestId = pingRequestId;

        public void Encode(IEncoder encoder) {
            encoder.Encode(pingRequestId);
        }

        public void Decode(IDecoder decoder) {
            pingRequestId = decoder.GetLong();
        }
    }
}