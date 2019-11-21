using System;

namespace GameNetworking {
    using Models;

    public class GameClientPlayersMovementController {
        private WeakReference weakGameClient;
        private NetworkPlayersStorage storage;
        
        private GameClient Instance {
            get { return this.weakGameClient?.Target as GameClient; }
        }

        internal GameClientPlayersMovementController(GameClient client, NetworkPlayersStorage storage) {
            this.weakGameClient = new WeakReference(client);
            this.storage = storage;
        }

        public void Update() {
            this.storage.ForEachConverted(player => player as Models.Client.NetworkPlayer, player => {
                if (!player.IsLocalPlayer) { 
                    this.Instance.InstanceDelegate?.GameInstanceMovePlayer(player, player.inputState.direction, player.Transform.position); 
                }
            });
        }
    }
}