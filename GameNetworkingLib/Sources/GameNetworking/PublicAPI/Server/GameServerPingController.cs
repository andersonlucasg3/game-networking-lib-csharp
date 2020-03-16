using System;
using System.Collections.Generic;

namespace GameNetworking {
    using Models;
    using Models.Contract.Server;
    using Models.Server;
    using Messages.Server;
    using Commons;

    public class GameServerPingController<PlayerType> : BaseWorker<GameServer<PlayerType>>, NetworkPlayersStorage<PlayerType>.IListener where PlayerType : NetworkPlayer, new() {
        private readonly Dictionary<int, PingPlayer<PlayerType>> pingPlayers = new Dictionary<int, PingPlayer<PlayerType>>();
        private PingPlayer<PlayerType>[] pingPlayersArray;

        public float PingInterval { get; set; }

        public GameServerPingController(GameServer<PlayerType> instance, NetworkPlayersStorage<PlayerType> storage, IMainThreadDispatcher dispatcher) : base(instance, dispatcher) {
            storage.listeners.Add(this);
        }

        public float GetPingValue(PlayerType player) {
            return this.pingPlayers[player.playerId].PingValue;
        }

        internal void Update() {
            if (this.pingPlayersArray == null) { return; }
            PingPlayer<PlayerType> pingPlayer;
            for (int i = 0; i < this.pingPlayersArray.Length; i++) {
                pingPlayer = this.pingPlayersArray[i];
                if (!pingPlayer.PingSent && pingPlayer.CanSendNextPing) {
                    pingPlayer.SendingPing();
                    this.instance.Send(new PingRequestMessage(), pingPlayer.player);
                }
            }
        }

        public float PongReceived(PlayerType player) {
            var pingPlayer = this.pingPlayers[player.playerId];
            var pingValue = pingPlayer?.ReceivedPong() ?? 0F;
            player.mostRecentPingValue = pingValue;
            return pingValue;
        }

        void NetworkPlayersStorage<PlayerType>.IListener.PlayerStorageDidAdd(PlayerType player) {
            this.pingPlayers[player.playerId] = new PingPlayer<PlayerType>(player);
            this.UpdateArray();
        }

        void NetworkPlayersStorage<PlayerType>.IListener.PlayerStorageDidRemove(PlayerType player) {
            if (this.pingPlayers.ContainsKey(player.playerId)) {
                this.pingPlayers.Remove(player.playerId);
                this.UpdateArray();
            }
        }

        private void UpdateArray() {
            this.pingPlayersArray = new List<PingPlayer<PlayerType>>(this.pingPlayers.Values).ToArray();
        }
    }

    internal class PingPlayer<PlayerType> : WeakReference where PlayerType : NetworkPlayer, new() {
        private WeakReference pingController;

        private float pingSentTime;

        private float PingElapsedTime { get { return CurrentTime() - this.pingSentTime; } }

        internal bool PingSent { get; private set; }
        internal bool CanSendNextPing { get { return this.PingElapsedTime > (PingController?.PingInterval ?? 0.5F); } }
        internal float PingValue { get; private set; }

        internal PlayerType player { get { return (PlayerType)this.Target; } }

        internal GameServerPingController<PlayerType> PingController {
            get { return pingController?.Target as GameServerPingController<PlayerType>; }
            set {
                if (value == null) { pingController = null; return; }
                pingController = new WeakReference(value);
            }
        }

        internal PingPlayer(PlayerType instance) : base(instance) { }

        internal void SendingPing() {
            this.PingSent = true;
            this.pingSentTime = CurrentTime();
        }

        internal float ReceivedPong() {
            this.PingSent = false;
            this.PingValue = this.PingElapsedTime;
            return this.PingValue;
        }

        public override bool Equals(object obj) {
            if (obj is PlayerType player) {
                return this.player.Equals(player);
            }
            return Equals(this, obj);
        }

        private float CurrentTime() {
            return (float)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
