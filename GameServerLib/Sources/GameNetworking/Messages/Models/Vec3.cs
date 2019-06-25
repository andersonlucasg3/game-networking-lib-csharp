using Messages.Coders;

namespace GameNetworking.Messages.Models {
    public class Vec3: ICodable {
        public float x;
        public float y;
        public float z;

        void IDecodable.Decode(IDecoder decoder) {
            this.x = decoder.DecodeFloat();
            this.y = decoder.DecodeFloat();
            this.z = decoder.DecodeFloat();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.x);
            encoder.Encode(this.y);
            encoder.Encode(this.z);
        }
    }
}