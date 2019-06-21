using System;
using MatchMaking;
using MatchMaking.Connection;
using MatchMaking.Models;
using MatchMaking.Protobuf;
using Logging;
using System.Threading;

namespace MatchMakingClientTestApp {
    public class GameClient : Client {
    }

    class TestMatchMaking: IMatchMakingClientDelegate<GameClient> {
        readonly MatchMakingClient<GameClient> client;

        public bool IsConnected {
            get { return this.client.IsConnected; }
        }

        public bool IsConnecting {
            get;
            private set;
        }

        public TestMatchMaking() {
            this.client = new MatchMakingClient<GameClient>();
            this.client.Delegate = this;
        }

        public void Connect() {
            this.IsConnecting = true;
            this.client.Start("0.0.0.0", 6289);
        }

        public void PeakMessage() {
            this.client.Read();
        }

        #region IMatchMakingClientDelegate

        public void MatchMakingClientDidConnect(MatchMakingClient<GameClient> matchMaking) {
            this.IsConnecting = false;
            matchMaking.Login(
                "5B92DB791D914642A0DE624D6A9132BA36E3C6A6178846D8B0656619CCF9E55BAA7BFD98C19E45A1B5169B15E5E346F7",
                "anderson"
            );

            matchMaking.RequestMatch();
        }

        public void MatchMakingClientDidRequestConnectToGameServer(MatchMakingClient<GameClient> matchMaking, ConnectGameInstanceResponse message) {
            Logger.Log(typeof(TestMatchMaking), "Just received connect to game instance " + message);

            matchMaking.Ready();
        }

        #endregion
    }

    class Program {
        static void Main(string[] args) {
            var test = new TestMatchMaking();

            test.Connect();

            while (test.IsConnected || test.IsConnecting) {
                test.PeakMessage();
            }

            Log("Terminating application...");
        }

        public static void Log(string message) {
            Logger.Log(typeof(Program), message);
        }
    }
}
