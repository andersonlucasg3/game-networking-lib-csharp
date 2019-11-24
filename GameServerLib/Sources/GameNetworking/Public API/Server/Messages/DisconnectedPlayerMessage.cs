using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages.Server {
    public class DisconnectedPlayerMessage : ITypedMessage {
        int ITypedMessage.Type {
            get { return (int)MessageType.CONNECTED_PLAYER; }
        }

        public int playerId;

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.Int();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
        }
    }
}
