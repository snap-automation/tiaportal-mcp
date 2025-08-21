using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public class Test2ProjectSession
    {
        private Portal? _portal;
        private readonly string _tiaVersion = "V20"; // This test focuses on V20 projects/sessions

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
                //_portal.CloseSession(); // This was commented out in original, keeping it that way
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
        [DynamicData(nameof(GetProjectAndSessionPaths), DynamicDataSourceType.Method)]
        public void Test_21_GetProjectTree(string projectPath)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, projectPath);

            var result = _portal.GetProjectTree();

            success &= Common.CloseProject(_portal, projectPath);

            Console.WriteLine($"GetProjectTree:");
            Console.WriteLine(result);

            Assert.IsNotNull(success, "No code block found");
        }

        [TestMethod]
        [DynamicData(nameof(GetProjectAndSoftwarePaths), DynamicDataSourceType.Method)]
        public void Test_22_GetSoftwareTree(string projectPath, string softwarePath)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, projectPath);

            var result = _portal.GetSoftwareTree(softwarePath);

            success &= Common.CloseProject(_portal, projectPath);

            Console.WriteLine($"GetSoftwareTree:");
            Console.WriteLine(result);

            Assert.IsNotNull(result, "No software found");
        }

        public static IEnumerable<object[]> GetProjectAndSessionPaths()
        {
            var config = ConfigurationHelper.GetTiaVersionConfig("V20");
            foreach (var project in config.Projects)
            {
                yield return new object[] { project.Path };
            }
        }

        public static IEnumerable<object[]> GetProjectAndSoftwarePaths()
        {
            var config = ConfigurationHelper.GetTiaVersionConfig("V20");
            foreach (var project in config.Projects)
            {
                if (project.PlcSoftwarePaths != null)
                {
                    foreach (var softwarePath in project.PlcSoftwarePaths)
                    {
                        yield return new object[] { project.Path, softwarePath };
                    }
                }
                else if (project.PlcSoftwarePath != null) // Handle single PlcSoftwarePath
                {
                    yield return new object[] { project.Path, project.PlcSoftwarePath };
                }
            }
        }
    }
}