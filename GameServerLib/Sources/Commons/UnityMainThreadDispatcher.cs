using UnityEngine;
using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour {
    public static UnityMainThreadDispatcher instance { get; private set; }

    private readonly Queue<Action> executionQueue = new Queue<Action>();

    protected virtual void Awake() {
        if (instance == null) {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    protected virtual void OnDestroy() {
        instance = null;
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