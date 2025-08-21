using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using Microsoft.Win32; // Required for Registry access

namespace TiaMcpServer.Test
{
    public static class ConfigurationHelper
    {
        private static IConfigurationRoot _configuration;
        private static TestConfiguration _testConfiguration;

        static ConfigurationHelper()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _testConfiguration = _configuration.Get<TestConfiguration>();
        }

        public static TestConfiguration GetTestConfiguration()
        {
            return _testConfiguration;
        }

        public static TiaVersion GetTiaVersionConfig(string version)
        {
            return _testConfiguration.TiaVersions.FirstOrDefault(v => v.Version.Equals(version, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsTiaPortalVersionInstalled(string version)
        {
            // Example: Check for TIA Portal V18 or V20 installation in the registry
            // This path might vary slightly based on TIA Portal version and installation
            string registryPath = $"SOFTWARE\\Siemens\\Automation\\Portal\\{version}";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                return key != null;
            }
        }
    }
}