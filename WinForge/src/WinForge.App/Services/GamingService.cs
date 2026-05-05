using System.Text;
using WinForge.Models;

namespace WinForge.Services;

public sealed class GamingService
{
    private readonly CommandService _commands = new();

    private static string GamingFolder => Path.Combine(AppPaths.Root, "Gaming");
    private static string GamingStatePath => Path.Combine(GamingFolder, "gaming-state.json");
    private static string SessionStatePath => Path.Combine(GamingFolder, "gaming-session.json");
    private static string ReportPath => Path.Combine(GamingFolder, "gaming-report.txt");

    public Task<string> ApplyMaximumFpsAsync(GamingOptions options)
    {
        return _commands.RunPowerShellAsync(BuildApplyScript(options));
    }

    public Task<string> RevertMaximumFpsAsync()
    {
        return _commands.RunPowerShellAsync(BuildRevertScript());
    }

    public Task<string> StartSessionAsync(GamingOptions options)
    {
        return _commands.RunPowerShellAsync(BuildStartSessionScript(options));
    }

    public Task<string> EndSessionAsync()
    {
        return _commands.RunPowerShellAsync(BuildEndSessionScript());
    }

    public Task<string> SetSelectedGameHighPerformanceAsync(string gamePath)
    {
        return _commands.RunPowerShellAsync(BuildGpuPreferenceScript(gamePath));
    }

    public Task<string> GenerateReportAsync()
    {
        return _commands.RunPowerShellAsync(BuildReportScript());
    }

