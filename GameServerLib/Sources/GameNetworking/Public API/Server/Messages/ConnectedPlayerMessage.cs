using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages.Server {
    public class ConnectedPlayerMessage: ITypedMessage {
        public static int Type {
            get { return (int)MessageType.CONNECTED_PLAYER; }
        }

        int ITypedMessage.Type {
            get { return ConnectedPlayerMessage.Type; }
        }

        public int playerId;
        public bool isMe = false;

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.Int();
            this.isMe = decoder.Bool();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.isMe);
        }
    }
}
