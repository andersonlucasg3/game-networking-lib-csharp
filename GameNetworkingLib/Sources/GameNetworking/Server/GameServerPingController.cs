using System.Collections.Generic;
using System.Linq;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Server
{
    public interface IGameServerPingController<TPlayer>
        where TPlayer : IPlayer
    {
        void Update();
        float PongReceived(TPlayer player, long pingRequestId);
    }

    public class GameServerPingController<TPlayer> : IGameServerPingController<TPlayer>, IPlayerCollectionListener<TPlayer>
        where TPlayer : Player
    {
        private readonly Dictionary<int, PingPlayer<TPlayer>> pingPlayers = new Dictionary<int, PingPlayer<TPlayer>>();
        private PingPlayer<TPlayer>[] pingPlayersArray;

        public GameServerPingController(PlayerCollection<int, TPlayer> storage)
        {
            storage.listeners.Add(this);
        }

        public float pingInterval { get; set; } = 1F;

        public void Update()
        {
            if (pingPlayersArray == null) return;
            for (var index = 0; index < pingPlayersArray.Length; index++) VerifyAndSendPing(pingPlayersArray[index]);
        }

        public float PongReceived(TPlayer from, long pingRequestId)
        {
            from.lastReceivedPongRequest = TimeUtils.CurrentTime();

            if (pingPlayers.TryGetValue(from.playerId, out var pingPlayer))
            {
                pingPlayer.ReceivedPong(pingRequestId);
                return from.mostRecentPingValue;
            }

            return 0F;
        }

        void IPlayerCollectionListener<TPlayer>.PlayerStorageDidAdd(TPlayer player)
        {
            pingPlayers[player.playerId] = new PingPlayer<TPlayer>(player) {pingController = this};
            UpdateArray();
        }

        void IPlayerCollectionListener<TPlayer>.PlayerStorageDidRemove(TPlayer player)
        {
            if (pingPlayers.ContainsKey(player.playerId))
            {
                pingPlayers.Remove(player.playerId);
                UpdateArray();
            }
        }

        private void VerifyAndSendPing(PingPlayer<TPlayer> pingPlayer)
        {
            pingPlayer.Checkup();
            if (pingPlayer.canSendNextPing) pingPlayer.SendingPing();
        }

        private void UpdateArray()
        {
            pingPlayersArray = new List<PingPlayer<TPlayer>>(pingPlayers.Values).ToArray();
        }
    }

    internal class PingPlayer<TPlayer>
        where TPlayer : Player
    {
        private long pingRequestId;
        private double pingSentTime;
        private readonly Queue<double> pingValues = new Queue<double>();

        internal PingPlayer(TPlayer instance)
        {
            player = instance;
        }

        private double pingElapsedTime => TimeUtils.CurrentTime() - pingSentTime;

        internal GameServerPingController<TPlayer> pingController { get; set; }
        internal bool pingSent { get; private set; }

        internal bool canSendNextPing
        {
            get
            {
                var isPlayerIdentified = player.remoteIdentifiedEndPoint.HasValue;
                return isPlayerIdentified && pingElapsedTime > (pingController?.pingInterval ?? 0.5F);
            }
        }

        internal TPlayer player { get; }

        internal void Checkup()
        {
            pingSent = canSendNextPing;
        }

        internal void SendingPing()
        {
            pingSent = true;
            pingSentTime = TimeUtils.CurrentTime();
            player.Send(new PingRequestMessage(pingRequestId++), Channel.unreliable);
        }

        internal void ReceivedPong(long pingRequestId)
        {
            if (pingRequestId < this.pingRequestId - 2) return;

            pingSent = false;

            if (pingValues.Count >= 10) pingValues.Dequeue();

            pingValues.Enqueue(pingElapsedTime);

            player.mostRecentPingValue = (float) (pingValues.Aggregate(0.0, (current, each) => current + each) / pingValues.Count);
        }

        public override bool Equals(object obj)
        {
            if (obj is TPlayer player) return this.player.Equals(player);
            return Equals(this, obj);
        }

        public override int GetHashCode()
        {
            return player.GetHashCode();
        }
    }
}