namespace TiaMcpServer
{
    public class CliOptions
    {
        public int? TiaMajorVersion { get; set; }
        public string? Transport { get; set; } // "stdio" or "http"

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

                    case "-transport":
                    case "--transport":
                        if (i + 1 < args.Length)
                        {
                            options.Transport = args[i + 1].ToLower();
                            i++;
                        }
                        break;
                }
            }
            return options;
        }
    }
}
