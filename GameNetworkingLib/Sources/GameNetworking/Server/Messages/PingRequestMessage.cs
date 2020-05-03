using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    internal class PingRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.ping;

        public long pingRequestId { get; private set; }

        public PingRequestMessage() { }

        public PingRequestMessage(long pingRequestId) => this.pingRequestId = pingRequestId;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.pingRequestId);
        }

        public void Decode(IDecoder decoder) {
            this.pingRequestId = decoder.GetLong();
        }
    }
}