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
        public void Test_302_GetDevice(SimpleTiaTestCase testCase)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool overallSuccess = true;

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);
            overallSuccess &= success;

            if (success)
            {
                foreach (var devicePath in testCase.DevicePaths)
                {
                    var result = _portal.GetDevice(devicePath);
                    if (result != null)
                    {
                        Console.WriteLine($"Device: {result?.Name} found under {devicePath}");
                    }
                    else
                    {
                        Console.WriteLine($"Device not found under {devicePath}");
                    }
                    overallSuccess &= result != null;
                }
            }

            success = Common.CloseProject(_portal, testCase.ProjectPath);
            overallSuccess &= success;

            Console.WriteLine($"overallSuccess={overallSuccess}");

            Assert.IsTrue(overallSuccess, "One or more devices not found");
        }

        [TestMethod]
        //Alimentar simpletiatestcase json com todas informa'~oes originais do projeto v20
        //alimentar com todas atualizacoes do alexandro p/ v18
        //ajustar os nomes (tirar test e simple)
        //garantir a filtragem via hard code (default = 18)
        //criar uma segunda test case com um segundo projeto v18.
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