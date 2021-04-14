using GameNetworking.Messages.Coders;

namespace GameNetworking.Messages.Models
{
    public interface ITypedMessage : ICodable
    {
        int type { get; }
    }
}