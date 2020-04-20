using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;

namespace GameNetworking {
    public interface IUnreliableClientMessageSender {
        void Send(ITypedMessage message);
    }

    public class UnreliableClientConnectionController {
        private readonly IUnreliableClientMessageSender client;

        private readonly Action timeOutAction = null;

        private int retryCount = 0;
        private double startTime = -1F;

        private double elapsedTime => TimeUtils.CurrentTime() - this.startTime;

        public bool isConnecting { get; private set; } = false;
        public float secondsBetweenRetries { get; set; } = 3F;
        public int maximumNumberOfRetries { get; set; } = 3;

        public UnreliableClientConnectionController(IUnreliableClientMessageSender client, Action timeOutAction) {
            this.client = client;
            this.timeOutAction = timeOutAction;
        }

        public void Connect() {
            if (this.isConnecting) { return; }
            this.isConnecting = true;
            this.retryCount = 0;
            this.startTime = TimeUtils.CurrentTime();

            this.Send();
        }

        public void Stop() {
            this.isConnecting = false;
        }

        public void Update() {
            if (!this.isConnecting) { return; }

            if (this.elapsedTime >= this.secondsBetweenRetries) {
                if (this.retryCount >= this.maximumNumberOfRetries) {
                    this.Stop();
                    this.DispatchTimeOut();
                    return;
                }

                this.retryCount++;
                this.startTime = TimeUtils.CurrentTime();
                this.Send();
            }
        }

        #region Private Methods

        private void DispatchTimeOut() {
            this.timeOutAction?.Invoke();
        }

        private void Send() {
            var connect = new UnreliableConnectMessage();
            this.client.Send(connect);
            this.client.Send(connect);
        }

        #endregion
    }
}