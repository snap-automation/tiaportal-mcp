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

        [McpServerTool, Description("Connect to TIA Portal")]
        public static string ConnectPortal()
        {
            try
            {
                if (_portal.ConnectPortal())
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Connected to TIA Portal successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to connect to TIA Portal.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error connecting to TIA Portal: {ex.Message}");
            }
        }

        [McpServerTool, Description("Check if TIA Portal is connected")]
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

        [McpServerTool, Description("Disconnect from TIA Portal")]
        public static string DisconnectPortal()
        {
            try
            {
                _portal.DisconnectPortal();
                return JsonRpcMessageWrapper.ToJson(1, "Disconnected from TIA Portal successfully.");
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error disconnecting from TIA Portal: {ex.Message}");
            }
        }

        #endregion

        #region project/session

        [McpServerTool, Description("Get list of open projects/local sessions")]
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

        [McpServerTool, Description("Open a TIA Portal project/local session")]
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
                // switch case extension
                switch (extension)
                {
                    case ".ap20":
                        success = _portal.OpenProject(projectPath);
                        break;

                    case ".als20":
                        success = _portal.OpenSession(projectPath);
                        break;
                }

                if (success)
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Project opened successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to open project.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error opening project: {ex.Message}");
            }
        }

        [McpServerTool, Description("Save the current TIA Portal project/local session")]
        public static string SaveProject()
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    if (_portal.SaveSession())
                    {
                        return JsonRpcMessageWrapper.ToJson(1, "Local session saved successfully.");
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, false, "Failed to save local session.");
                    }
                }
                else 
                {
                    if (_portal.SaveProject())
                    {
                        return JsonRpcMessageWrapper.ToJson(1, "Project saved successfully.");
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, false, "Failed to save project.");
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error saving project/local session: {ex.Message}");
            }
        }

        [McpServerTool, Description("Save the current TIA Portal project with a new name")]
        public static string SaveAsProject(string newProjectPath)
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Cannot save local session as {newProjectPath}.");
                }
                else
                {
                    if (_portal.SaveAsProject(newProjectPath))
                    {
                        return JsonRpcMessageWrapper.ToJson(1, $"Project saved successfully as {newProjectPath}.");
                    }
                    else
                    {
                        return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to save project as {newProjectPath}.");
                    }
                }
                
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error saving project/local session as {newProjectPath}: {ex.Message}");
            }
        }

        [McpServerTool, Description("Close the current TIA Portal project")]
        public static string CloseProject()
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    _portal.CloseSession();

                    return JsonRpcMessageWrapper.ToJson(1, "Local session closed successfully.");
                }
                else
                {
                    _portal.CloseProject();

                    return JsonRpcMessageWrapper.ToJson(1, "Project closed successfully.");
                }
                
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error closing project/local session: {ex.Message}");
            }
        }

        #endregion

        #region devices

        [McpServerTool, Description("Get the structure of the current project/local session")]
        public static string GetStructure()
        {
            try
            {
                return _portal.GetStructure();
            }
            catch (Exception ex)
            {
                return $"Error retrieving structure of the project/local session: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get a list of devices in the project")]
        public static List<string> GetDevices()
        {
            try
            {
                return _portal.GetDevices();
            }
            catch (Exception ex)
            {
                return [$"Error retrieving devices: {ex.Message}"];
            }
        }

        [McpServerTool, Description("Get a device by its path in the project")]
        public static string GetDevice(string devicePath)
        {
            try
            {
                var device = _portal.GetDevice(devicePath);

                if (device == null)
                {
                    return $"Device '{devicePath}' not found.";
                }

                return device.Name;
            }
            catch (Exception ex)
            {
                return $"Error retrieving device: {ex.Message}";
            }
        }

        #endregion

        #region plc software

        [McpServerTool, Description("Compile the software, which is given by softwarePath")]
        public static string CompileSoftware(string softwarePath)
        {
            try
            {
                var result = _portal.CompileSoftware(softwarePath);

                if (!result.Equals("Error"))
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Software compiled with {result}.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, $"Failed to compile software with {result}");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error compiling software: {ex.Message}");
            }
        }

        #endregion

        #region code blocks

        [McpServerTool, Description("Get a code block (FB, FC, OB) from the software, which is given by softwarePath/groupPath/blockName.")]
        public static string GetCodeBlock(string softwarePath, string groupPath, string blockName)
        {
            try
            {
                return _portal.GetCodeBlock(softwarePath, groupPath, blockName);
            }
            catch (Exception ex)
            {
                return $"Error retrieving data block: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get a list of all code blocks (FB, FC, OB) from the software, which is given by softwarePath.")]
        public static List<string> GetCodeBlocks(string softwarePath)
        {
            try
            {
                return _portal.GetCodeBlocks(softwarePath);
            }
            catch (Exception ex)
            {
                return [$"Error retrieving code blocks: {ex.Message}"];
            }
        }

        [McpServerTool, Description("Export a code block (FB, FC, OB) given by softwarePath/groupPath/blockName to a specified exportPath.")]
        public static string ExportCodeBlock(string softwarePath, string groupPath, string blockName, string exportPath)
        {
            try
            {
                if (_portal.ExportCodeBlock(softwarePath, groupPath, blockName, exportPath))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Code block exported successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export code block.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting code block: {ex.Message}");
            }
        }

        [McpServerTool, Description("Export all code blocks (FB, FC, OB) from the software, given by softwarePath, to a specified path.")]
        public static string ExportCodeBlocks(string softwarePath, string exportPath)
        {
            try
            {
                if (_portal.ExportCodeBlocks(softwarePath, exportPath))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Code blocks exported successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export code blocks.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting code blocks: {ex.Message}");
            }
        }

        #endregion

        #region data blocks

        [McpServerTool, Description("Get a data block (InstanceDB, GlobalDB, ArrayDB), given by softwarePath/groupPath/blockName.")]
        public static string GetDataBlock(string softwarePath, string groupPath, string blockName)
        {
            try
            {
                return _portal.GetDataBlock(softwarePath, groupPath, blockName);
            }
            catch (Exception ex)
            {
                return $"Error retrieving data block: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get list of data blocks (InstanceDB, GlobalDB, ArrayDB) from software, given by softwarePath, in the project.")]
        public static List<string> GetDataBlocks(string softwarePath)
        {
            try
            {
                return _portal.GetDataBlocks(softwarePath);
            }
            catch (Exception ex)
            {
                return [$"Error retrieving data blocks: {ex.Message}"];
            }
        }

        [McpServerTool, Description("Export a data block given by softwarePath/groupPath/blockName to a specified exportPath (InstanceDB, GlobalDB, ArrayDB).")]
        public static string ExportDataBlock(string softwarePath, string groupPath, string blockName, string exportPath)
        {
            try
            {
                if (_portal.ExportDataBlock(softwarePath, groupPath, blockName, exportPath))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Data block exported successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export data block.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting data block: {ex.Message}");
            }
        }

        [McpServerTool, Description("Export all data blocks (InstanceDB, GlobalDB, ArrayDB) from the software, given by softwarePath, to a specified path.")]
        public static string ExportDataBlocks(string softwarePath, string exportPath)
        {
            try
            {
                if (_portal.ExportDataBlocks(softwarePath, exportPath))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "Data blocks exported successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export data blocks.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting data blocks: {ex.Message}");
            }
        }

        #endregion

        #region types

        [McpServerTool, Description("Get a user defined type (UDT) from the software, which is given by softwarePath.")]
        public static string GetUserDefinedType(string softwarePath, string typeName)
        {
            try
            {
                return _portal.GetUserDefinedType(softwarePath, typeName);
            }
            catch (Exception ex)
            {
                return $"Error retrieving user defined type: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get a list of all user defined types (UDT) from the software, which is given by softwarePath.")]
        public static List<string> GetUserDefinedTypes(string softwarePath)
        {
            try
            {
                return _portal.GetUserDefinedTypes(softwarePath);
            }
            catch (Exception ex)
            {
                return [$"Error retrieving user defined types: {ex.Message}"];
            }
        }

        [McpServerTool, Description("Export a user defined types (UDT) by name from the software, which is giveb by softwarePath.")]
        public static string ExportUserDefinedType(string softwarePath, string exportPath, string typeName)
        {
            try
            {
                if (_portal.ExportUserDefinedType(softwarePath, exportPath, typeName))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "User defined type exported successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export user defined type.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting user defined type: {ex.Message}");
            }
        }

        [McpServerTool, Description("Export all user defined types (UDT) from the software, which is giveb by softwarePath.")]
        public static string ExportUserDefinedTypes(string softwarePath, string exportPath)
        {
            try
            {
                if (_portal.ExportUserDefinedTypes(softwarePath, exportPath))
                {
                    return JsonRpcMessageWrapper.ToJson(1, "User defined types exported successfully.");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to export user defined types.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error exporting user defined types: {ex.Message}");
            }
        }

        #endregion
    }
}
