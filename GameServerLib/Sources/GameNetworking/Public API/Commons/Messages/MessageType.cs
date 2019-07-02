namespace GameNetworking.Messages {
    public enum MessageType : int {
        SPAWN_REQUEST = 100,
        MOVE_REQUEST = 101,
        CONNECTED_PLAYER = 102,
        SYNC = 103,
        START_GAME = 104
    }
}