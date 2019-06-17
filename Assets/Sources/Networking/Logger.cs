using System;

public static class Logger {
    public static void Log(string context, string message) {
        Console.WriteLine(context + " - " + message);
    }
}