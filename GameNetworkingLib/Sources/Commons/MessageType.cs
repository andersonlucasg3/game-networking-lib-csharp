namespace GameNetworking.Messages
{
    public enum MessageType
    {
        connect = 100,
        connectedPlayer,
        disconnect,
        disconnectedPlayer,
        ping,
        pong,
        pingResult,
        natIdentifier
    }
}