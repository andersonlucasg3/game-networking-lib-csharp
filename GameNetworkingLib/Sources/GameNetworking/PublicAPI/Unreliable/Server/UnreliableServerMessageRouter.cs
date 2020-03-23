using GameNetworking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons.Server;
using GameNetworking.Executors.Server;
using GameNetworking.Messages;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Messages.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableServerMessageRouter<TPlayer> : GameServerMessageRouter<UnreliableGameServer<TPlayer>, UnreliableNetworkingServer, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        internal UnreliableServerMessageRouter(IMainThreadDispatcher dispatcher) : base(dispatcher) { }

        public override void Route(MessageContainer container, TPlayer player) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
                case MessageType.connect: this.Execute(new UnreliableConnectExecutor<TPlayer>(this.server)); break;
                default: base.Route(container, player); break;
            }
        }
    }
}