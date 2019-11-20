namespace UnityEngine {
    public class GameObject : Object {
        public Transform transform { get; }

        public T GetComponent<T>() where T : Component {
            return null;
        }
    }
}