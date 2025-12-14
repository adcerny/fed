using Microsoft.Extensions.Logging;
using System;

namespace Fed.AzureFunctions.Runner
{
    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.ForegroundColor = logLevel == LogLevel.Error ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.WriteLine(formatter(state, exception));
            Console.ResetColor();
        }
    }
}
