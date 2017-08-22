using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.AzureAppServices.FunctionalTests
{
    internal class TestLogger: ILogger, IDisposable
    {
        private readonly ILoggerFactory _factory;

        private readonly ILogger _logger;

        public TestLogger(ILoggerFactory factory, ILogger logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}