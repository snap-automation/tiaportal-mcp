using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq; // Needed for .Where()

namespace TiaMcpServer.Test
{
    public static class TiaTestCases
    {
        private static List<SimpleTiaTestCase> GetTestCasesByVersion()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tia.simple.testcases.json");
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<SimpleTiaTestCase> testCases = JsonConvert.DeserializeObject<List<SimpleTiaTestCase>>(jsonContent);

            int tiaMajorVersion = 20; // Default value
            string envVersion = Environment.GetEnvironmentVariable("TIA_MCP_TEST_VERSION");
            if (!string.IsNullOrEmpty(envVersion) && int.TryParse(envVersion, out int parsedVersion))
            {
                tiaMajorVersion = parsedVersion;
            }

            return testCases.Where(tc => tc.Version == tiaMajorVersion).ToList();
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                yield return new object[] { testCase };
            }
        }

        public static IEnumerable<object[]> GetDeviceDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.DevicePaths != null)
                {
                    foreach (var devicePath in testCase.DevicePaths)
                    {
                        yield return new object[] { testCase, devicePath };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetDeviceItemSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.DeviceItemPaths != null)
                {
                    foreach (var deviceItemPath in testCase.DeviceItemPaths)
                    {
                        yield return new object[] { testCase, deviceItemPath };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetPlcSoftwareSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.PlcSoftware != null)
                {
                    foreach (var plcSoftware in testCase.PlcSoftware)
                    {
                        yield return new object[] { testCase, plcSoftware };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetBlockDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.PlcSoftware != null && testCase.BlockPaths != null)
                {
                    foreach (var blockPathInfo in testCase.BlockPaths)
                    {
                        var plcSoftware = testCase.PlcSoftware.FirstOrDefault(p => p.Path == blockPathInfo.PlcSoftwarePath);
                        if (plcSoftware != null)
                        {
                            yield return new object[] { testCase, plcSoftware, blockPathInfo.Path };
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetTypeDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.PlcSoftware != null && testCase.TypePaths != null)
                {
                    foreach (var typePathInfo in testCase.TypePaths)
                    {
                        var plcSoftware = testCase.PlcSoftware.FirstOrDefault(p => p.Path == typePathInfo.PlcSoftwarePath);
                        if (plcSoftware != null)
                        {
                            yield return new object[] { testCase, plcSoftware, typePathInfo.Path };
                        }
                    }
                }
            }
        }
    }
}