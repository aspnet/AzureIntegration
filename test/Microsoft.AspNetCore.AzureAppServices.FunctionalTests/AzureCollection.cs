using Xunit;

namespace Microsoft.AspNetCore.AzureAppServices.FunctionalTests
{
    [CollectionDefinition("Azure")]
    public class AzureCollection : ICollectionFixture<AzureFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}