﻿using System;
using System.Linq;
using TiaMcpServer.ModelContextProtocol;
using TiaMcpServer.Siemens;
using TiaMcpServer.Test.DynamicData;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public class Test5McpServer
    {
        [TestInitialize]
        public void ClassInit()
        {
            var response = McpServer.Connect();
        }

        [TestCleanup]
        public void ClassCleanup()
        {
            McpServer.Disconnect();
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath)]
        //[DataRow(Settings.Session1ProjectPath)]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_500_McpServer_OpenCloseProject(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.CloseProject();
            WriteMessage("McpServer.CloseProject()", response);

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath)]
        //[DataRow(Settings.Session1ProjectPath)]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_501_McpServer_GetProjectTree(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetProjectTree();
            WriteMessage("McpServer.GetProjectTree()", response);

            Console.WriteLine("Structure: " + response?.Tree);

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath)]
        //[DataRow(Settings.Session1ProjectPath)]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_502_McpServer_GetState(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetState();
            WriteMessage("McpServer.GetState()", response);

            Console.WriteLine("GetState: Project = '" + response?.Project + "'");
            Console.WriteLine("GetState: Session = '" + response?.Session + "'");
            Console.WriteLine("GetState: IsConnected = '" + response?.IsConnected + "'");

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath)]
        //[DataRow(Settings.Session1ProjectPath)]
        public void Test_503_McpServer_GetProjects()
        {

            McpServer.Connect();
            //McpServer.OpenProject(projectPath);

            var success = true;
            var response = McpServer.GetProjects();

            if (response == null)
            {
                Console.WriteLine("McpServer.GetProjects(): response is null");
            }
            else
            {
                WriteMessage($"McpServer.GetProjects()", response);

                if (response.Items != null)
                {
                    response.Items.ToList().ForEach(item =>
                    {
                        Console.WriteLine($"Project {item.Name}");
                        WriteAttributes(item);
                    });
                }
                else
                {
                    Console.WriteLine("McpServer.GetProjects(): no attributes found");
                }
            }

            //McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath)]
        //[DataRow(Settings.Session1ProjectPath)]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_504_McpServer_GetDevices(SimpleTiaTestCase testCase)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetDevices();
            WriteMessage("McpServer.GetDevices()", response);

            Console.WriteLine("GetDevices:");
            response?.Items?.ToList().ForEach(item => Console.WriteLine($"- '{item}'"));

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0)]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath)]
        [DynamicData(nameof(TiaTestCases.GetPlcSoftwareSource), typeof(TiaTestCases))]
        public void Test_505_McpServer_GetSoftwareInfo(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftwareInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetSoftwareInfo(plcSoftwareInfo.Path);
            WriteMessage("McpServer.GetSoftwareInfo()", response);

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, "HMI_0")]
        //[DataRow(Settings.Session1ProjectPath, "PC-System_1")]
        [DynamicData(nameof(TiaTestCases.GetDeviceDataSource), typeof(TiaTestCases))]
        public void Test_506_McpServer_GetDeviceInfo(SimpleTiaTestCase testCase, string devicePath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetDeviceInfo(devicePath);
            WriteMessage("McpServer.GetDeviceInfo()", response);

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, "PC-System_0/PLC_0")]
        //[DataRow(Settings.Session1ProjectPath, "PC-System_1/Software PLC_1")]
        [DynamicData(nameof(TiaTestCases.GetDeviceItemSource), typeof(TiaTestCases))]
        public void Test_507_McpServer_GetDeviceItemInfo(SimpleTiaTestCase testCase, string deviceItemPath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetDeviceItemInfo(deviceItemPath);
            WriteMessage("McpServer.GetDeviceItemInfo()", response);

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }


        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "0_OBs/Main_1")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "0_OBs/Main_1")]
        [DynamicData(nameof(TiaTestCases.GetBlockDataSource), typeof(TiaTestCases))]
        public void Test_508_McpServer_GetBlockInfo(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftwareInfo, string blockPath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetBlockInfo(plcSoftwareInfo.Path, blockPath);
            WriteMessage("McpServer.GetBlockInfo()", response);

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }


        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "Common/CarrierRegister/ML_SubstratState")]
        [DynamicData(nameof(TiaTestCases.GetTypeDataSource), typeof(TiaTestCases))]
        public void Test_509_McpServer_GetTypeInfo(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftwareInfo, string typePath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            McpServer.Connect();
            McpServer.OpenProject(testCase.ProjectPath);

            var success = true;
            var response = McpServer.GetTypeInfo(plcSoftwareInfo.Path, typePath);
            WriteMessage("McpServer.GetTypeInfo()", response);

            McpServer.CloseProject();

            Assert.IsTrue(success, "McpServer failed");
        }

        private static void WriteMessage(string method, ResponseMessage? rm)
        {
            if (rm == null)
            {
                Console.WriteLine($"{method}: content is null");
            }
            else
            {

                Console.WriteLine($"{method}: {rm.Message}");
            }
        }

        private static void WriteAttributes(ResponseAttributes? ra)
        {
            if (ra != null && ra.Attributes != null)
            {
                foreach (var attribute in ra.Attributes)
                {
                    Console.WriteLine($"- {attribute.Name}: {attribute.Value}");
                }
            }
            else
            {
                Console.WriteLine($"no attributes found");
            }
        }


    }
}