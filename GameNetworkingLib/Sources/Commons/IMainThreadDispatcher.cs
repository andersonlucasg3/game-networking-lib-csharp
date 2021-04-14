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
        private static readonly Thread _mainThread = Thread.CurrentThread;
        private static readonly object _acceptLock = new object();
        private static readonly object _reliableLock = new object();
        private static readonly object _unreliableLock = new object();

        private static Thread acceptThread;
        private static Thread unreliableThread;
        private static Thread reliableThread;

        internal static void ConfigureAccept(Thread thread)
        {
#if !UNIT_TESTS
            if (thread != null && acceptThread != null) throw new InvalidOperationException("There is already a instance of ACCEPT thread alive");
#endif
            lock (_acceptLock)
            {
                acceptThread = thread;
            }
        }

        internal static void ConfigureReliable(Thread thread)
        {
#if !UNIT_TESTS
            if (thread != null && reliableThread != null) throw new InvalidOperationException("There is already a instance of RELIABLE thread alive");
#endif
            lock (_reliableLock)
            {
                reliableThread = thread;
            }
        }

        internal static void ConfigureUnreliable(Thread thread)
        {
#if !UNIT_TESTS
            if (thread != null && unreliableThread != null) throw new InvalidOperationException("There is already a instance of UNRELIABLE thread alive");
#endif
            lock (_unreliableLock)
            {
                unreliableThread = thread;
            }
        }

        public static void AssertMainThread(bool isMainThread = true)
        {
            AreEqual(_mainThread, Thread.CurrentThread, !isMainThread);
        }

        public static void AssertAcceptThread()
        {
            lock (_acceptLock)
            {
                AreEqual(acceptThread, Thread.CurrentThread);
            }
        }

        public static void AssertReliableChannel()
        {
            lock (_reliableLock)
            {
                AreEqual(reliableThread, Thread.CurrentThread);
            }
        }

        public static void AssertUnreliableChannel()
        {
            lock (_unreliableLock)
            {
                AreEqual(unreliableThread, Thread.CurrentThread);
            }
        }

        private static void AreEqual(object o1, object o2, bool inverted = false)
        {
            if (inverted && !o1.Equals(o2) || o1.Equals(o2)) return;
            
            Thread.CurrentThread.Abort();
        }
    }
}
