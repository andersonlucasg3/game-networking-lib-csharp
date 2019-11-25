using Messages.Coders;
using UnityEngine;

namespace GameNetworking.Messages {
    class Vec2 : ICodable {
        public float x;
        public float y;

        public void Set(float x, float y) {
            this.x = x;
            this.y = y;
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.x = decoder.Float();
            this.y = decoder.Float();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.x);
            encoder.Encode(this.y);
        }

        public static implicit operator Vector2(Vec2 v) {
            var vec = Vector2.zero;
            vec.Set(v.x, v.y);
            return vec;
        }

        public static implicit operator Vec2(Vector2 v) {
            return new Vec2 { x = v.x, y = v.y };
        }
    }
}