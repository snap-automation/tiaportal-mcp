Here is a compact PRP that keeps one test class and one data-driven method, pulls rows from JSON, and runs only the rows that match a single TIA version selected via RunSettings or env var. No helper runner, no class duplication.

# PRP — Dynamic data rows + per-run TIA version

## Summary

Refactor MSTests to:

* Load test cases from a JSON file that includes `version` per case
* Pick a single TIA major version per test run params.
* Filter DynamicData rows to only that version
* Configure assembly binding once per test run according to the selected version
* Skip cleanly when the version is not installed or files are missing

## Files to add or change

1. `tests/TiaMcpServer.Test/tia.testcases.json`
2. `tests/TiaMcpServer.Test/TiaTestCases.cs` — JSON loader + DynamicData provider
3. `tests/TiaMcpServer.Test/TiaTestRuntime.cs` — version selection, registry probe, resolver wiring
4. `tests/TiaMcpServer.Test/PortalE2ETests.cs` — single data-driven test using the matrix
5. Optional run settings: `tests/TiaMcpServer.Test/Tia.V18.runsettings`, `Tia.V20.runsettings`

## JSON schema and example

Place next to the test assembly and mark as “Copy to Output Directory: Copy always”.

```json
[
  {
    "name": "Local_Project1_main",
    "version": 20,
    "projectPath": "C:\\Users\\Automation\\Desktop\\J4U\\NEW\\TestProject1\\TestProject1.ap20",
    "exportRoot": "D:\\Temp\\TIA-Portal\\Project1",
    "plcSoftwarePaths": [ "PLC_0", "PC-System_0/Software PLC_0" ],
    "projectType": "Local"
  },
  {
    "name": "Local_Project1_grouped",
    "version": 18,
    "projectPath": "C:\\Users\\Automation\\Desktop\\J4U\\V18\\TestProject1\\TestProject1.ap18",
    "exportRoot": "D:\\Temp\\TIA-Portal\\Project1_V18",
    "plcSoftwarePaths": [ "Group1/PLC_1", "Group1/PC-System_1/Software PLC_1" ],
    "projectType": "Local"
  },
  {
    "name": "MU_Session1",
    "version": 20,
    "projectPath": "D:\\Siemens\\Sessions\\TestSession1\\TestSession1_LS_1.als20",
    "exportRoot": "D:\\Temp\\TIA-Portal\\TestSession1",
    "plcSoftwarePaths": [ "PC-System_1/Software PLC_1" ],
    "projectType": "MultiUser"
  }
]
```

## Code

**TiaTestRuntime.cs**

```csharp
using Microsoft.Win32;
using System.IO;

namespace TiaMcpServer.Test;

public static class TiaTestRuntime
{
    public static int PickVersionOrDefault()
    {
        var s = context.Properties["TiaMajorVersion"]?.ToString() v : "20";
    }

    public static bool VersionInstalled(int major)
    {
        using var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var k = root.OpenSubKey($@"SOFTWARE\Siemens\Automation\_InstalledSW\TIAP{major}\TIA_Opns");
        var p = k?.GetValue("Path")?.ToString();
        return !string.IsNullOrWhiteSpace(p) && Directory.Exists(p);
    }

    public static ResolveEventHandler? ConfigureVersion(int major)
    {
        TiaMcpServer.Siemens.Engineering.TiaMajorVersion = major;

        if (major < 20)
        {
            AppDomain.CurrentDomain.AssemblyResolve += TiaMcpServer.Siemens.Engineering.Resolver;
            return TiaMcpServer.Siemens.Engineering.Resolver;
        }
        else
        {
            TiaMcpServer.Siemens.Openness.Initialize(major);
            return null;
        }
    }

    public static void CleanupHandler(ResolveEventHandler? handler)
    {
        if (handler != null)
            AppDomain.CurrentDomain.AssemblyResolve -= handler;
    }
}
```

**TiaTestCases.cs**

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TiaMcpServer.Test;

public sealed record TiaTestCase(
    string Name,
    int Version,
    string ProjectPath,
    string ExportRoot,
    IReadOnlyList<string>? PlcSoftwarePaths = null,
    string? ProjectType = null
);

