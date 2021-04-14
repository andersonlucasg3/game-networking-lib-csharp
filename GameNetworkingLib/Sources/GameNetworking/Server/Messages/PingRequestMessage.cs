using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    struct PingRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.ping;

        public long pingRequestId { get; private set; }

        public PingRequestMessage(long pingRequestId) => this.pingRequestId = pingRequestId;

        public void Encode(IEncoder encoder) {
            encoder.Encode(pingRequestId);
        }

        public void Decode(IDecoder decoder) {
            pingRequestId = decoder.GetLong();
        }
    }
}