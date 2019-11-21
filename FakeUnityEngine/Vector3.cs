namespace UnityEngine {
    public struct Vector3 {
        public float x;
        public float y;
        public float z;

        public static Vector3 zero { get; }

        public static Vector3 operator *(Vector3 vec, float value) => new Vector3();
        public static Vector3 operator +(Vector3 vec1, Vector3 vec2) => new Vector3();
        public static bool operator ==(Vector3 vec1, Vector3 vec2) => true;
        public static bool operator !=(Vector3 vec1, Vector3 vec2) => true;
    }
}