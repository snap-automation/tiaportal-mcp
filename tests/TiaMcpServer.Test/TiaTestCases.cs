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

        public static IEnumerable<object[]> GetBlocksDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.PlcSoftware != null && testCase.BlocksPaths != null)
                {
                    foreach (var blocksPathInfo in testCase.BlocksPaths)
                    {
                        var plcSoftware = testCase.PlcSoftware.FirstOrDefault(p => p.Path == blocksPathInfo.PlcSoftwarePath);
                        if (plcSoftware != null)
                        {
                            yield return new object[] { testCase, plcSoftware, blocksPathInfo.Regex };
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetTypesDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.PlcSoftware != null && testCase.TypesPaths != null)
                {
                    foreach (var typesPathInfo in testCase.TypesPaths)
                    {
                        var plcSoftware = testCase.PlcSoftware.FirstOrDefault(p => p.Path == typesPathInfo.PlcSoftwarePath);
                        if (plcSoftware != null)
                        {
                            yield return new object[] { testCase, plcSoftware, typesPathInfo.Regex };
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetExportBlockDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ExportBlock != null)
                {
                    foreach (var exportBlockInfo in testCase.ExportBlock)
                    {
                        yield return new object[] { testCase, exportBlockInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetImportBlockDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ImportBlock != null)
                {
                    foreach (var importBlockInfo in testCase.ImportBlock)
                    {
                        yield return new object[] { testCase, importBlockInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetExportTypeDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ExportType != null)
                {
                    foreach (var exportTypeInfo in testCase.ExportType)
                    {
                        yield return new object[] { testCase, exportTypeInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetImportTypeDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ImportType != null)
                {
                    foreach (var importTypeInfo in testCase.ImportType)
                    {
                        yield return new object[] { testCase, importTypeInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetExportBlocksDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ExportBlocks != null)
                {
                    foreach (var exportBlocksInfo in testCase.ExportBlocks)
                    {
                        yield return new object[] { testCase, exportBlocksInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetExportTypesDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ExportTypes != null)
                {
                    foreach (var exportTypesInfo in testCase.ExportTypes)
                    {
                        yield return new object[] { testCase, exportTypesInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetExportBlockAsDocumentsDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ExportBlockAsDocuments != null)
                {
                    foreach (var exportBlockAsDocumentsInfo in testCase.ExportBlockAsDocuments)
                    {
                        yield return new object[] { testCase, exportBlockAsDocumentsInfo };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetExportBlocksAsDocumentsDataSource()
        {
            var filteredTestCases = GetTestCasesByVersion();
            foreach (var testCase in filteredTestCases)
            {
                if (testCase.ExportBlocksAsDocuments != null)
                {
                    foreach (var exportBlocksAsDocumentsInfo in testCase.ExportBlocksAsDocuments)
                    {
                        yield return new object[] { testCase, exportBlocksAsDocumentsInfo };
                    }
                }
            }
        }

    }
}
    
