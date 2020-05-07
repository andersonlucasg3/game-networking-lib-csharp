using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Sockets;
using GameNetworking.Server;

namespace GameNetworking.Executors.Server {
    class NatIdentifierRequestExecutor<TPlayer> : IExecutor<IGameServer<TPlayer>, NatIdentifierRequestMessage>
        where TPlayer : GameNetworking.Server.Player {
        private readonly NetEndPoint remoteEndPoint;

        public NatIdentifierRequestExecutor(NetEndPoint remoteEndPoint) {
            this.remoteEndPoint = remoteEndPoint;
        }

        public void Execute(IGameServer<TPlayer> model, NatIdentifierRequestMessage message) {
            if (!model.playerCollection.TryGetPlayer(message.playerId, out TPlayer player)) { return; }
            player.remoteIdentifiedEndPoint = this.remoteEndPoint;
            model.networkServer.Register(this.remoteEndPoint, player);

            model.listener.GameServerPlayerDidConnect(player, Channel.unreliable);

            var serverEndPoint = model.networkServer.listeningOnEndPoint;
            player.Send(new NatIdentifierResponseMessage(serverEndPoint.host, serverEndPoint.port), Channel.unreliable);
        }
    }
}