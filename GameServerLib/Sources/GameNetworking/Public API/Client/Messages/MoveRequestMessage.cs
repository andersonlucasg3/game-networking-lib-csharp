using Messages.Coders;

namespace GameNetworking.Messages.Client {
    public class MoveRequestMessage: ICodable {
        public Vec3 direction;

        public MoveRequestMessage() {
            this.direction = new Vec3();
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.direction = decoder.Object<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.direction);
        }
    }
}