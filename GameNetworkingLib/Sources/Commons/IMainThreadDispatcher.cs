using System;

namespace GameNetworking.Commons {
    public interface IMainThreadDispatcher {
        void Enqueue(Action action);
    }
}