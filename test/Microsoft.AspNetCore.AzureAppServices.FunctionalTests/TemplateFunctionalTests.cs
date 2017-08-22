using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.AzureAppServices.FunctionalTests
{
    [Collection("Azure")]
    public class TemplateFunctionalTests
    {
        readonly AzureFixture _fixture;

        private readonly ITestOutputHelper _outputHelper;

        public TemplateFunctionalTests(AzureFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task DotnetNewWebRunsInWebApp()
        {
            using (var logger = GetLogger())
            {
                Assert.NotNull(_fixture.Azure);

                var site = await _fixture.Deploy("Templates\\BasicAppServices.json", null);
                var testDirectory = GetTestDirectory();
                var publishDir = testDirectory.CreateSubdirectory("publish");

                var dotnet = DotNet(logger, testDirectory);

                var result = await dotnet.ExecuteAsync("new web");
                result.AssertSuccess();

                var publishResult = await dotnet.ExecuteAsync($"publish -o \"{publishDir.FullName}\"");
                publishResult.AssertSuccess();

                await site.UploadFilesAsync(publishDir, "", await site.GetPublishingProfileAsync(), logger);

                var httpClient = site.CreateClient();
                var getresult = await httpClient.GetAsync("/");
                getresult.EnsureSuccessStatusCode();

                Assert.Equal("Hello World!", await getresult.Content.ReadAsStringAsync());
            }
        }

        private TestLogger GetLogger([CallerMemberName] string callerName = null)
        {
            _fixture.TestLog.StartTestLog(_outputHelper, nameof(TemplateFunctionalTests), out var factory, callerName);
            return new TestLogger(factory, factory.CreateLogger(callerName));
        }


        private TestCommand DotNet(TestLogger logger, DirectoryInfo workingDirectory)
        {
            return new TestCommand("dotnet")
            {
                Logger = logger,
                WorkingDirectory = workingDirectory.FullName
            };
        }

        private DirectoryInfo GetTestDirectory([CallerMemberName] string callerName = null)
        {
            if (Directory.Exists(callerName))
            {
                Directory.Delete(callerName, recursive:true);
            }
            return Directory.CreateDirectory(callerName);
        }
    }
}