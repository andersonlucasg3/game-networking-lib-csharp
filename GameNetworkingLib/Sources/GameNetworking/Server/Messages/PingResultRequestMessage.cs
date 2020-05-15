using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    struct PingResultRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.pingResult;

        public int playerId { get; private set; }
        public float pingValue { get; private set; }

        public PingResultRequestMessage(int playerId, float value) {
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