    private static string BuildApplyScript(GamingOptions options)
    {
        var script = new StringBuilder();
        AddHeader(script);
        script.AppendLine("Write-Output 'Applying Maximum FPS Gaming Mode'");
        script.AppendLine("$currentPower = (powercfg /getactivescheme) -join ' '");
        script.AppendLine("$state = [ordered]@{");
        script.AppendLine("    Created = (Get-Date).ToString('s')");
        script.AppendLine("    PreviousPowerSchemeText = $currentPower");
        script.AppendLine("    MouseSpeed = Get-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseSpeed'");
        script.AppendLine("    MouseThreshold1 = Get-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseThreshold1'");
        script.AppendLine("    MouseThreshold2 = Get-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseThreshold2'");
        script.AppendLine("    AutoGameModeEnabled = Get-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'AutoGameModeEnabled'");
        script.AppendLine("    AllowAutoGameMode = Get-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'AllowAutoGameMode'");
        script.AppendLine("    GameDvrEnabled = Get-RegistryValue 'HKCU:\\System\\GameConfigStore' 'GameDVR_Enabled'");
        script.AppendLine("    GameDvrFseBehaviorMode = Get-RegistryValue 'HKCU:\\System\\GameConfigStore' 'GameDVR_FSEBehaviorMode'");
        script.AppendLine("    AppCaptureEnabled = Get-RegistryValue 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR' 'AppCaptureEnabled'");
        script.AppendLine("    UseNexusForGameBarEnabled = Get-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'UseNexusForGameBarEnabled'");
        script.AppendLine("    HwSchMode = Get-RegistryValue 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers' 'HwSchMode'");
        script.AppendLine("}");
        script.AppendLine("$state | ConvertTo-Json -Depth 5 | Out-File -FilePath $statePath -Encoding UTF8");
        script.AppendLine("Write-Output \"Saved gaming state to $statePath\"");

        if (options.EnableGameMode)
        {
            script.AppendLine("Ensure-Key 'HKCU:\\Software\\Microsoft\\GameBar'");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Software\\Microsoft\\GameBar' -Name 'AutoGameModeEnabled' -Type DWord -Value 1");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Software\\Microsoft\\GameBar' -Name 'AllowAutoGameMode' -Type DWord -Value 1");
            script.AppendLine("Write-Output 'Game Mode enabled.'");
        }

        if (options.BestPerformancePower)
        {
            script.AppendLine("try { powercfg /setactive SCHEME_MIN | Out-Null; Write-Output 'Best performance power plan requested.' } catch { Write-Output \"Power plan change failed: $($_.Exception.Message)\" }");
        }

        if (options.DisableUsbAndPciePowerSaving)
        {
            script.AppendLine("Set-PowerAcValue '2a737441-1930-4402-8d77-b2bebba308a3' '48e6b7a6-50f5-4782-a5d4-53bb8f07e226' 0 'USB selective suspend disabled for AC.'");
            script.AppendLine("Set-PowerAcValue '501a4d13-42af-4429-9fd1-a8218c268e20' 'ee12f906-d277-404b-b6da-e5fa1a576df5' 0 'PCIe link state power saving disabled for AC.'");
            script.AppendLine("try { powercfg /setactive SCHEME_CURRENT | Out-Null } catch {}");
        }

        if (options.DisableBackgroundRecording)
        {
            script.AppendLine("Ensure-Key 'HKCU:\\System\\GameConfigStore'");
            script.AppendLine("Ensure-Key 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR'");
            script.AppendLine("Set-ItemProperty 'HKCU:\\System\\GameConfigStore' -Name 'GameDVR_Enabled' -Type DWord -Value 0");
            script.AppendLine("Set-ItemProperty 'HKCU:\\System\\GameConfigStore' -Name 'GameDVR_FSEBehaviorMode' -Type DWord -Value 2");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR' -Name 'AppCaptureEnabled' -Type DWord -Value 0");
            script.AppendLine("Write-Output 'Background capture and Game DVR recording disabled.'");
        }

        if (options.DisableXboxGameBar)
        {
            script.AppendLine("Ensure-Key 'HKCU:\\Software\\Microsoft\\GameBar'");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Software\\Microsoft\\GameBar' -Name 'UseNexusForGameBarEnabled' -Type DWord -Value 0");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Software\\Microsoft\\GameBar' -Name 'ShowStartupPanel' -Type DWord -Value 0");
            script.AppendLine("Write-Output 'Xbox Game Bar launch overlay reduced.'");
        }

        if (options.EnableHardwareGpuScheduling)
        {
            script.AppendLine("try { Ensure-Key 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers'; Set-ItemProperty 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers' -Name 'HwSchMode' -Type DWord -Value 2; Write-Output 'Hardware accelerated GPU scheduling requested. Restart may be required.' } catch { Write-Output \"HAGS change failed: $($_.Exception.Message)\" }");
        }

        if (options.DisableMouseAcceleration)
        {
            script.AppendLine("Set-ItemProperty 'HKCU:\\Control Panel\\Mouse' -Name 'MouseSpeed' -Value '0'");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Control Panel\\Mouse' -Name 'MouseThreshold1' -Value '0'");
            script.AppendLine("Set-ItemProperty 'HKCU:\\Control Panel\\Mouse' -Name 'MouseThreshold2' -Value '0'");
            script.AppendLine("Write-Output 'Mouse acceleration disabled.'");
        }

        if (options.DisableNetworkPowerSaving)
        {
            script.AppendLine("try { Get-NetAdapter -Physical -ErrorAction SilentlyContinue | Where-Object Status -eq 'Up' | ForEach-Object { try { Set-NetAdapterPowerManagement -Name $_.Name -AllowComputerToTurnOffDevice Disabled -ErrorAction Stop | Out-Null; Write-Output \"Network power saving disabled for $($_.Name).\" } catch { Write-Output \"Network power setting skipped for $($_.Name).\" } } } catch { Write-Output \"Network adapter power saving change failed: $($_.Exception.Message)\" }");
        }

        if (options.FlushDns)
        {
            script.AppendLine("try { ipconfig /flushdns | Out-Null; Write-Output 'DNS cache flushed.' } catch { Write-Output 'DNS flush skipped.' }");
        }

        if (options.ApplyGamingDns)
        {
            script.AppendLine("try { Get-DnsClientServerAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -and $_.ServerAddresses.Count -gt 0 } | Out-Null; Write-Output 'Gaming DNS option selected. Use the Network Profiles page to apply a permanent DNS profile.' } catch {}");
        }

        if (options.ClearDirectXShaderCache)
        {
            AddShaderCacheCleanup(script);
        }

        if (options.DisableVisualEffects)
        {
            script.AppendLine("try { Set-ItemProperty 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects' -Name 'VisualFXSetting' -Type DWord -Value 2; Write-Output 'Visual effects set to performance preference.' } catch { Write-Output 'Visual effects change skipped.' }");
        }

        if (options.CloseBackgroundApps)
        {
            AddCloseBackgroundApps(script, options.BackgroundApps);
        }

        script.AppendLine("Write-Output 'Maximum FPS Gaming Mode finished.'");
        script.AppendLine("Write-Output 'Restart recommended if GPU scheduling, overlays or driver backed settings were changed.'");
        return script.ToString();
    }

