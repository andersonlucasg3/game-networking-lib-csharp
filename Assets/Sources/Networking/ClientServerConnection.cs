using System;
using System.Net;
using System.Net.Sockets;

public class ClientServerConnection {
    private Socket serverSocket;
    private Socket clientSocket;

    public ClientServerConnection(Socket serverSocket, Socket clientSocket) {
        this.serverSocket = serverSocket;
        this.clientSocket = clientSocket;   
    }

    private void StartConnection() {
        
    }
}