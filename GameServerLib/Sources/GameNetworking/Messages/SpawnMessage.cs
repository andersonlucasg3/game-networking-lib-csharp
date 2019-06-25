using Messages.Coders;

namespace GameNetworking.Messages {
    public class SpawnMessage: ICodable {
        public int spawnId;

        void IDecodable.Decode(IDecoder decoder) {
            this.spawnId = decoder.DecodeInt();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.spawnId);
        }
    }
}