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
        public List<BlockPathInfo>? BlockPaths { get; set; }
        public List<TypePathInfo>? TypePaths { get; set; }
        public List<BlocksPathInfo>? BlocksPaths { get; set; }
        public List<TypesPathInfo>? TypesPaths { get; set; }
        public List<ExportBlockInfo>? ExportBlock { get; set; }
        public List<ImportBlockInfo>? ImportBlock { get; set; }
        public List<ExportTypeInfo>? ExportType { get; set; }
        public List<ImportTypeInfo>? ImportType { get; set; }
        public List<ExportBlocksInfo>? ExportBlocks { get; set; }
        public List<ExportTypesInfo>? ExportTypes { get; set; }
        public List<ExportBlockAsDocumentsInfo>? ExportBlockAsDocuments { get; set; }
        public List<ExportBlocksAsDocumentsInfo>? ExportBlocksAsDocuments { get; set; }



        public override string ToString()
        {
            return $"{Name} (V{Version})";
        }
    }
}