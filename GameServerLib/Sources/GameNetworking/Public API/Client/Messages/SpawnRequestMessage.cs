using Messages.Coders;

namespace GameNetworking.Messages.Client {
    public class SpawnRequestMessage: ICodable {
        public int spawnObjectId;

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.spawnObjectId);
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.spawnObjectId = decoder.Int();
        }
    }
}