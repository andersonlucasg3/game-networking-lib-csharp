using System;

namespace GameNetworking {
    public abstract class BaseWorker<GameType> where GameType: class, IGameInstance {
        private readonly WeakReference weakServer;

        protected GameType Instance => this.weakServer?.Target as GameType;

        protected BaseWorker(GameType instance) {
            this.weakServer = new WeakReference(instance);
        }
    }
}