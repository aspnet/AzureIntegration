// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Microsoft.AspNetCore.AzureKeyVault.HostingStartup.AzureKeyVaultHostingStartup))]

namespace Microsoft.AspNetCore.AzureKeyVault.HostingStartup
{
    /// <summary>
    /// A dynamic KeyVault lightup experience
    /// </summary>
    public class AzureKeyVaultHostingStartup : IHostingStartup
    {
        private const string HostingStartupName = "KeyVault";
        private const string ConfigurationFeatureName = "Configuration";
        private const string DataProtectionFeatureName = "DataProtection";

        /// <inheritdoc />
        public void Configure(IWebHostBuilder builder)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
            var keyVaultClient = new KeyVaultClient(authenticationCallback);

            var baseConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .Build();

            if (baseConfiguration.IsEnabled(HostingStartupName))
            {
                builder.ConfigureServices((context, collection) =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddConfiguration(baseConfiguration)
                        .AddConfiguration(context.Configuration)
                        .Build();

                    if (configuration.IsEnabled(HostingStartupName, DataProtectionFeatureName))
                    {
                        collection.AddDataProtection().ProtectKeysWithAzureKeyVault(keyVaultClient, null);
                    }
                });

                if (baseConfiguration.IsEnabled(HostingStartupName, ConfigurationFeatureName) &&
                    baseConfiguration.TryGetOption(HostingStartupName, ConfigurationFeatureName, out var vault))
                {
                    builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                    {
                        configurationBuilder.AddAzureKeyVault(vault, keyVaultClient, new DefaultKeyVaultSecretManager());
                    });
                }
            }
        }
    }
}
