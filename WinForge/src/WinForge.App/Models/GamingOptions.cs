namespace WinForge.Models;

public sealed class GamingOptions
{
    public bool EnableGameMode { get; set; } = true;
    public bool BestPerformancePower { get; set; } = true;
    public bool DisableBackgroundRecording { get; set; } = true;
    public bool DisableNetworkPowerSaving { get; set; } = true;
    public bool DisableUsbAndPciePowerSaving { get; set; } = true;
    public bool DisableMouseAcceleration { get; set; } = true;
    public bool FlushDns { get; set; } = true;
    public bool ClearDirectXShaderCache { get; set; } = true;
    public bool DisableXboxGameBar { get; set; }
    public bool EnableHardwareGpuScheduling { get; set; }
    public bool DisableVisualEffects { get; set; }
    public bool ApplyGamingDns { get; set; }
    public bool CloseBackgroundApps { get; set; }
    public string BackgroundApps { get; set; } = string.Empty;
    public string SelectedGamePath { get; set; } = string.Empty;
}
