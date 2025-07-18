using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;
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
                    return JsonRpcMessageWrapper.ToJson(1, "Connected to TIA-Portal successfully", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Failed to connect to TIA-Portal", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error connecting to TIA-Portal: {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Check if TIA-Portal is connected")]
        public static string IsConnected()
        {
            try
            {
                bool isConnected = _portal.IsConnected();

                return JsonRpcMessageWrapper.ToJson(1, isConnected, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error checking connection: {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Disconnect from TIA-Portal")]
        public static string DisconnectPortal()
        {
            try
            {
                _portal.DisconnectPortal();
                return JsonRpcMessageWrapper.ToJson(1, "Disconnected from TIA-Portal successfully", false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error disconnecting from TIA-Portal: {ex.Message}", true);
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
                if (string.IsNullOrEmpty(state))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "TIA-Portal MCP server state\n- Failed to retrieve state", true);
                }

                return JsonRpcMessageWrapper.ToJson(1, state, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving TIA-Portal MCP server state: {ex.Message}", true);
            }
        }

        #endregion

        #region project/session

        [McpServerTool, Description("Get list of open local projects/sessions")]
        public static string GetOpenProjects()
        {
            try
            {
                var projects = _portal.GetOpenProjects();

                projects.AddRange(_portal.GetOpenSessions());

                return JsonRpcMessageWrapper.ToJson(1, projects, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving open projects: {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, "Invalid project file extension. Use .apXX for projects or .alsXX for sessions, where XX=18,19,20,....", false);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Project '{path}' opened successfully", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to open project '{path}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error opening project '{path}': {ex.Message}", true);
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
                        return JsonRpcMessageWrapper.ToJson(1, "Local session saved successfully", false);
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, "Failed to save local session", true);
                    }
                }
                else
                {
                    if (_portal.SaveProject())
                    {
                        return JsonRpcMessageWrapper.ToJson(1, "Local project saved successfully", false);
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, "Failed to save project", true);
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error saving local project/session: {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Cannot save local session as '{newProjectPath}'", true);
                }
                else
                {
                    if (_portal.SaveAsProject(newProjectPath))
                    {
                        return JsonRpcMessageWrapper.ToJson(1, $"Local project saved successfully as '{newProjectPath}'", false);
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, $"Failed to save local project as '{newProjectPath}'", true);
                    }
                }

            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error saving local project/session as '{newProjectPath}': {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Close the current TIA-Portal project/session")]
        public static string CloseProject()
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    _portal.CloseSession();

                    return JsonRpcMessageWrapper.ToJson(1, "Local session closed successfully", false);
                }
                else
                {
                    _portal.CloseProject();

                    return JsonRpcMessageWrapper.ToJson(1, "Local project closed successfully", false);
                }

            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error closing local project/session: {ex.Message}", true);
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

                return JsonRpcMessageWrapper.ToJson(1, structure, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving structure of current local the project/session: {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Get info from a device from the current project/session")]
        public static string GetDeviceInfo(
            [Description("devicePath: defines the path in the project structure to the device")] string devicePath)
        {
            try
            {
                var device = _portal.GetDevice(devicePath);

                if (device == null)
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Device '{devicePath}' not found", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, device.Name, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving device '{devicePath}': {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Get info from a device item from the current project/session")]
        public static string GetDeviceItemInfo(
            [Description("deviceItemPath: defines the path in the project structure to the device item")] string deviceItemPath)
        {
            try
            {
                var deviceItem = _portal.GetDeviceItem(deviceItemPath);

                if (deviceItem == null)
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Device item '{deviceItemPath}' not found", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, deviceItem.Name, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving device item '{deviceItemPath}': {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Get a list of all devices in the project/session")]
        public static string GetDevices()
        {
            try
            {
                var list = _portal.GetDevices();
                if (list.Count == 0)
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"No devices found in project/session", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, list, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving devices: {ex.Message}", true);
            }
        }

        #endregion

        #region plc software

        [McpServerTool, Description("Compile the plc software")]
        public static string CompileSoftware(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath)
        {
            try
            {
                var result = _portal.CompileSoftware(softwarePath);

                if (!result.Equals("Error"))
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Software '{softwarePath}' compiled with {result}", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to compile software '{softwarePath}' with {result}", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error compiling software '{softwarePath}': {ex.Message}", true);
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
                if (block == null)
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Block '{blockPath}' not found in '{softwarePath}'", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, block.Name + ", " + block.Namespace + ", " + block.ProgrammingLanguage, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving block '{blockPath}': {ex.Message}", true);
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
                if (list.Count == 0)
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"No blocks with regex '{regexName}' found in '{softwarePath}'", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, list, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving code blocks: {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Block exported successfully from '{blockPath}' to '{exportPath}'", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to export block from '{blockPath}' to '{exportPath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error exporting block from '{blockPath}' to '{exportPath}': {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Block imported successfully from '{importPath}' to '{groupPath}'", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to import block from '{importPath}' to '{groupPath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error importing block from '{importPath}' to '{groupPath}': {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Blocks '{regexName}' from '{softwarePath}' exported successfully ", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to export blocks '{regexName}' from '{softwarePath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error exporting blocks '{regexName}' from '{softwarePath}': {ex.Message}", true);
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
                if (type == null)
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Type '{typePath}' not found in '{softwarePath}'", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, type.Name + ", " + type.Namespace, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving type '{typePath}' in '{softwarePath}': {ex.Message}", true);
            }
        }

        [McpServerTool, Description("Get a list of types from the plc software")]
        public static string GetTypes(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                var types = _portal.GetTypes(softwarePath, regexName).ToArray().ToString();

                if (string.IsNullOrEmpty(types))
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"No types with regex '{regexName}' found in '{softwarePath}'", false);
                }

                return JsonRpcMessageWrapper.ToJson(1, types, false);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving user defined types: {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Type exported successfully from '{typePath}' to '{exportPath}'", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to export type from '{typePath}' to '{exportPath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error exporting type from '{typePath}' to '{exportPath}': {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Type imported successfully from '{importPath}' to '{groupPath}'", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to import type from '{importPath}' to '{groupPath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error importing type from '{importPath}' to '{groupPath}': {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Types '{regexName}' from '{softwarePath}' exported successfully", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to export types '{regexName}' from '{softwarePath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error exporting types '{regexName}' from '{softwarePath}': {ex.Message}", true);
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Documents exported successfully from '{blockPath}' to '{exportPath}'", false);
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Failed to export documents from '{blockPath}' to '{exportPath}'", true);
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error exporting documents from '{blockPath}' to '{exportPath}': {ex.Message}", true);
            }
        }

        #endregion
    }
}
