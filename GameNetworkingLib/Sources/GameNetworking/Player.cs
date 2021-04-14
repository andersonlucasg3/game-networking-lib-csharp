using System;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Sockets;

namespace GameNetworking {
    namespace Server {
        public interface IPlayer : IEquatable<IPlayer> {
            int playerId { get; }
            float mostRecentPingValue { get; }

            void Send(ITypedMessage message, Channel channel);

            void Disconnect();
        }

        internal interface IPlayerMessageListener {
            void PlayerDidReceiveMessage(MessageContainer container, IPlayer from);
        }

        public class Player : IPlayer, IReliableChannelListener, INetworkServerMessageListener {

            internal double lastReceivedPongRequest;
            internal ReliableChannel reliableChannel { get; private set; }
            internal UnreliableChannel unreliableChannel { get; private set; }
            internal NetEndPoint? remoteIdentifiedEndPoint { get; set; }
            internal IPlayerMessageListener listener { get; set; }

            public int playerId { get; internal set; }
            public float mostRecentPingValue { get; internal set; }

            public Player() { lastReceivedPongRequest = TimeUtils.CurrentTime(); }

            #region Internal methods

            internal void Configure(int playerId) => this.playerId = playerId;
            internal void Configure(ReliableChannel reliable, UnreliableChannel unreliable) {
                reliableChannel = reliable;
                unreliableChannel = unreliable;

                reliableChannel.listener = this;
            }

            #endregion

            #region Public methods

            public void Send(ITypedMessage message, Channel channel) {
                switch (channel) {
                    case Channel.reliable: reliableChannel.Send(message); break;
                    case Channel.unreliable:
                        if (!remoteIdentifiedEndPoint.HasValue) { break; }
                        unreliableChannel.Send(message, remoteIdentifiedEndPoint.Value); break;
                }
            }

            public void Disconnect() {
                reliableChannel.CloseChannel();
                if (remoteIdentifiedEndPoint.HasValue) {
                    unreliableChannel.CloseChannel(remoteIdentifiedEndPoint.Value);
                }
            }

            #endregion

            #region IEquatable

            bool IEquatable<IPlayer>.Equals(IPlayer other) => playerId == other.playerId;
            public override int GetHashCode() => playerId.GetHashCode();

            #endregion

            void IReliableChannelListener.ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container) {
                listener?.PlayerDidReceiveMessage(container, this);
            }

            void INetworkServerMessageListener.NetworkServerDidReceiveMessage(MessageContainer container) {
                listener?.PlayerDidReceiveMessage(container, this);
            }
        }
    }

    namespace Client {
        public interface IPlayer {
            int playerId { get; }
            float mostRecentPingValue { get; }

            bool isLocalPlayer { get; }
        }

        public class Player : IPlayer {
            internal double lastReceivedPingRequest;

            public int playerId { get; private set; }
            public bool isLocalPlayer { get; internal set; }
            public float mostRecentPingValue { get; internal set; }

            public Player() : base() => lastReceivedPingRequest = TimeUtils.CurrentTime();

            internal void Configure(int playerId, bool isLocalPlayer) {
                this.playerId = playerId;
                this.isLocalPlayer = isLocalPlayer;
            }
        }
    }
}
