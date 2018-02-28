// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.AzureKeyVault.HostingStartup.Tests
{
    public class HostinStartupTests
    {
        [Fact]
        public void Configure_AddsConfiguration()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_HostingStartup__KeyVault__ConfigurationEnabled", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_HostingStartup__KeyVault__ConfigurationVault", "http://vault");

            var callbackCalled = false;
            var builder = new WebHostBuilder().Configure(app => { });

            var mockHostingStartup = new MockAzureKeyVaultHostingStartup(
                (configurationBuilder, client, vault) =>
                {
                    callbackCalled = true;
                    Assert.NotNull(configurationBuilder);
                    Assert.NotNull(client);
                    Assert.Equal("http://vault", vault);
                }
            );

            mockHostingStartup.Configure(builder);
            var _ = new TestServer(builder);

            Assert.True(callbackCalled);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("FALSE")]
        [InlineData("false")]
        public void Configure_SkipsConfiguration_IfDisabled(string value)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_HostingStartup__KeyVault__ConfigurationEnabled", value);
            Environment.SetEnvironmentVariable("ASPNETCORE_HostingStartup__KeyVault__ConfigurationVault", "http://vault");

            var callbackCalled = false;
            var builder = new WebHostBuilder().Configure(app => { });

            var mockHostingStartup = new MockAzureKeyVaultHostingStartup(
                (configurationBuilder, client, vault) =>
                {
                    callbackCalled = true;
                }
            );

            mockHostingStartup.Configure(builder);
            var _ = new TestServer(builder);

            Assert.False(callbackCalled);
        }

        private class MockAzureKeyVaultHostingStartup : AzureKeyVaultHostingStartup
        {
            private readonly Action<IConfigurationBuilder, KeyVaultClient, string> _configurationCallback;

            public MockAzureKeyVaultHostingStartup(
                Action<IConfigurationBuilder, KeyVaultClient, string> configurationCallback)
            {
                _configurationCallback = configurationCallback;
            }

            internal override void AddConfiguration(IConfigurationBuilder configurationBuilder, KeyVaultClient client, string keyVault)
            {
                _configurationCallback(configurationBuilder, client, keyVault);
            }
        }
    }
}
