namespace TiaMcpServer.Test
{
    public class ExportTypeInfo
    {
        public string softwarePath { get; set; }
        public string exportPath { get; set; }
        public string typePath { get; set; }
        public bool preservePath { get; set; }

        public override string ToString()
        {
            return $"SoftwarePath: {softwarePath}, ExportPath: {exportPath}, TypePath: {typePath}, PreservePath: {preservePath}";
        }
    }
}
