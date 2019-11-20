namespace UnityEngine {
    public class GameObject : Object {
        public Transform transform { get; }

        public T GetComponent<T>() where T : Component {
            return null;
        }

        public bool TryGetComponent<T>(out T component) where T : Component {
            component = null;
            return false;
        }
    }
}