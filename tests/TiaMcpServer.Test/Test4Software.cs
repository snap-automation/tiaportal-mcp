using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test
{
    [TestClass]
    [DoNotParallelize]
    public class Test4Software
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
            // ...
        }

        #region plc software

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath1, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath2, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath3, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath4, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath5, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath6, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath7, "")]
        //[DataRow(Settings.Project2ProjectPath, Settings.Project2PlcSoftwarePath, "")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "Safety1st")]
        [DynamicData(nameof(TiaTestCases.GetPlcSoftwareSource), typeof(TiaTestCases))]
        public void Test_400_CompilePlcSoftware(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftware)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            var state = "";
            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            // Pass an empty string instead of null to satisfy the non-nullable reference type requirement.
            var result = _portal.CompileSoftware(plcSoftware.Path, plcSoftware.Password);
            if (result != null)
            {
                state = result.State.ToString();
                success &= true;
            }
            else
            {
                state = "Error";
                success &= false;
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Console.WriteLine($"CompilePlcSoftware: result={state}");

            Assert.IsFalse(state.Equals("Error"), "Compile PlcSoftware failed");
        }

        

        #endregion

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "01 - Organizations Blocks/Main")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "0_OBs/Main_1")]
        [DynamicData(nameof(TiaTestCases.GetBlockDataSource), typeof(TiaTestCases))]
        public void Test_411_GetBlock(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftware, string blockPath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.GetBlock(plcSoftware.Path, blockPath);

            if (result != null && blockPath.Contains(result.Name, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Block found: '{blockPath}");
            }
            else
            {
                Console.WriteLine($"Code Block not found. Expected: '{blockPath}'");
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);


            Assert.IsNotNull(result, "No code block found");
        }

        [TestMethod]
                [DynamicData(nameof(TiaTestCases.GetTypeDataSource), typeof(TiaTestCases))]
        public void Test_412_GetType(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftware, string typePath)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.GetType(plcSoftware.Path, typePath);

            if (result != null && typePath.Contains(result.Name, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Type found: '{typePath}'");
            }
            else
            {
                Console.WriteLine($"Type not found. Expected: '{typePath}'");
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsNotNull(result, "No types");
        }
        public void Test_412_GetType(string projectPath, string softwarePath, string typePath)
        {
            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, projectPath);

            var result = _portal.GetType(softwarePath, typePath);

            if (result != null && typePath.Contains(result.Name, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Type found: '{typePath}");
            }
            else
            {
                Console.WriteLine($"Type not found. Expected: '{typePath}'");
            }

            success &= Common.CloseProject(_portal, projectPath);

            Assert.IsNotNull(result, "No types");
        }

                [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "^M.+")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath1, "")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath2, "")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "")]
        [DynamicData(nameof(TiaTestCases.GetBlocksDataSource), typeof(TiaTestCases))]
        public void Test_413_GetBlocks(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftware, string regexName)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            // If the project failed to open, fail the test immediately.
            if (!success)
            {
                Assert.Fail($"Failed to open project at: {testCase.ProjectPath}. Another project might already be open.");
            }

            var result = _portal.GetBlocks(plcSoftware.Path, regexName);

            // write list to console
            Console.WriteLine("Blocks:");
            foreach (var block in result)
            {
                try
                {
                    Console.WriteLine($"- {block.GetType().Name}, {block.Name}, IsConsistent='{block.IsConsistent}', MemoryLayout='{block.MemoryLayout}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing block: {ex.Message}");
                }
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsNotNull(result, "No blocks found");
        }

                [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "DataTyp.+")]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, "ErrTyp.+")]
        [DynamicData(nameof(TiaTestCases.GetTypesDataSource), typeof(TiaTestCases))]
        public void Test_414_GetTypes(SimpleTiaTestCase testCase, PlcSoftwareInfo plcSoftware, string regexName)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.GetTypes(plcSoftware.Path, regexName);

            // write list to console
            Console.WriteLine("Types:");
            foreach (var type in result)
            {
                Console.WriteLine($"- {type.GetType().Name}, {type.Name}");
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsNotNull(result, "No types");
        }

                [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "0_OBs/Main_1", true)]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "0_OBs/Main_1", false)]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "1_Tests/FC_Block_1", true)]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "1_Tests/DB_Block_1", true)]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, Settings.Session1ExportPath, "0_OBs/Main_1", true)]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, Settings.Session1ExportPath, "0_OBs/Main_1", false)]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "Common/CarrierRegister/GLOBAL_POSITIONING", true)]
        [DynamicData(nameof(TiaTestCases.GetExportBlockDataSource), typeof(TiaTestCases))]
        public void Test_415_ExportBlock(SimpleTiaTestCase testCase, ExportBlockInfo exportBlockInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ExportBlock(exportBlockInfo.softwarePath, exportBlockInfo.blockPath, exportBlockInfo.exportPath, exportBlockInfo.preservePath);

            if (result != null)
            {
                Console.WriteLine($"Exported Block: {result.GetType().Name}, {result.Name}, SoftwarePath: {exportBlockInfo.softwarePath}, BlockPath: {exportBlockInfo.blockPath}, ExportPath: {exportBlockInfo.exportPath}, PreservePath: {exportBlockInfo.preservePath}");
                success &= true;
            }
            else
            {
                Console.WriteLine("No block exported.");
                success &= false;
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(success, "Failed to export code block");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "0_OBs", Settings.Project1ExportPath0 + "\\0_OBs\\Main_1.xml")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "1_Tests", Settings.Project1ExportPath0 + "\\1_Tests\\FC_Block_1.xml")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "1_Tests", Settings.Project1ExportPath0 + "\\1_Tests\\DB_Block_1.xml")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "Common/CarrierRegister", Settings.Project1ExportPath0 + "\\Common\\CarrierRegister\\GLOBAL_POSITIONING.xml")]
        [DynamicData(nameof(TiaTestCases.GetImportBlockDataSource), typeof(TiaTestCases))]
        public void Test_415_ImportBlock(SimpleTiaTestCase testCase, ImportBlockInfo importBlockInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ImportBlock(importBlockInfo.softwarePath, importBlockInfo.groupPath, importBlockInfo.importPath);

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(result, "Failed to import code block");
        }
        

                [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "Common/CarrierRegister/ML_SubstratState", true)]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "Common/CarrierRegister/ML_CarrierRegisterShort", true)]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, Settings.Project1ExportPath0, "Common/CarrierRegister/ML_CarrierRegisterShort", false)]
        //[DataRow(Settings.Session1ProjectPath, Settings.Session1PlcSoftwarePath, Settings.Session1ExportPath, "Common/CarrierRegister/ML_SubstratState")]
        [DynamicData(nameof(TiaTestCases.GetExportTypeDataSource), typeof(TiaTestCases))]
        public void Test_416_ExportType(SimpleTiaTestCase testCase, ExportTypeInfo exportTypeInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ExportType(exportTypeInfo.softwarePath, exportTypeInfo.typePath, exportTypeInfo.exportPath, exportTypeInfo.preservePath);

            if (result != null)
            {
                Console.WriteLine($"Exported Type: {result.GetType().Name}, {result.Name}");
                success &= true;
            }
            else
            {
                Console.WriteLine("No type exported.");
                success &= false;
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(success, "Failed to export types");
        }

        [TestMethod]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "Common/CarrierRegister", Settings.Project1ExportPath0 + "\\Common\\CarrierRegister\\ML_SubstratState.xml")]
        //[DataRow(Settings.Project1ProjectPath, Settings.Project1PlcSoftwarePath0, "Common/CarrierRegister", Settings.Project1ExportPath0 + "\\Common\\CarrierRegister\\ML_CarrierRegisterShort.xml")]
        [DynamicData(nameof(TiaTestCases.GetImportTypeDataSource), typeof(TiaTestCases))]
             public void Test_416_ImportType(SimpleTiaTestCase testCase, ImportTypeInfo importTypeInfo)
            {
                 if (testCase.Version != Engineering.TiaMajorVersion)
                     Assert.Inconclusive($"Skipping test for version {testCase.Version}.");
    
                if (_portal == null)
                {
                    Assert.Fail("TiaPortal instance is not initialized");
                }
   
                bool success = Common.OpenProject(_portal, testCase.ProjectPath);
   
                var result = _portal.ImportType(importTypeInfo.softwarePath, importTypeInfo.groupPath, importTypeInfo.importPath);
   
                success &= Common.CloseProject(_portal, testCase.ProjectPath);
   
                Assert.IsTrue(result, "Failed to import types");
            }

