using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages {
    public class MoveRequestMessage: ITypedMessage {
        public static int Type {
            get { return (int)MessageType.MOVE_REQUEST; }
        }

        int ITypedMessage.Type {
            get { return MoveRequestMessage.Type; }
        }

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