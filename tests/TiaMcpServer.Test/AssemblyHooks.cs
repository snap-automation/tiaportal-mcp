using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    public class AssemblyHooks
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Runs once before any tests in the assembly  
            // context.WriteLine("Assembly initialization started");
            int tiaMajorVersion = 20; // Default value

            if (context.Properties.Contains("TIA_MCP_TEST_VERSION"))
            {
                string runSettingsVersion = context.Properties["TIA_MCP_TEST_VERSION"].ToString();
                if (int.TryParse(runSettingsVersion, out int parsedVersion))
                {
                    tiaMajorVersion = parsedVersion;
                }
            }

            Engineering.TiaMajorVersion = tiaMajorVersion;

            if (Engineering.TiaMajorVersion < 20)
            {
                AppDomain.CurrentDomain.AssemblyResolve += Engineering.Resolver;
            }
            else
            {
                Openness.Initialize(Engineering.TiaMajorVersion);
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Runs once after all tests in the assembly  
            // Console.WriteLine("Assembly cleanup completed");
        }
    }
}
