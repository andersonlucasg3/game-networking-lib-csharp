using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class Connection {
    private const int BUFFER_SIZE = 1024;

    private INetworking networking;
    private Socket socket;

    private byte[] buffer = new byte[BUFFER_SIZE];

    private List<IDataReceiverListener> listeners = new List<IDataReceiverListener>();

    public Connection(Socket socket, INetworking serverNetworking) {
        this.networking = serverNetworking;
        this.socket = socket;
        StartConnection();
    }

    public void AddDataReceiverListener(IDataReceiverListener listener) {
        if(!listeners.Contains(listener)) {
            listeners.Add(listener);
        }
    }

    public void RemoveDataReceiverListener(IDataReceiverListener listener) {
        listeners.Remove(listener);
    }

    public bool isConnected() {
        return socket.Connected;
    }

    public void SendData(byte[] data) {
        if(isConnected()) {
            socket.Send(data);
        }
    }

    private void StartConnection() {
        socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveDataCallback, this);
    }

    private void ClientSocketReceivedResult(IAsyncResult result) {
        try {
            int bytesLenghtReceived = socket.EndReceive(result);
            byte[] bytesReceived = new byte[bytesLenghtReceived];
            Array.Copy(buffer, bytesReceived, bytesLenghtReceived);
            onReceivedData(bytesReceived);
        } catch(Exception exception) {
            Logger.Log("ClientSocketReceivedResult", "Exception: " + exception.Message);
            CloseConnection();
        }
    }

    private void onReceivedData(byte[] data) {
        foreach (IDataReceiverListener listener in listeners) {
            listener.onReceivedDataFromConnection(this, data);
        }
    }
    
    public void CloseConnection() {
        try {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        } finally {            
            networking.RemoveConnection(this);
        }
    }

    private static void ReceiveDataCallback(IAsyncResult result) {
        Connection conn = (Connection) result.AsyncState;
        conn.ClientSocketReceivedResult(result);
    }
}