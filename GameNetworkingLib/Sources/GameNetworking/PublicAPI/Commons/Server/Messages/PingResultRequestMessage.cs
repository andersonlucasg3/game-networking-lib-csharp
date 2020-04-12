using Messages.Models;
using Messages.Coders;

namespace GameNetworking.Messages.Server {
    internal class PingResultRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.pingResult;

        public float pingValue { get; private set; }
        public int playerId { get; private set; }

        public PingResultRequestMessage() {
            this.pingValue = 0;
            this.playerId = 0;
        }

        internal PingResultRequestMessage(int playerId, float value) {
            this.playerId = playerId;
            this.pingValue = value;
        }

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.pingValue);
        }

        public void Decode(IDecoder decoder) {
            this.playerId = decoder.GetInt();
            this.pingValue = decoder.GetFloat();
        }
    }
}