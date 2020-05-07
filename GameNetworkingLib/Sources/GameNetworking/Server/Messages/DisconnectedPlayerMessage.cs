using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    public struct DisconnectedPlayerMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.disconnectedPlayer;

        public int playerId { get; set; }

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.GetInt();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
        }
    }
}
