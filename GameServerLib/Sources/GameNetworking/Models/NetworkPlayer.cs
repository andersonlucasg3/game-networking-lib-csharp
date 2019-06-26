using UnityEngine;
using System;

namespace GameNetworking.Models {
    public class NetworkPlayer {
        private static readonly System.Random random = new System.Random();

        private WeakReference weakGameObject;

        public int PlayerId {
            get; private set;
        }

        public GameObject GameObject {
            get { return this.weakGameObject?.Target as GameObject; }
            internal set { this.weakGameObject = new WeakReference(value); }
        }

        public NetworkPlayer() {
            this.PlayerId = random.Next();
        }

        public override bool Equals(object obj) {
            if (obj is NetworkPlayer) {
                return this.PlayerId == ((NetworkPlayer)obj).PlayerId;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}