using Messages.Models;
using System;

namespace GameNetworking {
    using Messages;

    internal interface IGameClientMessageRouterDelegate {
        void StartGame();
        void SpawnPlayer(SpawnMessage message);
        void SyncPlayer(SyncMessage message);
        void MirrorPlayerInfo(PlayerMirrorInfo message);
        void CustomServerMessage(MessageContainer container);
    }

    internal class GameClientMessageRouter {
        private WeakReference weakDelegate;

        public IGameClientMessageRouterDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameClientMessageRouterDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        internal void Route(MessageContainer container) {
            if (container.Is(typeof(StartGameMessage))) {
                this.Delegate?.StartGame();
            } else if (container.Is(typeof(SpawnMessage))) {
                this.Delegate?.SpawnPlayer(container.Parse<SpawnMessage>());
            } else if (container.Is(typeof(SyncMessage))) {
                this.Delegate?.SyncPlayer(container.Parse<SyncMessage>());
            } else if (container.Is(typeof(PlayerMirrorInfo))) {
                this.Delegate?.MirrorPlayerInfo(container.Parse<PlayerMirrorInfo>());
            } else {
                this.Delegate?.CustomServerMessage(container);
            }
        }
    }
}