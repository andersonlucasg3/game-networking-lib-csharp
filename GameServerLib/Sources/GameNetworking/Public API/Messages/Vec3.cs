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
    }

    internal static class Vector3Ext {
        internal static void CopyToVec3(this Vector3 op, ref Vec3 instance) {
            instance.x = op.x;
            instance.y = op.y;
            instance.z = op.z;
        }
    }
}
