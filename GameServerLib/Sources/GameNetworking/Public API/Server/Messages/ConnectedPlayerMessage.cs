using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages.Server {
    public class ConnectedPlayerMessage: ITypedMessage {
        int ITypedMessage.type => (int)MessageType.CONNECTED_PLAYER;

        public int playerId;
        public bool isMe;

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.Int();
            this.isMe = decoder.Bool();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.isMe);
        }
    }
}
