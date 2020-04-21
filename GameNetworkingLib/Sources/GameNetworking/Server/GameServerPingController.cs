using System.Collections.Generic;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Server {
    public interface IGameServerPingController<TPlayer>
        where TPlayer : IPlayer {

        void Update();
        float PongReceived(TPlayer player);
    }

    public class GameServerPingController<TPlayer> : IGameServerPingController<TPlayer>, IPlayerCollectionListener<TPlayer>
        where TPlayer : class, IPlayer {
        private readonly Dictionary<int, PingPlayer<TPlayer>> pingPlayers = new Dictionary<int, PingPlayer<TPlayer>>();
        private PingPlayer<TPlayer>[] pingPlayersArray;

        public float pingInterval { get; set; } = 1F;

        public GameServerPingController(PlayerCollection<int, TPlayer> storage) {
            storage.listeners.Add(this);
        }

        public void Update() {
            if (this.pingPlayersArray == null) { return; }
            for (int index = 0; index < this.pingPlayersArray.Length; index++) {
                this.VerifyAndSendPing(this.pingPlayersArray[index]);
            }
        }

        private void VerifyAndSendPing(PingPlayer<TPlayer> pingPlayer) {
            pingPlayer.Checkup();
            if (pingPlayer.canSendNextPing) {
                pingPlayer.SendingPing();
                pingPlayer.player.Send(new PingRequestMessage(), Channel.unreliable);
            }
        }

        public float PongReceived(TPlayer from) {
            if (!(from is Player player)) { return 0F; }

            player.lastReceivedPongRequest = TimeUtils.CurrentTime();

            if (this.pingPlayers.TryGetValue(player.playerId, out PingPlayer<TPlayer> pingPlayer)) {
                var pingValue = pingPlayer.ReceivedPong();
                player.mostRecentPingValue = pingValue;
                return pingValue;
            }
            return 0;
        }

        void IPlayerCollectionListener<TPlayer>.PlayerStorageDidAdd(TPlayer player) {
            this.pingPlayers[player.playerId] = new PingPlayer<TPlayer>(player) { pingController = this };
            this.UpdateArray();
        }

        void IPlayerCollectionListener<TPlayer>.PlayerStorageDidRemove(TPlayer player) {
            if (this.pingPlayers.ContainsKey(player.playerId)) {
                this.pingPlayers.Remove(player.playerId);
                this.UpdateArray();
            }
        }

        private void UpdateArray() {
            this.pingPlayersArray = new List<PingPlayer<TPlayer>>(this.pingPlayers.Values).ToArray();
        }
    }

    internal class PingPlayer<TPlayer>
        where TPlayer : class, IPlayer {
        internal GameServerPingController<TPlayer> pingController { get; set; }

        private double pingSentTime;

        private double pingElapsedTime { get { return TimeUtils.CurrentTime() - this.pingSentTime; } }

        internal bool pingSent { get; private set; }
        internal bool canSendNextPing { get { return this.pingElapsedTime > (pingController?.pingInterval ?? 0.5F); } }

        internal TPlayer player { get; }

        internal PingPlayer(TPlayer instance) {
            this.player = instance;
        }

        internal void Checkup() => this.pingSent = this.canSendNextPing;

        internal void SendingPing() {
            this.pingSent = true;
            this.pingSentTime = TimeUtils.CurrentTime();
        }

        internal float ReceivedPong() {
            this.pingSent = false;
            if (!(this.player is Player player)) { return 0F; }
            return player.mostRecentPingValue = (float)this.pingElapsedTime;
        }

        public override bool Equals(object obj) {
            if (obj is TPlayer player) {
                return this.player.Equals(player);
            }
            return Equals(this, obj);
        }

        public override int GetHashCode() => player.GetHashCode();
    }
}
