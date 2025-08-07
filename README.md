# TIA-Portal MCP-Server

A MCP server to connect to Siemens TIA Portal and interact with TIA Portal projects directly from VS Code with GitHub Copilot.

## Features

- Connect to a TIA Portal instance
- Browse and interact with TIA Portal projects
- Perform basic project operations from within VS Code

## Requirements

- __.net Framework 4.8__ installed
- __Siemens TIA Portal V20__ installed and running on your machine
- Check if under `Environment Variables/User variable for user <name>` the variable `TiaPortalLocation` is set to `C:\Program Files\Siemens\Automation\Portal V20`
- User must be in Windows User Group `Siemens TIA Openness`

## Copilot Chat

(tiaportal-mcp, v0.0.9)

```
Give me a complete, detailed list of all functions from vscode-tiaportal-mcp server
```

```
Here is a complete, detailed list of all functions provided by the vscode-tiaportal-mcp server:

mcp_vscode-tiapor_close_project
 Close the current TIA-Portal project/session.

mcp_vscode-tiapor_compile_software
 Compile the PLC software.
 Parameters:
 - password (optional): Password for administration access.
 - softwarePath: Path in the project structure to the PLC software.

mcp_vscode-tiapor_connect_portal
 Connect to TIA-Portal.

mcp_vscode-tiapor_disconnect_portal
 Disconnect from TIA-Portal.

mcp_vscode-tiapor_export_as_documents
 Export as documents (.s7dcl/.s7res) from a block in the PLC software to a specified path.
 Parameters:
 - blockPath: Path to the block in the project structure.
 - exportPath: Path to export the documents.
 - preservePath (optional): Preserve the PLC software structure.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_export_block
 Export a block from PLC software to a file.
 Parameters:
 - blockPath: Path to the block.
 - exportPath: Path to export the block.
 - preservePath (optional): Preserve the PLC software structure.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_export_blocks
 Export all blocks from the PLC software to a path.
 Parameters:
 - exportPath: Path to export the blocks.
 - preservePath (optional): Preserve the PLC software structure.
 - regexName (optional): Name or regex to filter blocks.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_export_type
 Export a type from the PLC software.
 Parameters:
 - exportPath: Path to export the type.
 - preservePath (optional): Preserve the PLC software structure.
 - softwarePath: Path to the PLC software.
 - typePath: Path to the type in the project structure.

mcp_vscode-tiapor_export_types
 Export types from the PLC software to a path.
 Parameters:
 - exportPath: Path to export the types.
 - preservePath (optional): Preserve the PLC software structure.
 - regexName (optional): Name or regex to filter types.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_get_block_info
 Get information about a block in the PLC software.
 Parameters:
 - blockPath: Path to the block.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_get_blocks
 Get a list of blocks in the PLC software.
 Parameters:
 - regexName (optional): Name or regex to filter blocks.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_get_device_info
 Get information from a device in the current project/session.
 Parameters:
 - devicePath: Path to the device.

mcp_vscode-tiapor_get_device_item_info
 Get information from a device item in the current project/session.
 Parameters:
 - deviceItemPath: Path to the device item.

mcp_vscode-tiapor_get_devices
 Get a list of all devices in the project/session.

mcp_vscode-tiapor_get_open_projects
 Get a list of open local projects/sessions.

mcp_vscode-tiapor_get_software_info
 Get PLC software information.
 Parameters:
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_get_state
 Get the state of the TIA-Portal MCP server.

mcp_vscode-tiapor_get_structure
 Get the structure of the current local project/session.

mcp_vscode-tiapor_get_type_info
 Get information about a type in the PLC software.
 Parameters:
 - softwarePath: Path to the PLC software.
 - typePath: Path to the type.

mcp_vscode-tiapor_get_types
 Get a list of types from the PLC software.
 Parameters:
 - regexName (optional): Name or regex to filter types.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_import_block
 Import a block file to the PLC software.
 Parameters:
 - groupPath: Path to the group where to import the block.
 - importPath: Path of the XML file to import.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_import_type
 Import a type from a file into the PLC software.
 Parameters:
 - groupPath: Path to the group where to import the type.
 - importPath: Path of the XML file to import.
 - softwarePath: Path to the PLC software.

mcp_vscode-tiapor_is_connected
 Check if TIA-Portal is connected.

mcp_vscode-tiapor_open_project
 Open a TIA-Portal local project/session.
 Parameters:
 - path: Path to the project/session.

mcp_vscode-tiapor_save_as_project
 Save the current TIA-Portal project/session with a new name.
 Parameters:
 - newProjectPath: New path to save the project.

mcp_vscode-tiapor_save_project
 Save the current TIA-Portal local project/session.

All function responses from TIA-Portal use special wording and must not be modified.
```

## mcphost with ollama using qwen2.5

