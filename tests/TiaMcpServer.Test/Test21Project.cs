using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public sealed class Test21Project
    {
        private Portal? _portal;
        private readonly string _tiaVersion = "V20"; // This test focuses on V20 projects

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
        [DynamicData(nameof(GetProjectPaths), DynamicDataSourceType.Method)]
        public void Test_212_OpenProject(string path)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(path);

            Console.WriteLine($"OpenProject: {path}, result={result}");

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
        [DynamicData(nameof(GetProjectPaths), DynamicDataSourceType.Method)]
        public void Test_214_CloseProject(string path)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(path);
            result &= _portal.CloseProject();

            Console.WriteLine($"CloseProject: {path}, result={result}");

            Assert.IsTrue(result, "Failed to close project");
        }

        [TestMethod]
        [DynamicData(nameof(GetProjectPaths), DynamicDataSourceType.Method)]
        public void Test_215_SaveProject(string path)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(path);
            result &= _portal.SaveProject();

            Console.WriteLine($"SaveProject: {path}, result={result}");

            Assert.IsTrue(result, "Failed to save project");
        }

        [TestMethod]
        [DynamicData(nameof(GetProjectAndNewPaths), DynamicDataSourceType.Method)]
        public void Test_216_SaveAsProject(string path, string newPath)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var result = _portal.OpenProject(path);
            result &= _portal.SaveAsProject(newPath);

            Assert.IsTrue(result, "Failed to save project as new project");
        }

        public static IEnumerable<object[]> GetProjectPaths()
        {
            var config = ConfigurationHelper.GetTiaVersionConfig("V20");
            foreach (var project in config.Projects.Where(p => p.Type == "Local"))
            {
                yield return new object[] { project.Path };
            }
        }

        public static IEnumerable<object[]> GetProjectAndNewPaths()
        {
            var config = ConfigurationHelper.GetTiaVersionConfig("V20");
            foreach (var project in config.Projects.Where(p => p.Type == "Local" && p.NewPath != null))
            {
                yield return new object[] { project.Path, project.NewPath };
            }
        }
    }
}