using Microsoft.Extensions.Logging;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public sealed class Test3Devices
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
                _portal.CloseSession();
            }
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetDeviceDataSource), typeof(TiaTestCases))]
        public void Test_302_GetDevice(SimpleTiaTestCase testCase, string devicePath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.GetDevice(devicePath);
            if (result != null)
            {
                Console.WriteLine($"Device: {result?.Name} found under {devicePath}");
            }
            else
            {
                Console.WriteLine($"Device not found under {devicePath}");
            }

            success &= result != null;

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Console.WriteLine($"success={success}");

            Assert.IsTrue(success, "No Device found");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, "PLC_0")]
        //[DataRow(Settings.Project1ProjectPath, "PC-System_0/Software PLC_0")]
        //[DataRow(Settings.Project1ProjectPath, "HMI_0/HMI_RT_1")]
        //[DataRow(Settings.Project1ProjectPath, "Group1/PLC_1")]
        //[DataRow(Settings.Project1ProjectPath, "Group1/PC-System_1/Software PLC_1")]
        //[DataRow(Settings.Project1ProjectPath, "Group1/Group1.1/PLC_1.1")]
        //[DataRow(Settings.Project1ProjectPath, "Group1/Group1.1/PC-System_1.1/Software PLC_1.1")]
        [DynamicData(nameof(TiaTestCases.GetDeviceItemSource), typeof(TiaTestCases))]
        public void Test_302_GetDeviceItem(SimpleTiaTestCase c, string deviceItemPath)
        {
            if (c.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {c.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, c.ProjectPath);

            var result = _portal.GetDeviceItem(deviceItemPath);
            if (result != null)
            {
                Console.WriteLine($"DeviceItem: {result?.Name} found under {deviceItemPath}");
            }
            else
            {
                Console.WriteLine($"DeviceItem not found under {deviceItemPath}");
            }

            success &= result != null;

            success &= Common.CloseProject(_portal, c.ProjectPath);

            Console.WriteLine($"success={success}");

            Assert.IsTrue(success, "No DeviceItem found");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath)]
        //[DataRow(Settings.Session1ProjectPath)]
        [DynamicData(nameof(TiaTestCases.GetTestCases), typeof(TiaTestCases))]
        public void Test_303_GetDevices(SimpleTiaTestCase c)
        {
            if (c.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {c.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, c.ProjectPath);

            var list = _portal.GetDevices();
            if (list != null)
            {
                Console.WriteLine($"Devices: {list.Count} found");

                list.ForEach(device =>
                {
                    Console.WriteLine($"Device: {device.Name}");
                });
            }
            else
            {
                Console.WriteLine($"Devices not found");
            }

            success &= list != null;

            success &= Common.CloseProject(_portal, c.ProjectPath);

            Console.WriteLine($"success={success}");

            Assert.IsTrue(success, "No Devices found");
        }
    }
}