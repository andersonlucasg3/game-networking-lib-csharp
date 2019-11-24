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
            Logging.Logger.Log(this.GetType(), $"Executing spawn for playerid-{this.player.PlayerId}");

            this.player.SpawnId = this.message.spawnObjectId;

            var spawned = this.server.listener?.GameServerSpawnCharacter(this.player);
            this.player.GameObject = spawned;

            var playerSpawn = new PlayerSpawnMessage {
                playerId = this.player.PlayerId,
                spawnId = this.player.SpawnId
            };
            spawned.transform.position.CopyToVec3(ref playerSpawn.position);
            spawned.transform.eulerAngles.CopyToVec3(ref playerSpawn.rotation);
            this.server.SendBroadcast(playerSpawn);

            var self = this;
            this.server.AllPlayers().ForEach(each => {
                if (each == self.player || each.GameObject == null) { return; }
                var spawn = new PlayerSpawnMessage();
                spawn.playerId = each.PlayerId;
                spawn.spawnId = each.SpawnId;
                each.GameObject.transform.position.CopyToVec3(ref spawn.position);
                each.GameObject.transform.eulerAngles.CopyToVec3(ref spawn.rotation);
                self.server.Send(spawn, self.player.Client);
            });
        }
    }
}