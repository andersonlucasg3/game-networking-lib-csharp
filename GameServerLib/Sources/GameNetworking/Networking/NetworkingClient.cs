using Networking;
using Networking.Models;
using Messages.Coders;
using Messages.Models;
using Messages.Streams;
using Commons;
using System.Threading;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingClient : WeakDelegate<INetworkingClientDelegate>, INetworkingDelegate {
        private INetworking networking;
        private NetworkClient client;

        private Thread proletariatTherad;

        public NetworkingClient() {
            this.networking = new NetSocket();
            this.networking.Delegate = this;
        }

        public void Connect(string host, int port) {
            this.networking.Connect(host, port);
        }

        public void Disconnect() {
            if (this.client?.Client != null) {
                this.networking.Disconnect(this.client.Client);
            }
        }

        public void Send(ITypedMessage message) {
            this.client?.Write(message);
        }

        #region Private Methods

        private void CreateAndStartThread() {
            proletariatTherad = new Thread(ThreadWork);
            proletariatTherad.Start();
        }

        private void AbortWork() {
            proletariatTherad?.Abort();
            proletariatTherad = null;
        }

        private void ThreadWork() {
            do {
                if (this.client?.Client != null) {
                    byte[] bytes = this.networking.Read(this.client.Client);
                    this.client.Reader.Add(bytes);
                    var message = this.client.Reader.Decode();
                    this.Delegate?.NetworkingClientDidReadMessage(message);

                    this.networking.Flush(this.client.Client);
                }
            } while (ShouldKeepWorking());
        }

        private bool ShouldKeepWorking() {
            var connected = this.client?.Client?.IsConnected ?? false;
            var isAborted = this.proletariatTherad?.ThreadState == ThreadState.Aborted || this.proletariatTherad.ThreadState == ThreadState.AbortRequested;
            return connected || !isAborted;
        }

        #endregion

        #region INetworkingDelegate

        void INetworkingDelegate.NetworkingDidConnect(NetClient client) {
            this.client = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.Delegate?.NetworkingClientDidConnect();

            this.CreateAndStartThread();
        }

        void INetworkingDelegate.NetworkingConnectDidTimeout() {
            this.client = null;
            this.Delegate?.NetworkingClientConnectDidTimeout();
        }

        void INetworkingDelegate.NetworkingDidDisconnect(NetClient client) {
            this.client = null;
            this.Delegate?.NetworkingClientDidDisconnect();

            this.AbortWork();
        }

        #endregion
    }
}