    private static string BuildRevertScript()
    {
        var script = new StringBuilder();
        AddHeader(script);
        script.AppendLine("if (-not (Test-Path $statePath)) { Write-Output 'No gaming state file was found. Nothing to revert.'; exit 0 }");
        script.AppendLine("$state = Get-Content $statePath -Raw | ConvertFrom-Json");
        script.AppendLine("Write-Output 'Reverting Maximum FPS Gaming Mode where saved values are available.'");
        script.AppendLine("$schemeGuid = $null");
        script.AppendLine("if ($state.PreviousPowerSchemeText -match '([a-fA-F0-9-]{36})') { $schemeGuid = $Matches[1] }");
        script.AppendLine("if ($schemeGuid) { try { powercfg /setactive $schemeGuid | Out-Null; Write-Output \"Restored power scheme $schemeGuid.\" } catch { Write-Output 'Power scheme restore skipped.' } }");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseSpeed' $state.MouseSpeed 'String'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseThreshold1' $state.MouseThreshold1 'String'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseThreshold2' $state.MouseThreshold2 'String'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'AutoGameModeEnabled' $state.AutoGameModeEnabled 'DWord'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'AllowAutoGameMode' $state.AllowAutoGameMode 'DWord'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\System\\GameConfigStore' 'GameDVR_Enabled' $state.GameDvrEnabled 'DWord'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\System\\GameConfigStore' 'GameDVR_FSEBehaviorMode' $state.GameDvrFseBehaviorMode 'DWord'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR' 'AppCaptureEnabled' $state.AppCaptureEnabled 'DWord'");
        script.AppendLine("Restore-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'UseNexusForGameBarEnabled' $state.UseNexusForGameBarEnabled 'DWord'");
        script.AppendLine("Restore-RegistryValue 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers' 'HwSchMode' $state.HwSchMode 'DWord'");
        script.AppendLine("Write-Output 'Gaming revert completed. Restart may be required for some settings.'");
        return script.ToString();
    }

    private static string BuildStartSessionScript(GamingOptions options)
    {
        var script = new StringBuilder();
        AddHeader(script);
        script.AppendLine("Write-Output 'Starting Gaming Session Mode.'");
        script.AppendLine("$session = [ordered]@{ Started = (Get-Date).ToString('s'); PreviousPowerSchemeText = ((powercfg /getactivescheme) -join ' ') }");
        script.AppendLine("$session | ConvertTo-Json -Depth 4 | Out-File -FilePath $sessionPath -Encoding UTF8");
        script.AppendLine("try { powercfg /setactive SCHEME_MIN | Out-Null; Write-Output 'Performance power plan requested for this session.' } catch { Write-Output 'Power plan change skipped.' }");
        if (options.DisableNetworkPowerSaving)
        {
            script.AppendLine("try { Get-NetAdapter -Physical -ErrorAction SilentlyContinue | Where-Object Status -eq 'Up' | ForEach-Object { try { Set-NetAdapterPowerManagement -Name $_.Name -AllowComputerToTurnOffDevice Disabled -ErrorAction Stop | Out-Null } catch {} } ; Write-Output 'Network power saving reduced for this session.' } catch {}");
        }
        if (options.FlushDns)
        {
            script.AppendLine("try { ipconfig /flushdns | Out-Null; Write-Output 'DNS cache flushed.' } catch {}");
        }
        if (options.CloseBackgroundApps)
        {
            AddCloseBackgroundApps(script, options.BackgroundApps);
        }
        script.AppendLine("Write-Output 'Gaming Session Mode is active.'");
        return script.ToString();
    }

