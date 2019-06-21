using System;

namespace Networking.Models {
    using IO;

    public partial class Client {
        internal object raw;

        internal IReader reader;
        internal IWriter writer;

        public bool IsConnected { get { return this.Socket.IsConnected(); } }

        internal Client(object raw, IReader reader, IWriter writer) {
            this.raw = raw;
            this.reader = reader;
            this.writer = writer;
        }
    }
}