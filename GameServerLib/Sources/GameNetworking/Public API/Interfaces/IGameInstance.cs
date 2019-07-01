namespace GameNetworking {
    public interface IGameInstanceDelegate {
        void GameInstanceMovePlayer(Models.Server.NetworkPlayer player, IMovementController movementController);
    }

    public interface IGameInstance {
        IGameInstanceDelegate InstanceDelegate { get; set; }
        IMovementController MovementController { get; }
    }
}