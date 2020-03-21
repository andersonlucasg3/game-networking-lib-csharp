using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Contract.Client;
using GameNetworking.Executors;
using GameNetworking.Executors.Client;
using GameNetworking.Messages;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Commons;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commmons.Client {
    public class GameClientMessageRouter<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> 
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TNetworkingClient : INetworkingClient<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> client;
        private readonly IMainThreadDispatcher dispatcher;

        internal GameClientMessageRouter(GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> client, IMainThreadDispatcher dispatcher) {
            this.client = client;
            this.dispatcher = dispatcher;
        }

        private void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
                case MessageType.connectedPlayer: this.Execute(new ConnectedPlayerExecutor<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>(this.client, container.Parse<ConnectedPlayerMessage>())); break;
                case MessageType.ping: this.Execute(new PingRequestExecutor<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>(this.client)); break;
                case MessageType.pingResult: this.Execute(new PingResultRequestExecutor<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>(this.client, container.Parse<PingResultRequestMessage>())); break;
                case MessageType.disconnectedPlayer: this.Execute(new DisconnectedPlayerExecutor<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>(this.client, container.Parse<DisconnectedPlayerMessage>())); break;
                default: this.client?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }

        private IExecutor Make<TExecutor, TMessage>(MessageContainer container) 
            where TExecutor : class, IExecutor, IConfigurableExecutor<GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>, TMessage>, new()
            where TMessage : class, ITypedMessage, new() {
            var exec = new TExecutor();
            exec.Configure(this.client, this.dispatcher, container.Parse<TMessage>());
            return exec;
        }
    }

    public interface IConfigurableExecutor<TGame, TMessage> where TMessage : ITypedMessage {
        void Configure(TGame game, IMainThreadDispatcher dispatcher, TMessage message);
    }
}