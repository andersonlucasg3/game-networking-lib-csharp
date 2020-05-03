using System.Collections.Generic;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;
using System.Linq;

namespace GameNetworking.Server {
    public interface IGameServerPingController<TPlayer>
        where TPlayer : IPlayer {

        void Update();
        float PongReceived(TPlayer player, long pingRequestId);
    }

    public class GameServerPingController<TPlayer> : IGameServerPingController<TPlayer>, IPlayerCollectionListener<TPlayer>
        where TPlayer : Player {
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
            }
        }

        public float PongReceived(TPlayer from, long pingRequestId) {
            from.lastReceivedPongRequest = TimeUtils.CurrentTime();

            if (this.pingPlayers.TryGetValue(from.playerId, out PingPlayer<TPlayer> pingPlayer)) {
                pingPlayer.ReceivedPong(pingRequestId);
                return from.mostRecentPingValue;
            }
            return 0F;
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
        where TPlayer : Player {
        private Queue<double> pingValues = new Queue<double>();
        private double pingSentTime;
        private long pingRequestId = 0;
        private double pingElapsedTime { get { return TimeUtils.CurrentTime() - this.pingSentTime; } }

        internal GameServerPingController<TPlayer> pingController { get; set; }
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
            this.player.Send(new PingRequestMessage(this.pingRequestId++), Channel.unreliable);
        }

        internal void ReceivedPong(long pingRequestId) {
            if (pingRequestId < this.pingRequestId - 2) { return; } // allows one packet loss

            this.pingSent = false;

            if (this.pingValues.Count >= 10) {
                this.pingValues.Dequeue();
            }

            this.pingValues.Enqueue(this.pingElapsedTime);

            this.player.mostRecentPingValue = (float)(this.pingValues.Aggregate(0.0, (current, each) => current + each) / this.pingValues.Count);
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
