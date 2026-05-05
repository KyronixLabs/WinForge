using System.Text;
using WinForge.Models;

namespace WinForge.Services;

public sealed class FeatureToolService
{
    private readonly CommandService _commands = new();

    public Task<string> RunAsync(ToolAction action, CancellationToken cancellationToken = default)
    {
        return _commands.RunPowerShellAsync(action.Script, cancellationToken);
    }

    public IReadOnlyList<ToolAction> GetActions(string page)
    {
        return page switch
        {
            "Processes" => ProcessActions(),
            "Storage" => StorageActions(),
            "Downloads" => DownloadActions(),
            "Browsers" => BrowserActions(),
            "Network" => NetworkActions(),
            "Firewall" => FirewallActions(),
            "Privacy Tools" => PrivacyActions(),
            "Security Scan" => SecurityActions(),
            "Repair Tools" => RepairActions(),
            "System Health" => HealthActions(),
            "Health Score" => HealthScoreActions(),
            "Guided Optimise" => GuidedOptimiseActions(),
            "Monitoring" => MonitoringActions(),
            "Services" => ServiceManagerActions(),
            "Hardware" => HardwareActions(),
            "Deep Storage" => DeepStorageActions(),
            "Game Focus" => GameFocusActions(),
            "Network Profiles" => NetworkProfileActions(),
            "Update Repair" => UpdateRepairActions(),
            "Recovery" => RecoveryActions(),
            "Battery" => BatteryActions(),
            "Gaming" => GamingActions(),
            "Developer" => DeveloperActions(),
            "Visuals" => VisualActions(),
            "Context Menu" => ContextMenuActions(),
            "Search" => SearchActions(),
            "Tasks" => TaskActions(),
            "Apps" => AppActions(),
            "Windows Features" => WindowsFeatureActions(),
            "Hosts" => HostsActions(),
            "Proxy and VPN" => ProxyVpnActions(),
            "Time Sync" => TimeSyncActions(),
            "Safety" => SafetyActions(),
            "Changes" => ChangeActions(),
            "Restart" => RestartActions(),
            "Queue" => QueueActions(),
            "Reports" => ReportActions(),
            "Maintenance" => MaintenanceActions(),
            "Ignore List" => IgnoreListActions(),
            "Portable" => PortableActions(),
            "First Run" => FirstRunActions(),

            "Edition" => EditionActions(),
            "App History" => AppHistoryActions(),
            "Baselines" => BaselineActions(),
            "Persistence" => PersistenceActions(),
            "Extensions" => ExtensionActions(),
            "Permissions" => PermissionActions(),
            "Accounts" => AccountActions(),
            "Shares" => ShareActions(),
            "Remote Access" => RemoteAccessActions(),
            "History Cleanup" => HistoryCleanupActions(),
            "Desktop Tools" => DesktopToolActions(),
            "Explorer Tools" => ExplorerToolActions(),
            "Version Defaults" => VersionDefaultActions(),
            "Audit" => AuditActions(),
            "Help" => HelpActions(),
            "Advanced Mode" => AdvancedModeActions(),
            "Updater" => UpdaterActions(),
            "Themes" => ThemeActions(),
            "HomeForge" => HomeForgeActions(),
            "Support Bundle" => SupportBundleActions(),
            "Action Catalog" => ActionCatalogActions(),
            _ => Array.Empty<ToolAction>()
        };
    }

