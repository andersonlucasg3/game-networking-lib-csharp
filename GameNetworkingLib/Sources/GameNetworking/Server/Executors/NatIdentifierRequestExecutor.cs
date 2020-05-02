using GameNetworking.Channels;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Server;
using GameNetworking.Sockets;

namespace GameNetworking.Executors.Server {
    class NatIdentifierRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameServer<TPlayer>, NatIdentifierRequestMessage>
        where TPlayer : class, GameNetworking.Server.IPlayer {
        private readonly NetEndPoint remoteEndPoint;

        public NatIdentifierRequestExecutor(IGameServer<TPlayer> instance, NetEndPoint remoteEndPoint, NatIdentifierRequestMessage message) : base(instance, message) {
            this.remoteEndPoint = remoteEndPoint;
        }

        public override void Execute() {
            if (!this.instance.playerCollection.TryGetPlayer(this.message.playerId, out TPlayer player)) { return; }
            var channel = player.GetChannel<UnreliableChannel>(Channel.unreliable);
            channel.SetRemote(this.remoteEndPoint);
            this.instance.networkServer.unreliableChannel.Register(this.remoteEndPoint, channel);

            this.instance.listener.GameServerPlayerDidConnect(player, Channel.unreliable);

            var serverEndPoint = this.instance.networkServer.listeningOnEndPoint;
            player.Send(new NatIdentifierResponseMessage(serverEndPoint.host, serverEndPoint.port), Channel.unreliable);
        }
    }
}