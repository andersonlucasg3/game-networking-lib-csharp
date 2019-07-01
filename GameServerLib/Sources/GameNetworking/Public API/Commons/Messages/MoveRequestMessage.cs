using Messages.Coders;

namespace GameNetworking.Messages {
    public class MoveRequestMessage: ICodable {
        public Vec3 direction;
        public int playerId;

        public MoveRequestMessage() {
            this.direction = new Vec3();
        }
        public MoveRequestMessage(int playerId) : this() {
            this.playerId = playerId;
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.direction);
            encoder.Encode(this.playerId);
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.direction = decoder.Object<Vec3>();
            this.playerId = decoder.Int();
        }
    }
}