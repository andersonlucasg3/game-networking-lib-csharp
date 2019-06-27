using UnityEngine;
using System;
using Messages.Models;
using GameNetworking;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;

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
    protected string connectToHost = "127.0.0.1";

    [SerializeField]
    protected int port = 30000;

    protected virtual void Start() {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.SERVER: this.StartServer(); break;
        case MultiplayerBehaviourType.CLIENT: this.StartClient(); break;
        }
    }

    protected virtual void Update() {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.SERVER: this.UpdateServer(); break;
        case MultiplayerBehaviourType.CLIENT: this.UpdateClient(); break;
        }
    }

    protected void StartServer() {
        this.server = new GameServer { Delegate = this };
        this.server.Listen(this.port);
    }

    protected void StartClient() {
        this.client = new GameClient { Delegate = this };
        this.client.Connect(this.connectToHost, this.port);
    }

    private void UpdateServer() {
        this.server?.Update();
    }

    private void UpdateClient() {
        this.client?.Update();
    }

    public void RequestSpawn(int spawnId) {
        Logging.Logger.Log(this.GetType(), string.Format("RequestSpawn | spawnId: {0}", spawnId));
        this.client?.Send(new SpawnRequestMessage { spawnObjectId = spawnId });
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
