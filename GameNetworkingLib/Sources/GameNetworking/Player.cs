using System;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Sockets;

namespace GameNetworking {
    namespace Server {
        public interface IPlayer : IEquatable<IPlayer> {
            int playerId { get; }
            float mostRecentPingValue { get; }

            void Send(ITypedMessage message, Channel channel);
            TChannel GetChannel<TChannel>(Channel channel) where TChannel : class, IChannel;

            void Disconnect();
        }

        internal interface IPlayerMessageListener {
            void PlayerDidReceiveMessage(MessageContainer container, IPlayer from);
        }

        public class Player : IPlayer, IChannelListener {
            private ReliableChannel reliableChannel;
            private UnreliableChannel unreliableChannel;

            internal double lastReceivedPongRequest;

            internal IPlayerMessageListener listener { get; set; }

            public int playerId { get; internal set; }
            public float mostRecentPingValue { get; internal set; }

            public Player() { this.lastReceivedPongRequest = TimeUtils.CurrentTime(); }

            #region Internal methods

            internal void Configure(int playerId) => this.playerId = playerId;
            internal void Configure(ReliableChannel reliable, UnreliableChannel unreliable) {
                this.reliableChannel = reliable;
                this.unreliableChannel = unreliable;

                this.reliableChannel.listener = this;
                this.unreliableChannel.listener = this;
            }

            #endregion

            #region Public methods

            public void Send(ITypedMessage message, Channel channel) {
                this.GetChannel<IChannel>(channel).Send(message);
            }

            public TChannel GetChannel<TChannel>(Channel channel) where TChannel : class, IChannel {
                switch (channel) {
                case Channel.reliable: return this.reliableChannel as TChannel;
                case Channel.unreliable: return this.unreliableChannel as TChannel;
                default: return null;
                }
            }

            public void Disconnect() {
                this.reliableChannel.CloseChannel();
            }

            #endregion

            #region IEquatable

            bool IEquatable<IPlayer>.Equals(IPlayer other) => this.playerId == other.playerId;
            public override int GetHashCode() => this.playerId.GetHashCode();

            #endregion

            void IChannelListener.ChannelDidReceiveMessage(MessageContainer container, NetEndPoint from) {
                this.listener?.PlayerDidReceiveMessage(container, this);
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

            public Player() : base() => this.lastReceivedPingRequest = TimeUtils.CurrentTime();

            internal void Configure(int playerId, bool isLocalPlayer) {
                this.playerId = playerId;
                this.isLocalPlayer = isLocalPlayer;
            }
        }
    }
}
