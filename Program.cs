using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TiaMcpServer.Siemens;

namespace TiaMcpServer
{
    public class Program
    {
        public class CliOptions
        {
            public int? TiaMajorVersion { get; set; }
        }

        public static CliOptions ParseArgs(string[] args)
        {
            var options = new CliOptions();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "-tia-major-version":
                    case "--tia-major-version":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int v))
                        {
                            options.TiaMajorVersion = v;
                            i++;
                        }
                        break;
                }
            }
            return options;
        }

        public static async Task Main(string[] args)
        {
            var options = ParseArgs(args);

            Openness.Initialize(options.TiaMajorVersion);

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
