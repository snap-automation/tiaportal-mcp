using System;
using System.Collections.Generic;
using System.ComponentModel;
using ModelContextProtocol.Server;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.ModelContextProtocol
{
    [McpServerToolType]
    public static class McpServer
    {
        private static readonly Portal _portal = new Portal();

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

        #region project

        [McpServerTool, Description("Get list of open projects")]
        public static string GetOpenProjects()
        {
            try
            {
                var projects = _portal.GetOpenProjects();
                return JsonRpcMessageWrapper.ToJson(1, projects);
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error retrieving open projects: {ex.Message}");
            }
        }

        [McpServerTool, Description("Open a TIA Portal project")]
        public static string OpenProject(string projectPath)
        {
            try
            {
                if (_portal.OpenProject(projectPath))
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

        [McpServerTool, Description("Save the current TIA Portal project")]
        public static string SaveProject()
        {
            try
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
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error saving project: {ex.Message}");
            }
        }

        [McpServerTool, Description("Save the current TIA Portal project with a new name")]
        public static string SaveAsProject(string newProjectPath)
        {
            try
            {
                if (_portal.SaveAsProject(newProjectPath))
                {
                    return JsonRpcMessageWrapper.ToJson(1, $"Project successfully saved as [{newProjectPath}].");
                }
                else
                {
                    return JsonRpcMessageWrapper.ToJson(1, false, "Failed to save project as.");
                }
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error saving project as: {ex.Message}");
            }
        }

        [McpServerTool, Description("Close the current TIA Portal project")]
        public static string CloseProject()
        {
            try
            {
                _portal.CloseProject();
                return JsonRpcMessageWrapper.ToJson(1, "Project closed successfully.");
            }
            catch (Exception ex)
            {
                return JsonRpcMessageWrapper.ToJson(1, false, $"Error closing project: {ex.Message}");
            }
        }

        #endregion

        #region devices

        [McpServerTool, Description("Get list of devices in the project")]
        public static List<string> GetDevices()
        {
            try
            {
                return _portal.GetDevices();
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error retrieving devices: {ex.Message}" };
            }
        }

        #endregion

        #region code blocks

        [McpServerTool, Description("Get code block by groupPath/blockName in the project (FB, FC, OB).")]
        public static string GetCodeBlock(string groupPath, string blockName)
        {
            try
            {
                return _portal.GetCodeBlock(groupPath, blockName);
            }
            catch (Exception ex)
            {
                return $"Error retrieving data block: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get list of code blocks in the project (FB, FC, OB).")]
        public static List<string> GetCodeBlocks()
        {
            try
            {
                return _portal.GetCodeBlocks();
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error retrieving code blocks: {ex.Message}" };
            }
        }

        [McpServerTool, Description("Export a code block by groupPath/blockName to a specified exportPath (FB, FC, OB).")]
        public static string ExportCodeBlock(string groupPath, string blockName, string exportPath)
        {
            try
            {
                if (_portal.ExportCodeBlock(groupPath, blockName, exportPath))
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

        [McpServerTool, Description("Export all code blocks to a specified path (FB, FC, OB).")]
        public static string ExportCodeBlocks(string exportPath)
        {
            try
            {
                if (_portal.ExportCodeBlocks(exportPath))
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

        [McpServerTool, Description("Get data block by groupPath/blockName in the project (InstanceDB, GlobalDB, ArrayDB).")]
        public static string GetDataBlock(string groupPath, string blockName)
        {
            try
            {
                return _portal.GetDataBlock(groupPath, blockName);
            }
            catch (Exception ex)
            {
                return $"Error retrieving data block: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get list of data blocks in the project (InstanceDB, GlobalDB, ArrayDB).")]
        public static List<string> GetDataBlocks()
        {
            try
            {
                return _portal.GetDataBlocks();
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error retrieving data blocks: {ex.Message}" };
            }
        }

        [McpServerTool, Description("Export a data block by groupPath/blockName to a specified exportPath (InstanceDB, GlobalDB, ArrayDB).")]
        public static string ExportDataBlock(string groupPath, string blockName, string exportPath)
        {
            try
            {
                if (_portal.ExportDataBlock(groupPath, blockName, exportPath))
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

        [McpServerTool, Description("Export all data blocks to a specified path (InstanceDB, GlobalDB, ArrayDB).")]
        public static string ExportDataBlocks(string exportPath)
        {
            try
            {
                if (_portal.ExportDataBlocks(exportPath))
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

        [McpServerTool, Description("Get a user defined type (UDT)")]
        public static string GetUserDefinedType(string typeName)
        {
            try
            {
                return _portal.GetUserDefinedType(typeName);
            }
            catch (Exception ex)
            {
                return $"Error retrieving user defined type: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get list of user defined types (UDT)")]
        public static List<string> GetUserDefinedTypes()
        {
            try
            {
                return _portal.GetUserDefinedTypes();
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error retrieving user defined types: {ex.Message}" };
            }
        }

        [McpServerTool, Description("Export a user defined types (UDT) by name")]
        public static string ExportUserDefinedType(string exportPath, string typeName)
        {
            try
            {
                if (_portal.ExportUserDefinedType(exportPath, typeName))
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

        [McpServerTool, Description("Export all user defined types (UDT)")]
        public static string ExportUserDefinedTypes(string exportPath)
        {
            try
            {
                if (_portal.ExportUserDefinedTypes(exportPath))
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
