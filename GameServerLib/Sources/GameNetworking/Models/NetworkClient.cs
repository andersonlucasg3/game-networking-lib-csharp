using Networking.Models;
using Messages.Streams;

namespace GameNetworking.Models {
    public class NetworkClient {
        internal Client Client { get; private set; }
        internal IStreamReader Reader { get; private set; }
        internal IStreamWriter Writer { get; private set; }

        internal NetworkClient(Client client, IStreamReader reader, IStreamWriter writer) {
            this.Client = client;
            this.Reader = reader;
            this.Writer = writer;
        }
    }
}