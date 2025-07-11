using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HmiUnified;
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
        private TiaPortal? _tiaPortal;
        private ProjectBase? _project;
        private LocalSession? _localSession;

        #region public

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

        private string Indent(int n)
        {
            string indention = "  ";
            return string.Concat(Enumerable.Repeat(indention, n));
        }

        private void GetStructureDevices(StringBuilder sb, DeviceComposition? devices, int indent)
        {
            if (devices != null && devices.Count > 0)
            {
                sb.AppendLine($"{Indent(indent)}+- Devices[]");

                foreach (var device in devices)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- Device: {device.Name}");

                    if (device.DeviceItems != null && device.DeviceItems.Count > 0)
                    {
                        sb.AppendLine($"{Indent(indent + 2)}+- DeviceItems[]");

                        foreach (var deviceItem in device.DeviceItems)
                        {
                            sb.AppendLine($"{Indent(indent + 3)}+- DeviceItem: {deviceItem.Name}");

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
                sb.AppendLine($"{Indent(indent)}+- Groups[]");

                foreach (var group in groups)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- Group: {group.Name}");

                    GetStructureDevices(sb, group.Devices, indent + 2);

                    GetStructureGroups(sb, group.Groups, indent + 2);
                }
            }
        }

        private void GetStructureItems(StringBuilder sb, DeviceItemAssociation? items, int indent)
        {
            if (items != null && items.Count > 0)
            {
                sb.AppendLine($"{Indent(indent)}+- Items[]");

                foreach (var subItem in items)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- Item: {subItem.Name}");
                }
            }
        }

        private void GetStructureDeviceItems(StringBuilder sb, DeviceItemComposition? deviceItems, int indent)
        {
            if (deviceItems != null && deviceItems.Count > 0)
            {
                sb.AppendLine($"{Indent(indent)}+- DeviceItems[]");

                foreach (var deviceItem in deviceItems)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- DeviceItem: {deviceItem.Name}");

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
                    sb.AppendLine($"{Indent(indent + 1)}+- PlcSoftware: {plcSoftware.Name}");
                }

                if (softwareContainer?.Software is HmiSoftware hmiSoftware)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- HmiSoftware: {hmiSoftware.Name}");
                }

                if (softwareContainer?.Software is HmiTarget hmiTarget)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- HmiSoftware: {hmiTarget.Name}");
                }
            }
        }

        private void GetStructureUngroupedDeviceGroup(StringBuilder sb, DeviceSystemGroup? ungroupedDevicesGroup, int indent)
        {
            if (ungroupedDevicesGroup != null)
            {
                sb.AppendLine($"{Indent(indent)}+- UngroupedDevicesGroup: {ungroupedDevicesGroup.Name}");

                if (ungroupedDevicesGroup.Devices != null && ungroupedDevicesGroup.Devices.Count > 0)
                {
                    sb.AppendLine($"{Indent(indent + 1)}+- Devices[]");

                    foreach (var device in ungroupedDevicesGroup.Devices)
                    {
                        sb.AppendLine($"{Indent(indent + 2)}+- Device: {device.Name}");
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

        #region private helper devices

        private Device? GetDeviceByPath(string devicePath)
        {
            if (_project == null || _project.Devices == null)
            {
                return null;
            }

            // Split the device path by '/' to get each device name  
            var pathSegments = devicePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            Device? device = null;

            //foreach (var dPath in devicePaths)
            //{
            //    // 2. then search for DeviceItem ...
            //    if (currentDevice != null)
            //    {
            //        // Search within the current device for devices
            //        foreach (var device in currentDevice)
            //        {
            //            if (device.Name.Equals(dPath, StringComparison.OrdinalIgnoreCase))
            //            {
            //                currentDevice = device;
            //                break;
            //            }
            //        }
            //    }

            //    // 1. first search for device ...
            //    if (currentDevice == null)
            //    {
            //        // Start with the first device  
            //        currentDevice = _projectBase.Devices.FirstOrDefault(d => d.Name.Equals(dPath, StringComparison.OrdinalIgnoreCase));
            //    }

            //    if (currentDevice == null)
            //    {
            //        return null; // Device not found  
            //    }
            //}

            return device;
        }

        private DeviceItem? GetDeviceItemByPath(string deviceItemPath)
        {
            if (_project == null || _project.Devices == null)
            {
                return null;
            }

            // Split the device path by '/' to get each device name  
            var pathSegments = deviceItemPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            Device? currentDevice = null;
            DeviceItem? currentDeviceItem = null;

            foreach (var dPath in pathSegments)
            {
                // 2. then search for DeviceItem ...
                if (currentDevice != null)
                {
                    // Search within the current device's items  
                    DeviceItem? deviceItem = currentDevice.DeviceItems.FirstOrDefault(di => di.Name.Equals(dPath, StringComparison.OrdinalIgnoreCase));
                    if (deviceItem != null)
                    {
                        currentDeviceItem = deviceItem;
                    }
                }

                // 1. first search for device ...
                if (currentDevice == null)
                {
                    // Start with the first device  
                    currentDevice = _project.Devices.FirstOrDefault(d => d.Name.Equals(dPath, StringComparison.OrdinalIgnoreCase));
                }



                if (currentDevice == null && currentDeviceItem == null)
                {
                    return null; // Device not found  
                }
            }

            return currentDeviceItem;
        }

        #endregion

        #endregion

        #region plc software

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

        #region private helper plc software

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
                Device? matchedDevice = null;
                DeviceItem? matchedDeviceItem = null;

                // a pc based plc has a Device.Name = 'PC-System_1' or something like that, which is visible in the TIA-Portal IDE
                // use segment to find device
                matchedDevice = devices.FirstOrDefault(d => d.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
                if (matchedDevice != null)
                {
                    Console.WriteLine($"matched Device: {matchedDevice.Name}:{matchedDevice.DeviceItems.Count}");

                    // then use next segment to find device item
                    matchedDeviceItem = matchedDevice.DeviceItems.FirstOrDefault(deviceItem => deviceItem.Name.Equals(nextSegment, StringComparison.OrdinalIgnoreCase));
                    // but here we use next segment to find device item
                    softwareContainer = GetSoftwareContainerInDeviceItem(matchedDeviceItem, pathSegments, index + 1);
                    if (softwareContainer != null)
                    {
                        return softwareContainer;
                    }
                }

                // a hardware plc has a Device.Name = 'S7-1500/ET200MP-Station_1' or something like that, which is not visible in the TIA-Portal IDE
                // ignored segment for Device.Name and use it for DeviceItem.Name
                matchedDeviceItem = devices
                    .SelectMany(device => device.DeviceItems)
                    .FirstOrDefault(deviceItem => deviceItem.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
                if (matchedDeviceItem != null)
                {
                    return GetSoftwareContainerInDeviceItem(matchedDeviceItem, pathSegments, index);
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
                var matchedGroup = groups.FirstOrDefault(g => g.Name.Equals(segment));
                if (matchedGroup != null)
                {
                    Console.WriteLine($"matched Group: {matchedGroup.Name}:{matchedGroup.Devices.Count}:{matchedGroup.Groups.Count}");

                    // when segment matched
                    softwareContainer = GetSoftwareContainerInDevices(matchedGroup.Devices, pathSegments, index + 1);
                    if (softwareContainer != null)
                    {
                        return softwareContainer;
                    }

                    return GetSoftwareContainerInGroups(matchedGroup.Groups, pathSegments, index + 1);
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

        #endregion

        #endregion

        #region code blocks

        public string GetCodeBlock(string softwarePath, string groupPath, string blockName)
        {
            if (_project == null)
            {
                return string.Empty;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {
                    var group = GetPlcBlockGroupByPath(softwarePath, groupPath);

                    if (group != null)
                    {
                        var plcBlock = group.Blocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                        if (plcBlock != null)
                        {
                            if (plcBlock is CodeBlock b)
                            {
                                // avbailable attributes
                                // Attribute: [CodeModifiedDate, 05.06.2025 15:47:20]
                                // Attribute: [CompileDate, 05.06.2025 15:47:20]
                                // Attribute: [CreationDate, 19.05.2025 09:43:58]
                                // Attribute: [EventClass, Program cycle]
                                // Attribute: [HandleErrorsWithinBlock, False]
                                // Attribute: [HeaderAuthor,]
                                // Attribute: [HeaderFamily,]
                                // Attribute: [HeaderName,]
                                // Attribute: [HeaderVersion, 0.1]
                                // Attribute: [InterfaceModifiedDate, 21.07.2008 16:55:08]
                                // Attribute: [IsConsistent, True]
                                // Attribute: [IsIECCheckEnabled, False]
                                // Attribute: [IsKnowHowProtected, False]
                                // Attribute: [IsWriteProtected, False]
                                // Attribute: [ModifiedDate, 05.06.2025 15:47:20]
                                // Attribute: [Namespace,]
                                // Attribute: [PLCSimAdvancedSupport, True]
                                // Attribute: [ParameterModified, 21.07.2008 16:55:08]
                                // Attribute: [PriorityNumber, 1]
                                // Attribute: [ProcessImagePartNumber, 65535]
                                // Attribute: [ProgrammingLanguage, Siemens.Engineering.Contract.EnumToClientRepresentation]
                                // Attribute: [SecondaryType, ProgramCycle]
                                // Attribute: [SetENOAutomatically, False]
                                // Attribute: [StructureModified, 21.07.2008 16:55:08]
                                // Attribute: [VirtualPlcSupport, False]

                                return $"{groupPath}/{b.Name} ({b.ProgrammingLanguage}), {group.Name}";
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        public List<string> GetCodeBlocks(string softwarePath)
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
                    if (plcSoftware?.BlockGroup?.Blocks != null)
                    {
                        GetCodeBlocksRecursive(plcSoftware.BlockGroup, blocks);
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error getting blocks: {ex.Message}");
            }

            return blocks;
        }

        public bool ExportCodeBlock(string softwarePath, string groupPath, string blockName, string exportPath)
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

                    var block = group.Blocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                    if (block != null)
                    {
                        if (block is CodeBlock cb)
                        {
                            var dbPath = Path.Combine(exportPath, $"{cb.Name}.xml");

                            try
                            {
                                if (File.Exists(dbPath))
                                {
                                    File.Delete(dbPath);
                                }

                                cb.Export(new FileInfo(dbPath), ExportOptions.None);

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

        public bool ExportCodeBlocks(string softwarePath, string exportPath)
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
                        success = ExportCodeBlocksRecursive(blockGroup, exportPath) || success;
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error exporting blocks: {ex.Message}");
            }

            return success;
        }

        #region private helper code blocks

        private bool GetCodeBlocksRecursive(PlcBlockGroup group, List<string> result)
        {
            var anySuccess = false;

            foreach (var block in group.Blocks)
            {
                if (block is CodeBlock b)
                {
                    var groupPath = GetGroupPath(group);

                    result.Add($"{groupPath}/{b.Name}, ({b.ProgrammingLanguage}), {group.Name}");

                    anySuccess = true;
                }
            }

            foreach (var subgroup in group.Groups)
            {
                anySuccess = GetCodeBlocksRecursive(subgroup, result);
            }

            return anySuccess;
        }

        private bool ExportCodeBlocksRecursive(PlcBlockGroup group, string path)
        {
            var anySuccess = false;

            foreach (var block in group.Blocks)
            {
                if (block is CodeBlock b)
                {
                    var bPath = Path.Combine(path, $"{b.Name}.xml");
                    try
                    {
                        if (File.Exists(bPath))
                        {
                            File.Delete(bPath);
                        }

                        b.Export(new FileInfo(bPath), ExportOptions.None);

                        anySuccess = true;
                    }
                    catch (Exception)
                    {
                        // Console.WriteLine($"Error exporting code block '{block.Name}': {ex.Message}");
                        continue;
                    }

                }
            }

            foreach (var subgroup in group.Groups)
            {
                var subExportPath = Path.Combine(path, subgroup.Name);

                if (!Directory.Exists(subExportPath))
                {
                    Directory.CreateDirectory(subExportPath);
                }

                anySuccess = ExportCodeBlocksRecursive(subgroup, subExportPath) || anySuccess;
            }

            return anySuccess;
        }

        #endregion

        #endregion

        #region data blocks

        public string GetDataBlock(string softwarePath, string groupPath, string blockName)
        {
            if (_project == null)
            {
                return string.Empty;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {
                    var group = GetPlcBlockGroupByPath(softwarePath, groupPath);

                    if (group != null)
                    {
                        var plcBlock = group.Blocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                        if (plcBlock != null)
                        {
                            if (plcBlock is DataBlock b)
                            {
                                // available attributes:
                                // [DBAccessibleFromOPCUA, False]
                                // [DBAccessibleFromWebserver, True]
                                // [DownloadWithoutReinit, False]
                                // [HeaderAuthor, ErEr]
                                // [HeaderFamily, SERVO]
                                // [HeaderName, Global]
                                // [HeaderVersion, 0.1]
                                // [InterfaceModifiedDate, 16.05.2025 09:15:03]
                                // [IsConsistent, True]
                                // [IsKnowHowProtected, False]
                                // [IsOnlyStoredInLoadMemory, False]
                                // [IsPLCDB, False]
                                // [IsRetainMemResEnabled, False]
                                // [IsWriteProtectedInAS, False]
                                // [MemoryReserve, 100]
                                // [ModifiedDate, 16.05.2025 09:15:03]
                                // [Namespace,]
                                // [ParameterModified, 16.05.2025 09:15:03]
                                // [ProgrammingLanguage, Siemens.Engineering.Contract.EnumToClientRepresentation]
                                // [StructureModified, 16.05.2025 09:15:03]

                                return $"{groupPath}/{b.Name} ({b.ProgrammingLanguage}), {group.Name}";
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        public List<string> GetDataBlocks(string softwarePath)
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
                    if (plcSoftware?.BlockGroup?.Blocks != null)
                    {
                        GetDataBlocksRecursive(plcSoftware.BlockGroup, blocks);
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error getting blocks: {ex.Message}");
            }

            return blocks;
        }

        public bool ExportDataBlock(string softwarePath, string groupPath, string blockName, string exportPath)
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

                    var block = group.Blocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                    if (block != null)
                    {
                        // check if the block is DataBlock
                        if (block is DataBlock db)
                        {
                            var dbPath = Path.Combine(exportPath, $"{db.Name}.xml");

                            try
                            {
                                // Delete if already exists
                                if (File.Exists(dbPath))
                                {
                                    File.Delete(dbPath);
                                }

                                db.Export(new FileInfo(dbPath), ExportOptions.None);

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

        public bool ExportDataBlocks(string softwarePath, string exportPath)
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
                        success = ExportDataBlocksRecursive(blockGroup, exportPath) || success;
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error exporting blocks: {ex.Message}");
            }

            return success;
        }

        #region private helper data blocks

        private bool GetDataBlocksRecursive(PlcBlockGroup group, List<string> result)
        {
            var anySuccess = false;

            foreach (var block in group.Blocks)
            {
                if (block is DataBlock b)
                {
                    var groupPath = GetGroupPath(group);

                    result.Add($"{groupPath}/{b.Name}, ({b.ProgrammingLanguage}), {group.Name}");

                    anySuccess = true;
                }
            }

            foreach (var subgroup in group.Groups)
            {
                anySuccess = GetDataBlocksRecursive(subgroup, result);
            }

            return anySuccess;
        }

        private bool ExportDataBlocksRecursive(PlcBlockGroup group, string path)
        {
            var anySuccess = false;

            foreach (var block in group.Blocks)
            {
                if (block is DataBlock b)
                {
                    var bPath = Path.Combine(path, $"{b.Name}.xml");
                    try
                    {
                        if (File.Exists(bPath))
                        {
                            File.Delete(bPath);
                        }

                        b.Export(new FileInfo(bPath), ExportOptions.None);

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
                var subExportPath = Path.Combine(path, subgroup.Name);

                if (!Directory.Exists(subExportPath))
                {
                    Directory.CreateDirectory(subExportPath);
                }

                anySuccess = ExportDataBlocksRecursive(subgroup, subExportPath) || anySuccess;
            }

            return anySuccess;
        }

        #endregion

        #endregion

        #region types

        public Dictionary<string, string> GetUserDefinedTypeMembers(string softwarePath, string typeName)
        {
            var members = new Dictionary<string, string>();

            if (_project == null)
            {
                return members;
            }

            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    var udtGroup = plcSoftware?.TypeGroup;
                    if (udtGroup != null)
                    {
                        PlcType plcType = udtGroup.Types.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

                        if (plcType is PlcStruct plcStruct)
                        {
                            //foreach (var member in plcStruct.
                            //{
                            //    members.Add(member.Name, member.DataType.Name);
                            //}
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Handle exception if needed
            }
            return members;
        }

        public string GetUserDefinedType(string softwarePath, string typeName)
        {
            if (_project == null)
            {
                return string.Empty;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var udtGroup = plcSoftware?.TypeGroup;

                if (udtGroup != null)
                {
                    // Assuming we want to return the first UDT name
                    //var firstUdt = udtGroup.Types.FirstOrDefault();
                    //if (firstUdt != null)
                    //{
                    //    return firstUdt.Name;
                    //}

                    var udt = udtGroup.Types.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

                    if (udt != null)
                    {
                        return udt.Name;
                    }
                }
            }

            return string.Empty;

        }

        public List<string> GetUserDefinedTypes(string softwarePath)
        {
            if (_project == null)
            {
                return [];
            }

            var userDefinedTypes = new List<string>();
            try
            {
                var softwareContainer = GetSoftwareContainer(softwarePath);
                if (softwareContainer?.Software is PlcSoftware plcSoftware)
                {
                    var udtGroup = plcSoftware?.TypeGroup;

                    if (udtGroup != null)
                    {
                        GetUserDefinedTypesRecursive(udtGroup, userDefinedTypes);
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error getting user defined types: {ex.Message}");
            }

            return userDefinedTypes;
        }

        public bool ExportUserDefinedType(string softwarePath, string exportPath, string typeName)
        {
            var success = false;

            if (_project == null)
            {
                return success;
            }

            var softwareContainer = GetSoftwareContainer(softwarePath);
            if (softwareContainer?.Software is PlcSoftware plcSoftware)
            {
                var udtGroup = plcSoftware?.TypeGroup;

                if (udtGroup != null)
                {
                    var udt = udtGroup.Types.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                    if (udt != null)
                    {
                        var udtPath = Path.Combine(exportPath, $"{udt.Name}.xml");

                        try
                        {
                            if (File.Exists(udtPath))
                            {
                                File.Delete(udtPath);
                            }

                            udt.Export(new FileInfo(udtPath), ExportOptions.None);

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

        public bool ExportUserDefinedTypes(string softwarePath, string exportPath)
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
                        success = ExportUserDefinedTypesRecursive(udtGroup, exportPath) || success;
                    }
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error exporting user defined types: {ex.Message}");
            }

            return success;
        }

        #region private helper user defined types

        private bool GetUserDefinedTypesRecursive(PlcTypeGroup group, List<string> result)
        {
            var anySuccess = false;

            foreach (PlcType udt in group.Types)
            {
                result.Add(udt.Name);

                anySuccess = true;
            }

            foreach (PlcTypeGroup subgroup in group.Groups)
            {
                anySuccess = GetUserDefinedTypesRecursive(subgroup, result);
            }

            return anySuccess;
        }

        private bool ExportUserDefinedTypesRecursive(PlcTypeGroup group, string path)
        {
            var anySuccess = false;

            // Export all types in this group
            foreach (PlcType udt in group.Types)
            {
                var udtPath = Path.Combine(path, $"{udt.Name}.xml");
                try
                {
                    if (File.Exists(udtPath))
                    {
                        File.Delete(udtPath);
                    }

                    udt.Export(new FileInfo(udtPath), ExportOptions.None);

                    anySuccess = true;
                }
                catch (Exception)
                {
                    // Console.WriteLine($"Error exporting UDT '{udt.Name}': {ex.Message}");
                    continue;
                }
            }

            foreach (PlcTypeGroup subgroup in group.Groups)
            {
                var subExportPath = Path.Combine(path, subgroup.Name);
                if (!Directory.Exists(subExportPath))
                {
                    Directory.CreateDirectory(subExportPath);
                }

                anySuccess = ExportUserDefinedTypesRecursive(subgroup, subExportPath) || anySuccess;
            }

            return anySuccess;
        }

        #endregion

        #endregion

        #endregion

        #region private helper for code/data/udts ...

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

                //var attribs = currentGroup.GetAttributes(AttributeAccessOptions.ReadOnly);
                //foreach (var attr in attribs)
                //{
                //    Debug.WriteLine($"RetrievePlcBlockGroupByPath.currentGroup.Attribute: {attr}");
                //}

                foreach (var groupName in groupNames)
                {
                    currentGroup = currentGroup.Groups.FirstOrDefault(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                    //Debug.WriteLine($"RetrievePlcBlockGroupByPath: {currentGroup.Name}");

                    if (currentGroup == null)
                    {
                        return null; // Group not found !
                    }
                }


                return currentGroup;
            }

            return null;
        }

        private string GetGroupPath(PlcBlockGroup group)
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

        #endregion

    }

}
