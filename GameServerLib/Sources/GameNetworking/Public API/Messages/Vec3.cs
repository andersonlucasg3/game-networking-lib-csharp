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

        public Vector3 ToVector3() {
            return new Vector3(this.x, this.y, this.z);
        }
    }

    internal static class Vector3Ext {
        internal static Vec3 ToVec3(this Vector3 op) {
            return new Vec3 {
                x = op.x,
                y = op.y,
                z = op.z
            };
        }
    }
}
