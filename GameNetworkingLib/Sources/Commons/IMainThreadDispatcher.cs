using System;
using System.Threading;

namespace GameNetworking.Commons
{
    public interface IMainThreadDispatcher
    {
        void Enqueue(Action action);
    }

    internal static class ThreadChecker
    {
        private static readonly Thread mainThread = Thread.CurrentThread;
        private static readonly object acceptLock = new object();
        private static readonly object reliableLock = new object();
        private static readonly object unreliableLock = new object();

        private static Thread acceptThread;
        private static Thread unreliableThread;
        private static Thread reliableThread;

        internal static void ConfigureAccept(Thread thread)
        {
            if (thread != null && acceptThread != null) throw new InvalidOperationException("There is already a instance of ACCEPT thread alive");
            lock (acceptLock)
            {
                acceptThread = thread;
            }
        }

        internal static void ConfigureReliable(Thread thread)
        {
            if (thread != null && reliableThread != null) throw new InvalidOperationException("There is already a instance of RELIABLE thread alive");
            lock (reliableLock)
            {
                reliableThread = thread;
            }
        }

        internal static void ConfigureUnreliable(Thread thread)
        {
            if (thread != null && unreliableThread != null) throw new InvalidOperationException("There is already a instance of UNRELIABLE thread alive");
            lock (unreliableLock)
            {
                unreliableThread = thread;
            }
        }

        public static void AssertMainThread(bool isMainThread = true)
        {
            AreEqual(mainThread, Thread.CurrentThread, !isMainThread);
        }

        public static void AssertAcceptThread()
        {
            lock (acceptLock)
            {
                AreEqual(acceptThread, Thread.CurrentThread);
            }
        }

        public static void AssertReliableChannel()
        {
            lock (reliableLock)
            {
                AreEqual(reliableThread, Thread.CurrentThread);
            }
        }

        public static void AssertUnreliableChannel()
        {
            lock (unreliableLock)
            {
                AreEqual(unreliableThread, Thread.CurrentThread);
            }
        }

        private static void AreEqual(object o1, object o2, bool inverted = false)
        {
            if (o1 == null || o2 == null)
                if (!o1.Equals(o2) || inverted)
                    Thread.CurrentThread.Abort();
        }
    }
}