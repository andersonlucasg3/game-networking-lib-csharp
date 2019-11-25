namespace UnityEngine {
    public struct Vector2 {
        public static Vector2 zero { get; }
        
        public float x;
        public float y;

        public void Set(float x, float y) {
            this.x = x;
            this.y = y;
        }
        

        public static Vector2 operator *(Vector2 vec, float value) => new Vector2();
        public static Vector2 operator +(Vector2 vec1, Vector2 vec2) => new Vector2();
        public static bool operator ==(Vector2 vec1, Vector2 vec2) => true;
        public static bool operator !=(Vector2 vec1, Vector2 vec2) => true;
    }
}