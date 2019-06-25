using Messages.Coders;

namespace GameNetworking.Messages {
    using Models;

    public class MoveMessage: ICodable {
        public Vec3 direction;

        void IDecodable.Decode(IDecoder decoder) {
            this.direction = decoder.Decode<Vec3>();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.direction);
        }
    }
}