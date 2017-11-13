// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Hosting
{
    public class AppModelDetector
    {
        // We use Hosting package to detect AspNetCore version
        // it contains light-up implemenation that we care about and
        // would have to be used by aspnet core web apps
        private const string AspNetCoreAssembly = "Microsoft.AspNetCore.Hosting";

        public AppModelDetectionResult Detect(DirectoryInfo directory)
        {
            var result = new AppModelDetectionResult();
            string entryPoint = null;

            // Try reading web.config and resolving framework and app path
            var webConfig = new FileInfo(Path.Combine(directory.FullName, "web.config"));
            if (webConfig.Exists)
            {
                if (TryParseWebConfig(webConfig, out var framework, out entryPoint))
                {
                    result.Framework = framework;
                }
                else
                {
                    // web.config exists so default to full framework
                    result.Framework = RuntimeFramework.DotNetFramework;
                }
            }

            // If we found entry point let's look for .deps.json
            // in some cases it exists in desktop too
            FileInfo depsJson = null;
            FileInfo runtimeConfig = null;

            if (!string.IsNullOrWhiteSpace(entryPoint))
            {
                depsJson = new FileInfo(Path.ChangeExtension(entryPoint, ".deps.json"));
                runtimeConfig = new FileInfo(Path.ChangeExtension(entryPoint, ".runtimeconfig.json"));
            }

            if (depsJson == null || !depsJson.Exists)
            {
                depsJson = directory.GetFiles("*.deps.json").FirstOrDefault();
            }

            if (runtimeConfig == null || !runtimeConfig.Exists)
            {

                runtimeConfig = directory.GetFiles("*.runtimeconfig.json").FirstOrDefault();
            }

            if (depsJson != null &&
                depsJson.Exists  &&
                TryParseDependencies(depsJson, out var aspNetCoreVersion))
            {
                result.AspNetCoreVersion = aspNetCoreVersion;
            }

            // Lets try assembly version
            if (string.IsNullOrWhiteSpace(result.AspNetCoreVersion))
            {
                var aspNetCoreDll = new FileInfo(Path.Combine(directory.FullName, AspNetCoreAssembly + ".dll"));
                if (aspNetCoreDll.Exists &&
                    TryParseAssembly(aspNetCoreDll, out aspNetCoreVersion))
                {
                    result.AspNetCoreVersion = aspNetCoreVersion;
                }
            }

            if (result.Framework == RuntimeFramework.DotNetCore &&
                runtimeConfig != null &&
                runtimeConfig.Exists &&
                TryParseRuntimeConfig(runtimeConfig, out var runtimeVersion))
            {
                result.FrameworkVersion = runtimeVersion;
            }

            return result;
        }

        private bool TryParseAssembly(FileInfo aspNetCoreDll, out string aspNetCoreVersion)
        {
            aspNetCoreVersion = null;
            try
            {
                using (var stream = aspNetCoreDll.OpenRead())
                using (var peReader = new PEReader(stream))
                {
                    var metadataReader = peReader.GetMetadataReader();
                    var assemblyDefinition = metadataReader.GetAssemblyDefinition();
                    aspNetCoreVersion = assemblyDefinition.Version.ToString();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryParseDependencies(FileInfo depsJson, out string aspnetCoreVersion)
        {
            aspnetCoreVersion = null;
            try
            {
                using (var streamReader = depsJson.OpenText())
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var json = JObject.Load(jsonReader);

                    var libraryPrefix = AspNetCoreAssembly+ "/";

                    var library = json.Descendants().OfType<JProperty>().FirstOrDefault(property => property.Name.StartsWith(libraryPrefix));
                    if (library != null)
                    {
                        aspnetCoreVersion = library.Name.Substring(libraryPrefix.Length);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        private bool TryParseRuntimeConfig(FileInfo runtimeConfig, out string frameworkVersion)
        {
            frameworkVersion = null;
            try
            {
                using (var streamReader = runtimeConfig.OpenText())
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var json = JObject.Load(jsonReader);
                    frameworkVersion = (string)json?["runtimeOptions"]
                        ?["framework"]
                        ?["version"];

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryParseWebConfig(FileInfo webConfig, out RuntimeFramework? framework, out string entryPoint)
        {
            framework = null;
            entryPoint = null;

            try
            {
                var xdocument = XDocument.Load(webConfig.FullName);
                var aspNetCoreHandler = xdocument.Root?
                    .Element("system.webServer")
                    .Element("aspNetCore");

                if (aspNetCoreHandler == null)
                {
                    return false;
                }

                var processPath = (string) aspNetCoreHandler.Attribute("processPath");
                var arguments = (string) aspNetCoreHandler.Attribute("arguments");

                if (processPath.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    processPath.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(arguments))
                {
                    framework = RuntimeFramework.DotNetCore;
                    var entryPointPart = arguments.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(entryPointPart))
                    {
                        try
                        {
                            entryPoint = Path.GetFullPath(Path.Combine(webConfig.DirectoryName, entryPointPart));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    framework = RuntimeFramework.DotNetFramework;

                    try
                    {
                        entryPoint = Path.GetFullPath(Path.Combine(webConfig.DirectoryName, processPath));
                    }
                    catch (Exception)
                    {    
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}