using Messages.Coders;

namespace GameNetworking.Messages.Client {
    public class MoveRequestMessage: ICodable {
        public Vec3 direction;

        void IDecodable.Decode(IDecoder decoder) {
            this.direction = decoder.Object<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.direction);
        }
    }
}