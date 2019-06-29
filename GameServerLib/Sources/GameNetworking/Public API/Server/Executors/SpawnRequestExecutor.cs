using UnityEngine;

namespace GameNetworking.Executors.Server {
    using Messages;
    using Models.Server;
    using Messages.Client;
    using Messages.Server;
    
    internal struct SpawnRequestExecutor: IExecutor {
        private readonly GameServer server;
        private readonly SpawnRequestMessage message;
        private readonly NetworkPlayer player;

        internal SpawnRequestExecutor(GameServer server, SpawnRequestMessage message, NetworkPlayer player) {
            this.server = server;
            this.message = message;
            this.player = player;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), "Executing...");

            var id = this.message.spawnObjectId;
            var spawned = this.server.Delegate?.GameServerSpawnCharacter(id, this.player);
            this.player.GameObject = spawned;

            var playerSpawn = new PlayerSpawnMessage {
                playerId = this.player.PlayerId,
                spawnId = id
            };
            spawned.transform.position.CopyToVec3(ref playerSpawn.position);
            spawned.transform.eulerAngles.CopyToVec3(ref playerSpawn.rotation);
            this.server.SendBroadcast(playerSpawn);
        }
    }
}