using System;

public static class Logger {
    public static void Log(Type context, string message) {
        Console.WriteLine(context + " - " + message);
    }
}