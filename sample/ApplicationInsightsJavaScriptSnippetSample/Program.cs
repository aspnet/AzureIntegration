// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.ApplicationInsights.HostingStartup;
using Microsoft.AspNetCore.Hosting;

namespace ApplicationInsightsJavaScriptSnippetSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();
            new ApplicationInsightsHostingStartup().Configure(builder);
            var host = builder.Build();

            host.Run();
        }
    }
}
