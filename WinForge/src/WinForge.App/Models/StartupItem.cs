namespace WinForge.Models;

public sealed class StartupItem
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool OriginalIsEnabled { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Hive { get; set; } = string.Empty;
    public string RegistryPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public string DisabledPath { get; set; } = string.Empty;
}
