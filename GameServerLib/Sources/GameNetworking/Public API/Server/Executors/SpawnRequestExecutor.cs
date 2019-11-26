namespace GameNetworking.Executors.Server {
    using Models.Server;
    using Messages.Client;
    using Messages.Server;
    using Logging;

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
            Logger.Log($"Executing spawn for playerid-{this.player.playerId}");

            this.player.spawnId = this.message.spawnObjectId;

            var spawned = this.server.listener?.GameServerSpawnCharacter(this.player);
            this.player.gameObject = spawned;

            var self = this;
            this.server.AllPlayers().ForEach(each => {
                if (each.gameObject == null) { return; }

                // Sends the spawn message to all players
                var playerSpawn = new PlayerSpawnMessage {
                    playerId = self.player.playerId,
                    spawnId = self.player.spawnId,
                    position = spawned.transform.position,
                    rotation = spawned.transform.eulerAngles
                };
                self.server.Send(playerSpawn, each.client);

                if (each == self.player) { return; }

                // Sends the existing players spawn message to the player that just requested
                var spawn = new PlayerSpawnMessage {
                    playerId = each.playerId,
                    spawnId = each.spawnId,
                    position = each.transform.position,
                    rotation = each.transform.eulerAngles
                };
                self.server.Send(spawn, self.player.client);
            });
        }
    }
}