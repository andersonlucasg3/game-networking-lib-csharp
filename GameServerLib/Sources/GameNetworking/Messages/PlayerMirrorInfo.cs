using Messages.Coders;

namespace GameNetworking.Messages {
    public class PlayerMirrorInfo: ICodable {
        public int playerId;

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.DecodeInt();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
        }
    }
}
