using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.AspNetCore.AzureAppServices.FunctionalTests
{
    public class LoggingInterceptor : IServiceClientTracingInterceptor
    {
        private readonly ILogger _logger;

        public LoggingInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public void Information(string message)
        {
            LoggerExtensions.LogInformation(_logger, message);
        }

        public void TraceError(string invocationId, Exception exception)
        {
            LoggerExtensions.LogInformation(_logger, exception, "Exception in {invocationId}", invocationId);
        }

        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            LoggerExtensions.LogInformation(_logger, response.AsFormattedString());
        }

        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            LoggerExtensions.LogInformation(_logger, request.AsFormattedString());
        }

        public void Configuration(string source, string name, string value) { }

        public void EnterMethod(string invocationId, object instance, string method, IDictionary<string, object> parameters) { }

        public void ExitMethod(string invocationId, object returnValue) { }
    }
}