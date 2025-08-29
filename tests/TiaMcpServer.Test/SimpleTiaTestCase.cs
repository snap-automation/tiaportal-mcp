using System.Collections.Generic;

namespace TiaMcpServer.Test
{
    public class SimpleTiaTestCase
    {
        public string? Name { get; set; }
        public int Version { get; set; }
        public bool MultiUser { get; set; }
        public List<PlcSoftwareInfo>? PlcSoftware { get; set; }
        public string? ProjectPath { get; set; }
        public string? ExportRoot { get; set; }
        public List<string>? DevicePaths { get; set; }
        public List<string>? DeviceItemPaths { get; set; }
        public List<string>? BlockPaths { get; set; }

        public override string ToString()
        {
            return $"{Name} (V{Version})";
        }
    }
}