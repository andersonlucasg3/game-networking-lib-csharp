public interface IDataReceiverListener {
    void onReceivedDataFromConnection(Connection connection, byte[] data);
}