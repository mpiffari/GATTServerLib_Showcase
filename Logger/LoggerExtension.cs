using Microsoft.Extensions.Logging;

namespace Showcase.Logger;

public static class LoggerExtensions
{
    public static bool IsEnabled(this LogLevel logLevel)
    {
        // Select which levels keep active
        switch ( logLevel )
        {
            case LogLevel.Trace:
                return false;
            case LogLevel.Critical:
                return true;
            case LogLevel.Debug:
                return true;
            case LogLevel.Error:
                return true;
            case LogLevel.Information:
                return true;
            case LogLevel.None:
                return true;
            case LogLevel.Warning:
                return true;
            default:
                return false;
        };
    }

    public static bool IsEnabled(this LoggerScope scope)
    {
        // Select which category keep active
        switch ( scope )
        {
            case LoggerScope.GATT_SERVER_LIB_CONSUMER:
                return true;
            
            default:
                return false;
        };
    }

    public static EventId EventId(this LoggerScope scope)
    {
        return new EventId((int)scope, scope.ToString());
    }

    public static LoggerScope? Scope(this EventId eventId)
    {
        if (eventId.Name == LoggerScope.GATT_SERVER_LIB_CONSUMER.ToString())
        {
            return LoggerScope.GATT_SERVER_LIB_CONSUMER;
        }

        return null;
    }
}