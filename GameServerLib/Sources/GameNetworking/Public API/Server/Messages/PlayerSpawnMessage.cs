using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages.Server {
    public class PlayerSpawnMessage: ITypedMessage {
        public static int Type {
            get { return (int)MessageType.SPAWN_REQUEST; }
        }

        int ITypedMessage.Type {
            get { return PlayerSpawnMessage.Type; }
        }

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