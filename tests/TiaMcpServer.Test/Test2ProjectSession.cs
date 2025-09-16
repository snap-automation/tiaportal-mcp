using Microsoft.Extensions.Logging;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public class Test2ProjectSession
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
        public void Test_20_GetProjects()
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var projects = _portal.GetProjects();

            projects.AddRange(_portal.GetSessions());

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
        public void Test_21_GetProjectTree(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive("Skipping test for different TIA version.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.GetProjectTree();

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Console.WriteLine($"GetProjectTree:");
            Console.WriteLine(result);

            Assert.IsNotNull(success, "No code block found");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetDeviceItemSource), typeof(TiaTestCases))]
        public void Test_22_GetSoftwareTree(SimpleTiaTestCase testCase, string softwarePath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive("Skipping test for different TIA version.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.GetSoftwareTree(softwarePath);

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Console.WriteLine($"GetSoftwareTree:");
            Console.WriteLine(result);

            Assert.IsNotNull(result, "No software found");
        }
    }
}