using Messages.Coders;

namespace GameNetworking.Messages {
    using Models;

    public class SpawnMessage: ICodable {
        public int spawnId;
        public int playerId;
        public Vec3 position;
        public Vec3 rotation;

        void IDecodable.Decode(IDecoder decoder) {
            this.spawnId = decoder.DecodeInt();
            this.playerId = decoder.DecodeInt();
            this.position = decoder.Decode<Vec3>();
            this.rotation = decoder.Decode<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.spawnId);
            encoder.Encode(this.playerId);
            encoder.Encode(this.position);
            encoder.Encode(this.rotation);
        }
    }
}