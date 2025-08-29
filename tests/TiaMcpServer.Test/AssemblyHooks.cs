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
            Engineering.TiaMajorVersion = 20;

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
