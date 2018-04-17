﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationInsightsJavaScriptSnippetTest
{
    public class LoggingTest : ApplicationInsightsFunctionalTest
    {
        public LoggingTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public Task DefaultAILogFiltersApplied_ForNetCoreApp20_Portable() => DefaultAILogFiltersApplied("netcoreapp2.0", ApplicationType.Portable);

        [Fact]
        public Task DefaultAILogFiltersApplied_ForNetCoreApp20_Standalone() => DefaultAILogFiltersApplied("netcoreapp2.0", ApplicationType.Standalone);

        [Fact]
        public Task DefaultAILogFiltersApplied_ForNetCoreApp21_Portable() => DefaultAILogFiltersApplied("netcoreapp2.1", ApplicationType.Portable);

        [Fact]
        public Task DefaultAILogFiltersApplied_ForNetCoreApp21_Standalone() => DefaultAILogFiltersApplied("netcoreapp2.1", ApplicationType.Standalone);

        private async Task DefaultAILogFiltersApplied(string targetFramework, ApplicationType applicationType)
        {
            var responseText = await RunRequest(targetFramework, applicationType, "DefaultLogging");

            // Enabled by default
            Assert.Contains("System warning log", responseText);
            // Disabled by default
            Assert.DoesNotContain("System information log", responseText);
            // Disabled by default
            Assert.DoesNotContain("System trace log", responseText);

            // Enabled by default
            Assert.Contains("Microsoft warning log", responseText);
            // Disabled by default but overridden by ApplicationInsights.settings.json
            Assert.Contains("Microsoft information log", responseText);
            // Disabled by default
            Assert.DoesNotContain("Microsoft trace log", responseText);

            // Enabled by default
            Assert.Contains("Custom warning log", responseText);
            // Enabled by default
            Assert.Contains("Custom information log", responseText);
            // Disabled by default
            Assert.DoesNotContain("Custom trace log", responseText);

            // Enabled by default
            Assert.Contains("Specific warning log", responseText);
            // Enabled by default
            Assert.Contains("Specific information log", responseText);
            // Disabled by default but overridden by ApplicationInsights.settings.json
            Assert.Contains("Specific trace log", responseText);
        }

        [Fact]
        public Task CustomAILogFiltersApplied_ForNetCoreApp20_Portable() => CustomAILogFiltersApplied("netcoreapp2.0", ApplicationType.Portable);

        [Fact]
        public Task CustomAILogFiltersApplied_ForNetCoreApp20_Standalone() => CustomAILogFiltersApplied("netcoreapp2.0", ApplicationType.Standalone);

        [Fact]
        public Task CustomAILogFiltersApplied_ForNetCoreApp21_Portable() => CustomAILogFiltersApplied("netcoreapp2.1", ApplicationType.Portable);

        [Fact]
        public Task CustomAILogFiltersApplied_ForNetCoreApp21_Standalone() => CustomAILogFiltersApplied("netcoreapp2.1", ApplicationType.Standalone);

        private async Task CustomAILogFiltersApplied(string targetFramework, ApplicationType applicationType)
        {
            var responseText = await RunRequest(targetFramework, applicationType, "CustomLogging");
            // Custom logger allows only namespaces with 'o' in the name

            Assert.DoesNotContain("System warning log", responseText);
            Assert.DoesNotContain("System information log", responseText);
            Assert.DoesNotContain("System trace log", responseText);

            // Enabled by default
            Assert.Contains("Microsoft warning log", responseText);
            Assert.Contains("Microsoft information log", responseText);
            Assert.DoesNotContain("Microsoft trace log", responseText);

            // Enabled by default
            Assert.Contains("Custom warning log", responseText);
            Assert.Contains("Custom information log", responseText);
            Assert.DoesNotContain("Custom trace log", responseText);

            // Enabled by default
            Assert.DoesNotContain("Specific warning log", responseText);
            Assert.DoesNotContain("Specific information log", responseText);
            Assert.DoesNotContain("Specific trace log", responseText);
        }

        private async Task<string> RunRequest(string targetFramework, ApplicationType applicationType, string environment)
        {
            string responseText;
            var testName = $"ApplicationInsightsLoggingTest_{applicationType}";
            using (StartLog(out var loggerFactory, testName))
            {
                var logger = loggerFactory.CreateLogger(nameof(JavaScriptSnippetTest));
                var deploymentParameters = new DeploymentParameters(GetApplicationPath(), ServerType.Kestrel,
                    RuntimeFlavor.CoreClr, RuntimeArchitecture.x64)
                {
                    PublishApplicationBeforeDeployment = true,
                    PreservePublishedApplicationForDebugging = PreservePublishedApplicationForDebugging,
                    TargetFramework = "netcoreapp2.0",
                    Configuration = GetCurrentBuildConfiguration(),
                    ApplicationType = applicationType,
                    EnvironmentName = environment,
                    EnvironmentVariables =
                    {
                        new KeyValuePair<string, string>(
                            "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES",
                            "Microsoft.AspNetCore.ApplicationInsights.HostingStartup"),
                        new KeyValuePair<string, string>(
                            "HOME",
                            Path.Combine(GetApplicationPath(), "home"))
                    },
                };

                using (var deployer = ApplicationDeployerFactory.Create(deploymentParameters, loggerFactory))
                {
                    var deploymentResult = await deployer.DeployAsync();
                    var httpClientHandler = new HttpClientHandler();
                    var httpClient = deploymentResult.CreateHttpClient(httpClientHandler);

                    // Request to base address and check if various parts of the body are rendered & measure the cold startup time.
                    var response = await RetryHelper.RetryRequest(
                        async () => await httpClient.GetAsync("/log"),
                        logger: logger, cancellationToken: deploymentResult.HostShutdownToken);

                    Assert.False(response == null, "Response object is null because the client could not " +
                                                   "connect to the server after multiple retries");

                    responseText = await response.Content.ReadAsStringAsync();
                }
            }
            return responseText;
        }
    }
}
