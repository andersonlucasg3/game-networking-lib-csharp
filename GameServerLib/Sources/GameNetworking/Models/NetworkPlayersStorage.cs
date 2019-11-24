using System;
using System.Collections.Generic;
using Commons;

namespace GameNetworking.Models {
    using Server;

    public interface INetworkPlayerStorageChangeDelegate {
        void PlayerStorageDidAdd(NetworkPlayer player);
        void PlayerStorageDidRemove(NetworkPlayer player);
    }

    public class NetworkPlayersStorage: WeakDelegates<INetworkPlayerStorageChangeDelegate> {
        public List<NetworkPlayer> Players { get; }

        public NetworkPlayersStorage() {
            this.Players = new List<NetworkPlayer>();
        }

        public void Add(NetworkPlayer player) {
            this.Players.Add(player);
            this.ForEach((INetworkPlayerStorageChangeDelegate del) => {
                del.PlayerStorageDidAdd(player);
            });
        }

        public void Remove(NetworkPlayer player) {
            this.Players.Remove(player);
            this.ForEach((INetworkPlayerStorageChangeDelegate del) => {
                del.PlayerStorageDidRemove(player);
            });
        }

        public void ForEach(Action<NetworkPlayer> action) {
            this.Players.ForEach(action);
        }

        public void ForEachConverted<TOutput>(Converter<NetworkPlayer, TOutput> converter, Action<TOutput> action) {
            this.Players.ForEach(player => {
                action.Invoke(converter.Invoke(player));
            });
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<NetworkPlayer, TOutput> converter) {
            return this.Players.ConvertAll(converter);
        }

        public NetworkPlayer Find(Predicate<NetworkPlayer> predicate) {
            return this.Players.Find(predicate);
        }

        public List<NetworkPlayer> FindAll(Predicate<NetworkPlayer> predicate) {
            return this.Players.FindAll(predicate);
        }

        public List<TOutput> ConvertFindingAll<TOutput>(Predicate<NetworkPlayer> predicate, Converter<NetworkPlayer, TOutput> converter) {
            List<TOutput> output = new List<TOutput>();
            this.Players.ForEach(player => { if (predicate.Invoke(player)) { output.Add(converter.Invoke(player)); } });
            return output;
        }
    }
}