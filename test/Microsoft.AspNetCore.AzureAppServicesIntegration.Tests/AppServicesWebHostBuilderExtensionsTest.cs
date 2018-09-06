// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Azure.AppServices.Tests
{
    public class AppServicesWebHostBuilderExtensionsTest
    {
        [Fact]
        public void UseAzureAppServices_RegisterLogger()
        {
            var mock = new Mock<IWebHostBuilder>();

            mock.Object.UseAzureAppServices();

            mock.Verify(builder => builder.ConfigureServices(It.IsNotNull<Action<IServiceCollection>>()), Times.AtLeastOnce);
        }

        
        [Fact]
        public async Task UseAzureAppServices_RegistersEventSourceLogger()
        {
            var listener = new TestEventListener();

            var webHostBuilder = new WebHostBuilder()
                .UseAzureAppServices()
                // Simulate production config
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
                .Configure(builder => builder.Run(context => context.Response.WriteAsync("Hello")));

            using (var testServer = new TestServer(
                webHostBuilder))
            {
                await testServer.CreateClient().GetStringAsync("/");
            }

            var events = listener.EventData.ToArray();
            Assert.Contains(events, args => 
                args.EventSource.Name == "Microsoft-Extensions-Logging" &&
                args.Payload.Contains("Request starting HTTP/2.0 GET http://localhost/  "));
        }

        private class TestEventListener : EventListener
        {
            private volatile bool _disposed;
            private ConcurrentQueue<EventWrittenEventArgs> _events = new ConcurrentQueue<EventWrittenEventArgs>();
            public IEnumerable<EventWrittenEventArgs> EventData => _events;
            protected override void OnEventSourceCreated(EventSource eventSource)
            {
                 if (eventSource.Name == "Microsoft-Extensions-Logging")
                 {
                     EnableEvents(eventSource, EventLevel.Informational);
                 }
             }
            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                if (!_disposed)
                {
                    _events.Enqueue(eventData);
                }
            }
            public override void Dispose()
            {
                _disposed = true;
                base.Dispose();
            }
        }
    }
}
