// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ApplicationInsightsJavaScriptSnippetTest
{
    public class Validator
    {
        private HttpClient _httpClient;

        private HttpClientHandler _httpClientHandler;

        private readonly ILogger _logger;

        private readonly DeploymentResult _deploymentResult;

        public Validator(
            HttpClient httpClient,
            HttpClientHandler httpClientHandler,
            ILogger logger,
            DeploymentResult deploymentResult)
        {
            _httpClient = httpClient;
            _httpClientHandler = httpClientHandler;
            _logger = logger;
            _deploymentResult = deploymentResult;
        }

        public async Task VerifyScriptCheckPage(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogInformation("Home page content : {0}", responseContent);
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ValidateLayoutPage(responseContent);
        }

        private void ValidateLayoutPage(string responseContent)
        {
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "test\\ApplicationInsightsJavaScriptSnippetTest\\Rendered.html"));
            if (File.Exists(path))
            {
                TextReader textReader = File.OpenText(path);
                Assert.Equal(
                    textReader.ReadToEnd(),
                    responseContent,
                    ignoreCase: true,
                    ignoreLineEndingDifferences: true,
                    ignoreWhiteSpaceDifferences: true);
            }
        }
    }
}
