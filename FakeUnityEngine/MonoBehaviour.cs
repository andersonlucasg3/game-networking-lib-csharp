using System.Collections;

namespace UnityEngine {
    public abstract class MonoBehaviour {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public bool enabled { get; set; }

        public void StartCoroutine(IEnumerator action) {

        }

        public void DontDestroyOnLoad(GameObject @object) {

        }
    }
}