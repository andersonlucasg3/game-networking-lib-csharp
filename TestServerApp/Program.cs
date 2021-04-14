#if !UNITY_64

using System;
using System.Collections.Generic;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Sockets;
using GameNetworking.Server;
using Logging;

namespace TestServerApp
{
    internal class Program : IMainThreadDispatcher, IGameServerListener<Player>
    {
        private readonly List<Action> actions = new List<Action>();

        private GameServer<Player> server;

        public void GameServerPlayerDidConnect(Player player, Channel channel)
        {
            Logger.Log($"GameServerPlayerDidConnect - {channel}");
        }

        public void GameServerPlayerDidDisconnect(Player player)
        {
            Logger.Log("GameServerPlayerDidDisconnect");
        }

        public void GameServerDidReceiveClientMessage(MessageContainer container, Player player)
        {
            Logger.Log($"GameServerDidReceiveClientMessage - type {container.type}");
            if (container.type == 1001) Send(container);
        }

        public void Enqueue(Action action)
        {
            lock (this)
            {
                actions.Add(action);
            }
        }

        private static void Main(string[] _)
        {
            var program = new Program();
            program.server = new GameServer<Player>(new NetworkServer(new TcpSocket(), new UdpSocket()), new GameServerMessageRouter<Player>(program));

            program.server.Start(64000);

            program.server.listener = program;

            while (true)
            {
                lock (program)
                {
                    program.actions.ForEach(a => a.Invoke());
                    program.actions.RemoveAll(m => true);
                }

                program.server.Update();
            }
        }

        private void Send(MessageContainer message)
        {
            Enqueue(new Executor<Executor, GameServer<Player>, Message>(server, message).Execute);
        }
    }

    internal struct Message : ITypedMessage
    {
        public int type => 1001;

        public int playerId;
        public int messageId;

        public Message(int playerId, int messageId)
        {
            this.playerId = playerId;
            this.messageId = messageId;
        }

        public void Decode(IDecoder decoder)
        {
            playerId = decoder.GetInt();
            messageId = decoder.GetInt();
        }

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(playerId);
            encoder.Encode(messageId);
        }
    }

    internal struct Executor : IExecutor<GameServer<Player>, Message>
    {
        void IExecutor<GameServer<Player>, Message>.Execute(GameServer<Player> model, Message message)
        {
            var player = model.playerCollection.FindPlayer(message.playerId);
            Logger.Log($"Received message from playerId-{message.playerId}, as playerId-{player.playerId}, messageId-{message.messageId}");
            model.SendBroadcast(message, Channel.reliable);
            model.SendBroadcast(message, Channel.unreliable);
        }
    }
}

#endif
