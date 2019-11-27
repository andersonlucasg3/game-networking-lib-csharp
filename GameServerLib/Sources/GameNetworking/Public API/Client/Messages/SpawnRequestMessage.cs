using Messages.Coders;
using Messages.Models;

namespace GameNetworking.Messages.Client {
    public class SpawnRequestMessage: ITypedMessage {
        public static int Type {
            get { return (int)MessageType.SPAWN_REQUEST; }
        }

        int ITypedMessage.type {
            get { return SpawnRequestMessage.Type; }
        }
        
        public int spawnObjectId;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.spawnObjectId);
        }

        public void Decode(IDecoder decoder) {
            this.spawnObjectId = decoder.Int();
        }
    }
}