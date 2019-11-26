namespace UnityEngine {
    public struct Vector3 {
        public static Vector3 zero { get; }
        
        public float x;
        public float y;
        public float z;

        public void Set(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator *(Vector3 vec, float value) => new Vector3();
        public static Vector3 operator +(Vector3 vec1, Vector3 vec2) => new Vector3();
        public static bool operator ==(Vector3 vec1, Vector3 vec2) => true;
        public static bool operator !=(Vector3 vec1, Vector3 vec2) => true;
    }
}