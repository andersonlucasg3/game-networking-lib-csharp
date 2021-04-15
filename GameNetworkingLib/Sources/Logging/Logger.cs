using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Logging
{
    public static class Logger
    {
        public static readonly List<Action<string>> externalLoggers = new List<Action<string>>();
        public static bool IsLoggingEnabled { get; set; } = true;

        public static void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!IsLoggingEnabled) return;

            string fileName;
            
            int index = filePath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                int realIndex = index + 1;
                fileName = filePath.Substring(realIndex, filePath.Length - realIndex - 3); // 3 for .cs
            }
            else
            {
                index = filePath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                int realIndex = index + 1;
                fileName = filePath.Substring(realIndex, filePath.Length - realIndex - 3);
            }

            string messageString = $"[{fileName}.{memberName}() : {lineNumber}] {message}";
            Console.WriteLine(messageString);

            externalLoggers.ForEach(each => each.Invoke($"[GameNetworking] {message}"));
        }
    }
}
