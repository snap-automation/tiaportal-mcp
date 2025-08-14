using ModelContextProtocol.Server;
using System.ComponentModel;

namespace TiaMcpServer.ModelContextProtocol
{
    [McpServerPromptType]
    public static class McpPrompts
    {
        #region Basic Connection Templates

        [McpServerPrompt, Description("Connect to TIA Portal")]
        public static string ConnectToPortal()
        {
            return @"Connect to TIA Portal using the MCP server.
This will establish a connection to either a running TIA Portal instance or start a new one.
Use the connect_portal tool to initiate the connection.";
        }

        [McpServerPrompt, Description("Open a TIA Portal project")]
        public static string OpenProject(string projectPath)
        {
            return $@"Open the TIA Portal project located at: {projectPath}
Make sure to provide the full path to the project file (.ap18, .ap19, .ap20, etc.) or local session file (.als18, .als19, .als20, etc.).
Use the open_project tool with the specified path.";
        }

        [McpServerPrompt, Description("Close the currently open TIA Portal project")]
        public static string CloseProject()
        {
            return @"Close the currently open TIA Portal project.
This will close the active project and return TIA Portal to the main screen.
Use the close_project tool to close the current project.";
        }

        #endregion

        #region Project Information Templates

        [McpServerPrompt, Description("Get the structure of the current TIA Portal project")]
        public static string GetProjectStructure()
        {
            return @"Retrieve the complete structure of the current TIA Portal project.
This will show all devices, device items, groups, and PLC software in a hierarchical format.
Use the get_structure tool to display the project organization and locate software paths for other operations.";
        }

        #endregion

        #region Export Templates

        [McpServerPrompt, Description("Export blocks from PLC software")]
        public static string ExportBlocks(string softwarePath, string exportPath, string regexName, bool preservePath)
        {
            return $@"Export blocks from PLC software at: {softwarePath}
Export to directory: {exportPath}
{(string.IsNullOrEmpty(regexName) ? "Export all blocks" : $"Filter blocks using regex pattern: {regexName}")}
{(preservePath ? "Preserve original folder structure" : "Export to flat structure")}

Common parameter values:
- regexPattern: Use empty string """" for all blocks, or patterns like ""FB_.*"" for function blocks
- preservePath: Use false for flat export, true to maintain folder structure

Use the export_blocks tool with these parameters:
- softwarePath: {softwarePath}
- exportPath: {exportPath}
- regexName: {regexName}
- preservePath: {preservePath.ToString().ToLower()}";
        }

        [McpServerPrompt, Description("Export types from PLC software")]
        public static string ExportTypes(string softwarePath, string exportPath, string regexName, bool preservePath)
        {
            return $@"Export user-defined types from PLC software at: {softwarePath}
Export to directory: {exportPath}
{(string.IsNullOrEmpty(regexName) ? "Export all types" : $"Filter types using regex pattern: {regexName}")}
{(preservePath ? "Preserve original folder structure" : "Export to flat structure")}

Use the export_types tool with these parameters:
- softwarePath: {softwarePath}
- exportPath: {exportPath}
- regexName: {regexName}
- preservePath: {preservePath.ToString().ToLower()}";
        }

        [McpServerPrompt, Description("Export blocks as documents (.s7dcl/.s7res format)")]
        public static string ExportBlocksAsDocuments(string softwarePath, string exportPath, string regexName, bool preservePath)
        {
            return $@"Export blocks as SIMATIC SD documents (.s7dcl/.s7res format) from PLC software at: {softwarePath}
Export to directory: {exportPath}
{(string.IsNullOrEmpty(regexName) ? "Export all blocks as documents" : $"Filter blocks using regex pattern: {regexName}")}
{(preservePath ? "Preserve original folder structure" : "Export to flat structure")}
This format is useful for documentation and external tools that work with SIMATIC SD format.

Use the export_blocks_as_documents tool with these parameters:
- softwarePath: {softwarePath}
- exportPath: {exportPath}
- regexName: {regexName}
- preservePath: {preservePath.ToString().ToLower()}";
        }

        #endregion

        #region Convenience Export Templates

        [McpServerPrompt, Description("Export all blocks from PLC software (flat structure)")]
        public static string ExportAllBlocksFlattened(string softwarePath, string exportPath)
        {
            return ExportBlocks(softwarePath, exportPath, "", false);
        }

        [McpServerPrompt, Description("Export all blocks from PLC software (preserve folder structure)")]
        public static string ExportAllBlocksStructured(string softwarePath, string exportPath)
        {
            return ExportBlocks(softwarePath, exportPath, "", true);
        }

        [McpServerPrompt, Description("Export all types from PLC software (flat structure)")]
        public static string ExportAllTypesFlattened(string softwarePath, string exportPath)
        {
            return ExportTypes(softwarePath, exportPath, "", false);
        }

        [McpServerPrompt, Description("Export all types from PLC software (preserve folder structure)")]
        public static string ExportAllTypesStructured(string softwarePath, string exportPath)
        {
            return ExportTypes(softwarePath, exportPath, "", true);
        }

        [McpServerPrompt, Description("Export all blocks as documents from PLC software (flat structure)")]
        public static string ExportAllBlocksAsDocumentsFlattened(string softwarePath, string exportPath)
        {
            return ExportBlocksAsDocuments(softwarePath, exportPath, "", false);
        }

        [McpServerPrompt, Description("Export all blocks as documents from PLC software (preserve folder structure)")]
        public static string ExportAllBlocksAsDocumentsStructured(string softwarePath, string exportPath)
        {
            return ExportBlocksAsDocuments(softwarePath, exportPath, "", true);
        }

        #endregion
    }
}