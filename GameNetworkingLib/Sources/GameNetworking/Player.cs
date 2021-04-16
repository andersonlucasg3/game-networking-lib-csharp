using System;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Sockets;

namespace GameNetworking
{
    namespace Server
    {
        public interface IPlayer : IEquatable<IPlayer>
        {
            int playerId { get; }
            float mostRecentPingValue { get; }

            void Send(ITypedMessage message, ChannelType channelType);

            void Disconnect();
        }

        internal interface IPlayerMessageListener
        {
            void PlayerDidReceiveMessage(MessageContainer container, IPlayer from);
        }

        public class Player : IPlayer, IChannelListener<ReliableChannel>, INetworkServerMessageListener
        {
            private ReliableChannel reliableChannel { get; set; }
            private UnreliableChannel unreliableChannel { get; set; }
            internal NetEndPoint? remoteIdentifiedEndPoint { get; set; }
            internal IPlayerMessageListener listener { get; set; }
            
            public int playerId { get; private set; }
            public float mostRecentPingValue { get; internal set; }

            #region Public methods
            
            public Player()
            {
                //
            }

            public void Send(ITypedMessage message, ChannelType channelType)
            {
                switch (channelType)
                {
                    case ChannelType.reliable:
                        reliableChannel.Send(message);
                        break;
                    case ChannelType.unreliable:
                        if (!remoteIdentifiedEndPoint.HasValue) break;
                        unreliableChannel.Send(message, remoteIdentifiedEndPoint.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(channelType), channelType, null);
                }
            }

            public void Disconnect()
            {
                reliableChannel.CloseChannel();
                if (remoteIdentifiedEndPoint.HasValue) unreliableChannel.CloseChannel(remoteIdentifiedEndPoint.Value);
            }

            #endregion
            
            #region Internal methods

            internal void Configure(int thisPlayerId)
            {
                playerId = thisPlayerId;
            }
            
            internal void Configure(ReliableChannel reliable)
            {
                reliableChannel = reliable;
                reliableChannel.listener = this;
            }

            internal void Configure(UnreliableChannel unreliable)
            {
                unreliableChannel = unreliable;
            }

            #endregion

            #region IEquatable

            bool IEquatable<IPlayer>.Equals(IPlayer other)
            {
                return playerId == other?.playerId;
            }

            #endregion
            
            void IChannelListener<ReliableChannel>.ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container)
            {
                listener?.PlayerDidReceiveMessage(container, this);
            }
            
            void INetworkServerMessageListener.NetworkServerDidReceiveMessage(MessageContainer container)
            {
                listener?.PlayerDidReceiveMessage(container, this);
            }
        }
    }

    namespace Client
    {
        public interface IPlayer
        {
            int playerId { get; }
            float mostRecentPingValue { get; }

            bool isLocalPlayer { get; }
        }

        public class Player : IPlayer
        {
            internal double lastReceivedPingRequest;

            public Player()
            {
                lastReceivedPingRequest = TimeUtils.CurrentTime();
            }

            public int playerId { get; private set; }
            public bool isLocalPlayer { get; internal set; }
            public float mostRecentPingValue { get; internal set; }

            internal void Configure(int playerId, bool isLocalPlayer)
            {
                this.playerId = playerId;
                this.isLocalPlayer = isLocalPlayer;
            }
        }
    }
}
