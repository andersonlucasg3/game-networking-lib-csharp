using System.Collections.Generic;

namespace UnityEngine {
    public class GameObject : Object {
        private List<Component> components;

        public Transform transform { get; }

        public GameObject() {
            this.components = new List<Component>();
            this.transform = new Transform();
        }

        public GameObject(string name) : this() {

        }

        public T AddComponent<T>() where T : Component, new() {
            T comp = new T();
            this.components.Add(comp);
            return comp;
        }

        public T GetComponent<T>() where T : Component {
            return (T)this.components.Find((each) => each is T);
        }

        public bool TryGetComponent<T>(out T component) where T : Component {
            component = (T)this.components.Find((each) => each is T);
            return component != null;
        }
    }
}