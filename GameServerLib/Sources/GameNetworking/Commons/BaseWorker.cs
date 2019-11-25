using System;

namespace GameNetworking.Commons {
    public abstract class BaseWorker<GameType> where GameType: class, IGameInstance {
        private readonly WeakReference weakServer;

        protected GameType instance => this.weakServer?.Target as GameType;

        protected BaseWorker(GameType instance) {
            this.weakServer = new WeakReference(instance);
        }
    }
}