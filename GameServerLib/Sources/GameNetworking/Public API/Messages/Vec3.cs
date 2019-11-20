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

        public override string ToString() {
            return string.Format("({0}, {1}, {2})", this.x, this.y, this.z);
        }
    }

    public static class Vector3Ext {
        public static void CopyToVec3(this Vector3 op, ref Vec3 instance) {
            instance.x = op.x;
            instance.y = op.y;
            instance.z = op.z;
        }
    }
}