[TestMethod]
        [DynamicData(nameof(TiaTestCases.GetExportBlocksDataSource), typeof(TiaTestCases))]
        public void Test_417_ExportBlocks(SimpleTiaTestCase testCase, ExportBlocksInfo exportBlocksInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ExportBlocks(exportBlocksInfo.softwarePath, exportBlocksInfo.exportPath, exportBlocksInfo.regexName, exportBlocksInfo.preservePath);

            if (result != null)
            {
                Console.WriteLine($"Exported Block:");
                foreach (var block in result)
                {
                    Console.WriteLine($"- {block.GetType().Name}, {block.Name}");
                }

                success &= true;
            }
            else
            {
                Console.WriteLine("No blocks exported.");

                success &= false;
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(success, "Failed to export blocks");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetExportTypesDataSource), typeof(TiaTestCases))]
        public void Test_418_ExportTypes(SimpleTiaTestCase testCase, ExportTypesInfo exportTypesInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ExportTypes(exportTypesInfo.softwarePath, exportTypesInfo.exportPath, exportTypesInfo.regexName, exportTypesInfo.preservePath);
            if (result != null)
            {
                Console.WriteLine($"Exported Types:");
                foreach (var type in result)
                {
                    Console.WriteLine($"- {type.GetType().Name}, {type.Name}");
                }

                success &= true;
            }
            else
            {
                Console.WriteLine("No types exported.");

                success &= false;
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(success, "Failed to export types");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetExportBlockAsDocumentsDataSource), typeof(TiaTestCases))]
        public void Test_421_ExportBlockAsDocuments(SimpleTiaTestCase testCase, ExportBlockAsDocumentsInfo exportBlockAsDocumentsInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ExportAsDocuments(exportBlockAsDocumentsInfo.softwarePath, exportBlockAsDocumentsInfo.blockPath, exportBlockAsDocumentsInfo.exportPath, exportBlockAsDocumentsInfo.preservePath);

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(result, "Failed to export blocks as documents (s7dcl/s7res)");
        }

        [TestMethod]
        [DynamicData(nameof(TiaTestCases.GetExportBlocksAsDocumentsDataSource), typeof(TiaTestCases))]
        public void Test_422_ExportBlocksAsDocuments(SimpleTiaTestCase testCase, ExportBlocksAsDocumentsInfo exportBlocksAsDocumentsInfo)
        {
            if (testCase.Version != Engineering.TiaMajorVersion)
                Assert.Inconclusive($"Skipping test for version {testCase.Version}.");

            if (_portal == null)
            {
                Assert.Fail("TiaPortal instance is not initialized");
            }

            bool success = Common.OpenProject(_portal, testCase.ProjectPath);

            var result = _portal.ExportBlocksAsDocuments(exportBlocksAsDocumentsInfo.softwarePath, exportBlocksAsDocumentsInfo.exportPath, exportBlocksAsDocumentsInfo.regexName, exportBlocksAsDocumentsInfo.preservePath);

            if (result != null)
            {
                Console.WriteLine($"Exported Block as documents:");

                foreach (var block in result)
                {
                    Console.WriteLine($"- {block.GetType().Name}, {block.Name}, {block.ModifiedDate}");
                }

                success &= true;
            }
            else
            {
                Console.WriteLine("No blocks exported as documents.");

                success &= false;
            }

            success &= Common.CloseProject(_portal, testCase.ProjectPath);

            Assert.IsTrue(success, "Failed to export blocks as documents");
        }
    }
}
