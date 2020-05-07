using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    public struct ConnectedPlayerMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.connectedPlayer;

        public int playerId { get; set; }
        public bool isMe { get; set; }

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.GetInt();
            this.isMe = decoder.GetBool();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.isMe);
        }
    }
}
