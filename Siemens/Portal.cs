using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW;
using Siemens.Engineering.HW.Features;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.SW.Blocks;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Siemens.Engineering.Multiuser;

namespace TiaMcpServer.Siemens
{
    public class Portal
    {
        private TiaPortal? _tiaPortal;
        private ProjectBase? _projectBase;
        private PlcSoftware? _plcSoftware;

        #region public

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
            catch (Exception ex)
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
                _plcSoftware = null;
                _projectBase = null;

                _tiaPortal?.Dispose();
                _tiaPortal = null;
            }
            catch (Exception ex)
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
                return new List<string>();
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

        public List<string> GetOpenSessions()
        {
            if (_tiaPortal == null)
            {
                return new List<string>();
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

        public bool OpenProject(string projectPath)
        {
            if (_tiaPortal == null)
            {
                return false;
            }

            if (_projectBase != null)
            {
                (_projectBase as Project)?.Close();
                _projectBase = null;
            }

            try
            {
                var openProjects = GetOpenProjects();
                var projectName = Path.GetFileNameWithoutExtension(projectPath);

                if (openProjects.Contains(projectName))
                {
                    // Project is already open
                    _projectBase = _tiaPortal.Projects.FirstOrDefault(p => p.Name == projectName);

                    return _projectBase != null;
                }
                else
                {
                    // see [5.3.1 Projekt öffnen, S.113]
                    _projectBase = _tiaPortal.Projects.OpenWithUpgrade(new FileInfo(projectPath));

                    return _projectBase != null;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool OpenSession(string localSessionPath)
        {
            if (_tiaPortal == null)
            {
                return false;
            }

            if (_projectBase != null)
            {
                (_projectBase as Project)?.Close();

                _projectBase = null;
            }

            try
            {
                var openSessions = GetOpenSessions();
                var projectName = Path.GetFileNameWithoutExtension(localSessionPath);
                var sessionName = Regex.Replace(projectName, @"_(LS|ES)_\d$", string.Empty, RegexOptions.IgnoreCase);

                if (openSessions.Contains(sessionName))
                {
                    // Session is already open  
                    var localSession = _tiaPortal.LocalSessions.FirstOrDefault(s => s.Project.Name == sessionName);
                    if (localSession != null)
                    {
                        // Correctly cast MultiuserProject to Project  
                        _projectBase = localSession.Project;
                        return _projectBase != null;
                    }
                }
                else
                {
                    var newSession = _tiaPortal.LocalSessions.Open(new FileInfo(localSessionPath));
                    if (newSession != null)
                    {
                        // Correctly cast MultiuserProject to Project  
                        _projectBase = newSession.Project;
                        return _projectBase != null;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        public bool SaveProject()
        {
            if (_projectBase == null)
            {
                return false;
            }

            (_projectBase as Project)?.Save();

            return true;
        }

        public bool SaveAsProject(string path)
        {
            if (_projectBase == null)
            {
                return false;
            }

            var di = new DirectoryInfo(path);

            (_projectBase as Project)?.SaveAs(di);

            return true;
        }

        public bool CloseProject()
        {
            if (_projectBase == null)
            {
                return false;
            }

            (_projectBase as Project)?.Close();
            _projectBase = null;

            _plcSoftware = null;

            return true;
        }

        #endregion

        #region devices

        public List<string> GetDevices()
        {
            if (_projectBase == null)
            {
                return new List<string>();
            }

            var devices = new List<string>();

            if (_projectBase?.Devices != null)
            {
                foreach (Device device in _projectBase.Devices)
                {
                    devices.Add(device.Name);
                }
            }

            return devices;
        }

        #endregion

        #region code blocks

        public string GetCodeBlock(string groupPath, string blockName)
        {
            if (_projectBase == null)
            {
                return string.Empty;
            }

            var plcSoftware = GetFirstPlcSoftware();
            var blockGroup = plcSoftware?.BlockGroup;

            if (blockGroup != null)
            {
                var group = RetrievePlcBlockGroupByPath(groupPath);

                if (group != null)
                {
                    // Debug.WriteLine($"GetCodeBlock: {group.Name}, {group.Blocks.Count}");
                    // foreach (var block in group.Blocks)
                    // {
                    //     Debug.WriteLine($"GetCodeBlock: {block.Name}");
                    // }

                    // var attribs = group.GetAttributes(AttributeAccessOptions.ReadOnly);
                    // foreach (var attr in attribs)
                    // {
                    //     Debug.WriteLine($"GetCodeBlock.group.Attribute: {attr}");
                    // 
                    //     // GetCodeBlock.group.Attribute: [Name, 0_OBs]
                    // }

                    var plcBlock = group.Blocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                    if (plcBlock != null)
                    {
                        if (plcBlock is CodeBlock b)
                        {
                            // var attribs = b.GetAttributes(AttributeAccessOptions.ReadOnly);
                            // foreach (var attr in attribs)
                            // {
                            //     Debug.WriteLine($"GetCodeBlock.CodeBlock.Attribute: {attr}");
                            // 
                            //     // GetCodeBlock.CodeBlock.Attribute: [CodeModifiedDate, 05.06.2025 15:47:20]
                            //     // GetCodeBlock.CodeBlock.Attribute: [CompileDate, 05.06.2025 15:47:20]
                            //     // GetCodeBlock.CodeBlock.Attribute: [CreationDate, 19.05.2025 09:43:58]
                            //     // GetCodeBlock.CodeBlock.Attribute: [EventClass, Program cycle]
                            //     // GetCodeBlock.CodeBlock.Attribute: [HandleErrorsWithinBlock, False]
                            //     // GetCodeBlock.CodeBlock.Attribute: [HeaderAuthor,]
                            //     // GetCodeBlock.CodeBlock.Attribute: [HeaderFamily,]
                            //     // GetCodeBlock.CodeBlock.Attribute: [HeaderName,]
                            //     // GetCodeBlock.CodeBlock.Attribute: [HeaderVersion, 0.1]
                            //     // GetCodeBlock.CodeBlock.Attribute: [InterfaceModifiedDate, 21.07.2008 16:55:08]
                            //     // GetCodeBlock.CodeBlock.Attribute: [IsConsistent, True]
                            //     // GetCodeBlock.CodeBlock.Attribute: [IsIECCheckEnabled, False]
                            //     // GetCodeBlock.CodeBlock.Attribute: [IsKnowHowProtected, False]
                            //     // GetCodeBlock.CodeBlock.Attribute: [IsWriteProtected, False]
                            //     // GetCodeBlock.CodeBlock.Attribute: [ModifiedDate, 05.06.2025 15:47:20]
                            //     // GetCodeBlock.CodeBlock.Attribute: [Namespace,]
                            //     // GetCodeBlock.CodeBlock.Attribute: [PLCSimAdvancedSupport, True]
                            //     // GetCodeBlock.CodeBlock.Attribute: [ParameterModified, 21.07.2008 16:55:08]
                            //     // GetCodeBlock.CodeBlock.Attribute: [PriorityNumber, 1]
                            //     // GetCodeBlock.CodeBlock.Attribute: [ProcessImagePartNumber, 65535]
                            //     // GetCodeBlock.CodeBlock.Attribute: [ProgrammingLanguage, Siemens.Engineering.Contract.EnumToClientRepresentation]
                            //     // GetCodeBlock.CodeBlock.Attribute: [SecondaryType, ProgramCycle]
                            //     // GetCodeBlock.CodeBlock.Attribute: [SetENOAutomatically, False]
                            //     // GetCodeBlock.CodeBlock.Attribute: [StructureModified, 21.07.2008 16:55:08]
                            //     // GetCodeBlock.CodeBlock.Attribute: [VirtualPlcSupport, False]
                            // }

                            return $"{groupPath}/{b.Name} ({b.ProgrammingLanguage}), {group.Name}";
                        }
                    }
                }
            }

            return string.Empty;
        }

        public List<string> GetCodeBlocks()
        {
            if (_projectBase == null)
            {
                return new List<string>();
            }

            var blocks = new List<string>();

            try
            {
                var plcSoftware = GetFirstPlcSoftware();

                if (plcSoftware?.BlockGroup?.Blocks != null)
                {
                    GetCodeBlocksRecursive(plcSoftware.BlockGroup, blocks);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error getting blocks: {ex.Message}");
            }

            return blocks;
        }

        public bool ExportCodeBlock(string groupPath, string blockName, string exportPath)
        {
            if (_projectBase == null)
            {
                return false;
            }

            var plcSoftware = GetFirstPlcSoftware();
            var blockGroup = plcSoftware?.BlockGroup;

            if (blockGroup != null)
            {
                var group = RetrievePlcBlockGroupByPath(groupPath);
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
                        catch (Exception ex)
                        {
                            // Console.WriteLine($"Error exporting block '{blockName}': {ex.Message}");
                        }

                        return false;
                    }
                }
            }
            return false;
        }

        public bool ExportCodeBlocks(string exportPath)
        {
            if (_projectBase == null)
            {
                return false;
            }

            var success = false;

            try
            {
                var plcSoftware = GetFirstPlcSoftware();
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {
                    success = ExportCodeBlocksRecursive(blockGroup, exportPath) || success;
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error exporting blocks: {ex.Message}");
            }

            return success;
        }

        #endregion

        #region data blocks

        public string GetDataBlock(string groupPath, string blockName)
        {
            if (_projectBase == null)
            {
                return string.Empty;
            }

            var plcSoftware = GetFirstPlcSoftware();
            var blockGroup = plcSoftware?.BlockGroup;

            if (blockGroup != null)
            {
                var group = RetrievePlcBlockGroupByPath(groupPath);

                if (group != null)
                {
                    //Debug.WriteLine($"GetDataBlock: {group.Name}, {group.Blocks.Count}");
                    //foreach (var block in group.Blocks)
                    //{
                    //    Debug.WriteLine($"GetDataBlock: {block.Name}");
                    //}

                    var plcBlock = group.Blocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                    if (plcBlock != null)
                    {
                        if (plcBlock is DataBlock b)
                        {
                            // var attribs = b.GetAttributes(AttributeAccessOptions.ReadOnly);
                            // foreach (var attr in attribs)
                            // {
                            //     Debug.WriteLine($"GetDataBlock.DataBlock.Attribute: {attr}");
                            // 
                            //     // GetDataBlock.DataBlock.Attribute: [DBAccessibleFromOPCUA, False]
                            //     // GetDataBlock.DataBlock.Attribute: [DBAccessibleFromWebserver, True]
                            //     // GetDataBlock.DataBlock.Attribute: [DownloadWithoutReinit, False]
                            //     // GetDataBlock.DataBlock.Attribute: [HeaderAuthor, ErEr]
                            //     // GetDataBlock.DataBlock.Attribute: [HeaderFamily, SERVO]
                            //     // GetDataBlock.DataBlock.Attribute: [HeaderName, Global]
                            //     // GetDataBlock.DataBlock.Attribute: [HeaderVersion, 0.1]
                            //     // GetDataBlock.DataBlock.Attribute: [InterfaceModifiedDate, 16.05.2025 09:15:03]
                            //     // GetDataBlock.DataBlock.Attribute: [IsConsistent, True]
                            //     // GetDataBlock.DataBlock.Attribute: [IsKnowHowProtected, False]
                            //     // GetDataBlock.DataBlock.Attribute: [IsOnlyStoredInLoadMemory, False]
                            //     // GetDataBlock.DataBlock.Attribute: [IsPLCDB, False]
                            //     // GetDataBlock.DataBlock.Attribute: [IsRetainMemResEnabled, False]
                            //     // GetDataBlock.DataBlock.Attribute: [IsWriteProtectedInAS, False]
                            //     // GetDataBlock.DataBlock.Attribute: [MemoryReserve, 100]
                            //     // GetDataBlock.DataBlock.Attribute: [ModifiedDate, 16.05.2025 09:15:03]
                            //     // GetDataBlock.DataBlock.Attribute: [Namespace,]
                            //     // GetDataBlock.DataBlock.Attribute: [ParameterModified, 16.05.2025 09:15:03]
                            //     // GetDataBlock.DataBlock.Attribute: [ProgrammingLanguage, Siemens.Engineering.Contract.EnumToClientRepresentation]
                            //     // GetDataBlock.DataBlock.Attribute: [StructureModified, 16.05.2025 09:15:03]
                            // }

                            return $"{groupPath}/{b.Name} ({b.ProgrammingLanguage}), {group.Name}";
                        }
                    }
                }
            }

            return string.Empty;
        }

        public List<string> GetDataBlocks()
        {
            if (_projectBase == null)
            {
                return new List<string>();
            }

            var blocks = new List<string>();

            try
            {
                var plcSoftware = GetFirstPlcSoftware();

                if (plcSoftware?.BlockGroup?.Blocks != null)
                {
                    GetDataBlocksRecursive(plcSoftware.BlockGroup, blocks);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error getting blocks: {ex.Message}");
            }

            return blocks;
        }

        public bool ExportDataBlock(string groupPath, string blockName, string exportPath)
        {
            if (_projectBase == null)
            {
                return false;
            }

            var plcSoftware = GetFirstPlcSoftware();
            var blockGroup = plcSoftware?.BlockGroup;

            if (blockGroup != null)
            {
                var group = RetrievePlcBlockGroupByPath(groupPath);
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
                        catch (Exception ex)
                        {
                            // Console.WriteLine($"Error exporting block '{blockName}': {ex.Message}");
                        }

                        return false;
                    }
                }
            }
            return false;
        }

        public bool ExportDataBlocks(string exportPath)
        {
            if (_projectBase == null)
            {
                return false;
            }

            var success = false;

            try
            {
                var plcSoftware = GetFirstPlcSoftware();
                var blockGroup = plcSoftware?.BlockGroup;

                if (blockGroup != null)
                {
                    success = ExportDataBlocksRecursive(blockGroup, exportPath) || success;
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error exporting blocks: {ex.Message}");
            }

            return success;
        }

        #endregion

        #region types

        public Dictionary<string, string> GetUserDefinedTypeMembers(string typeName)
        {
            var members = new Dictionary<string, string>();

            if (_projectBase == null)
            {
                return members;
            }

            try
            {
                var plcSoftware = GetFirstPlcSoftware();
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
            catch (Exception ex)
            {
                // Handle exception if needed
            }
            return members;
        }

        public string GetUserDefinedType(string typeName)
        {
            if (_projectBase == null)
            {
                return string.Empty;
            }

            var plcSoftware = GetFirstPlcSoftware();
            var udtGroup = plcSoftware?.TypeGroup;

            if (udtGroup != null)
            {
                // Assuming we want to return the first UDT name
                var firstUdt = udtGroup.Types.FirstOrDefault();
                if (firstUdt != null)
                {
                    return firstUdt.Name;
                }
            }

            return string.Empty;

        }

        public List<string> GetUserDefinedTypes()
        {
            if (_projectBase == null)
            {
                return new List<string>();
            }

            var userDefinedTypes = new List<string>();

            try
            {
                var plcSoftware = GetFirstPlcSoftware();
                var udtGroup = plcSoftware?.TypeGroup;

                if (udtGroup != null)
                {
                    GetUserDefinedTypesRecursive(udtGroup, userDefinedTypes);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error getting user defined types: {ex.Message}");
            }

            return userDefinedTypes;
        }

        public bool ExportUserDefinedType(string exportPath, string typeName)
        {
            var success = false;

            if (_projectBase == null)
            {
                return success;
            }

            var plcSoftware = GetFirstPlcSoftware();
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
                    catch (Exception ex)
                    {
                        // Console.WriteLine($"Error exporting user defined type '{typeName}': {ex.Message}");
                    }
                }
            }

            return success;
        }

        public bool ExportUserDefinedTypes(string exportPath)
        {
            var success = false;

            if (_projectBase == null)
            {
                return success;
            }

            try
            {
                var plcSoftware = GetFirstPlcSoftware();
                var udtGroup = plcSoftware?.TypeGroup;

                if (udtGroup != null)
                {
                    success = ExportUserDefinedTypesRecursive(udtGroup, exportPath) || success;
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error exporting user defined types: {ex.Message}");
            }

            return success;
        }

        #endregion

        public void Dispose()
        {
            try
            {
                (_projectBase as Project)?.Close();
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error closing the project: {ex.Message}");
            }

            try
            {
                _tiaPortal?.Dispose();
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error closing the portal: {ex.Message}");
            }
        }

        #endregion

        #region private

        private PlcSoftware? GetFirstPlcSoftware()
        {
            if (_projectBase?.Devices == null)
            {
                return null;
            }

            if (_plcSoftware != null)
            {
                return _plcSoftware;
            }

            foreach (Device device in _projectBase.Devices)
            {
                if (device.DeviceItems != null)
                {
                    foreach (DeviceItem deviceItem in device.DeviceItems)
                    {
                        // Use GetService to get the SoftwareContainer feature
                        var softwareContainer = deviceItem.GetService<SoftwareContainer>();
                        if (softwareContainer?.Software is PlcSoftware plcSoftware)
                        {
                            _plcSoftware = plcSoftware;

                            return plcSoftware;
                        }
                    }
                }
            }

            return null;
        }


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
                    catch (Exception ex)
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
                    catch (Exception ex)
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
                catch (Exception ex)
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

        private PlcBlockGroup? RetrievePlcBlockGroupByPath(string path)
        {
            if (_projectBase == null)
            {
                return null;
            }

            var plcSoftware = GetFirstPlcSoftware();

            if (plcSoftware?.BlockGroup == null)
            {
                return null;
            }

            // Split the path by '/' to get each group name
            var groupNames = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            PlcBlockGroup? currentGroup = plcSoftware.BlockGroup;

            var attribs = currentGroup.GetAttributes(AttributeAccessOptions.ReadOnly);
            foreach (var attr in attribs)
            {
                Debug.WriteLine($"RetrievePlcBlockGroupByPath.currentGroup.Attribute: {attr}");
            }

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
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur while accessing the parent
                    break;
                }
                
                if(nullableGroup !=null)
                {
                    path = $"{nullableGroup.Name}/{path}";
                }
            }

            // cut off the first name auf first 'Programmbausteine/'
            path = path.Substring(path.IndexOf('/') + 1);

            return path;
        }

        #endregion
    }
}