    private static IReadOnlyList<ToolAction> ProcessActions() => new[]
    {
        new ToolAction
        {
            Title = "Top CPU processes",
            Description = "Shows the running processes currently using the most processor time.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-Process | Sort-Object CPU -Descending | Select-Object -First 20 ProcessName, Id, CPU, WorkingSet64, Path | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Top memory processes",
            Description = "Shows the apps and services using the most memory.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 25 ProcessName, Id, @{Name='MemoryMB';Expression={[math]::Round($_.WorkingSet64 / 1MB, 1)}}, Path | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Create process report",
            Description = "Saves a full process report to the WinForge reports folder.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('process-report-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
Get-Process | Sort-Object ProcessName | Select-Object ProcessName, Id, CPU, WorkingSet64, Path | Format-List | Out-File $path -Encoding UTF8
Write-Output "Process report saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> StorageActions() => new[]
    {
        new ToolAction
        {
            Title = "Drive space summary",
            Description = "Shows free space and total size for local drives.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Select-Object DeviceID, VolumeName, @{Name='FreeGB';Expression={[math]::Round($_.FreeSpace/1GB,2)}}, @{Name='SizeGB';Expression={[math]::Round($_.Size/1GB,2)}} | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Large folders in user profile",
            Description = "Scans common user folders and shows which ones are taking the most space.",
            Script = """
$ErrorActionPreference = 'Continue'
$folders = @('Desktop','Documents','Downloads','Pictures','Videos','Music') | ForEach-Object { Join-Path $env:USERPROFILE $_ } | Where-Object { Test-Path $_ }
foreach ($folder in $folders) {
    $size = (Get-ChildItem $folder -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum
    [pscustomobject]@{ Folder = $folder; SizeGB = [math]::Round($size / 1GB, 2) }
} | Sort-Object SizeGB -Descending | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Large files in Downloads",
            Description = "Lists the largest files in the Downloads folder without deleting anything.",
            Script = """
$ErrorActionPreference = 'Continue'
$downloads = Join-Path $env:USERPROFILE 'Downloads'
Get-ChildItem $downloads -File -Recurse -ErrorAction SilentlyContinue | Sort-Object Length -Descending | Select-Object -First 50 Name, @{Name='SizeMB';Expression={[math]::Round($_.Length/1MB,1)}}, FullName | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> DownloadActions() => new[]
    {
        new ToolAction
        {
            Title = "Downloads summary",
            Description = "Shows file counts and total size for the Downloads folder.",
            Script = """
$ErrorActionPreference = 'Continue'
$downloads = Join-Path $env:USERPROFILE 'Downloads'
$files = @(Get-ChildItem $downloads -File -Recurse -ErrorAction SilentlyContinue)
[pscustomobject]@{ Folder=$downloads; Files=$files.Count; SizeGB=[math]::Round(($files | Measure-Object Length -Sum).Sum/1GB,2) } | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Old installers review",
            Description = "Lists old installer style files for review.",
            Script = """
$ErrorActionPreference = 'Continue'
$downloads = Join-Path $env:USERPROFILE 'Downloads'
Get-ChildItem $downloads -File -Recurse -Include *.exe,*.msi,*.msix,*.zip,*.7z,*.rar -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Sort-Object LastWriteTime | Select-Object Name, LastWriteTime, @{Name='SizeMB';Expression={[math]::Round($_.Length/1MB,1)}}, FullName | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open Downloads folder",
            Description = "Opens the Downloads folder so files can be reviewed manually.",
            Script = """
Start-Process (Join-Path $env:USERPROFILE 'Downloads')
Write-Output 'Downloads folder opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> BrowserActions() => new[]
    {
        new ToolAction
        {
            Title = "Browser cache size",
            Description = "Estimates cache size for common browsers.",
            Script = """
$ErrorActionPreference = 'Continue'
$paths = @(
    @{Name='Edge'; Path=Join-Path $env:LOCALAPPDATA 'Microsoft\Edge\User Data\Default\Cache'},
    @{Name='Chrome'; Path=Join-Path $env:LOCALAPPDATA 'Google\Chrome\User Data\Default\Cache'},
    @{Name='Brave'; Path=Join-Path $env:LOCALAPPDATA 'BraveSoftware\Brave-Browser\User Data\Default\Cache'},
    @{Name='Firefox'; Path=Join-Path $env:LOCALAPPDATA 'Mozilla\Firefox\Profiles'}
)
foreach ($item in $paths) {
    if (Test-Path $item.Path) {
        $size = (Get-ChildItem $item.Path -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum
        [pscustomobject]@{ Browser=$item.Name; CachePath=$item.Path; SizeMB=[math]::Round($size/1MB,1) }
    }
} | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open Edge extensions",
            Description = "Opens the Edge extensions page for review.",
            Script = """
Start-Process 'msedge.exe' 'edge://extensions/'
Write-Output 'Edge extensions opened.'
"""
        },
        new ToolAction
        {
            Title = "Open Chrome extensions",
            Description = "Opens the Chrome extensions page if Chrome is installed.",
            Script = """
Start-Process 'chrome.exe' 'chrome://extensions/'
Write-Output 'Chrome extensions opened if Chrome is installed.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> NetworkActions() => new[]
    {
        new ToolAction
        {
            Title = "Network summary",
            Description = "Shows adapters, IP addresses, gateways and DNS servers.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-NetIPConfiguration | Select-Object InterfaceAlias, IPv4Address, IPv4DefaultGateway, DNSServer | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Connection test",
            Description = "Checks gateway, DNS and internet reachability.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Gateway test'
$gateway = (Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway } | Select-Object -First 1).IPv4DefaultGateway.NextHop
if ($gateway) { Test-Connection $gateway -Count 2 | Format-Table -AutoSize | Out-String }
Write-Output 'Internet test'
Test-Connection 1.1.1.1 -Count 2 | Format-Table -AutoSize | Out-String
Write-Output 'DNS test'
Resolve-DnsName microsoft.com | Select-Object -First 5 | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Flush DNS",
            Description = "Clears cached DNS results.",
            Script = """
ipconfig /flushdns
Write-Output 'DNS cache flushed.'
"""
        },
        new ToolAction
        {
            Title = "Renew IP address",
            Description = "Requests a fresh DHCP lease from the router.",
            Script = """
ipconfig /release
ipconfig /renew
Write-Output 'IP renewal requested.'
""",
            RequiresAdmin = true
        }
    };

    private static IReadOnlyList<ToolAction> FirewallActions() => new[]
    {
        new ToolAction
        {
            Title = "Firewall status",
            Description = "Shows Windows Firewall profile status.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-NetFirewallProfile | Select-Object Name, Enabled, DefaultInboundAction, DefaultOutboundAction | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Allowed inbound rules",
            Description = "Lists enabled inbound allow rules.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-NetFirewallRule -Enabled True -Direction Inbound -Action Allow | Select-Object -First 100 DisplayName, Profile, Enabled | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open ports viewer",
            Description = "Shows listening TCP ports and owning processes.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-NetTCPConnection -State Listen | ForEach-Object {
    $p = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
    [pscustomobject]@{ LocalAddress=$_.LocalAddress; Port=$_.LocalPort; Process=$p.ProcessName; PID=$_.OwningProcess }
} | Sort-Object Port | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> PrivacyActions() => new[]
    {
        new ToolAction
        {
            Title = "Privacy setting summary",
            Description = "Shows common privacy related registry values for the current user.",
            Script = """
$ErrorActionPreference = 'Continue'
$items = @(
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo',
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced',
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search'
)
foreach ($path in $items) {
    Write-Output $path
    Get-ItemProperty $path -ErrorAction SilentlyContinue | Format-List | Out-String
}
"""
        },
        new ToolAction
        {
            Title = "Open app permissions",
            Description = "Opens Windows app permissions settings.",
            Script = """
Start-Process 'ms-settings:privacy'
Write-Output 'Privacy settings opened.'
"""
        },
        new ToolAction
        {
            Title = "Open background apps settings",
            Description = "Opens the Windows background apps settings page where supported.",
            Script = """
Start-Process 'ms-settings:privacy-backgroundapps'
Write-Output 'Background apps settings opened where supported.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> SecurityActions() => new[]
    {
        new ToolAction
        {
            Title = "Security health scan",
            Description = "Checks Defender, Firewall, SMBv1, UAC and Remote Desktop state.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Defender status'
Get-MpComputerStatus | Select-Object AntivirusEnabled, RealTimeProtectionEnabled, AntivirusSignatureLastUpdated | Format-List | Out-String
Write-Output 'Firewall status'
Get-NetFirewallProfile | Select-Object Name, Enabled | Format-Table -AutoSize | Out-String
Write-Output 'SMBv1 feature'
Get-WindowsOptionalFeature -Online -FeatureName SMB1Protocol | Select-Object FeatureName, State | Format-List | Out-String
Write-Output 'Remote Desktop setting'
Get-ItemProperty 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name fDenyTSConnections -ErrorAction SilentlyContinue | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Defender exclusions review",
            Description = "Shows current Microsoft Defender exclusions.",
            Script = """
$ErrorActionPreference = 'Continue'
$p = Get-MpPreference
[pscustomobject]@{
    ExclusionPath = ($p.ExclusionPath -join '; ')
    ExclusionProcess = ($p.ExclusionProcess -join '; ')
    ExclusionExtension = ($p.ExclusionExtension -join '; ')
    ExclusionIpAddress = ($p.ExclusionIpAddress -join '; ')
} | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Local administrators",
            Description = "Lists members of the local Administrators group.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-LocalGroupMember -Group 'Administrators' | Select-Object Name, ObjectClass, PrincipalSource | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> RepairActions() => new[]
    {
        new ToolAction
        {
            Title = "Run SFC scan",
            Description = "Runs System File Checker. This can take several minutes.",
            RequiresAdmin = true,
            Script = """
sfc /scannow
"""
        },
        new ToolAction
        {
            Title = "Run DISM restore health",
            Description = "Repairs the Windows component store. This can take several minutes.",
            RequiresAdmin = true,
            Script = """
DISM /Online /Cleanup-Image /RestoreHealth
"""
        },
        new ToolAction
        {
            Title = "Reset Microsoft Store cache",
            Description = "Runs the Windows Store reset tool.",
            Script = """
wsreset.exe
Write-Output 'Microsoft Store cache reset started.'
"""
        },
        new ToolAction
        {
            Title = "Restart Explorer",
            Description = "Restarts Windows Explorer to refresh the desktop shell.",
            Script = """
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Process explorer.exe
Write-Output 'Windows Explorer restarted.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> HealthActions() => new[]
    {
        new ToolAction
        {
            Title = "Reliability events",
            Description = "Shows recent app crashes and Windows reliability records.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-CimInstance Win32_ReliabilityRecords -ErrorAction SilentlyContinue | Select-Object -First 50 TimeGenerated, SourceName, ProductName, Message | Format-Table -Wrap | Out-String
"""
        },
        new ToolAction
        {
            Title = "Critical event log summary",
            Description = "Shows recent critical and error events from the System log.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-WinEvent -FilterHashtable @{LogName='System'; Level=1,2; StartTime=(Get-Date).AddDays(-7)} -MaxEvents 60 -ErrorAction SilentlyContinue | Select-Object TimeCreated, ProviderName, Id, LevelDisplayName, Message | Format-Table -Wrap | Out-String
"""
        },
        new ToolAction
        {
            Title = "Problem devices",
            Description = "Shows devices currently reporting a problem status.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-PnpDevice -PresentOnly | Where-Object { $_.Status -ne 'OK' } | Select-Object Status, Class, FriendlyName, InstanceId | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> BatteryActions() => new[]
    {
        new ToolAction
        {
            Title = "Battery summary",
            Description = "Shows available battery information for laptops.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue | Select-Object Name, BatteryStatus, EstimatedChargeRemaining, EstimatedRunTime | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Generate battery report",
            Description = "Creates the built in Windows battery report.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('battery-report-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.html')
powercfg /batteryreport /output $path
Write-Output "Battery report saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Open battery settings",
            Description = "Opens Windows battery settings.",
            Script = """
Start-Process 'ms-settings:batterysaver'
Write-Output 'Battery settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> GamingActions() => new[]
    {
        new ToolAction
        {
            Title = "Gaming status",
            Description = "Shows Game Mode, capture settings and common gaming service state.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Game Mode'
Get-ItemProperty 'HKCU:\Software\Microsoft\GameBar' -ErrorAction SilentlyContinue | Select-Object AllowAutoGameMode, AutoGameModeEnabled, ShowStartupPanel | Format-List | Out-String
Write-Output 'Captures'
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -ErrorAction SilentlyContinue | Format-List | Out-String
Write-Output 'Gaming services'
Get-Service | Where-Object { $_.Name -match 'Xbox|Gaming' -or $_.DisplayName -match 'Xbox|Gaming' } | Select-Object Name, DisplayName, Status, StartType | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Enable Game Mode",
            Description = "Turns on Windows Game Mode for the current user.",
            Script = """
$ErrorActionPreference = 'Continue'
New-Item 'HKCU:\Software\Microsoft\GameBar' -Force | Out-Null
Set-ItemProperty 'HKCU:\Software\Microsoft\GameBar' -Name AllowAutoGameMode -Type DWord -Value 1
Set-ItemProperty 'HKCU:\Software\Microsoft\GameBar' -Name AutoGameModeEnabled -Type DWord -Value 1
Write-Output 'Game Mode enabled for the current user.'
"""
        },
        new ToolAction
        {
            Title = "Disable background game recording",
            Description = "Disables Game DVR background capture for lower background load.",
            Script = """
$ErrorActionPreference = 'Continue'
New-Item 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Force | Out-Null
Set-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Name AppCaptureEnabled -Type DWord -Value 0
New-Item 'HKCU:\System\GameConfigStore' -Force | Out-Null
Set-ItemProperty 'HKCU:\System\GameConfigStore' -Name GameDVR_Enabled -Type DWord -Value 0
Write-Output 'Background game recording disabled for the current user.'
"""
        },
        new ToolAction
        {
            Title = "Clear DirectX shader cache",
            Description = "Removes the DirectX shader cache. Games may rebuild shaders on next launch.",
            Script = """
$ErrorActionPreference = 'Continue'
$paths = @(
    Join-Path $env:LOCALAPPDATA 'D3DSCache',
    Join-Path $env:LOCALAPPDATA 'NVIDIA\DXCache',
    Join-Path $env:LOCALAPPDATA 'NVIDIA\GLCache'
)
foreach ($path in $paths) {
    if (Test-Path $path) {
        Get-ChildItem $path -Recurse -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Output "Cleared $path"
    }
}
"""
        },
        new ToolAction
        {
            Title = "Open graphics settings",
            Description = "Opens Windows graphics settings for per app GPU preferences.",
            Script = """
Start-Process 'ms-settings:display-advancedgraphics'
Write-Output 'Graphics settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> DeveloperActions() => new[]
    {
        new ToolAction
        {
            Title = "Developer machine scan",
            Description = "Checks common development tools and Windows developer features.",
            Script = """
$ErrorActionPreference = 'Continue'
$commands = 'git','dotnet','node','npm','python','pwsh','wsl','docker'
foreach ($cmd in $commands) {
    $found = Get-Command $cmd -ErrorAction SilentlyContinue
    [pscustomobject]@{ Tool=$cmd; Found=[bool]$found; Path=if($found){$found.Source}else{''} }
} | Format-Table -AutoSize | Out-String
Write-Output 'Windows features'
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux,VirtualMachinePlatform,Microsoft-Hyper-V-All -ErrorAction SilentlyContinue | Select-Object FeatureName, State | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Enable long paths",
            Description = "Allows long file paths for tools that support them.",
            RequiresAdmin = true,
            Script = """
New-Item 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Force | Out-Null
Set-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Name LongPathsEnabled -Type DWord -Value 1
Write-Output 'Long paths enabled. Some apps may need restarting.'
"""
        },
        new ToolAction
        {
            Title = "Open Developer settings",
            Description = "Opens Windows Developer settings.",
            Script = """
Start-Process 'ms-settings:developers'
Write-Output 'Developer settings opened.'
"""
        },
        new ToolAction
        {
            Title = "WSL status",
            Description = "Shows WSL version and installed distributions where available.",
            Script = """
$ErrorActionPreference = 'Continue'
wsl --status
wsl --list --verbose
"""
        }
    };

    private static IReadOnlyList<ToolAction> VisualActions() => new[]
    {
        new ToolAction
        {
            Title = "Visual effects summary",
            Description = "Shows common Explorer and visual effect settings.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects' -ErrorAction SilentlyContinue | Format-List | Out-String
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -ErrorAction SilentlyContinue | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Reduce transparency",
            Description = "Disables Windows transparency effects for a cleaner and lighter interface.",
            Script = """
New-Item 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Force | Out-Null
Set-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name EnableTransparency -Type DWord -Value 0
Write-Output 'Transparency effects disabled for the current user.'
"""
        },
        new ToolAction
        {
            Title = "Open visual effects settings",
            Description = "Opens the classic Windows performance options panel.",
            Script = """
SystemPropertiesPerformance.exe
Write-Output 'Visual effects settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> ContextMenuActions() => new[]
    {
        new ToolAction
        {
            Title = "Context menu registry review",
            Description = "Lists common shell extension locations that can affect right click speed.",
            Script = """
$ErrorActionPreference = 'Continue'
$paths = @(
    'HKCU:\Software\Classes\*\shellex\ContextMenuHandlers',
    'HKLM:\Software\Classes\*\shellex\ContextMenuHandlers',
    'HKCU:\Software\Classes\Directory\shellex\ContextMenuHandlers',
    'HKLM:\Software\Classes\Directory\shellex\ContextMenuHandlers'
)
foreach ($path in $paths) {
    Write-Output $path
    Get-ChildItem $path -ErrorAction SilentlyContinue | Select-Object PSChildName | Format-Table -AutoSize | Out-String
}
"""
        },
        new ToolAction
        {
            Title = "Open classic shell extensions location",
            Description = "Opens Registry Editor at a common context menu handler location.",
            Script = """
Start-Process regedit.exe
Write-Output 'Registry Editor opened. Review context menu handlers carefully.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> SearchActions() => new[]
    {
        new ToolAction
        {
            Title = "Search indexing status",
            Description = "Shows the Windows Search service and indexing related information.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-Service WSearch | Select-Object Name, Status, StartType | Format-List | Out-String
Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows Search' -ErrorAction SilentlyContinue | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open indexing options",
            Description = "Opens the Windows indexing options control panel.",
            Script = """
control.exe srchadmin.dll
Write-Output 'Indexing Options opened.'
"""
        },
        new ToolAction
        {
            Title = "Restart Windows Search",
            Description = "Restarts the Windows Search service.",
            RequiresAdmin = true,
            Script = """
Restart-Service WSearch -Force
Write-Output 'Windows Search service restarted.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> TaskActions() => new[]
    {
        new ToolAction
        {
            Title = "Startup scheduled tasks",
            Description = "Lists scheduled tasks that trigger at logon or startup.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-ScheduledTask | Where-Object { $_.Triggers -match 'Logon|Startup' } | Select-Object TaskName, TaskPath, State, Author | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "High frequency tasks",
            Description = "Lists enabled tasks for manual review.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-ScheduledTask | Where-Object { $_.State -ne 'Disabled' } | Select-Object -First 150 TaskName, TaskPath, State, Author | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open Task Scheduler",
            Description = "Opens Task Scheduler for manual changes.",
            Script = """
Start-Process taskschd.msc
Write-Output 'Task Scheduler opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> AppActions() => new[]
    {
        new ToolAction
        {
            Title = "Installed apps report",
            Description = "Creates a report of installed desktop apps from common uninstall registry keys.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$out = Join-Path $root ('installed-apps-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
$keys = @(
    'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
)
Get-ItemProperty $keys -ErrorAction SilentlyContinue | Where-Object DisplayName | Sort-Object DisplayName | Select-Object DisplayName, DisplayVersion, Publisher, InstallDate, EstimatedSize | Format-Table -AutoSize | Out-File $out -Encoding UTF8
Write-Output "Installed apps report saved to $out"
"""
        },
        new ToolAction
        {
            Title = "Open installed apps",
            Description = "Opens Windows installed apps settings.",
            Script = """
Start-Process 'ms-settings:appsfeatures'
Write-Output 'Installed apps settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> WindowsFeatureActions() => new[]
    {
        new ToolAction
        {
            Title = "Optional features summary",
            Description = "Shows common optional Windows features and their current state.",
            Script = """
$ErrorActionPreference = 'Continue'
$features = 'Microsoft-Windows-Subsystem-Linux','VirtualMachinePlatform','Microsoft-Hyper-V-All','Containers','IIS-WebServerRole','TelnetClient','SMB1Protocol','Printing-PrintToPDFServices-Features','Windows-Defender-ApplicationGuard'
Get-WindowsOptionalFeature -Online -FeatureName $features -ErrorAction SilentlyContinue | Select-Object FeatureName, State | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open Windows features",
            Description = "Opens the Windows Features control panel.",
            Script = """
optionalfeatures.exe
Write-Output 'Windows Features opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> HostsActions() => new[]
    {
        new ToolAction
        {
            Title = "Inspect hosts file",
            Description = "Shows active entries from the Windows hosts file.",
            Script = """
$ErrorActionPreference = 'Continue'
$path = Join-Path $env:WINDIR 'System32\drivers\etc\hosts'
Get-Content $path -ErrorAction SilentlyContinue | Where-Object { $_.Trim() -and -not $_.Trim().StartsWith('#') } | Out-String
"""
        },
        new ToolAction
        {
            Title = "Back up hosts file",
            Description = "Creates a backup copy of the hosts file in WinForge reports.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$source = Join-Path $env:WINDIR 'System32\drivers\etc\hosts'
$dest = Join-Path $root ('hosts-backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
Copy-Item $source $dest -Force
Write-Output "Hosts file backed up to $dest"
"""
        },
        new ToolAction
        {
            Title = "Open hosts file",
            Description = "Opens the hosts file in Notepad for review.",
            Script = """
$path = Join-Path $env:WINDIR 'System32\drivers\etc\hosts'
Start-Process notepad.exe $path
Write-Output 'Hosts file opened in Notepad.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> ProxyVpnActions() => new[]
    {
        new ToolAction
        {
            Title = "Proxy and VPN summary",
            Description = "Shows Windows proxy settings, WinHTTP proxy and VPN adapters.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Current user proxy settings'
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings' | Select-Object ProxyEnable, ProxyServer, AutoConfigURL | Format-List | Out-String
Write-Output 'WinHTTP proxy'
netsh winhttp show proxy
Write-Output 'VPN and virtual adapters'
Get-NetAdapter | Where-Object { $_.InterfaceDescription -match 'VPN|TAP|TUN|WireGuard|OpenVPN|Cisco|Fortinet|Virtual' -or $_.Name -match 'VPN|TAP|TUN|WireGuard|OpenVPN' } | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Reset WinHTTP proxy",
            Description = "Clears WinHTTP proxy configuration.",
            RequiresAdmin = true,
            Script = """
netsh winhttp reset proxy
Write-Output 'WinHTTP proxy reset.'
"""
        },
        new ToolAction
        {
            Title = "Open proxy settings",
            Description = "Opens Windows proxy settings.",
            Script = """
Start-Process 'ms-settings:network-proxy'
Write-Output 'Proxy settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> TimeSyncActions() => new[]
    {
        new ToolAction
        {
            Title = "Time sync status",
            Description = "Shows time zone and Windows Time service status.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-TimeZone | Format-List | Out-String
Get-Service W32Time | Select-Object Name, Status, StartType | Format-List | Out-String
w32tm /query /status
"""
        },
        new ToolAction
        {
            Title = "Sync time now",
            Description = "Requests an immediate Windows time sync.",
            RequiresAdmin = true,
            Script = """
Start-Service W32Time -ErrorAction SilentlyContinue
w32tm /resync
Write-Output 'Time sync requested.'
"""
        },
        new ToolAction
        {
            Title = "Open date and time settings",
            Description = "Opens Windows date and time settings.",
            Script = """
Start-Process 'ms-settings:dateandtime'
Write-Output 'Date and time settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> SafetyActions() => new[]
    {
        new ToolAction
        {
            Title = "Set simple mode",
            Description = "Stores simple mode preference. Riskier actions should stay hidden in future UI passes.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
New-Item -ItemType Directory -Path $root -Force | Out-Null
@{ Mode = 'Simple'; Updated = (Get-Date).ToString('s') } | ConvertTo-Json | Out-File (Join-Path $root 'safety-mode.json') -Encoding UTF8
Write-Output 'Safety mode set to Simple.'
"""
        },
        new ToolAction
        {
            Title = "Set advanced mode",
            Description = "Stores advanced mode preference for deeper tools.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
New-Item -ItemType Directory -Path $root -Force | Out-Null
@{ Mode = 'Advanced'; Updated = (Get-Date).ToString('s') } | ConvertTo-Json | Out-File (Join-Path $root 'safety-mode.json') -Encoding UTF8
Write-Output 'Safety mode set to Advanced.'
"""
        },
        new ToolAction
        {
            Title = "Safety mode status",
            Description = "Shows the current WinForge safety mode preference.",
            Script = """
$path = Join-Path $env:ProgramData 'WinForge\safety-mode.json'
if (Test-Path $path) { Get-Content $path | Out-String } else { Write-Output 'No safety mode has been saved yet.' }
"""
        }
    };

    private static IReadOnlyList<ToolAction> ChangeActions() => new[]
    {
        new ToolAction
        {
            Title = "Create change explanation guide",
            Description = "Creates a local guide explaining the main WinForge change categories.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('change-explanations-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
@'
WinForge change guide

Startup changes control apps that launch with Windows.
Clean up actions remove temporary data and caches.
Power actions change how Windows balances responsiveness and energy use.
Network actions reset caches and repair common adapter issues.
Privacy actions reduce optional personalisation and suggested content.
Security actions report risky settings and help keep protection enabled.
Repair actions run built in Windows recovery commands.

Every change should be reviewed before applying. Reversible changes are recorded in snapshots where supported.
'@ | Out-File $path -Encoding UTF8
Write-Output "Change guide saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Open snapshots folder",
            Description = "Opens the folder where optimisation snapshots are stored.",
            Script = """
$path = Join-Path $env:ProgramData 'WinForge\Snapshots'
New-Item -ItemType Directory -Path $path -Force | Out-Null
Start-Process $path
Write-Output 'Snapshots folder opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> RestartActions() => new[]
    {
        new ToolAction
        {
            Title = "Pending restart check",
            Description = "Checks common Windows locations that indicate a restart is pending.",
            Script = """
$ErrorActionPreference = 'Continue'
$checks = @(
    @{Name='Component Based Servicing'; Path='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending'},
    @{Name='Windows Update'; Path='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired'},
    @{Name='Pending file rename'; Path='HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager'; Value='PendingFileRenameOperations'}
)
foreach ($check in $checks) {
    if ($check.Value) {
        $value = (Get-ItemProperty $check.Path -Name $check.Value -ErrorAction SilentlyContinue).$($check.Value)
        [pscustomobject]@{ Check=$check.Name; Pending=[bool]$value }
    } else {
        [pscustomobject]@{ Check=$check.Name; Pending=(Test-Path $check.Path) }
    }
} | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open Windows Update",
            Description = "Opens Windows Update settings.",
            Script = """
Start-Process 'ms-settings:windowsupdate'
Write-Output 'Windows Update opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> QueueActions() => new[]
    {
        new ToolAction
        {
            Title = "Create empty action queue",
            Description = "Creates a queue file that future versions can use to stage changes before applying them.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'action-queue.json'
@{ Created=(Get-Date).ToString('s'); Items=@() } | ConvertTo-Json -Depth 5 | Out-File $path -Encoding UTF8
Write-Output "Action queue created at $path"
"""
        },
        new ToolAction
        {
            Title = "Show action queue",
            Description = "Displays the current action queue file if it exists.",
            Script = """
$path = Join-Path $env:ProgramData 'WinForge\action-queue.json'
if (Test-Path $path) { Get-Content $path | Out-String } else { Write-Output 'No action queue file exists yet.' }
"""
        }
    };

    private static IReadOnlyList<ToolAction> ReportActions() => new[]
    {
        new ToolAction
        {
            Title = "Export full report pack",
            Description = "Creates a combined report with system, network, firewall, security and storage summaries.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('winforge-report-pack-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
'WinForge report pack' | Out-File $path -Encoding UTF8
(Get-Date).ToString() | Out-File $path -Append -Encoding UTF8
'System' | Out-File $path -Append -Encoding UTF8
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, OsBuildNumber, CsName, CsProcessors, CsTotalPhysicalMemory | Format-List | Out-File $path -Append -Encoding UTF8
'Network' | Out-File $path -Append -Encoding UTF8
Get-NetIPConfiguration | Format-List | Out-File $path -Append -Encoding UTF8
'Firewall' | Out-File $path -Append -Encoding UTF8
Get-NetFirewallProfile | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
'Storage' | Out-File $path -Append -Encoding UTF8
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Select-Object DeviceID, VolumeName, FreeSpace, Size | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
Write-Output "Report pack saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Open reports folder",
            Description = "Opens the WinForge reports folder.",
            Script = """
$path = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $path -Force | Out-Null
Start-Process $path
Write-Output 'Reports folder opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> MaintenanceActions() => new[]
    {
        new ToolAction
        {
            Title = "Create weekly health report task",
            Description = "Creates a scheduled task that writes a weekly WinForge health report.",
            RequiresAdmin = true,
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
$reports = Join-Path $root 'Reports'
New-Item -ItemType Directory -Path $reports -Force | Out-Null
$script = Join-Path $root 'weekly-health-report.ps1'
@'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('weekly-health-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, OsBuildNumber, CsName | Format-List | Out-File $path -Encoding UTF8
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
'@ | Out-File $script -Encoding UTF8
schtasks /Create /TN "WinForge Weekly Health Report" /SC WEEKLY /D SUN /ST 10:00 /TR "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$script`"" /F | Out-Null
Write-Output 'Weekly health report task created.'
"""
        },
        new ToolAction
        {
            Title = "Remove WinForge maintenance tasks",
            Description = "Removes scheduled tasks created by WinForge maintenance tools.",
            RequiresAdmin = true,
            IsDestructive = true,
            Script = """
schtasks /Delete /TN "WinForge Weekly Health Report" /F 2>$null
Write-Output 'WinForge maintenance tasks removed where present.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> IgnoreListActions() => new[]
    {
        new ToolAction
        {
            Title = "Create ignore list",
            Description = "Creates editable ignore list files for apps, folders, services and rules.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Ignore'
New-Item -ItemType Directory -Path $root -Force | Out-Null
'apps' | Out-File (Join-Path $root 'apps.txt') -Encoding UTF8
'folders' | Out-File (Join-Path $root 'folders.txt') -Encoding UTF8
'services' | Out-File (Join-Path $root 'services.txt') -Encoding UTF8
'firewall-rules' | Out-File (Join-Path $root 'firewall-rules.txt') -Encoding UTF8
Write-Output "Ignore list files created in $root"
"""
        },
        new ToolAction
        {
            Title = "Open ignore list folder",
            Description = "Opens the ignore list folder.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Ignore'
New-Item -ItemType Directory -Path $root -Force | Out-Null
Start-Process $root
Write-Output 'Ignore list folder opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> PortableActions() => new[]
    {
        new ToolAction
        {
            Title = "Prepare portable folder",
            Description = "Creates a portable data folder beside the current executable where possible.",
            Script = """
$base = Split-Path -Parent ([Diagnostics.Process]::GetCurrentProcess().MainModule.FileName)
$portable = Join-Path $base 'WinForgePortableData'
New-Item -ItemType Directory -Path $portable -Force | Out-Null
@'
WinForge portable mode notes

Place this folder beside WinForge.exe to keep portable data together.
Current builds still use ProgramData for system level logs and snapshots.
Future builds can use this folder as the main portable data location.
'@ | Out-File (Join-Path $portable 'README.txt') -Encoding UTF8
Write-Output "Portable folder prepared at $portable"
"""
        },
        new ToolAction
        {
            Title = "Open portable folder",
            Description = "Opens the portable data folder if it exists.",
            Script = """
$base = Split-Path -Parent ([Diagnostics.Process]::GetCurrentProcess().MainModule.FileName)
$portable = Join-Path $base 'WinForgePortableData'
New-Item -ItemType Directory -Path $portable -Force | Out-Null
Start-Process $portable
Write-Output 'Portable folder opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> FirstRunActions() => new[]
    {
        new ToolAction
        {
            Title = "Create first run recommendation report",
            Description = "Creates a beginner friendly report showing suggested starting areas for this PC.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('first-run-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
$drive = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'" -ErrorAction SilentlyContinue
$free = if($drive){ [math]::Round($drive.FreeSpace/1GB,1) } else { 0 }
@"
WinForge first run report

Suggested start:
1. Review Startup and disable apps that are not needed at login.
2. Open Clean Up and review temporary file cleanup options.
3. Open Security and run the security health scan.
4. Open Network and run the connection test.
5. Open Reports and export a baseline report.

System drive free space: $free GB
Administrator: $([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
"@ | Out-File $path -Encoding UTF8
Write-Output "First run report saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Open first run report folder",
            Description = "Opens the reports folder containing first run reports.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
Start-Process $root
Write-Output 'Reports folder opened.'
"""
        }
    };


    private static IReadOnlyList<ToolAction> HealthScoreActions() => new[]
    {
        new ToolAction
        {
            Title = "Run full PC score",
            Description = "Creates a score based on startup load, free storage, firewall state, restart status and recent system errors.",
            Script = """
$ErrorActionPreference = 'Continue'
$score = 100
$notes = New-Object System.Collections.Generic.List[string]

$startupCount = 0
try {
    $startupCount += @(Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue).PSObject.Properties | Where-Object { $_.Name -notmatch '^PS' } | Measure-Object | Select-Object -ExpandProperty Count
    $startupCount += @(Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue).PSObject.Properties | Where-Object { $_.Name -notmatch '^PS' } | Measure-Object | Select-Object -ExpandProperty Count
} catch {}
if ($startupCount -gt 15) { $score -= 10; $notes.Add("High startup load: $startupCount entries") }

$drive = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'" -ErrorAction SilentlyContinue
if ($drive) {
    $freePercent = [math]::Round(($drive.FreeSpace / $drive.Size) * 100, 1)
    if ($freePercent -lt 15) { $score -= 20; $notes.Add("Low system drive space: $freePercent percent free") }
    elseif ($freePercent -lt 25) { $score -= 10; $notes.Add("System drive space should be reviewed: $freePercent percent free") }
}

$firewall = Get-NetFirewallProfile -ErrorAction SilentlyContinue
if ($firewall | Where-Object { -not $_.Enabled }) { $score -= 15; $notes.Add('One or more firewall profiles are disabled') }

$rebootKeys = @(
 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending',
 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired'
)
if ($rebootKeys | Where-Object { Test-Path $_ }) { $score -= 8; $notes.Add('Restart is pending') }

$criticalEvents = @(Get-WinEvent -FilterHashtable @{LogName='System'; Level=1; StartTime=(Get-Date).AddDays(-7)} -ErrorAction SilentlyContinue)
if ($criticalEvents.Count -gt 0) { $score -= [math]::Min(15, $criticalEvents.Count * 3); $notes.Add("Critical system events in last 7 days: $($criticalEvents.Count)") }

if ($score -lt 0) { $score = 0 }
[pscustomobject]@{
    OverallScore = $score
    StartupEntries = $startupCount
    Notes = if($notes.Count){ $notes -join '; ' } else { 'No major issues found' }
} | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Create score report",
            Description = "Saves a detailed health score report in the WinForge reports folder.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('health-score-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
'WinForge health score report' | Out-File $path -Encoding UTF8
(Get-Date).ToString() | Out-File $path -Append -Encoding UTF8
'Computer summary' | Out-File $path -Append -Encoding UTF8
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, OsBuildNumber, CsName, CsTotalPhysicalMemory | Format-List | Out-File $path -Append -Encoding UTF8
'Storage' | Out-File $path -Append -Encoding UTF8
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Select-Object DeviceID, VolumeName, @{Name='FreeGB';Expression={[math]::Round($_.FreeSpace/1GB,2)}}, @{Name='SizeGB';Expression={[math]::Round($_.Size/1GB,2)}} | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
'Firewall' | Out-File $path -Append -Encoding UTF8
Get-NetFirewallProfile | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
Write-Output "Health score report saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> GuidedOptimiseActions() => new[]
    {
        new ToolAction
        {
            Title = "Create guided scan",
            Description = "Builds a readable recommendation list without changing anything.",
            Script = """
$ErrorActionPreference = 'Continue'
$recommendations = New-Object System.Collections.Generic.List[string]
$downloads = Join-Path $env:USERPROFILE 'Downloads'
if (Test-Path $downloads) {
    $size = (Get-ChildItem $downloads -File -Recurse -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum
    if ($size -gt 5GB) { $recommendations.Add("Review Downloads folder. Estimated size: $([math]::Round($size/1GB,2)) GB") }
}
$startup = 0
try { $startup += (Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue).PSObject.Properties.Count } catch {}
if ($startup -gt 10) { $recommendations.Add("Review startup apps. Current user startup entries: $startup") }
if ((Get-NetFirewallProfile | Where-Object { -not $_.Enabled })) { $recommendations.Add('Enable Windows Firewall profiles') }
if ($recommendations.Count -eq 0) { $recommendations.Add('No urgent recommendations found') }
$recommendations | ForEach-Object { [pscustomobject]@{ Recommendation = $_ } } | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Create before snapshot report",
            Description = "Saves a baseline before optimisation so changes can be compared later.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('before-optimise-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
'Before optimisation snapshot' | Out-File $path -Encoding UTF8
(Get-Date).ToString() | Out-File $path -Append -Encoding UTF8
'Startup' | Out-File $path -Append -Encoding UTF8
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue | Format-List | Out-File $path -Append -Encoding UTF8
Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue | Format-List | Out-File $path -Append -Encoding UTF8
'Storage' | Out-File $path -Append -Encoding UTF8
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
'Services' | Out-File $path -Append -Encoding UTF8
Get-Service | Sort-Object Name | Select-Object Name, Status, StartType | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
Write-Output "Before snapshot saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Create after snapshot report",
            Description = "Saves a post optimisation snapshot for comparing results.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('after-optimise-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
'After optimisation snapshot' | Out-File $path -Encoding UTF8
(Get-Date).ToString() | Out-File $path -Append -Encoding UTF8
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, OsBuildNumber, CsName | Format-List | Out-File $path -Append -Encoding UTF8
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Select-Object DeviceID, VolumeName, FreeSpace, Size | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
Get-NetFirewallProfile | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
Write-Output "After snapshot saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> MonitoringActions() => new[]
    {
        new ToolAction
        {
            Title = "Create scheduled health scan",
            Description = "Creates a weekly scheduled health report task.",
            RequiresAdmin = true,
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
$reports = Join-Path $root 'Reports'
New-Item -ItemType Directory -Path $reports -Force | Out-Null
$script = Join-Path $root 'scheduled-health-scan.ps1'
@'
$reports = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $reports -Force | Out-Null
$path = Join-Path $reports ('scheduled-health-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, OsBuildNumber, CsName | Format-List | Out-File $path -Encoding UTF8
Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
Get-NetFirewallProfile | Format-Table -AutoSize | Out-File $path -Append -Encoding UTF8
'@ | Out-File $script -Encoding UTF8
schtasks /Create /TN "WinForge Scheduled Health Scan" /SC WEEKLY /D SUN /ST 09:00 /TR "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$script`"" /F | Out-Null
Write-Output 'Scheduled health scan created.'
"""
        },
        new ToolAction
        {
            Title = "Create startup baseline",
            Description = "Saves current startup entries so new startup items can be detected later.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Monitoring'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'startup-baseline.txt'
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue | Format-List | Out-File $path -Encoding UTF8
Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue | Format-List | Out-File $path -Append -Encoding UTF8
Get-ChildItem ([Environment]::GetFolderPath([Environment+SpecialFolder]::Startup)) -ErrorAction SilentlyContinue | Select-Object Name, FullName | Format-List | Out-File $path -Append -Encoding UTF8
Write-Output "Startup baseline saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Create installed apps baseline",
            Description = "Saves installed app data so new apps can be compared later.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Monitoring'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'installed-apps-baseline.csv'
$paths = 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*','HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
Get-ItemProperty $paths -ErrorAction SilentlyContinue | Where-Object DisplayName | Select-Object DisplayName, DisplayVersion, Publisher, InstallDate | Sort-Object DisplayName | Export-Csv $path -NoTypeInformation -Encoding UTF8
Write-Output "Installed apps baseline saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Create low disk alert script",
            Description = "Creates a script that shows a Windows notification when the system drive is below 15 percent free.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Monitoring'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'low-disk-alert.ps1'
@'
$drive = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'"
if ($drive) {
    $free = ($drive.FreeSpace / $drive.Size) * 100
    if ($free -lt 15) {
        Add-Type -AssemblyName PresentationFramework
        [System.Windows.MessageBox]::Show("System drive free space is below 15 percent.", "WinForge") | Out-Null
    }
}
'@ | Out-File $path -Encoding UTF8
Write-Output "Low disk alert script saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> ServiceManagerActions() => new[]
    {
        new ToolAction
        {
            Title = "Service overview",
            Description = "Shows service status and startup type.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-Service | Sort-Object Status, Name | Select-Object Name, DisplayName, Status, StartType | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Third party services",
            Description = "Lists non Microsoft services where publisher information is visible.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-CimInstance Win32_Service | Where-Object { $_.PathName -and $_.PathName -notmatch 'Windows\\System32|Microsoft' } | Select-Object Name, DisplayName, State, StartMode, PathName | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Save service snapshot",
            Description = "Saves current service startup modes for review.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('services-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.csv')
Get-CimInstance Win32_Service | Select-Object Name, DisplayName, State, StartMode, PathName | Export-Csv $path -NoTypeInformation -Encoding UTF8
Write-Output "Service snapshot saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> HardwareActions() => new[]
    {
        new ToolAction
        {
            Title = "Hardware profile",
            Description = "Creates a hardware summary for CPU, memory, board, graphics, drives and network adapters.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Computer'
Get-CimInstance Win32_ComputerSystem | Select-Object Manufacturer, Model, SystemType, TotalPhysicalMemory | Format-List | Out-String
Write-Output 'Processor'
Get-CimInstance Win32_Processor | Select-Object Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed | Format-List | Out-String
Write-Output 'Graphics'
Get-CimInstance Win32_VideoController | Select-Object Name, DriverVersion, AdapterRAM | Format-Table -AutoSize | Out-String
Write-Output 'Drives'
Get-CimInstance Win32_DiskDrive | Select-Object Model, MediaType, Size, Status | Format-Table -AutoSize | Out-String
Write-Output 'Network adapters'
Get-NetAdapter | Select-Object Name, InterfaceDescription, Status, LinkSpeed | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Detect PC type",
            Description = "Uses Windows chassis data to identify whether the machine looks like a desktop, laptop or compact PC.",
            Script = """
$chassis = (Get-CimInstance Win32_SystemEnclosure).ChassisTypes
$type = 'Unknown'
if ($chassis -match '8|9|10|14') { $type = 'Laptop or portable' }
elseif ($chassis -match '3|4|5|6|7|15|16') { $type = 'Desktop or workstation' }
elseif ($chassis -match '35') { $type = 'Mini PC or compact desktop' }
[pscustomobject]@{ PcType=$type; ChassisTypes=($chassis -join ',') } | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Temperature and throttling hints",
            Description = "Checks available thermal sensors and shows signs that can indicate throttling.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Thermal sensors available through Windows'
Get-CimInstance MSAcpi_ThermalZoneTemperature -Namespace root/wmi -ErrorAction SilentlyContinue | ForEach-Object {
    [pscustomobject]@{ Instance=$_.InstanceName; Celsius=[math]::Round(($_.CurrentTemperature / 10) - 273.15, 1) }
} | Format-Table -AutoSize | Out-String
Write-Output 'Processor current clock overview'
Get-CimInstance Win32_Processor | Select-Object Name, CurrentClockSpeed, MaxClockSpeed, LoadPercentage | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Problem devices",
            Description = "Lists devices that Windows reports with a problem code.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-CimInstance Win32_PnPEntity | Where-Object { $_.ConfigManagerErrorCode -ne 0 } | Select-Object Name, Manufacturer, ConfigManagerErrorCode, Status | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> DeepStorageActions() => new[]
    {
        new ToolAction
        {
            Title = "Deep storage map",
            Description = "Maps the largest folders in the user profile.",
            Script = """
$ErrorActionPreference = 'Continue'
$base = $env:USERPROFILE
Get-ChildItem $base -Directory -Force -ErrorAction SilentlyContinue | ForEach-Object {
    $size = (Get-ChildItem $_.FullName -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum
    [pscustomobject]@{ Folder=$_.FullName; SizeGB=[math]::Round($size/1GB,2) }
} | Sort-Object SizeGB -Descending | Select-Object -First 30 | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Duplicate candidate scan",
            Description = "Finds exact duplicate candidates in Downloads by matching file hashes. No files are deleted.",
            Script = """
$ErrorActionPreference = 'Continue'
$downloads = Join-Path $env:USERPROFILE 'Downloads'
$files = Get-ChildItem $downloads -File -Recurse -ErrorAction SilentlyContinue | Where-Object Length -gt 1MB | Select-Object -First 500
$hashes = foreach ($file in $files) {
    $hash = Get-FileHash $file.FullName -Algorithm SHA256 -ErrorAction SilentlyContinue
    if ($hash) { [pscustomobject]@{ Hash=$hash.Hash; SizeMB=[math]::Round($file.Length/1MB,1); Path=$file.FullName } }
}
$hashes | Group-Object Hash | Where-Object Count -gt 1 | ForEach-Object { $_.Group } | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Old installer review folder",
            Description = "Creates a report of old installers and archives for manual review.",
            Script = """
$ErrorActionPreference = 'Continue'
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('old-installers-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.csv')
$downloads = Join-Path $env:USERPROFILE 'Downloads'
Get-ChildItem $downloads -File -Recurse -Include *.exe,*.msi,*.zip,*.7z,*.rar -ErrorAction SilentlyContinue | Where-Object LastWriteTime -lt (Get-Date).AddDays(-30) | Select-Object Name, LastWriteTime, Length, FullName | Export-Csv $path -NoTypeInformation -Encoding UTF8
Write-Output "Old installer report saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> GameFocusActions() => new[]
    {
        new ToolAction
        {
            Title = "Create game profile template",
            Description = "Creates a template that can be filled with a game or heavy app path.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\GameProfiles'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'game-profile-template.json'
@{
    Name = 'My Game'
    ExePath = 'C:\Path\To\Game.exe'
    UseHighPerformancePower = $true
    DisableNotifications = $true
    RestoreAfterExit = $true
} | ConvertTo-Json -Depth 5 | Out-File $path -Encoding UTF8
Write-Output "Game profile template saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Create focus launcher template",
            Description = "Creates a launcher script that can switch to a performance power plan while an app is running.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\GameProfiles'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'focus-launcher-template.ps1'
@'
param([string]$AppPath)
if (-not (Test-Path $AppPath)) { throw "App path not found" }
$before = (powercfg /getactivescheme) -join " "
powercfg /setactive SCHEME_MIN | Out-Null
$process = Start-Process $AppPath -PassThru
Wait-Process -Id $process.Id
if ($before -match 'GUID:\s+([a-fA-F0-9-]+)') { powercfg /setactive $Matches[1] | Out-Null }
'@ | Out-File $path -Encoding UTF8
Write-Output "Focus launcher template saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Gaming settings status",
            Description = "Shows common Windows gaming settings and capture settings.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-ItemProperty 'HKCU:\Software\Microsoft\GameBar' -ErrorAction SilentlyContinue | Format-List | Out-String
Get-ItemProperty 'HKCU:\System\GameConfigStore' -ErrorAction SilentlyContinue | Format-List | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> NetworkProfileActions() => new[]
    {
        new ToolAction
        {
            Title = "Save current DNS profile",
            Description = "Exports current DNS server settings so they can be restored later.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\NetworkProfiles'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('dns-profile-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.json')
Get-DnsClientServerAddress | Select-Object InterfaceAlias, AddressFamily, ServerAddresses | ConvertTo-Json -Depth 5 | Out-File $path -Encoding UTF8
Write-Output "DNS profile saved to $path"
"""
        },
        new ToolAction
        {
            Title = "WiFi analyser",
            Description = "Shows WiFi signal and interface details where available.",
            Script = """
$ErrorActionPreference = 'Continue'
netsh wlan show interfaces
"""
        },
        new ToolAction
        {
            Title = "Public IP and DNS check",
            Description = "Shows the public IP address seen by the internet and current DNS servers.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Public IP'
try { Invoke-RestMethod 'https://api.ipify.org?format=text' } catch { Write-Output 'Public IP check failed' }
Write-Output 'DNS servers'
Get-DnsClientServerAddress | Where-Object ServerAddresses | Select-Object InterfaceAlias, AddressFamily, ServerAddresses | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Set Cloudflare DNS",
            Description = "Sets IPv4 DNS to Cloudflare on active adapters. A DNS profile should be saved first.",
            RequiresAdmin = true,
            Script = """
$adapters = Get-NetAdapter | Where-Object Status -eq 'Up'
foreach ($adapter in $adapters) {
    Set-DnsClientServerAddress -InterfaceIndex $adapter.InterfaceIndex -ServerAddresses '1.1.1.1','1.0.0.1'
}
Write-Output 'Cloudflare DNS applied to active adapters.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> UpdateRepairActions() => new[]
    {
        new ToolAction
        {
            Title = "Windows Update status",
            Description = "Shows update services, pending restart state and update related status.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Update services'
Get-Service wuauserv,bits,cryptsvc,msiserver -ErrorAction SilentlyContinue | Select-Object Name, Status, StartType | Format-Table -AutoSize | Out-String
Write-Output 'Pending restart keys'
$keys = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending','HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired'
foreach ($key in $keys) { [pscustomobject]@{ Key=$key; Exists=(Test-Path $key) } } | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Reset Windows Update cache",
            Description = "Stops update services, renames the update cache folder and restarts services.",
            RequiresAdmin = true,
            Script = """
$ErrorActionPreference = 'Continue'
Stop-Service wuauserv,bits,cryptsvc -Force -ErrorAction SilentlyContinue
$folder = Join-Path $env:WINDIR 'SoftwareDistribution'
if (Test-Path $folder) {
    $newName = 'SoftwareDistribution.WinForgeBackup.' + (Get-Date -Format 'yyyyMMddHHmmss')
    Rename-Item $folder $newName -ErrorAction SilentlyContinue
}
Start-Service cryptsvc,bits,wuauserv -ErrorAction SilentlyContinue
Write-Output 'Windows Update cache reset attempted.'
"""
        },
        new ToolAction
        {
            Title = "Export drivers",
            Description = "Exports installed third party drivers to a backup folder.",
            RequiresAdmin = true,
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\DriverBackup'
New-Item -ItemType Directory -Path $root -Force | Out-Null
dism /online /export-driver /destination:$root
Write-Output "Driver export attempted to $root"
"""
        }
    };

    private static IReadOnlyList<ToolAction> RecoveryActions() => new[]
    {
        new ToolAction
        {
            Title = "Create restore point",
            Description = "Creates a Windows restore point before major changes.",
            RequiresAdmin = true,
            Script = """
$ErrorActionPreference = 'Continue'
Checkpoint-Computer -Description 'WinForge restore point' -RestorePointType 'MODIFY_SETTINGS'
Write-Output 'Restore point command completed. Windows may limit how often restore points can be created.'
"""
        },
        new ToolAction
        {
            Title = "Open recovery drive tool",
            Description = "Opens the Windows recovery drive tool.",
            Script = """
Start-Process 'RecoveryDrive.exe'
Write-Output 'Recovery Drive tool opened.'
"""
        },
        new ToolAction
        {
            Title = "Create recovery checklist",
            Description = "Creates a recovery checklist with useful Windows repair locations.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('recovery-checklist-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
@'
WinForge recovery checklist

1. Create a restore point before major changes.
2. Export drivers before reinstalling Windows or changing hardware.
3. Create a recovery drive from Windows Recovery Drive.
4. Keep important data backed up outside this PC.
5. Use System Restore if a recent change causes issues.
6. Use Windows Recovery Environment for startup repair.

Useful commands:
sfc /scannow
DISM /Online /Cleanup-Image /RestoreHealth
chkdsk C: /scan
'@ | Out-File $path -Encoding UTF8
Write-Output "Recovery checklist saved to $path"
"""
        }
    };


    private static IReadOnlyList<ToolAction> EditionActions() => new[]
    {
        new ToolAction
        {
            Title = "Windows edition report",
            Description = "Shows Windows edition, build, activation status and install date.",
            Script = """
$ErrorActionPreference = 'Continue'
$cv = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
$license = cscript.exe //Nologo "$env:WINDIR\System32\slmgr.vbs" /xpr 2>&1
[pscustomobject]@{
    ProductName = $cv.ProductName
    DisplayVersion = $cv.DisplayVersion
    Build = $cv.CurrentBuildNumber
    Edition = $cv.EditionID
    InstallationType = $cv.InstallationType
    InstallDate = ([DateTimeOffset]::FromUnixTimeSeconds([int64]$cv.InstallDate)).DateTime
    RegisteredOwner = $cv.RegisteredOwner
} | Format-List | Out-String
Write-Output 'Activation'
$license
"""
        },
        new ToolAction
        {
            Title = "Save edition report",
            Description = "Saves Windows edition and activation information to the reports folder.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root ('windows-edition-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
$cv = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
$cv | Format-List | Out-String | Out-File $path -Encoding UTF8
cscript.exe //Nologo "$env:WINDIR\System32\slmgr.vbs" /xpr 2>&1 | Out-File $path -Append -Encoding UTF8
Write-Output "Edition report saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> AppHistoryActions() => new[]
    {
        new ToolAction
        {
            Title = "Create installed app baseline",
            Description = "Saves the current installed app list so future scans can show what changed.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Baselines'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'installed-apps-baseline.json'
$apps = Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
    Where-Object DisplayName |
    Select-Object DisplayName, DisplayVersion, Publisher, InstallDate, InstallLocation
$apps | ConvertTo-Json -Depth 4 | Out-File $path -Encoding UTF8
Write-Output "Installed app baseline saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Compare installed apps",
            Description = "Compares the current app list with the saved baseline.",
            Script = """
$path = Join-Path $env:ProgramData 'WinForge\Baselines\installed-apps-baseline.json'
if (-not (Test-Path $path)) { Write-Output 'No app baseline found. Create one first.'; return }
$old = Get-Content $path -Raw | ConvertFrom-Json
$new = Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue | Where-Object DisplayName | Select-Object DisplayName, DisplayVersion, Publisher
$oldNames = @($old | ForEach-Object DisplayName)
$newNames = @($new | ForEach-Object DisplayName)
Write-Output 'New apps since baseline'
$new | Where-Object { $oldNames -notcontains $_.DisplayName } | Format-Table -AutoSize | Out-String
Write-Output 'Removed apps since baseline'
$old | Where-Object { $newNames -notcontains $_.DisplayName } | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> BaselineActions() => new[]
    {
        new ToolAction
        {
            Title = "Home PC security baseline",
            Description = "Checks common security settings for a normal home PC.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Firewall profiles'
Get-NetFirewallProfile | Select-Object Name, Enabled, DefaultInboundAction | Format-Table -AutoSize | Out-String
Write-Output 'Defender status'
Get-MpComputerStatus | Select-Object AntivirusEnabled, RealTimeProtectionEnabled, BehaviorMonitorEnabled, IoavProtectionEnabled | Format-List | Out-String
Write-Output 'Remote Desktop'
Get-ItemProperty 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name fDenyTSConnections -ErrorAction SilentlyContinue | Format-List | Out-String
Write-Output 'SMBv1'
Get-WindowsOptionalFeature -Online -FeatureName SMB1Protocol | Select-Object FeatureName, State | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Privacy focused baseline",
            Description = "Creates a report of privacy areas that may need review.",
            Script = """
$ErrorActionPreference = 'Continue'
$paths = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo','HKCU:\Software\Microsoft\Windows\CurrentVersion\Search','HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'
foreach ($path in $paths) { Write-Output $path; Get-ItemProperty $path -ErrorAction SilentlyContinue | Format-List | Out-String }
"""
        },
        new ToolAction
        {
            Title = "Developer baseline",
            Description = "Checks common developer features without changing them.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Developer Mode'
Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock' -ErrorAction SilentlyContinue | Format-List | Out-String
Write-Output 'Optional features'
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux,VirtualMachinePlatform,Microsoft-Hyper-V-All -ErrorAction SilentlyContinue | Select-Object FeatureName, State | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> PersistenceActions() => new[]
    {
        new ToolAction
        {
            Title = "Persistence review",
            Description = "Reviews common places where apps start automatically.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Run keys'
Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue | Format-List | Out-String
Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Run' -ErrorAction SilentlyContinue | Format-List | Out-String
Write-Output 'Startup folders'
Get-ChildItem ([Environment]::GetFolderPath('Startup')) -ErrorAction SilentlyContinue | Select-Object Name, FullName | Format-Table -AutoSize | Out-String
Get-ChildItem ([Environment]::GetFolderPath('CommonStartup')) -ErrorAction SilentlyContinue | Select-Object Name, FullName | Format-Table -AutoSize | Out-String
Write-Output 'High level scheduled tasks'
Get-ScheduledTask | Where-Object { $_.State -ne 'Disabled' } | Select-Object -First 80 TaskName, TaskPath, State | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "PowerShell profile review",
            Description = "Checks whether PowerShell profile scripts exist.",
            Script = """
$profiles = @($PROFILE.AllUsersAllHosts,$PROFILE.AllUsersCurrentHost,$PROFILE.CurrentUserAllHosts,$PROFILE.CurrentUserCurrentHost)
foreach ($profilePath in $profiles) { [pscustomobject]@{ Path=$profilePath; Exists=(Test-Path $profilePath) } } | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> ExtensionActions() => new[]
    {
        new ToolAction
        {
            Title = "Browser extension folders",
            Description = "Lists extension folders for common Chromium based browsers.",
            Script = """
$ErrorActionPreference = 'Continue'
$locations = @(
    @{Browser='Edge'; Path=Join-Path $env:LOCALAPPDATA 'Microsoft\Edge\User Data\Default\Extensions'},
    @{Browser='Chrome'; Path=Join-Path $env:LOCALAPPDATA 'Google\Chrome\User Data\Default\Extensions'},
    @{Browser='Brave'; Path=Join-Path $env:LOCALAPPDATA 'BraveSoftware\Brave-Browser\User Data\Default\Extensions'}
)
foreach ($location in $locations) {
    if (Test-Path $location.Path) {
        Get-ChildItem $location.Path -Directory -ErrorAction SilentlyContinue | ForEach-Object { [pscustomobject]@{ Browser=$location.Browser; ExtensionId=$_.Name; Path=$_.FullName } }
    }
} | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open extension managers",
            Description = "Opens extension pages for Edge and Chrome if available.",
            Script = """
Start-Process 'msedge.exe' 'edge://extensions/' -ErrorAction SilentlyContinue
Start-Process 'chrome.exe' 'chrome://extensions/' -ErrorAction SilentlyContinue
Write-Output 'Extension manager pages opened where supported.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> PermissionActions() => new[]
    {
        new ToolAction { Title = "Open app permissions", Description = "Opens Windows privacy and permission settings.", Script = """
Start-Process 'ms-settings:privacy'
Write-Output 'Privacy permissions opened.'
""" },
        new ToolAction { Title = "Open camera permissions", Description = "Opens camera permission settings.", Script = """
Start-Process 'ms-settings:privacy-webcam'
Write-Output 'Camera permissions opened.'
""" },
        new ToolAction { Title = "Open microphone permissions", Description = "Opens microphone permission settings.", Script = """
Start-Process 'ms-settings:privacy-microphone'
Write-Output 'Microphone permissions opened.'
""" },
        new ToolAction { Title = "Open location permissions", Description = "Opens location permission settings.", Script = """
Start-Process 'ms-settings:privacy-location'
Write-Output 'Location permissions opened.'
""" }
    };

    private static IReadOnlyList<ToolAction> AccountActions() => new[]
    {
        new ToolAction
        {
            Title = "Local account review",
            Description = "Shows local users and important local groups.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Local users'
Get-LocalUser | Select-Object Name, Enabled, LastLogon, PasswordRequired, PasswordLastSet | Format-Table -AutoSize | Out-String
Write-Output 'Administrators'
Get-LocalGroupMember Administrators -ErrorAction SilentlyContinue | Select-Object Name, ObjectClass, PrincipalSource | Format-Table -AutoSize | Out-String
Write-Output 'Remote Desktop Users'
Get-LocalGroupMember 'Remote Desktop Users' -ErrorAction SilentlyContinue | Select-Object Name, ObjectClass, PrincipalSource | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> ShareActions() => new[]
    {
        new ToolAction
        {
            Title = "Shared folders review",
            Description = "Shows Windows network shares and their local paths.",
            Script = """
$ErrorActionPreference = 'Continue'
Get-SmbShare | Select-Object Name, Path, Description, ShareState, FolderEnumerationMode | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Open sharing settings",
            Description = "Opens advanced sharing settings.",
            Script = """
Start-Process 'control.exe' '/name Microsoft.NetworkAndSharingCenter /page Advanced'
Write-Output 'Advanced sharing settings opened.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> RemoteAccessActions() => new[]
    {
        new ToolAction
        {
            Title = "Remote access review",
            Description = "Checks Remote Desktop and Remote Assistance settings.",
            Script = """
$ErrorActionPreference = 'Continue'
Write-Output 'Remote Desktop setting'
Get-ItemProperty 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name fDenyTSConnections -ErrorAction SilentlyContinue | Format-List | Out-String
Write-Output 'Remote Desktop firewall rules'
Get-NetFirewallRule -DisplayGroup 'Remote Desktop' -ErrorAction SilentlyContinue | Select-Object DisplayName, Enabled, Profile | Format-Table -AutoSize | Out-String
Write-Output 'Remote Assistance firewall rules'
Get-NetFirewallRule | Where-Object DisplayName -like '*Remote Assistance*' | Select-Object DisplayName, Enabled, Profile | Format-Table -AutoSize | Out-String
"""
        }
    };

    private static IReadOnlyList<ToolAction> HistoryCleanupActions() => new[]
    {
        new ToolAction { Title = "Clear Explorer history", Description = "Clears File Explorer recent history.", Script = """
RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 1
Write-Output 'Explorer and recent item cleanup command sent.'
""" },
        new ToolAction { Title = "Clear Run history", Description = "Clears the current user's Run dialog history.", RequiresAdmin = false, Script = """
Remove-Item 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU' -Recurse -Force -ErrorAction SilentlyContinue
Write-Output 'Run history cleared.'
""" },
        new ToolAction { Title = "Clear clipboard", Description = "Clears the current clipboard content.", Script = """
Set-Clipboard -Value ''
Write-Output 'Clipboard cleared.'
""" }
    };

    private static IReadOnlyList<ToolAction> DesktopToolActions() => new[]
    {
        new ToolAction
        {
            Title = "Desktop clutter report",
            Description = "Reports desktop files, shortcuts and large items.",
            Script = """
$desktop = [Environment]::GetFolderPath('Desktop')
Get-ChildItem $desktop -Force -ErrorAction SilentlyContinue | Select-Object Name, Extension, Length, LastWriteTime, FullName | Sort-Object LastWriteTime | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Broken desktop shortcuts",
            Description = "Finds desktop shortcuts whose targets are missing.",
            Script = """
$desktop = [Environment]::GetFolderPath('Desktop')
$shell = New-Object -ComObject WScript.Shell
Get-ChildItem $desktop -Filter *.lnk -ErrorAction SilentlyContinue | ForEach-Object {
    $shortcut = $shell.CreateShortcut($_.FullName)
    if ($shortcut.TargetPath -and -not (Test-Path $shortcut.TargetPath)) { [pscustomobject]@{ Shortcut=$_.FullName; Target=$shortcut.TargetPath } }
} | Format-Table -AutoSize | Out-String
"""
        },
        new ToolAction
        {
            Title = "Start Menu shortcut report",
            Description = "Lists Start Menu shortcut files for review.",
            Script = """
$paths = @([Environment]::GetFolderPath('StartMenu'), [Environment]::GetFolderPath('CommonStartMenu'))
foreach ($path in $paths) { Get-ChildItem $path -Recurse -Filter *.lnk -ErrorAction SilentlyContinue | Select-Object Name, FullName }
"""
        }
    };

    private static IReadOnlyList<ToolAction> ExplorerToolActions() => new[]
    {
        new ToolAction
        {
            Title = "Explorer add on review",
            Description = "Lists common Explorer shell extension registry areas.",
            Script = """
$paths = @(
'HKLM:\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved',
'HKCU:\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved',
'HKLM:\Software\Microsoft\Windows\CurrentVersion\Explorer\ShellIconOverlayIdentifiers'
)
foreach ($path in $paths) { Write-Output $path; Get-ItemProperty $path -ErrorAction SilentlyContinue | Format-List | Out-String }
"""
        },
        new ToolAction
        {
            Title = "Restart Explorer",
            Description = "Restarts Windows Explorer. This can clear stuck shell behaviour.",
            Script = """
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Process explorer.exe
Write-Output 'Explorer restarted.'
"""
        }
    };

    private static IReadOnlyList<ToolAction> VersionDefaultActions() => new[]
    {
        new ToolAction
        {
            Title = "Version aware recommendations",
            Description = "Creates recommendations based on Windows version, edition and device type.",
            Script = """
$cv = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
$isLaptop = Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue
$rec = New-Object System.Collections.Generic.List[string]
if ($cv.ProductName -match 'Windows 10') { $rec.Add('Windows 10 detected. Keep visual effects conservative on older devices.') }
if ($cv.ProductName -match 'Windows 11') { $rec.Add('Windows 11 detected. Review startup apps and transparency effects for low end PCs.') }
if ($isLaptop) { $rec.Add('Laptop detected. Avoid aggressive high performance profiles unless plugged in.') } else { $rec.Add('Desktop detected. Performance power settings are usually safer than on laptops.') }
$rec.Add('Keep Defender and Firewall enabled for all profiles.')
$rec | ForEach-Object { Write-Output $_ }
"""
        }
    };

    private static IReadOnlyList<ToolAction> AuditActions() => new[]
    {
        new ToolAction
        {
            Title = "Create audit report",
            Description = "Collects recent WinForge logs and snapshot files into an audit report.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
$reportRoot = Join-Path $root 'Reports'
New-Item -ItemType Directory -Path $reportRoot -Force | Out-Null
$path = Join-Path $reportRoot ('audit-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
'WinForge audit report' | Out-File $path -Encoding UTF8
"Generated: $(Get-Date)" | Out-File $path -Append -Encoding UTF8
'Logs' | Out-File $path -Append -Encoding UTF8
Get-ChildItem (Join-Path $root 'Logs') -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 20 Name, LastWriteTime, Length | Format-Table -AutoSize | Out-String | Out-File $path -Append -Encoding UTF8
'Snapshots' | Out-File $path -Append -Encoding UTF8
Get-ChildItem (Join-Path $root 'Snapshots') -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 20 Name, LastWriteTime, Length | Format-Table -AutoSize | Out-String | Out-File $path -Append -Encoding UTF8
Write-Output "Audit report saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> HelpActions() => new[]
    {
        new ToolAction
        {
            Title = "Create help guide",
            Description = "Creates a local guide explaining the main WinForge areas.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'WinForge-help-guide.txt'
@'
WinForge help guide

Dashboard shows system readiness and health.
Optimise applies selected reversible options.
Startup controls apps that launch with Windows.
Storage and Deep Storage help find space pressure.
Security pages review Defender, Firewall, accounts, shares and persistence.
Repair pages run built in Windows repair commands.
Advanced tools should be reviewed before applying changes.
'@ | Out-File $path -Encoding UTF8
Write-Output "Help guide saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> AdvancedModeActions() => new[]
    {
        new ToolAction
        {
            Title = "Create advanced mode note",
            Description = "Creates a note explaining which areas should be handled carefully.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Config'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'advanced-mode.json'
@{
    Enabled = $false
    ProtectedAreas = @('Services','Firewall','Registry backed settings','Scheduled task changes','Windows features')
    Message = 'Advanced actions should be reviewed before use.'
} | ConvertTo-Json -Depth 5 | Out-File $path -Encoding UTF8
Write-Output "Advanced mode file saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> UpdaterActions() => new[]
    {
        new ToolAction
        {
            Title = "Create release notes template",
            Description = "Creates a simple release notes template for packaged WinForge builds.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Release'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'release-notes-template.md'
@'
# WinForge release notes

## Added

## Changed

## Fixed

## Notes

'@ | Out-File $path -Encoding UTF8
Write-Output "Release notes template saved to $path"
"""
        },
        new ToolAction
        {
            Title = "Create portable package folder",
            Description = "Creates a folder layout for a portable WinForge build.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\PortablePackage'
New-Item -ItemType Directory -Path $root,(Join-Path $root 'Data'),(Join-Path $root 'Reports'),(Join-Path $root 'Logs') -Force | Out-Null
Write-Output "Portable package folder created at $root"
"""
        }
    };

    private static IReadOnlyList<ToolAction> ThemeActions() => new[]
    {
        new ToolAction
        {
            Title = "Create theme preferences",
            Description = "Creates a theme preference file for future UI theme switching.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Config'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'themes.json'
@{
    Current = 'Dark Gold'
    Available = @('Dark Gold','Dark Green','Dark Blue','Compact','Large Text')
} | ConvertTo-Json -Depth 5 | Out-File $path -Encoding UTF8
Write-Output "Theme preferences saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> HomeForgeActions() => new[]
    {
        new ToolAction
        {
            Title = "Detect HomeForge",
            Description = "Checks for the default HomeForge server folder and creates a compatibility note.",
            Script = """
$homeForge = 'C:\HomeServer'
[pscustomobject]@{
    HomeForgeFolder = $homeForge
    Exists = Test-Path $homeForge
    Recommendation = 'Use the home server profile when this PC hosts always on apps.'
} | Format-List | Out-String
"""
        },
        new ToolAction
        {
            Title = "Create HomeForge friendly profile note",
            Description = "Creates notes for avoiding server breaking optimisations.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'homeforge-friendly-profile.txt'
@'
HomeForge friendly optimisation profile

Avoid disabling network adapters.
Avoid sleep or hibernate if the PC runs server apps.
Keep firewall enabled but review app rules carefully.
Do not disable Docker, database services or server app startup entries unless intended.
Schedule updates and restarts during a maintenance window.
'@ | Out-File $path -Encoding UTF8
Write-Output "HomeForge friendly profile note saved to $path"
"""
        }
    };

    private static IReadOnlyList<ToolAction> SupportBundleActions() => new[]
    {
        new ToolAction
        {
            Title = "Create support bundle",
            Description = "Exports common reports and WinForge logs into a zip file.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge'
$bundleRoot = Join-Path $root ('SupportBundle-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $bundleRoot -Force | Out-Null
Get-ComputerInfo | Out-File (Join-Path $bundleRoot 'computer-info.txt') -Encoding UTF8
Get-Process | Sort-Object CPU -Descending | Select-Object -First 50 ProcessName, Id, CPU, WorkingSet64 | Format-Table -AutoSize | Out-String | Out-File (Join-Path $bundleRoot 'processes.txt') -Encoding UTF8
Get-NetIPConfiguration | Format-List | Out-String | Out-File (Join-Path $bundleRoot 'network.txt') -Encoding UTF8
Get-NetFirewallProfile | Format-Table -AutoSize | Out-String | Out-File (Join-Path $bundleRoot 'firewall.txt') -Encoding UTF8
Copy-Item (Join-Path $root 'Logs') $bundleRoot -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $root 'Reports') $bundleRoot -Recurse -Force -ErrorAction SilentlyContinue
$zip = $bundleRoot + '.zip'
Compress-Archive -Path $bundleRoot -DestinationPath $zip -Force
Write-Output "Support bundle created at $zip"
"""
        }
    };

    private static IReadOnlyList<ToolAction> ActionCatalogActions() => new[]
    {
        new ToolAction
        {
            Title = "Export action catalogue",
            Description = "Creates a simple catalogue of action categories, risk notes and intended usage.",
            Script = """
$root = Join-Path $env:ProgramData 'WinForge\Reports'
New-Item -ItemType Directory -Path $root -Force | Out-Null
$path = Join-Path $root 'action-catalog.json'
@(
    @{Category='Optimise'; Risk='Low to medium'; Reversible='Mostly'; Notes='Uses selected toggles and snapshots'},
    @{Category='Startup'; Risk='Low'; Reversible='Yes'; Notes='Startup entries can be restored from saved state'},
    @{Category='Services'; Risk='Medium'; Reversible='Manual review'; Notes='Advanced mode recommended'},
    @{Category='Firewall'; Risk='Medium'; Reversible='Manual review'; Notes='Do not remove rules without understanding them'},
    @{Category='Repair'; Risk='Low to medium'; Reversible='Depends on Windows tool'; Notes='Runs built in repair commands'},
    @{Category='Security'; Risk='Low'; Reversible='Mostly'; Notes='Reports first, changes only when selected'}
) | ConvertTo-Json -Depth 5 | Out-File $path -Encoding UTF8
Write-Output "Action catalogue saved to $path"
"""
        }
    };

}
