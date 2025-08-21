using Microsoft.Extensions.Logging;
using Siemens.Engineering;
using System;
using System.Collections.Generic;
using System.Linq;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public sealed class Test22Session
    {
        private Portal? _portal;
        private readonly string _tiaVersion = "V20"; // This test focuses on V20 sessions

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

            var result = _portal.ConnectPortal();
        }

        [TestCleanup]
        public void ClassCleanup()
        {
            if (_portal != null)
            {
                _portal.CloseSession();
            }
        }

        [TestMethod]
        public void Test_221_GetOpenSessions()
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var sessions = _portal.GetSessions();

            sessions?.ForEach(session =>
            {
                Console.WriteLine($"Session: Name = '{session.Name}', Author = '{session.Author}'");
                TiaMcpServer.ModelContextProtocol.Helper.GetAttributeList(session).ForEach(attribute =>
                {
                    Console.WriteLine($"- {attribute.Name}: {attribute.Value}");
                });
            });

            Assert.IsNotNull(sessions, "Failed to retrieve open sessions");
        }

        [TestMethod]
        [DynamicData(nameof(GetSessionPaths), DynamicDataSourceType.Method)]
        public void Test_222_OpenSession(string path)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenSession(path);

            Console.WriteLine($"OpenSession: {path}, result={result}");

            Assert.IsTrue(result, "Failed to open session");
        }

        [TestMethod]
        public void Test_223_GetOpenSessions()
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var sessions = _portal.GetSessions();

            sessions?.ForEach(session =>
            {
                Console.WriteLine($"Session: Name = '{session.Name}', Author = '{session.Author}'");
                TiaMcpServer.ModelContextProtocol.Helper.GetAttributeList(session).ForEach(attribute =>
                {
                    Console.WriteLine($"- {attribute.Name}: {attribute.Value}");
                });
            });

            Assert.IsNotNull(sessions, "Failed to retrieve open sessions");
        }

        [TestMethod]
        [DynamicData(nameof(GetSessionPaths), DynamicDataSourceType.Method)]
        public void Test_224_CloseSession(string path)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenSession(path);
            result &= _portal.CloseSession();

            Console.WriteLine($"CloseSession: {path}, result={result}");

            Assert.IsTrue(result, "Failed to close session");
        }

        [TestMethod]
        [DynamicData(nameof(GetSessionPaths), DynamicDataSourceType.Method)]
        public void Test_225_SaveSession(string path)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenSession(path);
            result &= _portal.SaveSession();

            Console.WriteLine($"SaveSession: {path}, result={result}");

            Assert.IsTrue(result, "Failed to save session");
        }

        public static IEnumerable<object[]> GetSessionPaths()
        {
            var config = ConfigurationHelper.GetTiaVersionConfig("V20");
            foreach (var project in config.Projects.Where(p => p.Type == "MultiUser"))
            {
                yield return new object[] { project.Path };
            }
        }
    }
}