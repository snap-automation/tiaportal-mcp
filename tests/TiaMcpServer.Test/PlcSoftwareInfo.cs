namespace TiaMcpServer.Test
{
    public class PlcSoftwareInfo
    {
        public string Path { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Password))
                return Path;
            return $"{Path} (pwd: {Password})";
        }
    }
}
