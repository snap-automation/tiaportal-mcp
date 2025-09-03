namespace TiaMcpServer.Test
{
    public class ExportBlocksInfo
    {
        public string softwarePath { get; set; }
        public string exportPath { get; set; }
        public string regexName { get; set; }
        public bool preservePath { get; set; }

        public override string ToString()
        {
            return $"SoftwarePath: {softwarePath}, ExportPath: {exportPath}, RegexName: {regexName}, PreservePath: {preservePath}";
        }
    }
}