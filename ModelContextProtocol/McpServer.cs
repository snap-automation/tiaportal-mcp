using ModelContextProtocol.Server;
using Siemens.Engineering.CrossReference;
using Siemens.Engineering.HW;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.ModelContextProtocol
{
    [McpServerToolType]
    public static class McpServer
    {
        private static readonly Portal _portal = new();

        #region portal

        [McpServerTool, Description("Connect to TIA-Portal")]
        public static string ConnectPortal()
        {
            try
            {
                if (_portal.ConnectPortal())
                {
                    return JsonRpcMessage.SuccessData(1, true, "Connected to TIA-Portal");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, "Failed to connect to TIA-Portal");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, "Failed to connect to TIA-Portal", ex.Message);
            }
        }

        [McpServerTool, Description("Check if TIA-Portal is connected")]
        public static string IsConnected()
        {
            try
            {
                bool isConnected = _portal.IsConnected();

                return JsonRpcMessage.SuccessData(1, isConnected, $"IsConnected={isConnected}");
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, "Failed checking connection", ex.Message);
            }
        }

        [McpServerTool, Description("Disconnect from TIA-Portal")]
        public static string DisconnectPortal()
        {
            try
            {
                if (_portal.DisconnectPortal())
                {
                    return JsonRpcMessage.SuccessData(1, true, "Disconnected from TIA-Portal");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, "Failed disconnecting from TIA-Portal");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, "Failed disconnecting from TIA-Portal", ex.Message);
            }
        }

        #endregion

        #region status

        [McpServerTool, Description("Get the state of the TIA-Portal MCP server")]
        public static string GetState()
        {
            try
            {
                var state = _portal.GetState();

                return JsonRpcMessage.SuccessData(1, state, "TIA-Portal MCP server state");

            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, "Failed to retrieve TIA-Portal MCP server state", ex.Message);
            }
        }

        #endregion

        #region project/session

        [McpServerTool, Description("Get list of open local projects/sessions")]
        public static string GetOpenProjects()
        {
            try
            {
                var list = _portal.GetOpenProjects();

                list.AddRange(_portal.GetOpenSessions());

                return JsonRpcMessage.SuccessList<string>(1, list, "Open projects/sessions");
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, "Failed retrieving open projects", ex.Message);
            }
        }

        [McpServerTool, Description("Open a TIA-Portal local project/session")]
        public static string OpenProject(
            [Description("path: defines the path where to the project/session")] string path)
        {
            try
            {
                _portal.CloseProject();

                // get project extension
                string extension = Path.GetExtension(path).ToLowerInvariant();

                // use regex to check if extension is .ap\d+ or .als\d+
                if (!Regex.IsMatch(extension, @"^\.ap\d+$") &&
                    !Regex.IsMatch(extension, @"^\.als\d+$"))
                {
                    return JsonRpcMessage.Error(1, -32000, "Invalid project file extension. Use .apXX for projects or .alsXX for sessions, where XX=18,19,20,....");
                }

                bool success = false;

                if (extension.StartsWith(".ap"))
                {
                    success = _portal.OpenProject(path);
                }
                if (extension.StartsWith(".als"))
                {
                    success = _portal.OpenSession(path);
                }

                if (success)
                {
                    return JsonRpcMessage.SuccessData(1, success, $"Project '{path}' opened");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed to open project '{path}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed to open project '{path}'", ex.Message);
            }
        }

        [McpServerTool, Description("Save the current TIA-Portal local project/session")]
        public static string SaveProject()
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    if (_portal.SaveSession())
                    {
                        return JsonRpcMessage.SuccessData(1, true, "Local session saved");
                    }
                    else
                    {
                        return JsonRpcMessage.Error(1, -32000, "Failed to save local session");
                    }
                }
                else
                {
                    if (_portal.SaveProject())
                    {
                        return JsonRpcMessage.SuccessData(1, true, "Local project saved");
                    }
                    else
                    {
                        return JsonRpcMessage.Error(1, -32000, "Failed to save project");
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed saving local project/session", ex.Message);
            }
        }

        [McpServerTool, Description("Save current TIA-Portal project/session with a new name")]
        public static string SaveAsProject(
            [Description("newProjectPath: defines the new path where to save the project")] string newProjectPath)
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    return JsonRpcMessage.Error(1, -32000, $"Cannot save local session as '{newProjectPath}'");
                }
                else
                {
                    if (_portal.SaveAsProject(newProjectPath))
                    {
                        return JsonRpcMessage.SuccessData(1, true, $"Local project saved as '{newProjectPath}'");
                    }
                    else
                    {
                        return JsonRpcMessage.Error(1, -32000, $"Failed saving local project as '{newProjectPath}'");
                    }
                }

            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed saving local project/session as '{newProjectPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Close the current TIA-Portal project/session")]
        public static string CloseProject()
        {
            try
            {
                bool success;

                if (_portal.IsLocalSession)
                {
                    success = _portal.CloseSession();
                    if (success)
                    {
                        return JsonRpcMessage.SuccessData(1, success, "Local session closed");
                    }
                    else
                    {
                        return JsonRpcMessage.Error(1, -32000, "Failed closing local session");
                    }
                }
                else
                {
                    success = _portal.CloseProject();
                    if (success)
                    {
                        return JsonRpcMessage.SuccessData(1, success, "Local project closed");
                    }
                    else
                    {
                        return JsonRpcMessage.Error(1, -32000, "Failed closing project");
                    }
                }

            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed closing local project/session", ex.Message);
            }
        }

        #endregion

        #region devices

        [McpServerTool, Description("Get the structure of current local project/session")]
        public static string GetStructure()
        {
            try
            {
                var structure = _portal.GetStructure();

                if(!string.IsNullOrEmpty(structure))
                {
                    return JsonRpcMessage.SuccessData(1, structure, "Project structure");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, "Failed retrieving project structure");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving project structure", ex.Message);
            }
        }

        [McpServerTool, Description("Get info from a device from the current project/session")]
        public static string GetDeviceInfo(
            [Description("devicePath: defines the path in the project structure to the device")] string devicePath)
        {
            try
            {
                var device = _portal.GetDevice(devicePath);

                if (device != null)
                {
                    var info = new {
                        device.Name,
                        Description = device.ToString()
                    };
                    return JsonRpcMessage.SuccessData(1, info, $"Device info from '{devicePath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Device '{devicePath}' not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving device '{devicePath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Get info from a device item from the current project/session")]
        public static string GetDeviceItemInfo(
            [Description("deviceItemPath: defines the path in the project structure to the device item")] string deviceItemPath)
        {
            try
            {
                var deviceItem = _portal.GetDeviceItem(deviceItemPath);

                if (deviceItem != null)
                {
                    var info = new
                    {
                        deviceItem.Name,
                        Description = deviceItem.ToString()
                    };
                    return JsonRpcMessage.SuccessData(1, info, $"Device item info from '{deviceItemPath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Device item '{deviceItemPath}' not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving device item '{deviceItemPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Get a list of all devices in the project/session")]
        public static string GetDevices()
        {
            try
            {
                var list = _portal.GetDevices();
                if (list != null)
                {
                    return JsonRpcMessage.SuccessList<string>(1, list, $"Devices");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Devices in project/session not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving devices", ex.Message);
            }
        }

        #endregion

        #region plc software

        [McpServerTool, Description("Compile the plc software")]
        public static string CompileSoftware(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("password: the password to access adminsitration, default: no password")] string password = "")
        {
            try
            {
                var result = _portal.CompileSoftware(softwarePath, password);
                if (!result.Equals("Error"))
                {
                    return JsonRpcMessage.SuccessData(1, result, $"Software '{softwarePath}' compiled with {result}");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed compiling software '{softwarePath}'", result);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed compiling software '{softwarePath}'", ex.Message);
            }
        }

        #endregion

        #region blocks

        [McpServerTool, Description("Get a block info, which is located in the plc software")]
        public static string GetBlockInfo(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("blockPath: defines the path in the project structure to the block")] string blockPath)
        {
            try
            {
                var block = _portal.GetBlock(softwarePath, blockPath);
                if (block != null)
                {
                    var info = new
                    {
                        block.Name,
                        block.Namespace,
                        block.ProgrammingLanguage,
                        block.MemoryLayout,
                        block.IsConsistent,
                        block.HeaderName,
                        block.ModifiedDate,
                        block.IsKnowHowProtected,
                        Description = block.ToString()
                    };

                    return JsonRpcMessage.SuccessData(1, info, $"Block info from '{blockPath}' in '{softwarePath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Block '{blockPath}' in '{softwarePath}' not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving block '{blockPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Get a list of blocks, which are located in plc software")]
        public static string GetBlocks(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                var list = _portal.GetBlocks(softwarePath, regexName);
                if (list != null)
                {
                    return JsonRpcMessage.SuccessList<string>(1, list, $"Blocks");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Blocks with regex '{regexName}' in '{softwarePath}' not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving blocks with regex '{regexName}' in '{softwarePath}", ex.Message);
            }
        }

        [McpServerTool, Description("Export a block from plc software to file")]
        public static string ExportBlock(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("blockPath: defines the path in the project structure to the block")] string blockPath,
            [Description("exportPath: defines the path where to export the block")] string exportPath)
        {
            try
            {
                if (_portal.ExportBlock(softwarePath, blockPath, exportPath))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Block exported from '{blockPath}' to '{exportPath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed exporting block from '{blockPath}' to '{exportPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed exporting block from '{blockPath}' to '{exportPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Import a block file to plc software")]
        public static string ImportBlock(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("groupPath: defines the path in the project structure to the group, where to import the block")] string groupPath,
            [Description("importPath: defines the path of the xml file from where to import the block")] string importPath)
        {
            try
            {
                if (_portal.ImportBlock(softwarePath, groupPath, importPath))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Block imported from '{importPath}' to '{groupPath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed importing block from '{importPath}' to '{groupPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed importing block from '{importPath}' to '{groupPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Export all blocks from the plc software to path")]
        public static string ExportBlocks(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where to export the blocks")] string exportPath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                if (_portal.ExportBlocks(softwarePath, exportPath, regexName))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Blocks '{regexName}' from '{softwarePath}' exported");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed exporting blocks with '{regexName}' from '{softwarePath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed exporting blocks with '{regexName}' from '{softwarePath}'", ex.Message);
            }
        }

        #endregion

        #region types

        [McpServerTool, Description("Get a type info from the plc software")]
        public static string GetTypeInfo(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("typePath: defines the path in the project structure to the type")] string typePath)
        {
            try
            {
                var type = _portal.GetType(softwarePath, typePath);
                if (type != null)
                {
                    var info = new
                    {
                        type.Name,
                        type.Namespace,
                        type.IsConsistent,
                        type.ModifiedDate,
                        type.IsKnowHowProtected,
                        Description = type.ToString()
                    };
                    return JsonRpcMessage.SuccessData(1, info, $"Type info from '{typePath}' in '{softwarePath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Type '{typePath}' in '{softwarePath}' not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving type '{typePath}' in '{softwarePath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Get a list of types from the plc software")]
        public static string GetTypes(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                var list = _portal.GetTypes(softwarePath, regexName);

                if (list != null)
                {
                    return JsonRpcMessage.SuccessList<string>(1, list, $"Types");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Types with regex '{regexName}' in '{softwarePath}' not found");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed retrieving user defined types", ex.Message);
            }
        }

        [McpServerTool, Description("Export a type from the plc software")]
        public static string ExportType(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where export the type")] string exportPath,
            [Description("typePath: defines the path in the project structure to the type")] string typePath)
        {
            try
            {
                if (_portal.ExportType(softwarePath, typePath, exportPath))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Type exported from '{typePath}' to '{exportPath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed exporting type from '{typePath}' to '{exportPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed exporting type from '{typePath}' to '{exportPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Import a type from file into the plc software")]
        public static string ImportType(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("groupPath: defines the path in the project structure to the group, where to import the type")] string groupPath,
            [Description("importPath: defines the path of the xml file from where to import the type")] string importPath)
        {
            try
            {
                if (_portal.ImportType(softwarePath, groupPath, importPath))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Type imported from '{importPath}' to '{groupPath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed importing type from '{importPath}' to '{groupPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed importing type from '{importPath}' to '{groupPath}'", ex.Message);
            }
        }

        [McpServerTool, Description("Export types from the plc software to path")]
        public static string ExportTypes(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where to export the types")] string exportPath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                if (_portal.ExportTypes(softwarePath, exportPath, regexName))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Types '{regexName}' from '{softwarePath}' exported");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed exporting types '{regexName}' from '{softwarePath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed exporting types '{regexName}' from '{softwarePath}'", ex.Message);
            }
        }

        #endregion

        #region documents

        [McpServerTool, Description("Export as documents (.s7dcl/.s7res) from a block in the plc software to path")]
        public static string ExportAsDocuments(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("blockPath: defines the path in the project structure to the block")] string blockPath,
            [Description("exportPath: defines the path where to export the documents")] string exportPath)
        {
            try
            {
                if (_portal.ExportAsDocuments(softwarePath, blockPath, exportPath))
                {
                    return JsonRpcMessage.SuccessData(1, true, $"Documents exported from '{blockPath}' to '{exportPath}'");
                }
                else
                {
                    return JsonRpcMessage.Error(1, -32000, $"Failed exporting documents from '{blockPath}' to '{exportPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessage.Exception(1, -32000, $"Failed exporting documents from '{blockPath}' to '{exportPath}'", ex.Message);
            }
        }

        #endregion
    }
}
