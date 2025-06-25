using Siemens.Collaboration.Net;

namespace TiaMcpServer.Siemens
{
    public static class Openness
    {
        public static void Initialize(int? tiaMajorVersion = 20)
        {
            // with nuget packages:
            // 2.1 nuget package: Siemens.Collaboration.Net.TiaPortal.Openness.Resolver
            //     & User Environment Variable: TiaPortalLocation=C:\Program Files\Siemens\Automation\Portal V20
            // 2.2 nuget package: Siemens.Collaboration.Net.TiaPortal.Packages.Openness
            // 2.3 Api.Global.Openness().Initialize(tiaMajorVersion: 20); // fixed version 20

            // Initialize the Openness API with the specified TIA Portal major version
            Api.Global.Openness().Initialize(tiaMajorVersion: tiaMajorVersion);
        }
    }
}
