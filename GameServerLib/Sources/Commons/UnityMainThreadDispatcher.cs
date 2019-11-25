using UnityEngine;
using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour {
    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher instance {
        get {
            if (_instance == null) {
                _instance = new GameObject("MainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
            }
            return _instance;
        }
    }

    private readonly Queue<Action> executionQueue = new Queue<Action>();

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    protected virtual void OnDestroy() {
        _instance = null;
    }

    public virtual void Update() {
        lock (executionQueue) {
            while (executionQueue.Count > 0) {
                executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action) {
        lock (executionQueue) {
            executionQueue.Enqueue(action);
        }
    }
}