```
  ┃                                                                                            ┃
  ┃  Give me a complete, detailed list of all functionalities of the tia portal mcp server     ┃
  ┃                                                                                   (20:57)  ┃
  ┃                                                                                            ┃

  ┃                                                                                            ┃
  ┃  To provide you with a comprehensive list of functionalities related to the TIA Portal     ┃
  ┃  MCP (Master Control Program) server, I will go through each available function and        ┃
  ┃  describe its purpose:                                                                     ┃
  ┃                                                                                            ┃
  ┃  1. Connecting/Disconnecting from TIA-Portal:                                              ┃
  ┃  • vscode-tiaportal-mcp__connect_portal: Establishes connection to the TIA Portal.         ┃
  ┃  • vscode-tiaportal-mcp__disconnect_portal: Disconnects from the TIA Portal.               ┃
  ┃  2. Managing Projects/Sessions:                                                            ┃
  ┃  • vscode-tiaportal-mcp__open_project: Opens a local project or session in TIA Portal.     ┃
  ┃  • vscode-tiaportal-mcp__close_project: Closes the current open project/session.           ┃
  ┃  • vscode-tiaportal-mcp__save_project: Saves changes to the current project/session.       ┃
  ┃  • vscode-tiaportal-mcp__get_open_projects: Retrieves a list of currently opened           ┃
  ┃  projects/sessions.                                                                        ┃
  ┃  3. Project Structure and Content:                                                         ┃
  ┃  • vscode-tiaportal-mcp__get_structure: Gets the overall structure of the current local    ┃
  ┃  project/session.                                                                          ┃
  ┃  • vscode-tiaportal-mcp__get_types: Lists types available in the plc software.             ┃
  ┃  • vscode-tiaportal-mcp__get_blocks: Retrieves a list of blocks in the plc software that   ┃
  ┃  match a given name or regular expression.                                                 ┃
  ┃  4. Importing/Exporting Content:                                                           ┃
  ┃  • vscode-tiaportal-mcp__import_type: Imports types from an XML file into the plc          ┃
  ┃  software.                                                                                 ┃
  ┃  • vscode-tiaportal-mcp__export_type: Exports specific types to a defined path,            ┃
  ┃  optionally preserving the original structure.                                             ┃
  ┃  • vscode-tiaportal-mcp__import_block: Imports a block from an XML file into the plc       ┃
  ┃  software.                                                                                 ┃
  ┃  • vscode-tiaportal-mcp__export_block: Exports blocks to specified locations while         ┃
  ┃  maintaining structural integrity if desired.                                              ┃
  ┃  • vscode-tiaportal-mcp__export_types: Exports all or specific types within the plc        ┃
  ┃  software.                                                                                 ┃
  ┃  • vscode-tiaportal-mcp__export_blocks: Exports all blocks in the project/session.         ┃
  ┃  5. Block and Device Management:                                                           ┃
  ┃  • vscode-tiaportal-mcp__get_block_info: Retrieves detailed information about a block      ┃
  ┃  located in the plc software.                                                              ┃
  ┃  • vscode-tiaportal-mcp__get_device_info: Provides details about a device present in the   ┃
  ┃  project/session.                                                                          ┃
  ┃  • vscode-tiaportal-mcp__get_device_item_info: Gets specific information from an item      ┃
  ┃  within a device.                                                                          ┃
  ┃  6. Software Compilation:                                                                  ┃
  ┃  • vscode-tiaportal-mcp__compile_software: Compiles the PLC software, ensuring no errors   ┃
  ┃  and ready for deployment.                                                                 ┃
  ┃  7. State Information:                                                                     ┃
  ┃  • vscode-tiaportal-mcp__get_state: Retrieves information about the current state of TIA   ┃
  ┃  Portal or the connected server.                                                           ┃
  ┃  8. Checking Connection Status:                                                            ┃
  ┃  • vscode-tiaportal-mcp__is_connected: Checks if the connection to TIA Portal is active.   ┃
  ┃  9. Managing Types and Blocks in Detail:                                                   ┃
  ┃  • vscode-tiaportal-mcp__get_types: Lists all types within a specified software path.      ┃
  ┃  • vscode-tiaportal-mcp__get_block_info: Retrieves details about specific blocks.          ┃
  ┃  • vscode-tiaportal-mcp__get_software_info: Provides information on the PLC software.      ┃
  ┃  10. Exporting as Documents:                                                               ┃
  ┃  • vscode-tiaportal-mcp__export_as_documents: Exports block content into document formats  ┃
  ┃  (e.g., .s7dcl, .s7res).                                                                   ┃
  ┃                                                                                            ┃
  ┃                                                                                            ┃
  ┃  These functionalities cover a broad range of operations necessary for managing and        ┃
  ┃  manipulating projects in TIA Portal from an external tool or script. If you need more     ┃
  ┃  detailed information on any specific function or require further assistance with these    ┃
  ┃  functions, please let me know!                                                            ┃
  ┃   qwen2.5 (20:58)                                                                          ┃
  ┃                                                                                            ┃
```