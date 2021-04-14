using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Networking.Commons
{
    public interface IMessageAckHelperListener<TIngoingMessage>
        where TIngoingMessage : struct, ITypedMessage
    {
        void MessageAckHelperFailed();
        void MessageAckHelperReceivedExpectedResponse(NetEndPoint from, TIngoingMessage message);
    }

    public class MessageAckHelper<TOutgoingMessage, TIngoingMessage>
        where TIngoingMessage : struct, ITypedMessage
        where TOutgoingMessage : struct, ITypedMessage
    {
        private readonly double interval;
        private readonly TIngoingMessage referenceMessage;
        private readonly IClientMessageRouter rerouter;
        private readonly int retryCount;
        private readonly UnreliableChannel sender;

        private TOutgoingMessage message;
        private int retryIndex;

        private bool started;
        private double startedTime;
        private NetEndPoint to;

        public MessageAckHelper(UnreliableChannel sender, IClientMessageRouter rerouter, int maxRetryCount = 3, double intervalBetweenRetries = 1.0)
        {
            this.sender = sender;
            this.rerouter = rerouter;
            retryCount = maxRetryCount;
            interval = intervalBetweenRetries;
            referenceMessage = new TIngoingMessage();
        }

        public IMessageAckHelperListener<TIngoingMessage> listener { get; set; }

        public void Start(TOutgoingMessage message, NetEndPoint to)
        {
            this.message = message;
            this.to = to;

            retryIndex = 0;
            startedTime = TimeUtils.CurrentTime();
            Send();
            started = true;
        }

        public void Update()
        {
            if (started && TimeUtils.IsOverdue(startedTime, interval))
            {
                if (retryIndex >= retryCount)
                    listener?.MessageAckHelperFailed();
                else
                    Send();
            }
        }

        public void Route(NetEndPoint from, MessageContainer container)
        {
            if (container.Is(referenceMessage.type))
            {
                started = false;
                listener?.MessageAckHelperReceivedExpectedResponse(from, container.Parse<TIngoingMessage>());
            }
            else
            {
                rerouter.Route(container);
            }
        }

        private void Send()
        {
            sender.Send(message, to);
            startedTime = TimeUtils.CurrentTime();
            retryIndex++;
        }
    }
}