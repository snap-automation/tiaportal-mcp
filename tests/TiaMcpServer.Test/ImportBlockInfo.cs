namespace TiaMcpServer.Test
{
    public class ImportBlockInfo
    {
        public string softwarePath { get; set; }
        public string groupPath { get; set; }
        public string importPath { get; set; }

        public override string ToString()
        {
            return $"SoftwarePath: {softwarePath}, GroupPath: {groupPath}, ImportPath: {importPath}";
        }
    }
}

