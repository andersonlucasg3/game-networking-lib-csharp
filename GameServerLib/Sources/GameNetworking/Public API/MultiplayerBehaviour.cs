using UnityEngine;
using System;
using Messages.Models;
using GameNetworking;

[Serializable]
public enum MultiplayerBehaviourType {
    SERVER,
    CLIENT
}

public class MultiplayerBehaviour : MonoBehaviour, IGameServerDelegate, IGameClientDelegate {
    private GameServer server;
    private GameClient client;

    [SerializeField]
    protected GameObject[] spawnableObjects = new GameObject[0];

    [SerializeField]
    protected MultiplayerBehaviourType behaviourType = MultiplayerBehaviourType.SERVER;

    [SerializeField]
    protected string connectToHost = "localhost";

    [SerializeField]
    protected int port = 30000;

    protected virtual void Start() {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.SERVER: {
                this.server = new GameServer { Delegate = this };
                this.server.Listen(this.port);
            }
            break;
        case MultiplayerBehaviourType.CLIENT:
            this.client = new GameClient { Delegate = this };
            this.client.Connect(this.connectToHost, this.port);
            break;
        }
    }

    protected virtual void Update() {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.SERVER: this.UpdateServer(); break;
        case MultiplayerBehaviourType.CLIENT: this.UpdateClient(); break;
        }
    }

    private void UpdateServer() {
        this.server.Update();
    }

    private void UpdateClient() {
        this.client.Update();
    }

    #region IGameServerDelegate

    public virtual GameObject GameServerSpawnCharacter(int spawnId, GameNetworking.Models.NetworkPlayer player) {
        return null;
    }

    public virtual void GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.NetworkPlayer player) {

    }

    #endregion

    #region IGameClientDelegate

    public virtual void GameClientDidConnect() {

    }

    public virtual void GameClientConnectDidTimeout() {

    }

    public virtual void GameClientDidDisconnect() {

    }

    public virtual GameObject GameClientSpawnCharacter(int spawnId, GameNetworking.Models.NetworkPlayer player) {
        return null;
    }

    public virtual void GameClientDidReceiveMessage(MessageContainer container) {

    }

    #endregion
}
