namespace UnityEngine {
    public class GameObject {
        public Transform transform;

        public T GetComponent<T>() where T : MonoBehaviour {
            return null;
        }
    }
}