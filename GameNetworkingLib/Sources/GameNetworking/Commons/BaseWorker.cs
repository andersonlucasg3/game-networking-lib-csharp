using System;

namespace GameNetworking.Commons {
    public abstract class BaseWorker<TGame> where TGame : class {
        private readonly WeakReference weakServer;

        protected TGame instance => this.weakServer?.Target as TGame;
        protected IMainThreadDispatcher dispatcher { get; private set; }

        protected BaseWorker(TGame instance, IMainThreadDispatcher dispatcher) {
            this.weakServer = new WeakReference(instance);
            this.dispatcher = dispatcher;
        }
    }
}