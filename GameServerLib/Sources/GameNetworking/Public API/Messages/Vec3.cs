using Messages.Coders;
using UnityEngine;

namespace GameNetworking.Messages {
    public class Vec3: ICodable {
        public float x;
        public float y;
        public float z;

        void IDecodable.Decode(IDecoder decoder) {
            this.x = decoder.Float();
            this.y = decoder.Float();
            this.z = decoder.Float();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.x);
            encoder.Encode(this.y);
            encoder.Encode(this.z);
        }

        public void CopyToVector3(ref Vector3 instance) {
            instance.x = this.x;
            instance.y = this.y;
            instance.z = this.z;
        }

        public static implicit operator Vector3(Vec3 v) {
            var vec = Vector3.zero;
            vec.Set(v.x, v.y, v.z);
            return vec;
        }

        public static implicit operator Vec3(Vector3 v) {
            return new Vec3 { x = v.x, y = v.y, z = v.z };
        }

        public override string ToString() {
            return string.Format("({0}, {1}, {2})", this.x, this.y, this.z);
        }
    }
}
