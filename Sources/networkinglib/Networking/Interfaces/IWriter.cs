public interface IWriter {
    void Write(byte[] data);
    void Flush();
}