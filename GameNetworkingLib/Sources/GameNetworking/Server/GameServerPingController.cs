using System.Collections.Generic;
using System.Linq;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Server
{
    public interface IGameServerPingController<in TPlayer>
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

            if (!pingPlayers.TryGetValue(@from.playerId, out var pingPlayer)) return 0F;
            
            pingPlayer.ReceivedPong(pingRequestId);
            return @from.mostRecentPingValue;

        }

        void IPlayerCollectionListener<TPlayer>.PlayerStorageDidAdd(TPlayer player)
        {
            pingPlayers[player.playerId] = new PingPlayer<TPlayer>(player) {pingController = this};
            UpdateArray();
        }

        void IPlayerCollectionListener<TPlayer>.PlayerStorageDidRemove(TPlayer player)
        {
            if (!pingPlayers.ContainsKey(player.playerId)) return;

            pingPlayers.Remove(player.playerId);
            UpdateArray();
        }

        private static void VerifyAndSendPing(PingPlayer<TPlayer> pingPlayer)
        {
            pingPlayer.Checkup();
            if (pingPlayer.canSendNextPing) pingPlayer.SendingPing();
        }

        private void UpdateArray()
        {
            using (PooledList<PingPlayer<TPlayer>> tempList = PooledList<PingPlayer<TPlayer>>.Rent(pingPlayers.Values))
            {
                pingPlayersArray = tempList.ToArray();
            }
        }
    }

    internal class PingPlayer<TPlayer> where TPlayer : Player
    {
        private readonly Queue<double> _pingValues = new Queue<double>();
        
        private long _pingRequestId;
        private double _pingSentTime;

        private double pingElapsedTime => TimeUtils.CurrentTime() - _pingSentTime;

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
        
        internal PingPlayer(TPlayer instance)
        {
            player = instance;
        }

        internal void Checkup()
        {
            pingSent = canSendNextPing;
        }

        internal void SendingPing()
        {
            pingSent = true;
            _pingSentTime = TimeUtils.CurrentTime();
            player.Send(new PingRequestMessage(_pingRequestId++), Channel.unreliable);
        }

        internal void ReceivedPong(long pingRequestId)
        {
            if (pingRequestId < _pingRequestId - 2) return;

            pingSent = false;

            if (_pingValues.Count >= 10) _pingValues.Dequeue();

            _pingValues.Enqueue(pingElapsedTime);

            player.mostRecentPingValue = (float) (_pingValues.Aggregate(0.0, (current, each) => current + each) / _pingValues.Count);
        }

        public override bool Equals(object other)
        {
            if (other is TPlayer otherPlayer) return player.Equals(otherPlayer);
            return Equals(this, other);
        }

        public override int GetHashCode()
        {
            return player.GetHashCode();
        }
    }
}
