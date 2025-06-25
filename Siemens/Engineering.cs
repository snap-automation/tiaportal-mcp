using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TiaMcpServer.Siemens
{
    // manual Siemens.Engineering.dll resolve not used.
    public class Engineering
    {
        public static Assembly? Resolve(object sender, ResolveEventArgs args)
        {
            string siemensEngineeringDllName = "Siemens.Engineering";
            string subKeyName = @"SOFTWARE\Siemens\Automation\Openness";

            var assemblyName = new AssemblyName(args.Name);

            if (!assemblyName.Name.StartsWith(siemensEngineeringDllName))
                return null;

            using var regBaseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            using var opennessBaseKey = regBaseKey.OpenSubKey(subKeyName);

            using var registryKeyLatestTiaVersion = opennessBaseKey?.OpenSubKey(opennessBaseKey.GetSubKeyNames().Last());

            var requestedVersionOfAssembly = assemblyName.Version.ToString();

            using var assemblyVersionSubKey = registryKeyLatestTiaVersion
                                             ?.OpenSubKey("PublicAPI")
                                             ?.OpenSubKey(requestedVersionOfAssembly);

            var siemensEngineeringAssemblyPath = assemblyVersionSubKey?.GetValue(siemensEngineeringDllName).ToString();

            if (siemensEngineeringAssemblyPath == null || !File.Exists(siemensEngineeringAssemblyPath))
                return null;

            var assembly = Assembly.LoadFrom(siemensEngineeringAssemblyPath);

            return assembly;
        }
    }
}
