using System.Collections.Concurrent;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Server;
using GameNetworking.Networking;
using Logging;

namespace GameNetworking.Server
{
    public interface IGameServerClientAcceptorListener<TPlayer> where TPlayer : Player
    {
        IReadOnlyPlayerCollection<int, TPlayer> playerCollection { get; }

        void ClientAcceptorPlayerDidConnect(TPlayer player);
        void ClientAcceptorPlayerDidDisconnect(TPlayer player);

        void SendBroadcast(ITypedMessage message, ChannelType channelType);
    }

    public sealed class GameServerClientAcceptor<TPlayer> : INetworkServerListener where TPlayer : Player, new()
    {
        private readonly ConcurrentDictionary<ReliableChannel, TPlayer> channelCollection;
        private int playerIdCounter;

        public IGameServerClientAcceptorListener<TPlayer> listener { get; set; }

        public GameServerClientAcceptor()
        {
            channelCollection = new ConcurrentDictionary<ReliableChannel, TPlayer>();
        }

        private void AcceptClient(TPlayer player)
        {
            listener.ClientAcceptorPlayerDidConnect(player);

            var players = listener.playerCollection.values;
            for (var i = 0; i < players.Count; i++)
            {
                var each = players[i];

                // Sends the connected player message to all players
                var connectedAll = new ConnectedPlayerMessage
                {
                    playerId = player.playerId,
                    isMe = player.Equals(each)
                };
                each.Send(connectedAll, ChannelType.reliable);

                if (each.Equals(player)) continue;

                // Sends the existing players to the player that just connected
                var connectedSelf = new ConnectedPlayerMessage
                {
                    playerId = each.playerId,
                    isMe = false
                };
                player.Send(connectedSelf, ChannelType.reliable);
            }

            if (Logger.IsLoggingEnabled) Logger.Log($"(AcceptClient) count {listener.playerCollection.count}");
        }

        private void Disconnect(TPlayer player)
        {
            if (Logger.IsLoggingEnabled) Logger.Log($"(Disconnect) count {listener.playerCollection.count}");

            if (player == null) return;
            
            listener.ClientAcceptorPlayerDidDisconnect(player);
            listener.SendBroadcast(new DisconnectedPlayerMessage {playerId = player.playerId}, ChannelType.reliable);
        }

        void INetworkServerListener.NetworkServerReliableChannelConnected(ReliableChannel reliable)
        {
            var player = new TPlayer();
            player.Configure(playerIdCounter++);
            player.Configure(reliable);

            channelCollection[reliable] = player;

            AcceptClient(player);
        }

        void INetworkServerListener.NetworkServerUnreliableChannelConnected(UnreliableChannel unreliable)
        {
            // TODO: ...
        }

        void INetworkServerListener.NetworkServerReliableChannelDisconnected(ReliableChannel channel)
        {
            if (channelCollection.TryRemove(channel, out var player)) Disconnect(player);
        }
    }
}
