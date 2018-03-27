using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Microsoft.AspNetCore.AzureKeyVault.HostingStartup.AzureKeyVaultHostingStartup))]