using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class ServerNetworking {
    private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private List<ClientServerConnection> connections = new List<ClientServerConnection>();

    public void Start(int port) {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
        StartListening(endpoint);
    }

    public void Start(string host, int port) {
        IPHostEntry hostEntry = Dns.GetHostEntry(host);
        IPEndPoint endpoint = new IPEndPoint(hostEntry.AddressList[0], port);
        StartListening(endpoint);
    }

    private void StartListening(IPEndPoint endpoint) {
        serverSocket.Bind(endpoint);
        serverSocket.Listen(0);
        serverSocket.BeginAccept(ServerSocketAcceptCallback, this);
    }

    private static void ServerSocketAcceptCallback(IAsyncResult result) {
        try {
            ServerNetworking networking = (ServerNetworking) result.AsyncState;
            Socket clientSocket = networking.serverSocket.EndAccept(result);
            ClientServerConnection conn = new ClientServerConnection(networking.serverSocket, clientSocket);
            networking.connections.Add(conn);
        } catch(Exception exception) {
            Logger.Log("ServerSocketAcceptCallback", "Exception: " + exception.Message);
        }
    }   
}
