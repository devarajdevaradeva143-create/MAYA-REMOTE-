using System;
using System.IO;
using MayaRemote.Core.Interfaces;

namespace MayaRemote.Infrastructure.Logging;

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

public interface ILogger
{
    void Log(LogLevel level, string message, Exception? exception = null);
    IDisposable BeginScope(string state);
}

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        var color = level switch
        {
            LogLevel.Error or LogLevel.Critical => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Information => ConsoleColor.White,
            _ => ConsoleColor.Gray
        };
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] [{level}] {message}");
        Console.ForegroundColor = previous;
        if (exception != null) Console.WriteLine(exception);
    }

    public IDisposable BeginScope(string state) => new EmptyScope();

    private sealed class EmptyScope : IDisposable
    {
        public void Dispose() { }
    }
}

public sealed class FileLogger : ILogger
{
    private readonly string _path;
    private readonly object _lock = new();

    public FileLogger(string logFilePath)
    {
        _path = logFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
    }

    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        var line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        if (exception != null) line += $" | {exception}";
        lock (_lock)
        {
            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }

    public IDisposable BeginScope(string state) => new EmptyScope();

    private sealed class EmptyScope : IDisposable
    {
        public void Dispose() { }
    }
}

public static class LogManager
{
    public static ILogger Logger { get; set; } = new ConsoleLogger();
}
