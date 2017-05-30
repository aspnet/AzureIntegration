﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: HostingStartup(typeof(Microsoft.AspNetCore.ApplicationInsights.HostingStartup.ApplicationInsightsHostingStartup))]

// To be able to build as <OutputType>Exe</OutputType>
internal class Program { public static void Main() { } }

namespace Microsoft.AspNetCore.ApplicationInsights.HostingStartup
{
    /// <summary>
    /// A dynamic Application Insights lightup experience
    /// </summary>
    public class ApplicationInsightsHostingStartup : IHostingStartup
    {
        private const string ApplicationInsightsSettingsFile = "ApplicationInsights.settings.json";

        /// <summary>
        /// Calls UseApplicationInsights
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(IWebHostBuilder builder)
        {
            builder.UseApplicationInsights();

            builder.ConfigureServices(InitializeServices);
        }


        /// <summary>
        /// Adds the Javascript <see cref="TagHelperComponent"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> associated with the application.</param>
        private void InitializeServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigureOptions<LoggerFilterOptions>, DefaultApplicationInsightsLoggerFilters>();
            services.AddSingleton<IStartupFilter, ApplicationInsightsLoggerStartupFilter>();
            services.AddSingleton<ITagHelperComponent, JavaScriptSnippetTagHelperComponent>();

            var home = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(home))
            {
                var settingsFile = Path.Combine(home, "site", "diagnostics", ApplicationInsightsSettingsFile);
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile(settingsFile, optional: true);
                services.AddLogging().AddConfiguration(configurationBuilder.Build().GetSection("Logging"), replace: true);
            }
        }
    }

    internal class DefaultApplicationInsightsLoggerFilters : IConfigureOptions<LoggerFilterOptions>
    {
        private const string ApplicationInsightsLoggerFactory = "Microsoft.ApplicationInsights.AspNetCore.Logging.ApplicationInsightsLoggerProvider";

        private static readonly KeyValuePair<string, LogLevel>[] _defaultLoggingLevels = {
            new KeyValuePair<string, LogLevel>("Microsoft", LogLevel.Warning),
            new KeyValuePair<string, LogLevel>("System", LogLevel.Warning),
            new KeyValuePair<string, LogLevel>(null, LogLevel.Information)
        };

        public void Configure(LoggerFilterOptions options)
        {
            foreach (var pair in _defaultLoggingLevels)
            {
                options.Rules.Add(new LoggerFilterRule(
                    ApplicationInsightsLoggerFactory,
                    pair.Key,
                    null,
                    (type, name, level) => Debugger.IsAttached || level >= pair.Value
                ));
            }
        }
    }
}