    private static string BuildEndSessionScript()
    {
        var script = new StringBuilder();
        AddHeader(script);
        script.AppendLine("if (-not (Test-Path $sessionPath)) { Write-Output 'No active session state was found.'; exit 0 }");
        script.AppendLine("$session = Get-Content $sessionPath -Raw | ConvertFrom-Json");
        script.AppendLine("$schemeGuid = $null");
        script.AppendLine("if ($session.PreviousPowerSchemeText -match '([a-fA-F0-9-]{36})') { $schemeGuid = $Matches[1] }");
        script.AppendLine("if ($schemeGuid) { try { powercfg /setactive $schemeGuid | Out-Null; Write-Output \"Restored previous power scheme $schemeGuid.\" } catch { Write-Output 'Power scheme restore skipped.' } }");
        script.AppendLine("Remove-Item $sessionPath -Force -ErrorAction SilentlyContinue");
        script.AppendLine("Write-Output 'Gaming Session Mode ended.'");
        return script.ToString();
    }

    private static string BuildGpuPreferenceScript(string gamePath)
    {
        var safeGamePath = EscapePowerShell(gamePath);
        var script = new StringBuilder();
        AddHeader(script);
        script.AppendLine("$gamePath = '" + safeGamePath + "'");
        script.AppendLine("if ([string]::IsNullOrWhiteSpace($gamePath) -or -not (Test-Path $gamePath)) { Write-Output 'Choose a valid game exe first.'; exit 0 }");
        script.AppendLine("Ensure-Key 'HKCU:\\Software\\Microsoft\\DirectX\\UserGpuPreferences'");
        script.AppendLine("Set-ItemProperty 'HKCU:\\Software\\Microsoft\\DirectX\\UserGpuPreferences' -Name $gamePath -Value 'GpuPreference=2;' -Type String");
        script.AppendLine("Write-Output \"High performance GPU preference set for $gamePath. Restarting the game may be required.\"");
        return script.ToString();
    }

    private static string BuildReportScript()
    {
        var script = new StringBuilder();
        AddHeader(script);
        script.AppendLine("$lines = New-Object System.Collections.Generic.List[string]");
        script.AppendLine("$lines.Add('WinForge Gaming Report')");
        script.AppendLine("$lines.Add('Generated: ' + (Get-Date))");
        script.AppendLine("$lines.Add('')");
        script.AppendLine("$lines.Add('Power plan')");
        script.AppendLine("$lines.Add(((powercfg /getactivescheme) -join ' '))");
        script.AppendLine("$lines.Add('')");
        script.AppendLine("$lines.Add('Game Mode and capture settings')");
        script.AppendLine("$lines.Add('AutoGameModeEnabled: ' + (Get-RegistryValue 'HKCU:\\Software\\Microsoft\\GameBar' 'AutoGameModeEnabled'))");
        script.AppendLine("$lines.Add('GameDVR Enabled: ' + (Get-RegistryValue 'HKCU:\\System\\GameConfigStore' 'GameDVR_Enabled'))");
        script.AppendLine("$lines.Add('AppCaptureEnabled: ' + (Get-RegistryValue 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR' 'AppCaptureEnabled'))");
        script.AppendLine("$lines.Add('')");
        script.AppendLine("$lines.Add('Mouse')");
        script.AppendLine("$lines.Add('MouseSpeed: ' + (Get-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseSpeed'))");
        script.AppendLine("$lines.Add('MouseThreshold1: ' + (Get-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseThreshold1'))");
        script.AppendLine("$lines.Add('MouseThreshold2: ' + (Get-RegistryValue 'HKCU:\\Control Panel\\Mouse' 'MouseThreshold2'))");
        script.AppendLine("$lines.Add('')");
        script.AppendLine("$lines.Add('Graphics')");
        script.AppendLine("$lines.Add('Hardware GPU scheduling: ' + (Get-RegistryValue 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers' 'HwSchMode'))");
        script.AppendLine("$lines.Add('')");
        script.AppendLine("$lines.Add('Network adapters')");
        script.AppendLine("try { $lines.Add(((Get-NetAdapter -Physical -ErrorAction SilentlyContinue | Select-Object Name, Status, LinkSpeed | Format-Table -AutoSize | Out-String))) } catch { $lines.Add('Network adapter details unavailable.') }");
        script.AppendLine("$lines | Out-File -FilePath $reportPath -Encoding UTF8");
        script.AppendLine("Get-Content $reportPath");
        script.AppendLine("Write-Output \"Report saved to $reportPath\"");
        return script.ToString();
    }

