using System;
using System.Runtime.CompilerServices;

namespace Logging {
    public static class Logger {
        public static bool IsLoggingEnabled = true;

        public static void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0) {
            if (!IsLoggingEnabled) { return; }

            var index = filePath.LastIndexOf("\\");
            var fileName = filePath;
            if (index > 0) {
                var realIndex = index + 1;
                fileName = filePath.Substring(realIndex, filePath.Length - realIndex - 3); // 3 for .cs
            }

            Console.WriteLine($"[{fileName}.{memberName}() : {lineNumber}] {message}");
        }
    }
}