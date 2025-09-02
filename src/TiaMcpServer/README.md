# TiaMcpServer

Console application that exposes Siemens TIA Portal functionality through the Model Context Protocol (MCP).

## Layout
- `Program.cs` – application entry point that configures logging, dependency injection and starts the MCP server.
- `ModelContextProtocol/` – MCP tool and prompt implementations.
- `Siemens/` – wrappers over the TIA Portal Openness API for project and device operations.

## Build
Target framework is .NET Framework 4.8. Build with Visual Studio on Windows or `msbuild` on the command line. Use the `--tia-major-version` option to target older TIA Portal releases.
