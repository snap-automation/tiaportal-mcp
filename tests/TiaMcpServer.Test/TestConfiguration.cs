using System.Collections.Generic;

namespace TiaMcpServer.Test
{
    public class TestConfiguration
    {
        public List<TiaVersion> TiaVersions { get; set; }
    }

    public class TiaVersion
    {
        public string Version { get; set; }
        public List<Project> Projects { get; set; }
    }

    public class Project
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public string NewPath { get; set; }
        public string PlcSoftwarePath { get; set; } // For single path scenarios
        public List<string> PlcSoftwarePaths { get; set; } // For multiple paths
        public string ExportPath { get; set; } // For single path scenarios
        public List<string> ExportPaths { get; set; } // For multiple paths
    }
}