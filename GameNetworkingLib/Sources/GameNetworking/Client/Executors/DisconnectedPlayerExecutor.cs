using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client
{
    internal struct DisconnectedPlayerExecutor : IExecutor<IRemoteClientListener, DisconnectedPlayerMessage>
    {
        public void Execute(IRemoteClientListener model, DisconnectedPlayerMessage message)
        {
            model.RemoteClientDidDisconnect(message.playerId);
        }
    }
}