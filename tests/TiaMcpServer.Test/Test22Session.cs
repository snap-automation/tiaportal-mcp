using Microsoft.Extensions.Logging;
using Siemens.Engineering;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public sealed class Test22Session
    {
        
        private Portal? _portal;

        [TestInitialize]
        public void ClassInit()
        {
            
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
                _portal.Dispose();
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
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_222_OpenSession(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive("Skipping test for different TIA version.");
            if (!testCase.MultiUser)
                Assert.Inconclusive("Skipping non-multi-user test case.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenSession(testCase.ProjectPath);

            Console.WriteLine($"OpenSession: {testCase.ProjectPath}, result={result}");

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
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_224_CloseSession(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive("Skipping test for different TIA version.");
            if (!testCase.MultiUser)
                Assert.Inconclusive("Skipping non-multi-user test case.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenSession(testCase.ProjectPath);
            result &= _portal.CloseSession();

            Console.WriteLine($"CloseSession: {testCase.ProjectPath}, result={result}");

            Assert.IsTrue(result, "Failed to close session");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_225_SaveSession(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive("Skipping test for different TIA version.");
            if (!testCase.MultiUser)
                Assert.Inconclusive("Skipping non-multi-user test case.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenSession(testCase.ProjectPath);
            result &= _portal.SaveSession();

            Console.WriteLine($"SaveSession: {testCase.ProjectPath}, result={result}");

            Assert.IsTrue(result, "Failed to save session");
        }
    }
}