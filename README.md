# GameNetworking Lib

Hi, this library offer you some abstractions of TCP and UDP sockets to send and receive user defined message packets.

It works on top of these 2 protocols to maximize your options when developing a multiplayer game.

It's very ease to implement and see something working.

## Messages

Here I'll demonstrate how to define an message.

Messages are always [`ITypedMessage`](GameNetworkingLib/Sources/Networking/Messages/Models/ITypedMessage.cs) that is an inheritance of [`ICodable`](GameNetworkingLib/Sources/Networking/Messages/Interfaces/ICodable.cs). If you want to define an custom message (you might) you do it like this:
```csharp
enum MyMessageTypes: int {
    // Notice that the messages start from 1 thousand. The library uses the range 100..<1000 to its internal messages.
    message1 = 1000, 
    message2,
    message3
}

// Notice that in the message defined bellow we always encode in an defined order and decode in the SAME order.
// As message packets are only Array<byte> we need to write and read in the same order to ensure we a reading the same bytes that were written.
struct MyMessage: ITypedMessage {
    int ITypedMessage.type => 1000

    public Vector3 playerPosition { get; private set; }
    public Vector2 cameraRotation { get; private set; }

    // The encoder supports any struct that inherits from IEncodable
    void IEncodable.Encode(IEncoder encoder) {
        encoder.Encode(this.playerPosition);
        encoder.Encode(this.cameraRotation);
    }

    // The decoder supports any struct that inherits from IDecodable
    void IDecodable.Decode(IDecoder decoder) {
        this.playerPosition = this.GetObject<Vector3>();
        this.cameraRotation = this.GetObject<Vector2>();
    }
}

struct Vector3 : ICodable { 
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    void IEncodable.Encode(IEncoder encoder) {
        encoder.Encode(this.x);
        encoder.Encode(this.y);
        encoder.Encode(this.z);
    }

    void IDecodable.Decode(IDecoder decoder) {
        this.x = decoder.GetFloat();
        this.y = decoder.GetFloat();
        this.z = decoder.GetFloat();
    }
}

struct Vector2 : ICodable {
    public float x { get; set; }
    public float y { get; set; }

    void IEncodable.Encode(IEncoder encoder) {
        encoder.Encode(this.x);
        encoder.Encode(this.y);
    }

    void IDecodable.Decode(IDecoder decoder) {
        this.x = decoder.GetFloat();
        this.y = decoder.GetFloat();
    }
}
```

Well now we already have messages to send and receive.

To see how we implement a server you can look into the [`TestServer`](TestServerApp/Program.cs).

To see how we implement a client you can look into the [`TestClient`](TestClientApp/Program.cs).

## Connection

The library manages a client connection through a `ReliableChannel (TCP)` to allow communicating essential packets, the ones that cannot be missed. And also stablishes an `UnreliableChannel (UDP)` so you can send non essencial packets through it, such as player position updates, and sync messages.

The reliable layer is stablished as soon as you call `Connect`, but the unreliable takes some quick time to exchange EndPoint information with the server. When it does that, it ensures that clients bellow `NAT` will be reachable.

There are 2 main events to know when a client is ready to exchange messages, when you implement the [`IGameClientListener<TPlayer>`](GameNetworkingLib/Sources/GameNetworking/Client/GameClient.cs) interface.

When the client finishes connecting and is ready to send packets through a [`Channel`](GameNetworkingLib/Sources/Networking/Channels/Channel.cs) the core calls the method: `void GameClientDidConnect(Channel channel);`.
That `Channel` passed as parameter indicates which channel has stablished a connection.
If this event is not called with `Channel.unreliable` that means that there will be no UnreliableChannel available to exchange messages.

# Conclusion

That is a quick resume of how this works. 
(Writing of this document is still in progress)
