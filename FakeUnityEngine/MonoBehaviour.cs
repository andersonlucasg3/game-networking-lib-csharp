using System.Collections;

namespace UnityEngine {
    public abstract class MonoBehaviour: Behaviour {
        public Coroutine StartCoroutine(IEnumerator routine) {
            return new Coroutine();
        }
    }
}