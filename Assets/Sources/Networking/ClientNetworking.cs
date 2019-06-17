using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class ClientNetworking {
    private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private ServerConnection connection;

    public void Start(int port) {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
        StartConnecting(endpoint);
    }

    public void Start(string host, int port) {
        IPHostEntry hostEntry = Dns.GetHostEntry(host);
        IPEndPoint endpoint = new IPEndPoint(hostEntry.AddressList[0], port);
        StartConnecting(endpoint);
    }

    private void StartConnecting(IPEndPoint endpoint) {
        clientSocket.Connect(endpoint);        
    }

}