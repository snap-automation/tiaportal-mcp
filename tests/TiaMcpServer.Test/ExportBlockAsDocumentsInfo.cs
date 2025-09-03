namespace TiaMcpServer.Test
{
    public class ExportBlockAsDocumentsInfo
    {
        public string softwarePath { get; set; }
        public string exportPath { get; set; }
        public string blockPath { get; set; }
        public bool preservePath { get; set; }

        public override string ToString()
        {
            return $"SoftwarePath: {softwarePath}, ExportPath: {exportPath}, BlockPath: {blockPath}, PreservePath: {preservePath}";
        }
    }
}