    private static void AddHeader(StringBuilder script)
    {
        script.AppendLine("$ErrorActionPreference = 'Continue'");
        script.AppendLine("$gamingRoot = '" + EscapePowerShell(GamingFolder) + "'");
        script.AppendLine("$statePath = '" + EscapePowerShell(GamingStatePath) + "'");
        script.AppendLine("$sessionPath = '" + EscapePowerShell(SessionStatePath) + "'");
        script.AppendLine("$reportPath = '" + EscapePowerShell(ReportPath) + "'");
        script.AppendLine("New-Item -ItemType Directory -Path $gamingRoot -Force | Out-Null");
        script.AppendLine("function Ensure-Key([string]$Path) { if (-not (Test-Path $Path)) { New-Item -Path $Path -Force | Out-Null } }");
        script.AppendLine("function Get-RegistryValue([string]$Path, [string]$Name) { try { return (Get-ItemProperty -Path $Path -Name $Name -ErrorAction Stop).$Name } catch { return $null } }");
        script.AppendLine("function Restore-RegistryValue([string]$Path, [string]$Name, $Value, [string]$Kind) { try { Ensure-Key $Path; if ($null -eq $Value -or $Value -eq '') { Remove-ItemProperty -Path $Path -Name $Name -ErrorAction SilentlyContinue; Write-Output \"Removed $Name where no previous value existed.\" } else { if ($Kind -eq 'DWord') { Set-ItemProperty -Path $Path -Name $Name -Type DWord -Value ([int]$Value) } else { Set-ItemProperty -Path $Path -Name $Name -Value ([string]$Value) }; Write-Output \"Restored $Name.\" } } catch { Write-Output \"Could not restore $Name.\" } }");
        script.AppendLine("function Set-PowerAcValue([string]$Sub, [string]$Setting, [int]$Value, [string]$Message) { try { powercfg /setacvalueindex SCHEME_CURRENT $Sub $Setting $Value | Out-Null; Write-Output $Message } catch { Write-Output \"Power setting skipped: $Message\" } }");
    }

    private static void AddShaderCacheCleanup(StringBuilder script)
    {
        script.AppendLine("$shaderPaths = @(");
        script.AppendLine("    (Join-Path $env:LOCALAPPDATA 'D3DSCache'),");
        script.AppendLine("    (Join-Path $env:LOCALAPPDATA 'NVIDIA\\DXCache'),");
        script.AppendLine("    (Join-Path $env:LOCALAPPDATA 'NVIDIA\\GLCache'),");
        script.AppendLine("    (Join-Path $env:LOCALAPPDATA 'AMD\\DxCache'),");
        script.AppendLine("    (Join-Path $env:LOCALAPPDATA 'AMD\\GLCache')");
        script.AppendLine(")");
        script.AppendLine("foreach ($path in $shaderPaths) { try { if (Test-Path $path) { Get-ChildItem $path -Recurse -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue; Write-Output \"Cleared shader cache: $path\" } } catch { Write-Output \"Shader cache skipped: $path\" } }");
    }

    private static void AddCloseBackgroundApps(StringBuilder script, string processList)
    {
        var safeList = EscapePowerShell(processList);
        script.AppendLine("$processList = '" + safeList + "'");
        script.AppendLine("$processList.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ } | ForEach-Object { $name = [IO.Path]::GetFileNameWithoutExtension($_); try { Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue; Write-Output \"Closed background app: $name\" } catch { Write-Output \"Could not close: $name\" } }");
    }

    private static string EscapePowerShell(string value)
    {
        return value.Replace("'", "''");
    }
}
