namespace UnityEngine {
    public class Component : Object {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public Component() {
            this.gameObject = new GameObject();
        }
    }
}