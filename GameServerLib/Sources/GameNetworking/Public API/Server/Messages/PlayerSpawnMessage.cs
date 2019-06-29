using Messages.Coders;

namespace GameNetworking.Messages.Server {
    using Models;

    public class PlayerSpawnMessage: ICodable {
        public int spawnId;
        public int playerId;
        public Vec3 position;
        public Vec3 rotation;

        public PlayerSpawnMessage() {
            this.position = new Vec3();
            this.rotation = new Vec3();
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.spawnId = decoder.Int();
            this.playerId = decoder.Int();
            this.position = decoder.Object<Vec3>();
            this.rotation = decoder.Object<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.spawnId);
            encoder.Encode(this.playerId);
            encoder.Encode(this.position);
            encoder.Encode(this.rotation);
        }
    }
}