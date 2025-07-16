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
                    return JsonRpcMessageWrapper.ToJson(1, "Connected to TIA-Portal successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to connect to TIA-Portal.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error connecting to TIA-Portal: {ex.Message}");
            }
        }

        [McpServerTool, Description("Check if TIA-Portal is connected")]
        public static string IsConnected()
        {
            try
            {
                bool isConnected = _portal.IsConnected();
                return JsonRpcMessageWrapper.ToJson(1, isConnected);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error checking connection: {ex.Message}");
            }
        }

        [McpServerTool, Description("Disconnect from TIA-Portal")]
        public static string DisconnectPortal()
        {
            try
            {
                _portal.DisconnectPortal();
                return JsonRpcMessageWrapper.ToJson(1, "Disconnected from TIA-Portal successfully.");
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error disconnecting from TIA-Portal: {ex.Message}");
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

                return JsonRpcMessageWrapper.ToJson(1, projects);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving open projects: {ex.Message}");
            }
        }

        [McpServerTool, Description("Open a TIA-Portal local project/session")]
        public static string OpenProject(string projectPath)
        {
            try
            {
                _portal.CloseProject();

                // get project extension
                string extension = Path.GetExtension(projectPath).ToLowerInvariant();

                // use regex to check if extension is .ap\d+ or .als\d+
                if (!Regex.IsMatch(extension, @"^\.ap\d+$") &&
                    !Regex.IsMatch(extension, @"^\.als\d+$"))
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Invalid project file extension. Use .apXX for projects or .alsXX for sessions, where XX=18,19,20,....");
                }

                bool success = false;

                if (extension.StartsWith(".ap"))
                {
                    success = _portal.OpenProject(projectPath);
                }
                if (extension.StartsWith(".als"))
                {
                    success = _portal.OpenSession(projectPath);
                }

                if (success)
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Project opened successfully");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to open project");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error opening project: {ex.Message}");
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
                        return JsonRpcMessageWrapper.ToJson(1, "Local session saved successfully");
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, false, "Failed to save local session");
                    }
                }
                else
                {
                    if (_portal.SaveProject())
                    {
                        return JsonRpcMessageWrapper.ToJson(1, "Local project saved successfully");
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, false, "Failed to save project");
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error saving local project/session: {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Cannot save local session as '{newProjectPath}'");
                }
                else
                {
                    if (_portal.SaveAsProject(newProjectPath))
                    {
                        return JsonRpcMessageWrapper.ToJson(1, $"Local project saved successfully as '{newProjectPath}'");
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to save local project as '{newProjectPath}'");
                    }
                }

            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error saving local project/session as '{newProjectPath}': {ex.Message}");
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

                    return JsonRpcMessageWrapper.ToJson(1, "Local session closed successfully");
                }
                else
                {
                    _portal.CloseProject();

                    return JsonRpcMessageWrapper.ToJson(1, "Local project closed successfully");
                }

            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error closing local project/session: {ex.Message}");
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

                return JsonRpcMessageWrapper.ToJson(1, false, structure);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving structure of current local the project/session: {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Device '{devicePath}' not found.");
                }

                return JsonRpcMessageWrapper.ToJson(1, false, device.Name);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving device '{devicePath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Device item '{deviceItemPath}' not found.");
                }

                return JsonRpcMessageWrapper.ToJson(1, false, deviceItem.Name);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving device item '{deviceItemPath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, "No devices found in the current project/session");
                }

                return JsonRpcMessageWrapper.ToJson(1, list);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving devices: {ex.Message}");
            }
        }

        #endregion

        #region plc software

        [McpServerTool, Description("Compile the plc software.")]
        public static string CompileSoftware(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath)
        {
            try
            {
                var result = _portal.CompileSoftware(softwarePath);

                if (!result.Equals("Error"))
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Software '{softwarePath}' compiled with {result}");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to compile software '{softwarePath}' with {result}");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error compiling software '{softwarePath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Block '{blockPath}' not found in software '{softwarePath}'.");
                }
                return JsonRpcMessageWrapper.ToJson(1, false, block.Name + ", " + block.Namespace + ", " + block.ProgrammingLanguage);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving block '{blockPath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, $"No blocks found in software '{softwarePath}' with regex '{regexName}'.");
                }

                return JsonRpcMessageWrapper.ToJson(1, list);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving code blocks: {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Block exported successfully from '{blockPath}' to '{exportPath}'");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to export block from '{blockPath}' to '{exportPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting block from '{blockPath}' to '{exportPath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Block imported successfully from '{importPath}' to '{groupPath}'");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to import block from '{importPath}' to '{groupPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error importing block from '{importPath}' to '{groupPath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, "Blocks exported successfully");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export blocks");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting blocks: {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Type '{typePath}' not found in software '{softwarePath}'");
                }

                return JsonRpcMessageWrapper.ToJson(1, type.Name + ", " + type.Namespace);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, $"Error retrieving user defined type '{typePath}': {ex.Message}");
            }
        }

        [McpServerTool, Description("Get a list of types from the plc software")]
        public static List<string> GetTypes(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                return _portal.GetTypes(softwarePath, regexName);
            }
            catch (Exception ex)
            {
                return [$"Error retrieving user defined types: {ex.Message}"];
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Type exported successfully from '{typePath}' to '{exportPath}'");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to export type from '{typePath}' to '{exportPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting type from '{typePath}' to '{exportPath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Type imported successfully from '{importPath}' to '{groupPath}'");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to import type from '{importPath}' to '{groupPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error importing type from '{importPath}' to '{groupPath}': {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, "Types exported successfully");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export types");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting types: {ex.Message}");
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
                    return JsonRpcMessageWrapper.ToJson(1, $"Documents exported successfully from '{blockPath}' to '{exportPath}'");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to export documents from '{blockPath}' to '{exportPath}'");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting documents from '{blockPath}' to '{exportPath}': {ex.Message}");
            }
        }

        #endregion
    }
}
