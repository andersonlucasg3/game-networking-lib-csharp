using Messages.Models;
using Messages.Coders;

namespace GameNetworking.Messages.Server {
    internal class PingResultRequestMessage : ITypedMessage {
        public static int Type {
            get { return (int)MessageType.PING_RESULT; }
        }

        int ITypedMessage.Type {
            get { return PingResultRequestMessage.Type; }
        }

        public float pingValue;

        public PingResultRequestMessage() {
            this.pingValue = 0;
        }

        internal PingResultRequestMessage(float value) {
            this.pingValue = value;
        }

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.pingValue);
        }

        public void Decode(IDecoder decoder) {
            this.pingValue = decoder.Float();
        }
    }
}