// // Copyright (c) .NET Foundation. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.AzureAppServicesIntegration.Tests
{
    public class AppModelTests
    {
        [Theory]
        [InlineData("dotnet")]
        [InlineData("dotnet.exe")]
        [InlineData("%HOME%/dotnet")]
        [InlineData("%HOME%/dotnet.exe")]
        public void DetectsCoreFrameworkFromWebConfig(string processPath)
        {
            using (var temp = new TemporaryDirectory()
                .WithFile("web.config",$@"
<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <system.webServer>
    <handlers>
      <add name=""aspNetCore"" path=""*"" verb=""*"" modules=""AspNetCoreModule"" resourceType=""Unspecified"" />
    </handlers>
    <aspNetCore processPath=""{processPath}"" arguments="".\app.dll"" stdoutLogEnabled=""false"" stdoutLogFile="".\logs\stdout"" />
  </system.webServer>
</configuration>
"))
            {
                var result = new AppModelDetector().Detect(temp.Directory);
                Assert.Equal(RuntimeFramework.DotNetCore, result.Framework);
            }
        }


        private class TemporaryDirectory: IDisposable
        {
            public TemporaryDirectory()
            {
                Directory = new DirectoryInfo(Path.GetTempPath());
            }

            public DirectoryInfo Directory { get; }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(true);
                }
                catch (IOException)
                {
                }
            }

            public TemporaryDirectory WithFile(string name, string value)
            {
                File.WriteAllText(Path.Combine(Directory.FullName, name), value);
            }
        }
    }
}