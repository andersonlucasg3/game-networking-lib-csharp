using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Logging {
    public static class Logger {
        public static bool IsLoggingEnabled { get; set; } = true;

        public static readonly List<Action<string>> externalLoggers = new List<Action<string>>();

        public static void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0) {
            if (!IsLoggingEnabled) { return; }

            var index = filePath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
            var fileName = filePath;
            if (index > 0) {
                var realIndex = index + 1;
                fileName = filePath.Substring(realIndex, filePath.Length - realIndex - 3); // 3 for .cs
            }

            Console.WriteLine($"[{fileName}.{memberName}() : {lineNumber}] {message}");

            externalLoggers.ForEach(each => each.Invoke($"[GameNetworking] {message}"));
        }
    }
}