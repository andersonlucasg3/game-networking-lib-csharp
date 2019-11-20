using System.Collections;

namespace UnityEngine {
    public abstract class MonoBehaviour {
        public GameObject gameObject;
        public Transform transform;
        public bool enabled;

        public void StartCoroutine(IEnumerator action) {

        }

        public void DontDestroyOnLoad(GameObject @object) {

        }
    }
}