public static class TiaTestCases
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static IEnumerable<TiaTestCase> LoadAll()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tia.testcases.json");
        if (!File.Exists(path)) yield break;

        var data = JsonSerializer.Deserialize<List<TiaTestCase>>(File.ReadAllText(path), _json) ?? new();
        foreach (var c in data) yield return c;
    }

    // DynamicData source filtered to the selected version
    public static IEnumerable<object[]> Selected()
    {
        int selected = TiaTestRuntime.PickVersionOrDefault();
        foreach (var c in LoadAll().Where(x => x.Version == selected))
            yield return new object[] { c };
    }

    // Friendly row names in Test Explorer
    public static string DisplayName(MethodInfo _, object[] data)
    {
        var c = (TiaTestCase)data[0];
        return $"V{c.Version} | {c.Name}";
    }
}
```

**PortalE2ETests.cs**

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiaMcpServer.Siemens;

namespace TiaMcpServer.Test;

[TestClass]
[DoNotParallelize]
public sealed class PortalE2ETests
{
    public TestContext TestContext { get; set; } = null!;

    private static int _version;
    private static bool _versionPresent;
    private static ResolveEventHandler? _handler;
    private ILogger<Portal> _logger = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        _version = TiaTestRuntime.PickVersionOrDefault();
        _versionPresent = TiaTestRuntime.VersionInstalled(_version);
        // do not throw here; mark tests inconclusive in TestInitialize for clearer reporting
    }

    [TestInitialize]
    public void TestInit()
    {
        if (!_versionPresent)
            Assert.Inconclusive($"TIA V{_version} is not installed on this machine.");

        _handler = TiaTestRuntime.ConfigureVersion(_version);

        var lf = LoggerFactory.Create(b => { b.AddConsole(); b.SetMinimumLevel(LogLevel.Information); });
        _logger = lf.CreateLogger<Portal>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        TiaTestRuntime.CleanupHandler(_handler);
    }

    private static string NewTempExport(string root, string caseName)
    {
        var dir = Path.Combine(root, "run_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"), caseName, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [DataTestMethod]
    [DynamicData(nameof(TiaTestCases.Selected), typeof(TiaTestCases),
                 DynamicDataDisplayName = nameof(TiaTestCases.DisplayName))]
    public void Open_Save_SaveAs_smoke(TiaTestCase c)
    {
        if (!File.Exists(c.ProjectPath))
            Assert.Inconclusive($"Missing project file for case {c.Name}: {c.ProjectPath}");

        var portal = new Portal(_logger);

        try
        {
            Assert.IsTrue(portal.ConnectPortal(), "Portal connect failed");
            Assert.IsTrue(portal.OpenProject(c.ProjectPath), "Open failed");
            Assert.IsTrue(portal.SaveProject(), "Save failed");

            var outRoot = NewTempExport(c.ExportRoot, c.Name);
            var newPath = Path.Combine(outRoot, Path.GetFileNameWithoutExtension(c.ProjectPath) + "_Copy" + Path.GetExtension(c.ProjectPath));
            Assert.IsTrue(portal.SaveAsProject(newPath), "SaveAs failed");
            Assert.IsTrue(File.Exists(newPath), "SaveAs produced no file");
        }
        finally
        {
            portal.CloseProject();
        }
    }
}
```

## RunSettings samples

Pick the version per run through Run Settings. You do not need a TestCaseFilter because the provider already filters rows, but it is fine to keep.

**`project runsettings`**

```xml
<RunSettings>
  <TestRunParameters>
    <Parameter name="TiaMajorVersion" value="20" />
  </TestRunParameters>
</RunSettings>
```

## Developer workflow

* Open Test Explorer
* Select the desired `.runsettings` file
* Run tests. Only rows for that version appear under the method
* If the selected version is not installed, tests show as Inconclusive

## Acceptance criteria

* One `[DataTestMethod]` shows a row per case for the selected version
* No Siemens assembly version switching inside the process
* Tests pass on a machine with the selected version installed
* Missing version or project files mark tests Inconclusive, not Failed
* JSON cases can be extended without changing code

## Risks and notes

* Make sure `tia.testcases.json` is copied to output
* Keep classes that touch Portal marked `[DoNotParallelize]`
* Keep unit tests separate and free of Siemens references

If you want, I can also add a second data-driven method later that validates each `plcSoftwarePaths` entry by walking your resolver, but the structure above already supports that by adding another assertion loop in the same method.
