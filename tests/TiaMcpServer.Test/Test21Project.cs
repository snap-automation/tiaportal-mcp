using Microsoft.Extensions.Logging;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public sealed class Test21Project
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

            Assert.IsTrue(result, "TiaPortal instance is not initialized");
        }

        [TestCleanup]
        public void ClassCleanup()
        {
            if (_portal != null)
            {
                _portal.CloseProject();
            }
        }

        [TestMethod]
        public void Test_211_GetOpenProjects()
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var projects = _portal.GetProjects();

            projects?.ForEach(project =>
            {
                Console.WriteLine($"Project: Name = '{project.Name}', Author = '{project.Author}'");
                TiaMcpServer.ModelContextProtocol.Helper.GetAttributeList(project).ForEach(attribute =>
                {
                    Console.WriteLine($"- {attribute.Name}: {attribute.Value}");
                });
            });

            Assert.IsNotNull(projects, "Failed to retrieve open projects");

            // Assert.IsTrue(projects?.Count > 0, "No open projects found");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_212_OpenProject(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");
            if (testCase.MultiUser)
                Assert.Inconclusive("Skipping multi-user test case for this test class.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(testCase.ProjectPath);

            Console.WriteLine($"OpenProject: {testCase.ProjectPath}, result={result}");

            Assert.IsTrue(result, "Failed to open project");
        }

        [TestMethod]
        public void Test_213_GetOpenProjects()
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var projects = _portal.GetProjects();

            projects?.ForEach(project =>
            {
                Console.WriteLine($"Project: Name = '{project.Name}', Author = '{project.Author}'");
                TiaMcpServer.ModelContextProtocol.Helper.GetAttributeList(project).ForEach(attribute =>
                {
                    Console.WriteLine($"- {attribute.Name}: {attribute.Value}");
                });
            });

            Assert.IsNotNull(projects, "Failed to retrieve open projects");

            // Assert.IsTrue(projects?.Count > 0, "No open projects found");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_214_CloseProject(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");
            if (testCase.MultiUser)
                Assert.Inconclusive("Skipping multi-user test case for this test class.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(testCase.ProjectPath);
            result &= _portal.CloseProject();

            Console.WriteLine($"CloseProject: {testCase.ProjectPath}, result={result}");

            Assert.IsTrue(result, "Failed to close project");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_215_SaveProject(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");
            if (testCase.MultiUser)
                Assert.Inconclusive("Skipping multi-user test case for this test class.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(testCase.ProjectPath);
            result &= _portal.SaveProject();

            Console.WriteLine($"SaveProject: {testCase.ProjectPath}, result={result}");

            Assert.IsTrue(result, "Failed to save project");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_216_SaveAsProject(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");
            if (testCase.MultiUser)
                Assert.Inconclusive("Skipping multi-user test case for this test class.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(testCase.ProjectPath);
            result &= _portal.SaveAsProject(testCase.ExportRoot);

            Assert.IsTrue(result, "Failed to save project as new project");
        }
    }
}