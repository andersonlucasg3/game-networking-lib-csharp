using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages {
    public class StartGameMessage: ITypedMessage {
        public static int Type {
            get { return (int)MessageType.START_GAME; }
        }

        int ITypedMessage.Type {
            get { return StartGameMessage.Type; }
        }

        public void Decode(IDecoder decoder) { }

        public void Encode(IEncoder encoder) { }
    }
}