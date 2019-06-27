using System;

namespace Logging {
    public static class Logger {
        public static bool IsLoggingEnabled = true;

        public static void Log(Type context, string message) {
            if (!IsLoggingEnabled) { return; }

            Console.WriteLine("[{0}] {1}", context, message);
            UnityEngine.Debug.Log(string.Format("[{0}] {1}", context, message));
        }
    }
}