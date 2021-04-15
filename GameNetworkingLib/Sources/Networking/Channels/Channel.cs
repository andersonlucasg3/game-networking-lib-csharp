using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Channels
{
    public enum ChannelType
    {
        reliable,
        unreliable
    }
    
    public interface IChannelListener<in TDerived> where TDerived : Channel<TDerived>
    {
        void ChannelDidReceiveMessage(TDerived channel, MessageContainer container);
    }

    public abstract class Channel<TDerived> where TDerived : Channel<TDerived>
    {
        public IChannelListener<TDerived> listener { get; set; }
        
        protected void ReadAllMessages(MessageStreamReader reader, NetEndPoint fromEndPoint)
        {
            while (true)
            {
                using (MessageContainer container = reader.Decode()?.WithSender(fromEndPoint))
                {
                    if (container == null) return;
                    
                    listener?.ChannelDidReceiveMessage((TDerived) this, container);
                }
            }
        }
    }
}
