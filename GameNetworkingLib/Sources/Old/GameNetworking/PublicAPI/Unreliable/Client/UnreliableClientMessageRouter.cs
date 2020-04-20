using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Executors.Client;
using GameNetworking.Messages;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableClientMessageRouter<TPlayer> : GameClientMessageRouter<UnreliableGameClient<TPlayer>, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        internal UnreliableClientMessageRouter(IMainThreadDispatcher dispatcher) : base(dispatcher) { }

        internal override void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
                case MessageType.connect: this.Execute(new UnreliableConnectResponseExecutor<TPlayer>(this.game)); break;
                case MessageType.disconnect: this.Execute(new UnreliableDisconnectResponseExecutor<TPlayer>(this.game)); break;
                default: base.Route(container); break;
            }
        }
    }
}