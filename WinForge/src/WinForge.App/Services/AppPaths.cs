using System.IO;

namespace WinForge.Services;

public static class AppPaths
{
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "WinForge");

    public static string Logs => Path.Combine(Root, "Logs");
    public static string Snapshots => Path.Combine(Root, "Snapshots");
    public static string Reports => Path.Combine(Root, "Reports");
    public static string StartupStateFile => Path.Combine(Root, "startup-state.json");
    public static string StartupDisabled => Path.Combine(Root, "StartupDisabled");
    public static string Profiles => Path.Combine(Root, "Profiles");

    public static void Ensure()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Snapshots);
        Directory.CreateDirectory(Reports);
        Directory.CreateDirectory(StartupDisabled);
        Directory.CreateDirectory(Profiles);
    }
}
