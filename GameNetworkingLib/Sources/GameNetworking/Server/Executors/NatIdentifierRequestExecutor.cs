using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Sockets;
using GameNetworking.Server;

namespace GameNetworking.Executors.Server
{
    internal struct NatIdentifierRequestExecutor<TPlayer> :
        IExecutor<GameServerMessageRouter<TPlayer>.ServerModel<NetEndPoint>, NatIdentifierRequestMessage>
        where TPlayer : Player, new()
    {
        public void Execute(GameServerMessageRouter<TPlayer>.ServerModel<NetEndPoint> model, NatIdentifierRequestMessage message)
        {
            if (!model.server.playerCollection.TryGetPlayer(message.playerId, out var player)) return;
            player.remoteIdentifiedEndPoint = model.model;
            model.server.networkServer.Register(model.model, player);

            model.server.listener.GameServerPlayerDidConnect(player, Channel.unreliable);

            var serverEndPoint = model.server.networkServer.listeningOnEndPoint;
            player.Send(new NatIdentifierResponseMessage(serverEndPoint.address, serverEndPoint.port), Channel.unreliable);
        }
    }
}