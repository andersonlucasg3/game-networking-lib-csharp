using Messages.Coders;

namespace GameNetworking.Messages.Server {
    public class ConnectedPlayerMessage: ICodable {
        public int playerId;
        public bool isMe = false;

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
