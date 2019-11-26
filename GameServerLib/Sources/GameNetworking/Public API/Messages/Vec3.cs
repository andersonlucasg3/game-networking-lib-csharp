using Messages.Coders;
using UnityEngine;

namespace GameNetworking.Messages {
    public class Vec3: Vec2 {
        public float z;

        public override void Decode(IDecoder decoder) {
            base.Decode(decoder);
            this.z = decoder.Float();
        }

        public override void Encode(IEncoder encoder) {
            base.Encode(encoder);
            encoder.Encode(this.z);
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
