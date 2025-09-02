using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace ZuneModCore.Services;

public class FileLoggerProvider(string logDirectory) : ILoggerProvider
{
    private StreamWriter? _writer;

    public ILogger CreateLogger(string categoryName)
    {
        _writer ??= CreateLogFile();

        return new FileLogger(_writer, categoryName);
    }

    private StreamWriter CreateLogFile()
    {
        var logFilePath = Path.Combine(logDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}.log");
        var logFile = File.Open(logFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

        return new(logFile) { AutoFlush = true };
    }

    public void Dispose() => _writer?.Dispose();
}

/// <summary>
/// Initializes a new instance of the <see cref="FileLogger"/> class.
/// </summary>
/// <param name="name">The name of the logger.</param>
internal sealed class FileLogger(TextWriter writer, string name) : ILogger
{
    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        string formatted = formatter(state, exception);

        if (string.IsNullOrEmpty(formatted) && exception == null)
        {
            // With no formatted message or exception, there's nothing to print.
            return;
        }

        var message = $"{logLevel} [{name}]{Environment.NewLine}";
        if (string.IsNullOrEmpty(formatted))
        {
            Debug.Assert(exception != null);
            message += $"{exception}";
        }
        else if (exception == null)
        {
            message += $"{formatted}";
        }
        else
        {
            message += $"{formatted}{Environment.NewLine}{Environment.NewLine}{exception}";
        }

        writer.WriteLine(message);
    }
}
