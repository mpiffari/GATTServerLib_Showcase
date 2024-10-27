using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Showcase.Logger;

public enum LoggerScope
{
    [Description("LIB CONSUMER")]
    GATT_SERVER_LIB_CONSUMER,
}

public class Logger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!logLevel.IsEnabled())
        {
            return;
        }

        if (eventId.Scope() != null)
        {
            var LoggerScope = (LoggerScope)eventId.Scope()!;
            if (!LoggerScope.IsEnabled())
            {
                return;
            }
        }

        var logMessage = new StringBuilder($"{GetHeaderInfoLog(logLevel, eventId, exception == null)} {formatter(state, null)}");

        if (exception != null || logLevel == LogLevel.Error)
        {
            if (!( formatter(state, null).Length <= 0 ))
            {
                logMessage.Append($"\n  Custom message: {formatter(state, null)}");
            }

            if (exception?.GetType() != null)
            {
                logMessage.Append($"\n  Exception type: {exception.GetType().ToString() ?? "-"}");
            }

            if (!string.IsNullOrEmpty(exception?.Message))
            {
                logMessage.Append($"\n  Exception message: {exception?.Message ?? "-"}");
            }

            if (!string.IsNullOrEmpty(exception?.StackTrace))
            {
                logMessage.Append($"\n  Exception stack: {exception?.StackTrace ?? "-"}");
            }
        }

        Debug.WriteLine(logMessage.ToString());
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel.IsEnabled();
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return default!;
    }

    private static string GetHeaderInfoLog(LogLevel logLevel, EventId eventId, bool hasException)
    {
        // If LogLevel is error with an Exception - stack call has layer
        var frameNumber = logLevel == LogLevel.Error && !hasException ? 4 : 5;

        var fileName = new StackTrace(true).GetFrame(frameNumber).GetFileName() ?? "";
        var lineNumber = new StackTrace(true).GetFrame(frameNumber).GetFileLineNumber();
        var endFile = "";

        if (fileName.Contains('/')) //for macOS
        {
            endFile = fileName.Split('/').Last() ?? "";
        }
        else if (fileName.Contains('\\')) //for Windows
        {
            endFile = fileName.Split('\\').Last() ?? "";
        }

        // Log called from nuget
        if (endFile.Length <= 0 || lineNumber == 0)
        {
            return $"({(eventId.Name?.Length <= 0 ? "" : $"{eventId.Name}")}) {DateTime.Now:dd-MM-yyyy HH:mm:ss.fff} [{logLevel}] [nuget] > ";
        }

        // Log called from classes
        return $"({(eventId.Name?.Length <= 0 ? "" : $"{eventId.Name}")}) {DateTime.Now:dd-MM-yyyy HH:mm:ss.fff} [{logLevel}] [{endFile}:{lineNumber}] > ";
    }
}