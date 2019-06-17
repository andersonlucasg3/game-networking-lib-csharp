using System;
using System.Net;
using System.Net.Sockets;

public class ClientConnection {
    private const int BUFFER_SIZE = 1024;

    private ServerNetworking serverNetworking;
    private Socket clientSocket;

    private byte[] buffer = new byte[BUFFER_SIZE];

    public bool isClosed = false;


    public ClientConnection(Socket clientSocket, ServerNetworking serverNetworking) {
        this.serverNetworking = serverNetworking;
        this.clientSocket = clientSocket;   
        StartConnection();
    }

    public void SendData(byte[] data) {
        clientSocket.Send(data);
    }

    public void CloseConnection() {
        isClosed = true;
        
        try {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        } finally {            
            serverNetworking.RemoveConnection(this);
        }
    }

    private void StartConnection() {
        clientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveDataCallback, this);
    }

    private void ClientSocketReceivedResult(IAsyncResult result) {
        try {
            int bytesLenghtReceived = clientSocket.EndReceive(result);
            byte[] bytesReceived = new byte[bytesLenghtReceived];
            Array.Copy(buffer, bytesReceived, bytesLenghtReceived);            
        } catch(Exception exception) {
            Logger.Log("ClientSocketReceivedResult", "Exception: " + exception.Message);
            CloseConnection();
        }
    }

    private static void ReceiveDataCallback(IAsyncResult result) {
        ClientConnection conn = (ClientConnection) result.AsyncState;
        conn.ClientSocketReceivedResult(result);
    }
}