using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.AzureAppServices.FunctionalTests
{
    internal static class WebAppExtensions
    {
        public static HttpClient CreateClient(this IWebApp site)
        {
            var domain = site.GetHostNameBindings().First().Key;

            return new HttpClient { BaseAddress = new Uri("http://" + domain) };
        }

        public static async Task UploadFilesAsync(this IWebApp site, DirectoryInfo from, string to, IPublishingProfile publishingProfile, ILogger logger)
        {
            foreach (var info in from.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                if (info is FileInfo file)
                {
                    var address = new Uri(
                        "ftp://" + publishingProfile.FtpUrl + to + file.FullName.Substring(from.FullName.Length).Replace('\\', '/'));
                    logger.LogInformation($"Uploading {file.FullName} to {address}");

                    var request = (FtpWebRequest)WebRequest.Create(address);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.KeepAlive = true;
                    request.UseBinary = true;
                    request.UsePassive = false;
                    request.Credentials = new NetworkCredential(publishingProfile.FtpUsername, publishingProfile.FtpPassword);
                    request.ConnectionGroupName = "group";
                    using (var fileStream = File.OpenRead(file.FullName))
                    {
                        using (var requestStream = await request.GetRequestStreamAsync())
                        {
                            await fileStream.CopyToAsync(requestStream);
                        }
                    }
                    await request.GetResponseAsync();
                }
            }
        }
    }
}