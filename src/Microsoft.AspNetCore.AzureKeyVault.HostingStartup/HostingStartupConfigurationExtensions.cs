using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.AzureKeyVault.HostingStartup
{
    internal static class HostingStartupConfigurationExtensions
    {
        public static bool IsEnabled(this IConfiguration configuration, string hostingStartupName) => IsEnabled(configuration, hostingStartupName, "Enable");

        public static bool IsEnabled(this IConfiguration configuration, string hostingStartupName, string featureName)
        {
            if (configuration.TryGetOption(hostingStartupName, featureName, out var value))
            {
                value = value.ToLowerInvariant();
                return value != "false" && value != "0";
            }

            return true;
        }

        public static bool TryGetOption(this IConfiguration configuration, string hostingStartupName, string featureName, out string value)
        {
            value = configuration[$"Lightup:{hostingStartupName}:{featureName}"];
            return !string.IsNullOrEmpty(value);
        }
    }
}