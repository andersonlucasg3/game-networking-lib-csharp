using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class ServerNetworking: INetworking {
    private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private List<Connection> connections = new List<Connection>();

    public void Start(int port) {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
        StartConnecting(endpoint);
    }

    public void Start(string host, int port) {
        IPHostEntry hostEntry = Dns.GetHostEntry(host);
        IPEndPoint endpoint = new IPEndPoint(hostEntry.AddressList[0], port);
        StartConnecting(endpoint);
    }

    //Remove uma conexão fechada da lista de conexões (Se a conexão estiver aberta, ela é fechada antes)
    public void RemoveConnection(Connection conn) {
        if(!conn.isConnected()) {
            conn.CloseConnection();
        }
        connections.Remove(conn);
    }

    private void StartConnecting(IPEndPoint endpoint) {
        serverSocket.Bind(endpoint);
        serverSocket.Listen(0);
        serverSocket.BeginAccept(ServerSocketAcceptCallback, this);
    }

    private void ServerSocketAcceptedConnection(IAsyncResult result) {
        try {    
            Socket clientSocket = serverSocket.EndAccept(result);
            Connection conn = new Connection(clientSocket, this);
            connections.Add(conn);
        } catch(Exception exception) {
            Logger.Log("ServerSocketAcceptedConnection", "Exception: " + exception.Message);
        }
    }

    private static void ServerSocketAcceptCallback(IAsyncResult result) {
        ServerNetworking networking = (ServerNetworking) result.AsyncState;
        networking.ServerSocketAcceptedConnection(result);
    }   
}
