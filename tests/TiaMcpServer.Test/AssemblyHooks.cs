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

            string envVersion = Environment.GetEnvironmentVariable("TIA_MCP_TEST_VERSION");
            if (!string.IsNullOrEmpty(envVersion) && int.TryParse(envVersion, out int parsedVersion))
            {
                tiaMajorVersion = parsedVersion;
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
