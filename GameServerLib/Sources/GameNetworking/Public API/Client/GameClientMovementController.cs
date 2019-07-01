namespace GameNetworking {
    using Models;
    public class GameClientMovementController: MovementController<GameClient> {
        public GameClientMovementController(GameClient instance, NetworkPlayersStorage storage) : base(instance, storage) { 
            
        }

        public override void Update() {
            this.players.ForEachConverted(player => (Models.Client.NetworkPlayer)player, player => {
                this.Instance?.InstanceDelegate?.GameInstanceMovePlayer(player, this);
            });
        }
    }
}