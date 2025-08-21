using Microsoft.CodeCoverage.Core;
using Microsoft.Extensions.Logging;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    public sealed class Test1Portal
    {
        private Portal? _portal;
        private readonly string _tiaVersion = "V20"; // Default to V20 for now

        public Test1Portal()
        {
            // Default constructor for test runner
        }

        public Test1Portal(string tiaVersion)
        {
            _tiaVersion = tiaVersion;
        }

        [TestInitialize]
        public void ClassInit()
        {
            if (!ConfigurationHelper.IsTiaPortalVersionInstalled(_tiaVersion))
            {
                Assert.Inconclusive($"TIA Portal {_tiaVersion} is not installed on this machine.");
            }

            Openness.Initialize();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // or AddDebug(), AddTraceSource(), etc.
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            ILogger<Portal> logger = loggerFactory.CreateLogger<Portal>();
            _portal ??= new(logger);
        }
    }
}
