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
                    // requires .net8+
                    //await RunHttpHost();
                }
            }
            else
            {
                System.Console.WriteLine("User is not in the required group. Exiting...");
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

        //public static async Task RunHttpHost()
        //{
        //    var builder = WebApplication.CreateBuilder();

        //    // MCP Server Services hinzufügen
        //    builder.Services
        //        .AddMcpServer()
        //        .WithToolsFromAssembly();

        //    var app = builder.Build();

        //    // HTTP Endpoint für MCP
        //    app.MapPost("/mcp", async (HttpContext context, IMcpServer mcpServer) =>
        //    {
        //        try
        //        {
        //            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        //            var response = await mcpServer.ProcessRequestAsync(requestBody);

        //            context.Response.ContentType = "application/json";
        //            await context.Response.WriteAsync(response);
        //        }
        //        catch (Exception ex)
        //        {
        //            context.Response.StatusCode = 500;
        //            await context.Response.WriteAsync($"Error: {ex.Message}");
        //        }
        //    });

        //    Console.WriteLine("MCP Server running on http://localhost:5000/mcp");
        //    await app.RunAsync();
        //}
    }
}
