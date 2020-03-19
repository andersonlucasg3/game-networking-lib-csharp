using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages.Server {
    public class DisconnectedPlayerMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.disconnectedPlayer;

        public int playerId;

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.Int();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
        }
    }
}
