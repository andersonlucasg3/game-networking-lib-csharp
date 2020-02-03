using System;

namespace GameNetworking.Commons {
    public abstract class BaseWorker<GameType> where GameType : class {
        private readonly WeakReference weakServer;

        protected GameType instance => this.weakServer?.Target as GameType;
        protected readonly IMainThreadDispatcher dispatcher;

        protected BaseWorker(GameType instance, IMainThreadDispatcher dispatcher) {
            this.weakServer = new WeakReference(instance);
            this.dispatcher = dispatcher;
        }
    }
}