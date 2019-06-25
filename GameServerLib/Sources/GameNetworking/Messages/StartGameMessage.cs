using Messages.Coders;

namespace GameNetworking.Messages {
    public class StartGameMessage: ICodable {
        void IDecodable.Decode(IDecoder decoder) { }

        void IEncodable.Encode(IEncoder encoder) { }
    }
}