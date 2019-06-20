using System;

namespace Logging {
    public static class Logger {
        public static void Log(Type context, string message) {
            Console.WriteLine("[{0}] {1}", context, message);
        }
    }
}