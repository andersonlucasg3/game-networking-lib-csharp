namespace UnityEngine {
    public class GameObject {
        public Transform transform { get; }

        public T GetComponent<T>() where T : MonoBehaviour {
            return null;
        }
    }
}