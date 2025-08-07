using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TiaMcpServer.Siemens;

namespace TiaMcpServer
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var options = CliOptions.ParseArgs(args);

            Openness.Initialize(options.TiaMajorVersion);

            // Ensure user is in user group 'Siemens TIA Openness'
            if (await Openness.IsUserInGroup())
            {
                if (options.Transport == null || options.Transport == "stdio")
                {
                    Console.WriteLine("Starting MCP Server with STDIO transport...");

                    await RunStdioHost();
                }
                else
                {
                    Console.WriteLine("HTTP transport not implemented...");
                    // requires .net8+, but we are using .net48
                    // await RunHttpHost();
                }
            }
            else
            {
                Console.WriteLine("User is not in the required group. Exiting...");
            }
        }

        public static async Task RunStdioHost()
        {
            var builder = Host.CreateEmptyApplicationBuilder(settings: null);

            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            await builder.Build().RunAsync();
        }
    }
}
