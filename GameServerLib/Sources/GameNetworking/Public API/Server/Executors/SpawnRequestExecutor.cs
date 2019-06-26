using UnityEngine;

namespace GameNetworking.Executors.Server {
    using Messages;
    using Models;
    using Messages.Client;
    using Messages.Server;
    
    internal class SpawnRequestExecutor: IExecutor {
        private readonly GameServer server;
        private readonly SpawnRequestMessage message;
        private readonly ClientPlayerPair pair;

        internal SpawnRequestExecutor(GameServer server, SpawnRequestMessage message, ClientPlayerPair pair) {
            this.server = server;
            this.message = message;
            this.pair = pair;
        }

        void IExecutor.Execute() {
            var id = this.message.spawnObjectId;
            var spawned = this.server.Delegate?.GameServerSpawnCharacter(id, pair.Player);
            this.pair.Player.GameObject = spawned;

            var playerSpawn = new PlayerSpawnMessage {
                playerId = this.pair.Player.PlayerId,
                spawnId = id,
                position = spawned.transform.position.ToVec3(),
                rotation = spawned.transform.eulerAngles.ToVec3()
            };
            this.server.BroadcastMessage(playerSpawn);
        }
    }
}