namespace TiaMcpServer.Test
{
    public class ExportBlockInfo
    {
        public string softwarePath { get; set; }
        public string blockPath { get; set; }
        public string exportPath { get; set; }
        public bool preservePath { get; set; }

        public override string ToString()
        {
            return $"SoftwarePath: {softwarePath}, BlockPath: {blockPath}, ExportPath: {exportPath}, PreservePath: {preservePath}";
        }
    }
}
