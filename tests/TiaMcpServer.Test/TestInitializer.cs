
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    public class TestInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var tiaMajorVersionString = context.Properties["TiaMajorVersion"]?.ToString();
            if (int.TryParse(tiaMajorVersionString, out int tiaMajorVersion))
            {
                Engineering.TiaMajorVersion = tiaMajorVersion;
            }
            else
            {
                Engineering.TiaMajorVersion = 20; // Default value
            }

            if (Engineering.TiaMajorVersion < 20)
            {
                AppDomain.CurrentDomain.AssemblyResolve += Engineering.Resolver;
            }
            else
            {
                Openness.Initialize(Engineering.TiaMajorVersion);
            }
        }
    }
}
