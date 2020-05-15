using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    struct ConnectedPlayerExecutor : IExecutor<IRemoteClientListener, ConnectedPlayerMessage> {
        public void Execute(IRemoteClientListener model, ConnectedPlayerMessage message)
            => model.RemoteClientDidConnect(message.playerId, message.isMe);
    }
}