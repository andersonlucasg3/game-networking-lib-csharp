using System;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;

namespace GameNetworking {
    namespace Server {
        public interface IPlayer : IEquatable<IPlayer> {
            int playerId { get; }
            float mostRecentPingValue { get; }
            double lastReceivedPongRequest { get; }

            void Configure(ReliableChannel reliable, UnreliableChannel unreliable);
            void Configure(int playerId);

            void Receive();
            void Receive(Channel channel);
            void Send(ITypedMessage message, Channel channel);
            void Flush();
            void Flush(Channel channel);

            void Disconnect();
        }

        public class Player : IPlayer {
            public ReliableChannel reliableChannel { get; protected set; }
            public UnreliableChannel unreliableChannel { get; protected set; }

            public int playerId { get; internal set; }
            public float mostRecentPingValue { get; internal set; }
            public double lastReceivedPongRequest { get; internal set; }

            public Player() { this.lastReceivedPongRequest = TimeUtils.CurrentTime(); }

            public void Configure(int playerId) => this.playerId = playerId;

            public void Configure(ReliableChannel reliable, UnreliableChannel unreliable) {
                this.reliableChannel = reliable;
                this.unreliableChannel = unreliable;
            }

            public void Receive() {
                this.reliableChannel.Receive();
                this.unreliableChannel.Receive();
            }

            public void Receive(Channel channel) {
                this.GetChannel(channel).Receive();
            }

            public void Send(ITypedMessage message, Channel channel) {
                this.GetChannel(channel).Send(message);
            }

            public void Flush() {
                this.reliableChannel.Flush();
                this.unreliableChannel.Flush();
            }

            public void Flush(Channel channel) {
                this.GetChannel(channel).Flush();
            }

            public void Disconnect() {
                this.reliableChannel.CloseChannel();
            }

            public bool Equals(IPlayer other) => this.playerId == other.playerId;
            public override int GetHashCode() => this.playerId.GetHashCode();

            private IChannel GetChannel(Channel channel) {
                switch (channel) {
                case Channel.reliable: return this.reliableChannel;
                case Channel.unreliable: return this.unreliableChannel;
                default: return null;
                }
            }
        }
    }

    namespace Client {
        public interface IPlayer {
            int playerId { get; }
            float mostRecentPingValue { get; }
            double lastReceivedPingRequest { get; }

            bool isLocalPlayer { get; }

            void Configure(int playerId, bool isLocalPlayer);
        }

        public class Player : IPlayer {
            public int playerId { get; private set; }
            public bool isLocalPlayer { get; internal set; }
            public float mostRecentPingValue { get; internal set; }
            public double lastReceivedPingRequest { get; internal set; }

            public Player() : base() => this.lastReceivedPingRequest = TimeUtils.CurrentTime();

            public void Configure(int playerId, bool isLocalPlayer) {
                this.playerId = playerId;
                this.isLocalPlayer = isLocalPlayer;
            }
        }
    }
}
