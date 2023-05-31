using System;
using Logger.Interfaces;
using Microsoft.Extensions.Logging;

namespace Acies.Docs.Api
{
    public class CustomLogger : ILogger
    {
        private ILogService logService;

        public CustomLogger(ILogService logService)
        {
            this.logService = logService;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Warning || logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
                return true;

            return logService.LogConfig.LogLevel == Logger.Models.LogLevel.Debug;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Warning || logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
                logService.LogException(formatter(state, exception), "In some ASP.Net method", exception ?? new Exception());
            else if (logService.LogConfig.LogLevel == Logger.Models.LogLevel.Debug)
                logService.Log(Logger.Models.LogLevel.Debug, formatter(state, exception), "In some ASP.Net method");
        }
    }

    public class CustomLoggerProvider : ILoggerProvider
    {
        private ILogService logService;

        public CustomLoggerProvider(ILogService logService)
        {
            this.logService = logService;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(logService);
        }

        public void Dispose()
        {

        }
    }
}