using Messages.Coders;

namespace GameNetworking.Messages {
    using Messages.Models;

    public class SyncMessage: ICodable {
        public Vec3 position;
        public Vec3 rotation;

        void IDecodable.Decode(IDecoder decoder) {
            this.position = decoder.Decode<Vec3>();
            this.rotation = decoder.Decode<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.position);
            encoder.Encode(this.rotation);
        }
    }
}