using ModelContextProtocol.Server;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Nodes;
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
        public static ResponseConnect Connect()
        {
            try
            {
                if (_portal.ConnectPortal())
                {
                    return new ResponseConnect
                    {
                        Message = "Connected to TIA-Portal",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, "Failed to connect to TIA-Portal");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed to connect to TIA-Portal: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Disconnect from TIA-Portal")]
        public static ResponseDisconnect Disconnect()
        {
            try
            {
                if (_portal.DisconnectPortal())
                {
                    return new ResponseDisconnect
                    {
                        Message = "Disconnected from TIA-Portal",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, "Failed disconnecting from TIA-Portal");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed disconnecting from TIA-Portal: {ex.Message}", ex);
            }
        }

        #endregion

        #region state

        [McpServerTool, Description("Get the state of the TIA-Portal MCP server")]
        public static ResponseState GetState()
        {
            try
            {
                var state = _portal.GetState();

                if (state != null)
                {
                    return new ResponseState
                    {
                        Message = "TIA-Portal MCP server state retrieved",
                        IsConnected = state.IsConnected,
                        Project = state.Project,
                        Session = state.Session,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, "Failed to retrieve TIA-Portal MCP server state");
                }
                

            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed to retrieve TIA-Portal MCP server state: {ex.Message}", ex);
            }
        }

        #endregion

        #region project/session

        [McpServerTool, Description("Get list of open local projects/sessions")]
        public static ResponseOpenProjects GetOpenProjects()
        {
            try
            {
                var list = _portal.GetOpenProjects();

                list.AddRange(_portal.GetOpenSessions());

                return new ResponseOpenProjects
                {
                    Message = "Open projects and sessions retrieved",
                    Items = list,
                    Meta = new JsonObject
                    {
                        ["timestamp"] = DateTime.Now,
                        ["success"] = true
                    }
                };
            }
            catch (Exception ex)
            {
                throw new McpException(-32000, $"Failed retrieving open projects: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Open a TIA-Portal local project/session")]
        public static ResponseOpenProject OpenProject(
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
                    throw new McpException(-32000, "Invalid project file extension. Use .apXX for projects or .alsXX for sessions, where XX=18,19,20,....");
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
                    return new ResponseOpenProject
                    {
                        Message = $"Project '{path}' opened",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed to open project '{path}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed to open project '{path}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Save the current TIA-Portal local project/session")]
        public static ResponseSaveProject SaveProject()
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    if (_portal.SaveSession())
                    {
                        return new ResponseSaveProject
                        {
                            Message = "Local session saved",
                            Meta = new JsonObject
                            {
                                ["timestamp"] = DateTime.Now,
                                ["success"] = true
                            }
                        };
                    }
                    else
                    {
                        throw new McpException(-32000, "Failed to save local session");
                    }
                }
                else
                {
                    if (_portal.SaveProject())
                    {
                        return new ResponseSaveProject
                        {
                            Message = "Local project saved",
                            Meta = new JsonObject
                            {
                                ["timestamp"] = DateTime.Now,
                                ["success"] = true
                            }
                        };
                    }
                    else
                    {
                        throw new McpException(-32000, "Failed to save project");
                    }
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed saving local project/session: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Save current TIA-Portal project/session with a new name")]
        public static ResponseSaveAsProject SaveAsProject(
            [Description("newProjectPath: defines the new path where to save the project")] string newProjectPath)
        {
            try
            {
                if (_portal.IsLocalSession)
                {
                    throw new McpException(-32000, $"Cannot save local session as '{newProjectPath}'");
                }
                else
                {
                    if (_portal.SaveAsProject(newProjectPath))
                    {
                        return new ResponseSaveAsProject
                        {
                            Message = $"Local project saved as '{newProjectPath}'",
                            Meta = new JsonObject
                            {
                                ["timestamp"] = DateTime.Now,
                                ["success"] = true
                            }
                        };
                    }
                    else
                    {
                        throw new McpException(-32000, $"Failed saving local project as '{newProjectPath}'");
                    }
                }

            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed saving local project/session as '{newProjectPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Close the current TIA-Portal project/session")]
        public static ResponseCloseProject CloseProject()
        {
            try
            {
                bool success;

                if (_portal.IsLocalSession)
                {
                    success = _portal.CloseSession();
                    if (success)
                    {
                        return new ResponseCloseProject
                        {
                            Message = "Local session closed",
                            Meta = new JsonObject
                            {
                                ["timestamp"] = DateTime.Now,
                                ["success"] = true
                            }
                        };
                    }
                    else
                    {
                        throw new McpException(-32000, "Failed closing local session");
                    }
                }
                else
                {
                    success = _portal.CloseProject();
                    if (success)
                    {
                        return new ResponseCloseProject
                        {
                            Message = "Local project closed",
                            Meta = new JsonObject
                            {
                                ["timestamp"] = DateTime.Now,
                                ["success"] = true
                            }
                        };
                    }
                    else
                    {
                        throw new McpException(-32000, "Failed closing project");
                    }
                }

            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed closing local project/session: {ex.Message}", ex);
            }
        }

        #endregion

        #region devices

        [McpServerTool, Description("Get the structure of current local project/session")]
        public static ResponseStructure GetStructure()
        {
            try
            {
                var structure = _portal.GetStructure();

                if (!string.IsNullOrEmpty(structure))
                {
                    return new ResponseStructure
                    {
                        Message = "Project structure retrieved",
                        Structure = "```\n" + structure + "\n```",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, "Failed retrieving project structure");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving project structure: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get info from a device from the current project/session")]
        public static ResponseDeviceInfo GetDeviceInfo(
            [Description("devicePath: defines the path in the project structure to the device")] string devicePath)
        {
            try
            {
                var device = _portal.GetDevice(devicePath);

                if (device != null)
                {
                    var attributes = Helper.GetAttributeList(device);

                    return new ResponseDeviceInfo
                    {
                        Message = $"Device info retrieved from '{devicePath}'",
                        Name = device.Name,
                        Attributes = attributes,
                        Description = device.ToString(),
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving device info from '{devicePath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving device info from '{devicePath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get info from a device item from the current project/session")]
        public static ResponseDeviceItemInfo GetDeviceItemInfo(
            [Description("deviceItemPath: defines the path in the project structure to the device item")] string deviceItemPath)
        {
            try
            {
                var deviceItem = _portal.GetDeviceItem(deviceItemPath);

                if (deviceItem != null)
                {
                    var attributes = Helper.GetAttributeList(deviceItem);

                    return new ResponseDeviceItemInfo
                    {
                        Message = $"Device item info retrieved from '{deviceItemPath}'",
                        Name = deviceItem.Name,
                        Attributes = attributes,
                        Description = deviceItem.ToString(),
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving device item info from '{deviceItemPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving device item info from '{deviceItemPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get a list of all devices in the project/session")]
        public static ResponseDevices GetDevices()
        {
            try
            {
                var list = _portal.GetDevices();
                if (list != null)
                {
                    return new ResponseDevices
                    {
                        Message = "Devices retrieved",
                        Items = list,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving device");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving devices: {ex.Message}", ex);
            }
        }

        #endregion

        #region plc software

        [McpServerTool, Description("Get plc software info")]
        public static ResponseSoftwareInfo GetSoftwareInfo(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath)
        {
            try
            {
                var software = _portal.GetPlcSoftware(softwarePath);
                if (software != null)
                {

                    var attributes = Helper.GetAttributeList(software);

                    return new ResponseSoftwareInfo
                    {
                        Message = $"Software info retrieved from '{softwarePath}'",
                        Name = software.Name,
                        Attributes = attributes,
                        Description = software.ToString(),
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving software info from '{softwarePath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving software info from '{softwarePath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Compile the plc software")]
        public static ResponseCompileSoftware CompileSoftware(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("password: the password to access adminsitration, default: no password")] string password = "")
        {
            try
            {
                var result = _portal.CompileSoftware(softwarePath, password);
                if (result != null && !result.State.ToString().Equals("Error"))
                {
                    return new ResponseCompileSoftware
                    {
                        Message = $"Software '{softwarePath}' compiled with {result}",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed compiling software '{softwarePath}': {result}");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed compiling software '{softwarePath}': {ex.Message}", ex);
            }
        }

        #endregion

        #region blocks

        [McpServerTool, Description("Get a block info, which is located in the plc software")]
        public static ResponseBlockInfo GetBlockInfo(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("blockPath: defines the path in the project structure to the block")] string blockPath)
        {
            try
            {
                var block = _portal.GetBlock(softwarePath, blockPath);
                if (block != null)
                {
                    var attributes = Helper.GetAttributeList(block);

                    return new ResponseBlockInfo
                    {
                        Message = $"Block info retrieved from '{blockPath}' in '{softwarePath}'",
                        Name = block.Name,
                        TypeName = block.GetType().Name,
                        Namespace = block.Namespace,
                        ProgrammingLanguage = Enum.GetName(typeof(ProgrammingLanguage),block.ProgrammingLanguage),
                        MemoryLayout = Enum.GetName(typeof(MemoryLayout), block.MemoryLayout),
                        IsConsistent = block.IsConsistent,
                        HeaderName = block.HeaderName,
                        ModifiedDate = block.ModifiedDate,
                        IsKnowHowProtected = block.IsKnowHowProtected,
                        Attributes = attributes,
                        Description = block.ToString(),
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving block info from '{blockPath}' in '{softwarePath}'");
                }
            }
            catch (Exception ex)
            {
                throw new McpException(-32000, $"Failed retrieving block info from '{blockPath}' in '{softwarePath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get a list of blocks, which are located in plc software")]
        public static ResponseBlocks GetBlocks(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                var list = _portal.GetBlocks(softwarePath, regexName);

                var responseList = new List<ResponseBlockInfo>();
                foreach (var block in list)
                {
                    if (block != null)
                    {
                        var attributes = Helper.GetAttributeList(block);

                        responseList.Add(new ResponseBlockInfo
                        {
                            Name = block.Name,
                            TypeName = block.GetType().Name,
                            Namespace = block.Namespace,
                            ProgrammingLanguage = Enum.GetName(typeof(ProgrammingLanguage), block.ProgrammingLanguage),
                            MemoryLayout = Enum.GetName(typeof(MemoryLayout), block.MemoryLayout),
                            IsConsistent = block.IsConsistent,
                            HeaderName = block.HeaderName,
                            ModifiedDate = block.ModifiedDate,
                            IsKnowHowProtected = block.IsKnowHowProtected,
                            Attributes = attributes,
                            Description = block.ToString()
                        });
                    }
                }

                if (list != null)
                {
                    return new ResponseBlocks
                    {
                        Message = $"Blocks with regex '{regexName}' retrieved from '{softwarePath}'",
                        Items = responseList,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving blocks with regex '{regexName}' in '{softwarePath}");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving blocks with regex '{regexName}' in '{softwarePath}: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Export a block from plc software to file")]
        public static ResponseExportBlock ExportBlock(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("blockPath: defines the path in the project structure to the block")] string blockPath,
            [Description("exportPath: defines the path where to export the block")] string exportPath,
            [Description("preservePath: preserves the path/structure of the plc software")] bool preservePath = false)
        {
            try
            {
                var block = _portal.ExportBlock(softwarePath, blockPath, exportPath, preservePath);
                if (block != null)
                {
                    return new ResponseExportBlock
                    {
                        Message = $"Block exported from '{blockPath}' to '{exportPath}'",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed exporting block from '{blockPath}' to '{exportPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed exporting block from '{blockPath}' to '{exportPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Import a block file to plc software")]
        public static ResponseImportBlock ImportBlock(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("groupPath: defines the path in the project structure to the group, where to import the block")] string groupPath,
            [Description("importPath: defines the path of the xml file from where to import the block")] string importPath)
        {
            try
            {
                if (_portal.ImportBlock(softwarePath, groupPath, importPath))
                {
                    return new ResponseImportBlock
                    {
                        Message = $"Block imported from '{importPath}' to '{groupPath}'",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed importing block from '{importPath}' to '{groupPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed importing block from '{importPath}' to '{groupPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Export all blocks from the plc software to path")]
        public static ResponseExportBlocks ExportBlocks(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where to export the blocks")] string exportPath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "",
            [Description("preservePath: preserves the path/structure of the plc software")] bool preservePath = false)
        {
            try
            {
                var list = _portal.ExportBlocks(softwarePath, exportPath, regexName, preservePath);
                if (list != null)
                {
                    var responseList = new List<ResponseBlockInfo>();
                    foreach (var block in list)
                    {
                        if (block != null)
                        {
                            var attributes = Helper.GetAttributeList(block);

                            responseList.Add(new ResponseBlockInfo
                            {
                                Name = block.Name,
                                TypeName = block.GetType().Name,
                                Namespace = block.Namespace,
                                ProgrammingLanguage = Enum.GetName(typeof(ProgrammingLanguage), block.ProgrammingLanguage),
                                MemoryLayout = Enum.GetName(typeof(MemoryLayout), block.MemoryLayout),
                                IsConsistent = block.IsConsistent,
                                HeaderName = block.HeaderName,
                                ModifiedDate = block.ModifiedDate,
                                IsKnowHowProtected = block.IsKnowHowProtected,
                                Attributes = attributes,
                                Description = block.ToString()
                            });
                        }
                    }

                    return new ResponseExportBlocks
                    {
                        Message = $"Blocks with '{regexName}' from '{softwarePath}' to {exportPath} exported",
                        Items = responseList,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed exporting blocks with '{regexName}' from '{softwarePath}' to {exportPath}");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed exporting blocks with '{regexName}' from '{softwarePath}' to {exportPath}: {ex.Message}", ex);
            }
        }

        #endregion

        #region types

        [McpServerTool, Description("Get a type info from the plc software")]
        public static ResponseTypeInfo GetTypeInfo(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("typePath: defines the path in the project structure to the type")] string typePath)
        {
            try
            {
                var type = _portal.GetType(softwarePath, typePath);
                if (type != null)
                {
                    var attributes = Helper.GetAttributeList(type);

                    return new ResponseTypeInfo
                    {
                        Message = $"Type info retrieved from '{typePath}' in '{softwarePath}'",
                        Name = type.Name,
                        TypeName = type.GetType().Name,
                        Namespace = type.Namespace,
                        IsConsistent = type.IsConsistent,
                        ModifiedDate = type.ModifiedDate,
                        IsKnowHowProtected = type.IsKnowHowProtected,
                        Attributes = attributes,
                        Description = type.ToString(),
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving type info from '{typePath}' in '{softwarePath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving type info from '{typePath}' in '{softwarePath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get a list of types from the plc software")]
        public static ResponseTypes GetTypes(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "")
        {
            try
            {
                var list = _portal.GetTypes(softwarePath, regexName);

                var responseList = new List<ResponseTypeInfo>();
                foreach (var type in list)
                {
                    if (type != null)
                    {
                        var attributes = Helper.GetAttributeList(type);

                        responseList.Add(new ResponseTypeInfo
                        {
                            Name = type.Name,
                            TypeName = type.GetType().Name,
                            Namespace = type.Namespace,
                            IsConsistent = type.IsConsistent,
                            ModifiedDate = type.ModifiedDate,
                            IsKnowHowProtected = type.IsKnowHowProtected,
                            Attributes = attributes,
                            Description = type.ToString()
                        });
                    }
                }

                if (list != null)
                {
                    return new ResponseTypes
                    {
                        Message = $"Types with regex '{regexName}' retrieved from '{softwarePath}'",
                        Items = responseList,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed retrieving user defined types with regex '{regexName}' in '{softwarePath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed retrieving user defined types with regex '{regexName}' in '{softwarePath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Export a type from the plc software")]
        public static ResponseExportType ExportType(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where export the type")] string exportPath,
            [Description("typePath: defines the path in the project structure to the type")] string typePath,
            [Description("preservePath: preserves the path/structure of the plc software")] bool preservePath = false)
        {
            try
            {
                var type = _portal.ExportType(softwarePath, typePath, exportPath, preservePath);
                if (type != null)
                {
                    return new ResponseExportType
                    {
                        Message = $"Type exported from '{typePath}' to '{exportPath}'",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed exporting type from '{typePath}' to '{exportPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed exporting type from '{typePath}' to '{exportPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Import a type from file into the plc software")]
        public static ResponseImportType ImportType(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("groupPath: defines the path in the project structure to the group, where to import the type")] string groupPath,
            [Description("importPath: defines the path of the xml file from where to import the type")] string importPath)
        {
            try
            {
                if (_portal.ImportType(softwarePath, groupPath, importPath))
                {
                    return new ResponseImportType
                    {
                        Message = $"Type imported from '{importPath}' to '{groupPath}'",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed importing type from '{importPath}' to '{groupPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed importing type from '{importPath}' to '{groupPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Export types from the plc software to path")]
        public static ResponseExportTypes ExportTypes(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where to export the types")] string exportPath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "",
            [Description("preservePath: preserves the path/structure of the plc software")] bool preservePath = false)
        {
            try
            {
                var list = _portal.ExportTypes(softwarePath, exportPath, regexName, preservePath);
                if (list != null)
                {
                    var responseList = new List<ResponseTypeInfo>();
                    foreach (var type in list)
                    {
                        if (type != null)
                        {
                            var attributes = Helper.GetAttributeList(type);

                            responseList.Add(new ResponseTypeInfo
                            {
                                Name = type.Name,
                                TypeName = type.GetType().Name,
                                Namespace = type.Namespace,
                                IsConsistent = type.IsConsistent,
                                ModifiedDate = type.ModifiedDate,
                                IsKnowHowProtected = type.IsKnowHowProtected,
                                Attributes = attributes,
                                Description = type.ToString()
                            });
                        }
                    }

                    return new ResponseExportTypes
                    {
                        Message = $"Types with '{regexName}' from '{softwarePath}' to {exportPath} exported",
                        Items = responseList,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed exporting types '{regexName}' from '{softwarePath}' to {exportPath}");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed exporting types '{regexName}' from '{softwarePath}' to {exportPath}: {ex.Message}", ex);
            }
        }

        #endregion

        #region documents

        [McpServerTool, Description("Export as documents (.s7dcl/.s7res) from a block in the plc software to path")]
        public static ResponseExportAsDocuments ExportAsDocuments(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("blockPath: defines the path in the project structure to the block")] string blockPath,
            [Description("exportPath: defines the path where to export the documents")] string exportPath,
            [Description("preservePath: preserves the path/structure of the plc software")] bool preservePath = false)
        {
            try
            {
                if (_portal.ExportAsDocuments(softwarePath, blockPath, exportPath, preservePath))
                {
                    return new ResponseExportAsDocuments
                    {
                        Message = $"Documents exported from '{blockPath}' to '{exportPath}'",
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed exporting documents from '{blockPath}' to '{exportPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed exporting documents from '{blockPath}' to '{exportPath}': {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Export as documents (.s7dcl/.s7res) from a block in the plc software to path")]
        public static ResponseExportBlocksAsDocuments ExportBlocksAsDocuments(
            [Description("softwarePath: defines the path in the project structure to the plc software")] string softwarePath,
            [Description("exportPath: defines the path where to export the documents")] string exportPath,
            [Description("regexName: defines the name or regular expression to find the block. Use empty string (default) to find all")] string regexName = "",
            [Description("preservePath: preserves the path/structure of the plc software")] bool preservePath = false)
        {
            try
            {
                var list = _portal.ExportBlocksAsDocuments(softwarePath, exportPath, regexName, preservePath);
                if (list != null)
                {
                    var respnseList = new List<ResponseBlockInfo>();
                    foreach (var block in list)
                    {
                        if (block != null)
                        {
                            var attributes = Helper.GetAttributeList(block);

                            respnseList.Add(new ResponseBlockInfo
                            {
                                Name = block.Name,
                                TypeName = block.GetType().Name,
                                Namespace = block.Namespace,
                                ProgrammingLanguage = Enum.GetName(typeof(ProgrammingLanguage), block.ProgrammingLanguage),
                                MemoryLayout = Enum.GetName(typeof(MemoryLayout), block.MemoryLayout),
                                IsConsistent = block.IsConsistent,
                                HeaderName = block.HeaderName,
                                ModifiedDate = block.ModifiedDate,
                                IsKnowHowProtected = block.IsKnowHowProtected,
                                Attributes = attributes,
                                Description = block.ToString()
                            });
                        }
                    }

                    return new ResponseExportBlocksAsDocuments
                    {
                        Message = $"Documents exported to '{exportPath}'",
                        Items = respnseList,
                        Meta = new JsonObject
                        {
                            ["timestamp"] = DateTime.Now,
                            ["success"] = true
                        }
                    };
                }
                else
                {
                    throw new McpException(-32000, $"Failed exporting documents to '{exportPath}'");
                }
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Failed exporting documents to '{exportPath}': {ex.Message}", ex);
            }
        }

        #endregion
    }
}
