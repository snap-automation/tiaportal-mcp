using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiLogging.HmiLoggingCommon;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Multiuser;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TiaMcpServer.Siemens
{
    public class Portal
    {
        // closing parantheses for regex characters ommitted, because they are not relevant for regex detection
        private readonly char[] _regexChars = ['.', '^', '$', '*', '+', '?', '(', '[', '{', '\\', '|'];

        private TiaPortal? _tiaPortal;
        private ProjectBase? _project;
        private LocalSession? _localSession;

        #region helper for mcp server

        public bool ProjectIsValid
        {
            get
            {
                if (_project == null)
                {
                    return false;
                }

                // Check if the project is a valid Project instance
                if ((_localSession == null) && (_project is Project))
                {
                    return true;
                }

                // If it's a MultiuserProject, we can also check its validity
                if ((_localSession != null) && (_project is MultiuserProject))
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsLocalSession
        {
            get
            {
                return _localSession != null;
            }
        }

        public bool IsLocalProject
        {
            get
            {
                return _localSession == null;
            }
        }

        #endregion

        #region helper for unit tests

        public static bool IsLocalSessionFile(string sessionPath)
        {
            // Check if the path ends with '.als\d+' using regex
            var regex = new Regex(@"\.als\d+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(sessionPath);
        }

        public static bool IsLocalProjectFile(string projectPath)
        {
            // Check if the path ends with '.ap\d+' using regex
            var regex = new Regex(@"\.ap\d+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(projectPath);
        }

        public void Dispose()
        {
            try
            {
                (_project as Project)?.Close();
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error closing the project: {ex.Message}");
            }

            try
            {
                _tiaPortal?.Dispose();
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error closing the portal: {ex.Message}");
            }
        }

        #endregion

        #region portal

        public bool ConnectPortal()
        {
            try
            {
                // connect to running TIA Portal
                var processes = TiaPortal.GetProcesses();
                if (processes.Any())
                {
                    _tiaPortal = processes.First().Attach();

                    return true;
                }

                // start new TIA Portal
                _tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsConnected()
        {
            return _tiaPortal != null;
        }

        public void DisconnectPortal()
        {
            try
            {
                _project = null;

                _tiaPortal?.Dispose();
                _tiaPortal = null;
            }
            catch (Exception)
            {
                // Handle exception if needed, e.g., log it
            }
        }

        #endregion

        #region project

        public List<string> GetOpenProjects()
        {
            if (_tiaPortal == null)
            {
                return [];
            }

            var projects = new List<string>();

            if (_tiaPortal.Projects != null)
            {
                foreach (var project in _tiaPortal.Projects)
                {
                    projects.Add(project.Name);
                }
            }

            return projects;
        }

        public bool OpenProject(string projectPath)
        {
            if (_tiaPortal == null)
            {
                return false;
            }

            if (_project != null)
            {
                (_project as Project)?.Close();
                _project = null;
            }

            try
            {
                var openProjects = GetOpenProjects();
                var projectName = Path.GetFileNameWithoutExtension(projectPath);

                if (openProjects.Contains(projectName))
                {
                    // Project is already open
                    _project = _tiaPortal.Projects.FirstOrDefault(p => p.Name == projectName);

                    return _project != null;
                }
                else
                {
                    // see [5.3.1 Projekt öffnen, S.113]
                    _project = _tiaPortal.Projects.OpenWithUpgrade(new FileInfo(projectPath));

                    return _project != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveProject()
        {
            if (_project == null)
            {
                return false;
            }

            (_project as Project)?.Save();

            return true;
        }

        public bool SaveAsProject(string path)
        {
            if (_project == null)
            {
                return false;
            }

            var di = new DirectoryInfo(path);

            (_project as Project)?.SaveAs(di);

            return true;
        }

        public bool CloseProject()
        {
            if (_project == null)
            {
                return false;
            }

            (_project as Project)?.Close();
            _project = null;

            return true;
        }

        #endregion

        #region session

        public List<string> GetOpenSessions()
        {
            if (_tiaPortal == null)
            {
                return [];
            }

            var sessions = new List<string>();


            if (_tiaPortal.LocalSessions != null)
            {
                foreach (var session in _tiaPortal.LocalSessions)
                {
                    sessions.Add(session.Project.Name);
                }
            }

            return sessions;
        }

        public bool OpenSession(string localSessionPath)
        {
            if (_tiaPortal == null)
            {
                return false;
            }

            if (_localSession != null)
            {
                _project = null;
                _localSession?.Close();
                _localSession = null;
            }

            try
            {
                var openSessions = GetOpenSessions();
                var projectName = Path.GetFileNameWithoutExtension(localSessionPath);
                var sessionName = Regex.Replace(projectName, @"_(LS|ES)_\d$", string.Empty, RegexOptions.IgnoreCase);

                if (openSessions.Contains(sessionName))
                {
                    // Session is already open  
                    _localSession = _tiaPortal.LocalSessions.FirstOrDefault(s => s.Project.Name == sessionName);
                    if (_localSession != null)
                    {
                        // Correctly cast MultiuserProject to Project  
                        _project = _localSession.Project;
                        return _project != null;
                    }
                }
                else
                {
                    _localSession = _tiaPortal.LocalSessions.Open(new FileInfo(localSessionPath));
                    if (_localSession != null)
                    {
                        // Correctly cast MultiuserProject to Project  
                        _project = _localSession.Project;
                        return _project != null;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public bool SaveSession()
        {
            if (_localSession == null)
            {
                return false;
            }

            // Save session
            _localSession?.Save();

            return true;
        }

        //public bool UpdateSession()
        //{
        //    if (_localSession == null)
        //    {
        //        return false;
        //    }
        //    // Save session
        //    _ = _localSession.IsUptoDate;
        //    return true;
        //}

        public bool CloseSession()
        {
            if (_localSession == null)
            {
                return false;
            }

            _project = null;
            _localSession.Close();
            _localSession = null;

            return true;
        }

        #endregion

        #region devices

        public string GetStructure()
        {
            if (_project == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new();

            sb.AppendLine($"Project: {_project.Name}");

            GetStructureDevices(sb, _project.Devices, 0);

            GetStructureGroups(sb, _project.DeviceGroups, 0);

            GetStructureUngroupedDeviceGroup(sb, _project.UngroupedDevicesGroup, 0);

            return sb.ToString();
        }

        #region  private helper GetStructure...

        private string IndentText(int n)
        {
            string indention = "  ";
            return string.Concat(Enumerable.Repeat(indention, n));
        }

        private void GetStructureDevices(StringBuilder sb, DeviceComposition? devices, int indent)
        {
            if (devices != null && devices.Count > 0)
            {
                sb.AppendLine($"{IndentText(indent)}+- Devices[]");

                foreach (var device in devices)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- Device: {device.Name}");

                    if (device.DeviceItems != null && device.DeviceItems.Count > 0)
                    {
                        sb.AppendLine($"{IndentText(indent + 2)}+- DeviceItems[]");

                        foreach (var deviceItem in device.DeviceItems)
                        {
                            sb.AppendLine($"{IndentText(indent + 3)}+- DeviceItem: {deviceItem.Name}");

                            GetStructureDeviceItemSoftware(sb, deviceItem, indent + 4);

                            GetStructureItems(sb, deviceItem.Items, indent + 4);

                            GetStructureDeviceItems(sb, deviceItem.DeviceItems, indent + 4);
                        }
                    }
                }
            }
        }

        private void GetStructureGroups(StringBuilder sb, DeviceUserGroupComposition? groups, int indent)
        {
            if (groups != null && groups.Count > 0)
            {
                sb.AppendLine($"{IndentText(indent)}+- Groups[]");

                foreach (var group in groups)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- Group: {group.Name}");

                    GetStructureDevices(sb, group.Devices, indent + 2);

                    GetStructureGroups(sb, group.Groups, indent + 2);
                }
            }
        }

        private void GetStructureItems(StringBuilder sb, DeviceItemAssociation? items, int indent)
        {
            if (items != null && items.Count > 0)
            {
                sb.AppendLine($"{IndentText(indent)}+- Items[]");

                foreach (var subItem in items)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- Item: {subItem.Name}");
                }
            }
        }

        private void GetStructureDeviceItems(StringBuilder sb, DeviceItemComposition? deviceItems, int indent)
        {
            if (deviceItems != null && deviceItems.Count > 0)
            {
                sb.AppendLine($"{IndentText(indent)}+- DeviceItems[]");

                foreach (var deviceItem in deviceItems)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- DeviceItem: {deviceItem.Name}");

                    GetStructureDeviceItemSoftware(sb, deviceItem, indent + 2);

                    GetStructureItems(sb, deviceItem.Items, indent + 2);

                    GetStructureDeviceItems(sb, deviceItem.DeviceItems, indent + 2);

                }
            }
        }

        private void GetStructureDeviceItemSoftware(StringBuilder sb, DeviceItem? deviceItem, int indent)
        {
            // check if it contains plc software
            if (deviceItem != null)
            {
                // get from DeviceItem
                var softwareContainer = deviceItem.GetService<SoftwareContainer>();

                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- PlcSoftware: {plcSoftware.Name}");
                }

                if (softwareContainer?.Software is HmiSoftware hmiSoftware)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- HmiSoftware: {hmiSoftware.Name}");
                }

                if (softwareContainer?.Software is HmiTarget hmiTarget)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- HmiSoftware: {hmiTarget.Name}");
                }
            }
        }

        private void GetStructureUngroupedDeviceGroup(StringBuilder sb, DeviceSystemGroup? ungroupedDevicesGroup, int indent)
        {
            if (ungroupedDevicesGroup != null)
            {
                sb.AppendLine($"{IndentText(indent)}+- UngroupedDevicesGroup: {ungroupedDevicesGroup.Name}");

                if (ungroupedDevicesGroup.Devices != null && ungroupedDevicesGroup.Devices.Count > 0)
                {
                    sb.AppendLine($"{IndentText(indent + 1)}+- Devices[]");

                    foreach (var device in ungroupedDevicesGroup.Devices)
                    {
                        sb.AppendLine($"{IndentText(indent + 2)}+- Device: {device.Name}");
                    }
                }
            }
        }

        #endregion

        public List<string> GetDevices()
        {
            if (_project == null)
            {
                return [];
            }

            var devices = new List<string>();

            if (_project?.Devices != null)
            {
                foreach (Device device in _project.Devices)
                {
                    devices.Add(device.Name);
                }
            }

            return devices;
        }

        public Device? GetDevice(string devicePath)
        {
            if (_project == null)
            {
                return null;
            }

            // Retrieve the device by its path
            return GetDeviceByPath(devicePath);
        }

        public DeviceItem? GetDeviceItem(string deviceItemPath)
        {
            if (_project == null)
            {
                return null;
            }

            // Retrieve the device by its path
            return GetDeviceItemByPath(deviceItemPath);

        }

        #endregion

        #region software

        public string CompileSoftware(string softwarePath)
        {
            if (_project == null)
            {
                return "Error";
            }

            // var plcSoftware = GetFirstPlcSoftware(devicePath);
            var softwareContainer = GetSoftwareContainer(softwarePath);

            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                try
                {
                    ICompilable compileService = plcSoftware.GetService<ICompilable>();

                    CompilerResult result = compileService.Compile();

                    return result.State.ToString();
                }
                catch (Exception)
                {
                    return "Error";
                }
            }

            return "Error";
        }

        #endregion

        #region blocks/types

        public PlcBlock? GetBlock(string softwarePath, string blockPath)
        {
            if (_project == null)
            {
                return null;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {
                    var path = blockPath.Contains("/") ? blockPath.Substring(0, blockPath.LastIndexOf("/")) : string.Empty;
                    var regexName = blockPath.Contains("/") ? blockPath.Substring(blockPath.LastIndexOf("/") + 1) : blockPath;

                    PlcBlock? block = null;

                    var group = GetPlcBlockGroupByPath(softwarePath, path);
                    if (group != null)
                    {
                        if (regexName.IndexOfAny(_regexChars) >= 0)
                        {
                            try
                            {
                                var regex = new Regex(regexName, RegexOptions.IgnoreCase);
                                block = group.Blocks.FirstOrDefault(b => regex.IsMatch(b.Name)) as PlcBlock;
                            }
                            catch (Exception)
                            {
                                // Invalid regex, return null
                                return null;
                            }
                        }
                        else
                        {
                            block = group.Blocks.FirstOrDefault(b => b.Name.Equals(regexName, StringComparison.OrdinalIgnoreCase));
                        }

                        return block;
                    }
                }
            }

            return null;
        }

        public PlcType? GetType(string softwarePath, string typePath)
        {
            if (_project == null)
            {
                return null;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var typeGroup = plcSoftware?.TypeGroup;

                if (typeGroup != null)
                {
                    var path = typePath.Contains("/") ? typePath.Substring(0, typePath.LastIndexOf("/")) : string.Empty;
                    var regexName = typePath.Contains("/") ? typePath.Substring(typePath.LastIndexOf("/") + 1) : typePath;

                    PlcType? type = null;

                    var group = GetPlcTypeGroupByPath(softwarePath, path);
                    if (group != null)
                    {
                        if (regexName.IndexOfAny(_regexChars) >= 0)
                        {
                            try
                            {
                                var regex = new Regex(regexName, RegexOptions.IgnoreCase);
                                type = group.Types.FirstOrDefault(t => regex.IsMatch(t.Name)) as PlcType;
                            }
                            catch (Exception)
                            {
                                // Invalid regex, return null
                                return null;
                            }
                        }
                        else
                        {
                            type = group.Types.FirstOrDefault(t => t.Name.Equals(regexName, StringComparison.OrdinalIgnoreCase));
                        }

                        return type;
                    }
                }
            }

            return null;
        }

        public List<string> GetBlocks(string softwarePath, string regexName = "")
        {
            if (_project == null)
            {
                return [];
            }

            var blocks = new List<string>();
            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    var group = plcSoftware?.BlockGroup;

                    if (group != null)
                    {
                        GetBlocksRecursive(group, blocks, regexName);
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error getting blocks: {ex.Message}");
            }

            return blocks;
        }

        public List<string> GetTypes(string softwarePath, string regexName = "")
        {
            if (_project == null)
            {
                return [];
            }

            var types = new List<string>();
            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    var group = plcSoftware?.TypeGroup;

                    if (group != null)
                    {
                        GetTypesRecursive(group, types, regexName);
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error getting user defined types: {ex.Message}");
            }

            return types;
        }

        private bool GetBlocksRecursive(PlcBlockGroup group, List<string> result, string regexName = "")
        {
            var anySuccess = false;

            foreach (var composition in group.Blocks)
            {
                if (composition is PlcBlock block)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(regexName) && !Regex.IsMatch(block.Name, regexName, RegexOptions.IgnoreCase))
                        {
                            continue; // Skip this block if it doesn't match the pattern
                        }
                    }
                    catch (Exception)
                    {
                        // Invalid regex pattern, skip this block
                        continue;
                    }

                    var groupPath = GetPlcBlockGroupPath(group);

                    result.Add($"{groupPath}/{block.Name}, ({block.ProgrammingLanguage}), {group.Name}");

                    anySuccess = true;
                }
            }

            foreach (var subgroup in group.Groups)
            {
                anySuccess = GetBlocksRecursive(subgroup, result, regexName);
            }

            return anySuccess;
        }

        private bool GetTypesRecursive(PlcTypeGroup group, List<string> result, string regexName = "")
        {
            var anySuccess = false;

            foreach (var composition in group.Types)
            {
                if (composition is PlcType type)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(regexName) && !Regex.IsMatch(type.Name, regexName, RegexOptions.IgnoreCase))
                        {
                            continue; // Skip this block if it doesn't match the pattern
                        }
                    }
                    catch (Exception)
                    {
                        // Invalid regex pattern, skip this block
                        continue;
                    }

                    var groupPath = GetPlcTypeGroupPath(group);

                    result.Add($"{groupPath}/{type.Name}, {group.Name}");

                    anySuccess = true;
                }

            }

            foreach (PlcTypeGroup subgroup in group.Groups)
            {
                anySuccess = GetTypesRecursive(subgroup, result, regexName);
            }

            return anySuccess;
        }

        public bool ExportBlock(string softwarePath, string blockPath, string exportPath)
        {
            if (_project == null)
            {
                return false;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {
                    var path = blockPath.Contains("/") ? blockPath.Substring(0, blockPath.LastIndexOf("/")) : string.Empty;
                    var name = blockPath.Contains("/") ? blockPath.Substring(blockPath.LastIndexOf("/") + 1) : blockPath;

                    var group = GetPlcBlockGroupByPath(softwarePath, path);
                    if (group == null)
                    {
                        return false; // Group not found
                    }

                    var block = group.Blocks.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (block != null)
                    {
                        // check if the block is DataBlock
                        if (block is PlcBlock b)
                        {
                            exportPath = Path.Combine(exportPath, path.Replace('/', '\\'), $"{b.Name}.xml");

                            try
                            {
                                // Delete if already exists
                                if (File.Exists(exportPath))
                                {
                                    File.Delete(exportPath);
                                }

                                b.Export(new FileInfo(exportPath), ExportOptions.None);

                                return true;
                            }
                            catch (Exception)
                            {
                                // Console.WriteLine($"Error exporting block '{blockName}': {ex.Message}");
                            }

                            return false;
                        }
                    }
                }
            }

            return false;
        }

        public bool ImportBlock(string softwarePath, string groupPath, string importPath)
        {
            if (_project == null)
            {
                return false;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {

                    var group = GetPlcBlockGroupByPath(softwarePath, groupPath);
                    if (group == null)
                    {
                        return false; // Group not found
                    }

                    try
                    {
                        // Correct the argument type by using FileInfo instead of FileStream  
                        var fileInfo = new FileInfo(importPath);
                        if (fileInfo.Exists)
                        {
                            var list = group.Blocks.Import(fileInfo, ImportOptions.Override);
                            if (list != null && list.Count > 0)
                            {
                                return true;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool ExportType(string softwarePath, string typePath, string exportPath)
        {
            var success = false;

            if (_project == null)
            {
                return success;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var typeGroup = plcSoftware?.TypeGroup;

                if (typeGroup != null)
                {
                    var path = typePath.Contains("/") ? typePath.Substring(0, typePath.LastIndexOf("/")) : string.Empty;
                    var name = typePath.Contains("/") ? typePath.Substring(typePath.LastIndexOf("/") + 1) : typePath;

                    var group = GetPlcTypeGroupByPath(softwarePath, path);
                    if (group == null)
                    {
                        return false; // Group not found
                    }

                    var type = group.Types.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (type != null)
                    {
                        exportPath = Path.Combine(exportPath, path.Replace('/', '\\'), $"{type.Name}.xml");

                        try
                        {
                            if (File.Exists(exportPath))
                            {
                                File.Delete(exportPath);
                            }

                            type.Export(new FileInfo(exportPath), ExportOptions.None);

                            success = true;
                        }
                        catch (Exception)
                        {
                            // Console.WriteLine($"Error exporting user defined type '{typeName}': {ex.Message}");
                        }
                    }
                }
            }

            return success;
        }

        public bool ImportType(string softwarePath, string groupPath, string importPath)
        {
            var success = false;

            if (_project == null)
            {
                return success;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var typeGroup = plcSoftware?.TypeGroup;

                if (typeGroup != null)
                {
                    var group = GetPlcTypeGroupByPath(softwarePath, groupPath);
                    if (group == null)
                    {
                        return false; // Group not found
                    }

                    try
                    {
                        // Correct the argument type by using FileInfo instead of FileStream  
                        var fileInfo = new FileInfo(importPath);
                        if (fileInfo.Exists)
                        {
                            var list = group.Types.Import(fileInfo, ImportOptions.Override);
                            if (list != null && list.Count > 0)
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return success;
        }

        public bool ExportBlocks(string softwarePath, string exportPath, string regexName = "")
        {
            if (_project == null)
            {
                return false;
            }

            var success = false;
            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    var blockGroup = plcSoftware?.BlockGroup;

                    if (blockGroup != null)
                    {
                        success = ExportBlocksRecursive(blockGroup, exportPath, regexName) || success;
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error exporting blocks: {ex.Message}");
            }

            return success;
        }

        public bool ExportTypes(string softwarePath, string exportPath, string regexName = "")
        {
            var success = false;

            if (_project == null)
            {
                return success;
            }

            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    var udtGroup = plcSoftware?.TypeGroup;

                    if (udtGroup != null)
                    {
                        success = ExportTypesRecursive(udtGroup, exportPath, regexName) || success;
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error exporting user defined types: {ex.Message}");
            }

            return success;
        }

        private bool ExportBlocksRecursive(PlcBlockGroup group, string exportPath, string regexName = "")
        {
            var anySuccess = false;

            foreach (var composition in group.Blocks)
            {
                if (composition is PlcBlock block)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(regexName) && !Regex.IsMatch(block.Name, regexName, RegexOptions.IgnoreCase))
                        {
                            continue; // Skip this block if it doesn't match the pattern
                        }
                    }
                    catch (Exception)
                    {
                        // Invalid regex pattern, skip this block
                        continue;
                    }

                    var path = Path.Combine(exportPath, $"{block.Name}.xml");
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }

                        block.Export(new FileInfo(path), ExportOptions.None);

                        anySuccess = true;
                    }
                    catch (Exception)
                    {
                        // Console.WriteLine($"Error exporting block '{block.Name}': {ex.Message}");
                        continue;
                    }
                }
            }

            foreach (var subgroup in group.Groups)
            {
                var subPath = Path.Combine(exportPath, subgroup.Name);

                if (!Directory.Exists(subPath))
                {
                    Directory.CreateDirectory(subPath);
                }

                anySuccess = ExportBlocksRecursive(subgroup, subPath, regexName) || anySuccess;
            }

            return anySuccess;
        }

        private bool ExportTypesRecursive(PlcTypeGroup group, string exportPath, string regexName = "")
        {
            var anySuccess = false;

            // Export all types in this group
            foreach (PlcType composition in group.Types)
            {
                if (composition is PlcType type)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(regexName) && !Regex.IsMatch(type.Name, regexName, RegexOptions.IgnoreCase))
                        {
                            continue; // Skip this block if it doesn't match the pattern
                        }
                    }
                    catch (Exception)
                    {
                        // Invalid regex pattern, skip this block
                        continue;
                    }

                    var path = Path.Combine(exportPath, $"{type.Name}.xml");
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }

                        type.Export(new FileInfo(path), ExportOptions.None);

                        anySuccess = true;
                    }
                    catch (Exception)
                    {
                        // Console.WriteLine($"Error exporting UDT '{udt.Name}': {ex.Message}");
                        continue;
                    }
                }
            }

            foreach (PlcTypeGroup subgroup in group.Groups)
            {
                var subPath = Path.Combine(exportPath, subgroup.Name);
                if (!Directory.Exists(subPath))
                {
                    Directory.CreateDirectory(subPath);
                }

                anySuccess = ExportTypesRecursive(subgroup, subPath, regexName) || anySuccess;
            }

            return anySuccess;
        }

        public bool ExportAsDocuments(string softwarePath, string blockPath, string exportPath)
        {
            if (_project == null)
            {
                return false;
            }

            var success = false;

            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    if (plcSoftware != null)
                    {
                        // Export code blocks as documents
                        // https://docs.tia.siemens.cloud/r/en-us/v20/creating-and-managing-blocks/exporting-and-importing-blocks-in-simatic-sd-format-s7-1200-s7-1500/exporting-and-importing-blocks-in-simatic-sd-format-s7-1200-s7-1500

                        var groupPath = blockPath.Contains("/") ? blockPath.Substring(0, blockPath.LastIndexOf("/")) : string.Empty;
                        var blockName = blockPath.Contains("/") ? blockPath.Substring(blockPath.LastIndexOf("/") + 1) : blockPath;

                        var group = GetPlcBlockGroupByPath(softwarePath, groupPath);

                        //group?.Blocks.ForEach(b => Console.WriteLine($"Block: {b.Name}, Type: {b.GetType().Name}"));

                        // join exportPath and groupPath
                        if (!Directory.Exists(exportPath))
                        {
                            Directory.CreateDirectory(exportPath);
                        }

                        if (!string.IsNullOrEmpty(groupPath))
                        {
                            exportPath = Path.Combine(exportPath, groupPath);
                        }

                        if (!Directory.Exists(exportPath))
                        {
                            Directory.CreateDirectory(exportPath);
                        }

                        try
                        {
                            // delete files s7dcl/s7res if already exists
                            var blockFiles7dclPath = Path.Combine(exportPath, $"{blockName}.s7dcl");
                            if (File.Exists(blockFiles7dclPath))
                            {
                                File.Delete(blockFiles7dclPath);
                            }
                            var blockFiles7resPath = Path.Combine(exportPath, $"{blockName}.s7res");
                            if (File.Exists(blockFiles7resPath))
                            {
                                File.Delete(blockFiles7resPath);
                            }

                            var result = group?.Blocks.Find(blockName)?.ExportAsDocuments(new DirectoryInfo(exportPath), blockName);

                            if (result != null && result.State == DocumentResultState.Success)
                            {
                                success = true;
                            }
                        }
                        catch (EngineeringNotSupportedException)
                        {
                            // The export or import of blocks with mixed programming languages is not possible
                            // Console.WriteLine($"Error exporting block as document: {ex.Message}");
                        }
                        catch (Exception)
                        {
                            // Console.WriteLine($"Error creating export directory: {ex.Message}");
                        }

                    }

                }


            }
            catch (Exception)
            {
                // Console.WriteLine($"Error exporting blocks as documents: {ex.Message}");
            }
            return success;
        }

        #endregion

        #region private helper

        private SoftwareContainer? GetSoftwareContainer(string softwarePath)
        {
            if (_project == null)
            {
                return null;
            }

            string[] pathSegments = softwarePath.Split('/');
            int index = 0;

            if (index >= pathSegments.Length)
                return null;

            string segment = pathSegments[index];
            SoftwareContainer? softwareContainer = null;

            // in Devices
            if (_project.Devices != null)
            {
                softwareContainer = GetSoftwareContainerInDevices(_project.Devices, pathSegments, index);
                if (softwareContainer != null)
                {
                    return softwareContainer;
                }
            }

            // in Groups
            if (_project.DeviceGroups != null)
            {
                softwareContainer = GetSoftwareContainerInGroups(_project.DeviceGroups, pathSegments, index);
                if (softwareContainer != null)
                {
                    return softwareContainer;
                }
            }

            return null;
        }

        private SoftwareContainer? GetSoftwareContainerInDevices(DeviceComposition devices, string[] pathSegments, int index)
        {

            if (index >= pathSegments.Length)
                return null;

            string segment = pathSegments[index];
            string nextSegment = index + 1 < pathSegments.Length ? pathSegments[index + 1] : string.Empty;

            if (devices != null)
            {
                SoftwareContainer? softwareContainer = null;
                Device? device = null;
                DeviceItem? deviceItem = null;

                // a pc based plc has a Device.Name = 'PC-System_1' or something like that, which is visible in the TIA-Portal IDE
                // use segment to find device
                device = devices.FirstOrDefault(d => d.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
                if (device != null)
                {
                    // then use next segment to find device item
                    deviceItem = device.DeviceItems.FirstOrDefault(di => di.Name.Equals(nextSegment, StringComparison.OrdinalIgnoreCase));
                    // but here we use next segment to find device item
                    softwareContainer = GetSoftwareContainerInDeviceItem(deviceItem, pathSegments, index + 1);
                    if (softwareContainer != null)
                    {
                        return softwareContainer;
                    }
                }

                // a hardware plc has a Device.Name = 'S7-1500/ET200MP-Station_1' or something like that, which is not visible in the TIA-Portal IDE
                // ignored segment for Device.Name and use it for DeviceItem.Name
                deviceItem = devices
                    .SelectMany(d => d.DeviceItems)
                    .FirstOrDefault(di => di.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
                if (deviceItem != null)
                {
                    return GetSoftwareContainerInDeviceItem(deviceItem, pathSegments, index);
                }

            }

            return null;
        }

        private SoftwareContainer? GetSoftwareContainerInGroups(DeviceUserGroupComposition groups, string[] pathSegments, int index)
        {
            if (index >= pathSegments.Length)
                return null;

            string segment = pathSegments[index];
            SoftwareContainer? softwareContainer = null;

            if (groups != null)
            {
                var group = groups.FirstOrDefault(g => g.Name.Equals(segment));
                if (group != null)
                {
                    // when segment matched
                    softwareContainer = GetSoftwareContainerInDevices(group.Devices, pathSegments, index + 1);
                    if (softwareContainer != null)
                    {
                        return softwareContainer;
                    }

                    return GetSoftwareContainerInGroups(group.Groups, pathSegments, index + 1);
                }
            }

            return null;
        }

        private SoftwareContainer? GetSoftwareContainerInDeviceItem(DeviceItem deviceItem, string[] pathSegments, int index)
        {
            if (deviceItem != null)
            {
                // when segment matched
                if (index == pathSegments.Length - 1)
                {
                    // get from DeviceItem
                    var softwareContainer = deviceItem.GetService<SoftwareContainer>();
                    if (softwareContainer != null)
                    {
                        return softwareContainer;
                    }
                }
            }

            return null;
        }

        private Device? GetDeviceByPath(string devicePath)
        {
            if (_project?.Devices == null || string.IsNullOrWhiteSpace(devicePath))
                return null;

            var pathSegments = devicePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length == 0)
            {
                return null;
            }

            // Try top-level device first
            if (pathSegments.Length == 1)
            {
                return _project.Devices.FirstOrDefault(d => d.Name.Equals(pathSegments[0], StringComparison.OrdinalIgnoreCase));
            }

            // Traverse device groups
            DeviceUserGroupComposition? groups = _project.DeviceGroups;
            DeviceUserGroup? group = groups?.FirstOrDefault(g => g.Name.Equals(pathSegments[0], StringComparison.OrdinalIgnoreCase));

            if (group == null)
            {
                return null;
            }

            for (int i = 1; i < pathSegments.Length; i++)
            {
                // Try to find device in current group
                var device = group.Devices.FirstOrDefault(d => d.Name.Equals(pathSegments[i], StringComparison.OrdinalIgnoreCase));
                if (device != null)
                {
                    return device;
                }

                // Try to find subgroup
                group = group.Groups.FirstOrDefault(g => g.Name.Equals(pathSegments[i], StringComparison.OrdinalIgnoreCase));
                if (group == null)
                {
                    break;
                }
            }

            return null;
        }

        private DeviceItem? GetDeviceItemByPath(string deviceItemPath)
        {
            if (_project == null || _project.Devices == null)
            {
                return null;
            }

            // Split the device path by '/' to get each device name  
            var pathSegments = deviceItemPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            DeviceItem? deviceItem = null;

            // initial devices and groups
            var devices = _project.Devices;
            var groups = _project.DeviceGroups;

            for (int index = 0; index < pathSegments.Length; index++)
            {
                deviceItem = GetDeviceItemFromDevice(pathSegments, devices, index);

                if (deviceItem == null)
                {
                    // search in groups
                    var group = groups?.FirstOrDefault(g => g.Name.Equals(pathSegments[index], StringComparison.OrdinalIgnoreCase));
                    if (group != null)
                    {
                        devices = group.Devices;
                        if(devices != null)
                        {
                            deviceItem = GetDeviceItemFromDevice(pathSegments, devices, index + 1);
                        }

                        if (deviceItem != null)
                        {
                            return deviceItem;
                        }

                        // not found, but on the path
                        groups = group.Groups;
                        devices = group.Devices;
                    }
                }
                else 
                {
                    return deviceItem;
                }
            }

            return deviceItem;
        }

        private static DeviceItem? GetDeviceItemFromDevice(string[] pathSegments, DeviceComposition? devices, int index)
        {
            string segment = pathSegments[index];
            string nextSegment = index + 1 < pathSegments.Length ? pathSegments[index + 1] : string.Empty;

            DeviceItem? deviceItem = null;
            
            // a pc based plc has a Device.Name = 'PC-System_1' or something like that, which is visible in the TIA-Portal IDE
            // use segment to find device
            var device = devices.FirstOrDefault(d => d.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (device != null)
            {
                // then use next segment to find device item
                deviceItem = device.DeviceItems.FirstOrDefault(di => di.Name.Equals(nextSegment, StringComparison.OrdinalIgnoreCase));

            }

            // a hardware plc has a Device.Name = 'S7-1500/ET200MP-Station_1' or something like that, which is not visible in the TIA-Portal IDE
            if (device == null)
            {
                deviceItem = devices
                .SelectMany(d => d.DeviceItems)
                .FirstOrDefault(di => di.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
            }

            return deviceItem;
        }

        private PlcBlockGroup? GetPlcBlockGroupByPath(string softwarePath, string groupPath)
        {
            if (_project == null)
            {
                return null;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                if (plcSoftware?.BlockGroup == null)
                {
                    return null;
                }


                // Split the path by '/' to get each group name
                var groupNames = groupPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

                PlcBlockGroup? currentGroup = plcSoftware.BlockGroup;

                foreach (var groupName in groupNames)
                {
                    currentGroup = currentGroup.Groups.FirstOrDefault(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                    if (currentGroup == null)
                    {
                        return null; // Group not found !
                    }
                }

                return currentGroup;
            }

            return null;
        }

        private PlcTypeGroup? GetPlcTypeGroupByPath(string softwarePath, string groupPath)
        {
            if (_project == null)
            {
                return null;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                if (plcSoftware?.TypeGroup == null)
                {
                    return null;
                }

                var groupNames = groupPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

                PlcTypeGroup? currentGroup = plcSoftware.TypeGroup;

                foreach (var groupName in groupNames)
                {
                    currentGroup = currentGroup.Groups.FirstOrDefault(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                    if (currentGroup == null)
                    {
                        return null; // Group not found !
                    }
                }

                return currentGroup;
            }

            return null;
        }

        private string GetPlcBlockGroupPath(PlcBlockGroup group)
        {
            if (group == null)
            {
                return string.Empty;
            }

            PlcBlockGroup? nullableGroup = group;
            var path = group.Name;

            while (nullableGroup != null && nullableGroup.Parent != null)
            {
                try
                {
                    //group = (PlcBlockGroup) group.Parent;
                    if (group is PlcBlockSystemGroup systemGroup)
                    {
                        // do not get parent for system group
                        break;
                    }

                    nullableGroup = nullableGroup.Parent as PlcBlockGroup;
                }
                catch (Exception)
                {
                    // Handle any exceptions that may occur while accessing the parent
                    break;
                }

                if (nullableGroup != null)
                {
                    path = $"{nullableGroup.Name}/{path}";
                }
            }

            // cut off the first name auf first 'Programmbausteine/'
            path = path[(path.IndexOf('/') + 1)..];

            return path;
        }

        private string GetPlcTypeGroupPath(PlcTypeGroup group)
        {
            if (group == null)
            {
                return string.Empty;
            }

            PlcTypeGroup? nullableGroup = group;
            var path = group.Name;

            while (nullableGroup != null && nullableGroup.Parent != null)
            {
                try
                {
                    //group = (PlcTypeGroup) group.Parent;
                    if (group is PlcTypeSystemGroup systemGroup)
                    {
                        // do not get parent for system group
                        break;
                    }

                    nullableGroup = nullableGroup.Parent as PlcTypeGroup;
                }
                catch (Exception)
                {
                    // Handle any exceptions that may occur while accessing the parent
                    break;
                }

                if (nullableGroup != null)
                {
                    path = $"{nullableGroup.Name}/{path}";
                }
            }

            // cut off the first name auf first 'Programmbausteine/'
            path = path[(path.IndexOf('/') + 1)..];

            return path;
        }

        #endregion

    }

}
