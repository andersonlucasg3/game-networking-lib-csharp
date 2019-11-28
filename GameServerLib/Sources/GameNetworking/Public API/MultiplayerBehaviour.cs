using UnityEngine;
using System;
using System.Collections.Generic;
using Messages.Models;
using GameNetworking;
using GameNetworking.Messages.Client;
using GameNetworking.Models.Client;
using Networking;
using Networking.IO;
using Logging;

[Serializable]
public enum MultiplayerBehaviourType {
    SERVER,
    CLIENT
}

public class MultiplayerBehaviour : MonoBehaviour, IGameServerListener, IGameClientListener, IGameClientInstanceListener {
    protected GameServer server;
    protected GameClient client;

    [SerializeField]
    protected GameObject[] spawnableObjects = new GameObject[0];
    [SerializeField]
    protected MultiplayerBehaviourType behaviourType = MultiplayerBehaviourType.SERVER;
    [SerializeField]
    protected string connectToHost = "127.0.0.1";
    [SerializeField]
    protected int port = 30000;
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

    protected virtual void Stop() {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.SERVER: this.StopServer(); break;
        case MultiplayerBehaviourType.CLIENT: this.StopClient(); break;
        }
    }

    protected virtual void StartServer() {
        this.server = new GameServer(new NetSocket(new TCPNonBlockingSocket())) { listener = this };
        this.server.syncController.SyncInterval = this.syncIntervalMs / 1000.0F;
        this.server.Listen(this.port);
    }

    protected virtual void StopServer() {
        this.server.Stop();
    }

    protected virtual void StartClient() {
        this.client = new GameClient(new NetSocket(new TCPNonBlockingSocket())) { listener = this, instanceListener = this };
        this.client.Connect(this.connectToHost, this.port);
    }

    protected virtual void StopClient() {
        this.client.Disconnect();
    }

    private void UpdateServer() {
        this.server?.Update();
    }

    private void UpdateClient() {
        this.client?.Update();
    }

    public void RequestSpawn(int spawnId) {
        Logger.Log($"RequestSpawn | spawnId: {spawnId}");
        this.client?.Send(new SpawnRequestMessage { spawnObjectId = spawnId });
    }

    public void Send(ITypedMessage encodable, GameNetworking.Models.Server.NetworkPlayer player = null) {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.CLIENT: this.client?.Send(encodable); break;
        case MultiplayerBehaviourType.SERVER: this.server?.Send(encodable, player.client); break;
        }
    }

    public void Broadcast(ITypedMessage encodable) {
        this.server?.SendBroadcast(encodable);
    }

    public void Broadcast(ITypedMessage encodable, GameNetworking.Models.Server.NetworkPlayer excludePlayer) {
        this.server.SendBroadcast(encodable, excludePlayer);
    }

    public GameNetworking.Models.Server.NetworkPlayer FindPlayer(int playerId) {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.CLIENT: return this.client.FindPlayer(playerId);
        case MultiplayerBehaviourType.SERVER: return this.server.FindPlayer(playerId);
        }
        return null;
    }

    public List<GameNetworking.Models.Server.NetworkPlayer> AllPlayers() {
        switch (this.behaviourType) {
        case MultiplayerBehaviourType.CLIENT: return this.client.AllPlayers();
        case MultiplayerBehaviourType.SERVER: return this.server.AllPlayers();
        }
        return null;
    }

    #region IGameInstance

    public virtual bool GameInstanceSyncPlayer(GameNetworking.Models.Client.NetworkPlayer player, Vector3 position, Vector3 eulerAngles) {
        return false;
    }

    #endregion

    #region IGameServerDelegate

    public virtual void GameServerPlayerDidDisconnect(GameNetworking.Models.Server.NetworkPlayer player) {

    }

    public virtual GameObject GameServerSpawnCharacter(GameNetworking.Models.Server.NetworkPlayer player) {
        return null;
    }

    public virtual void GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.Server.NetworkPlayer player) {

    }

    #endregion

    #region IGameClientDelegate

    public virtual void GameClientDidConnect() {

    }

    public virtual void GameClientConnectDidTimeout() {

    }

    public virtual void GameClientDidDisconnect() {

    }

    public virtual GameObject GameClientSpawnCharacter(NetworkPlayer player) {
        return null;
    }

    public virtual void GameClientDidReceiveMessage(MessageContainer container) {

    }

    public virtual void GameClientNetworkPlayerDidDisconnect(NetworkPlayer player) {

    }

    #endregion
}
