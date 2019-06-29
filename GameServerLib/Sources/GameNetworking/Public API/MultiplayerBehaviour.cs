using UnityEngine;
using System;
using Messages.Models;
using GameNetworking;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Messages;

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

    [SerializeField]
    protected float moveStep = 1;

    [SerializeField]
    protected int syncIntervalMs = 200;

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
        this.server.movementController.SyncIntervalMs = this.syncIntervalMs / 1000.0F;
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

    public void Move(Vector3 direction) {
        MoveRequestMessage message = new MoveRequestMessage();
        direction.CopyToVec3(ref message.direction);
        this.client?.Send(message);
    }

    #region IGameServerDelegate

    public virtual GameObject GameServerSpawnCharacter(int spawnId, GameNetworking.Models.Server.NetworkPlayer player) {
        return null;
    }

    public virtual void GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.Server.NetworkPlayer player) {

    }

    public virtual void GameServerDidReceiveMoveRequest(Vector3 direction, GameNetworking.Models.Server.NetworkPlayer player, IMovementController movementController) {
        movementController.Move(player, direction, this.moveStep * Time.deltaTime);
    }

    #endregion

    #region IGameClientDelegate

    public virtual void GameClientDidConnect() {

    }

    public virtual void GameClientConnectDidTimeout() {

    }

    public virtual void GameClientDidDisconnect() {

    }

    public virtual GameObject GameClientSpawnCharacter(int spawnId, GameNetworking.Models.Client.NetworkPlayer player) {
        return null;
    }

    public virtual void GameClientDidReceiveMessage(MessageContainer container) {

    }

    #endregion
}
