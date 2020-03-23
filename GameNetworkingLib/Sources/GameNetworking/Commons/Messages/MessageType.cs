namespace GameNetworking.Messages {
    public enum MessageType {
        connect = 100,
        connectedPlayer,
        disconnectedPlayer,
        ping,
        pong,
        pingResult
    }
}