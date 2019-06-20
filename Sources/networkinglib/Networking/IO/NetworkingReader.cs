using System.Net.Sockets;
using System.Collections;

public sealed class NetworkingReader: IReader {
    private Socket socket;

    NetworkingReader(Socket socket) {
        this.socket = socket;
    }

    public byte[] Read() {
        byte[] buffer = new byte[4096];
        int count = this.socket.Receive(buffer);
        if (count < 4096) {
            byte[] shrinked = new byte[count];
            
        }
    }
}