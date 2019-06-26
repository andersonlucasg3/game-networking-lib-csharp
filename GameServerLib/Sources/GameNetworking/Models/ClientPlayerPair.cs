namespace GameNetworking.Models {
    internal class ClientPlayerPair {
        public NetworkClient Client {
            get; private set;
        }

        public NetworkPlayer Player {
            get; private set;
        }

        internal ClientPlayerPair(NetworkClient client, NetworkPlayer player) {
            this.Client = client;
            this.Player = player;
        }

        public override bool Equals(object obj) {
            if (obj is ClientPlayerPair) {
                return this.Player == ((ClientPlayerPair)obj).Player;
            }
            return Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(ClientPlayerPair pair, NetworkClient client) {
            return pair.Client == client;
        }

        public static bool operator !=(ClientPlayerPair pair, NetworkClient client) {
            return pair.Client != client;
        }

        public static bool operator ==(ClientPlayerPair pair, NetworkPlayer player) {
            return pair.Player.PlayerId == player.PlayerId;
        }

        public static bool operator != (ClientPlayerPair pair, NetworkPlayer player) {
            return pair.Player.PlayerId != player.PlayerId;
        }
    }
}