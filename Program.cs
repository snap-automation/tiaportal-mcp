using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TiaMcpServer.Siemens;

namespace TiaMcpServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Initialize Openness API with the default TIA Portal major version (20)
            Openness.Initialize();

            // Ensure user is in user group 'Siemens TIA Openness'
            if (await Openness.IsUserInGroup())
            {
                await RunHost();
            }
            else
            {
                System.Console.WriteLine("User is not in the required group. Exiting...");
            }
        }

        public static async Task RunHost()
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
