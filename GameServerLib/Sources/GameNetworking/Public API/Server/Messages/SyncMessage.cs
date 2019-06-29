using Messages.Coders;

namespace GameNetworking.Messages.Server {
    public class SyncMessage: ICodable {
        public int playerId;
        public Vec3 position;
        public Vec3 rotation;

        public SyncMessage() {
            this.position = new Vec3();
            this.rotation = new Vec3();
        }
        
        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.Int();
            this.position = decoder.Object<Vec3>();
            this.rotation = decoder.Object<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.position);
            encoder.Encode(this.rotation);
        }
    }
}