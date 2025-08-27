using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

namespace TiaMcpServer.Test
{
    public static class TiaTestCases
    {
        public static IEnumerable<object[]> GetTestCases()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tia.simple.testcases.json");
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<SimpleTiaTestCase> testCases = JsonConvert.DeserializeObject<List<SimpleTiaTestCase>>(jsonContent);

            foreach (var testCase in testCases)
            {
                
                yield return new object[] { testCase };
                
            }
        }
        public static IEnumerable<object[]> GetDeviceDataSource()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tia.simple.testcases.json");
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<SimpleTiaTestCase> testCases = JsonConvert.DeserializeObject<List<SimpleTiaTestCase>>(jsonContent);

            foreach (var testCase in testCases)
            {
                foreach (var devicePath in testCase.DevicePaths)
                {
                    yield return new object[] { testCase, devicePath };
                }
            }
        }

        public static IEnumerable<object[]> GetDeviceItemSource()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tia.simple.testcases.json");
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<SimpleTiaTestCase> testCases = JsonConvert.DeserializeObject<List<SimpleTiaTestCase>>(jsonContent);

            foreach (var testCase in testCases)
            {
                foreach (var deviceItemPath in testCase.DeviceItemPaths)
                {
                    yield return new object[] { testCase, deviceItemPath };
                }
            }
        }
    }
}
