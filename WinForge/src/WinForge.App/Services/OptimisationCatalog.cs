using WinForge.Models;

namespace WinForge.Services;

public static class OptimisationCatalog
{
    public static List<OptimisationOption> CreateDefaultOptions()
    {
        return new List<OptimisationOption>
        {
            new()
            {
                Id = "cleanup-user-temp",
                Category = "Clean Up",
                Title = "Clean user temporary files",
                Summary = "Removes files from the current user temporary folder.",
                Details = "Useful for clearing old installer scraps, caches and files left behind by apps.",
                IsSelected = true,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
$temp = [IO.Path]::GetTempPath()
Get-ChildItem -Path $temp -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Output "Cleaned user temporary files from $temp"
"""
            },
            new()
            {
                Id = "cleanup-windows-temp",
                Category = "Clean Up",
                Title = "Clean Windows temporary files",
                Summary = "Clears old files from the Windows temp folder.",
                Details = "Requires administrator access and skips files that Windows is currently using.",
                IsSelected = true,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
$path = Join-Path $env:windir 'Temp'
Get-ChildItem -Path $path -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Output "Cleaned Windows temporary files from $path"
"""
            },
            new()
            {
                Id = "cleanup-recycle-bin",
                Category = "Clean Up",
                Title = "Empty Recycle Bin",
                Summary = "Clears deleted files from all drives.",
                Details = "This frees space but cannot be reverted by WinForge.",
                IsSelected = false,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
Clear-RecycleBin -Force -ErrorAction SilentlyContinue
Write-Output 'Recycle Bin cleanup completed.'
"""
            },
            new()
            {
                Id = "cleanup-update-cache",
                Category = "Clean Up",
                Title = "Clean Windows Update cache",
                Summary = "Removes old downloaded update files.",
                Details = "Windows can download needed updates again later.",
                IsSelected = false,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
Stop-Service wuauserv -Force -ErrorAction SilentlyContinue
Stop-Service bits -Force -ErrorAction SilentlyContinue
$download = Join-Path $env:windir 'SoftwareDistribution\Download'
Get-ChildItem -Path $download -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Start-Service bits -ErrorAction SilentlyContinue
Start-Service wuauserv -ErrorAction SilentlyContinue
Write-Output 'Windows Update cache cleanup completed.'
"""
            },
            new()
            {
                Id = "network-flush-dns",
                Category = "Network",
                Title = "Flush DNS cache",
                Summary = "Clears cached DNS lookups.",
                Details = "Useful when websites, local services or domains resolve incorrectly.",
                IsSelected = true,
                RequiresAdmin = false,
                ApplyScript = """
ipconfig /flushdns
Write-Output 'DNS cache flushed.'
"""
            },
            new()
            {
                Id = "network-reset-winsock",
                Category = "Network",
                Title = "Reset Winsock",
                Summary = "Repairs common network stack issues.",
                Details = "A restart is usually recommended after this action.",
                IsSelected = false,
                ApplyScript = """
netsh winsock reset
Write-Output 'Winsock reset completed. Restart Windows when convenient.'
"""
            },
            new()
            {
                Id = "network-private-profile",
                Category = "Network",
                Title = "Set active network to Private",
                Summary = "Marks the current network as trusted home or office network.",
                Details = "Useful for local devices and services. Public networks should stay public.",
                IsSelected = false,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
Get-NetConnectionProfile | Where-Object { $_.NetworkCategory -ne 'DomainAuthenticated' } | ForEach-Object {
    Set-NetConnectionProfile -InterfaceIndex $_.InterfaceIndex -NetworkCategory Private
    Write-Output "Set $($_.Name) to Private."
}
"""
            },
            new()
            {
                Id = "power-disable-usb-suspend",
                Category = "Performance",
                Title = "Keep USB devices awake",
                Summary = "Disables USB selective suspend on AC power.",
                Details = "Useful for desktops, gaming PCs and always on machines.",
                IsSelected = false,
                ApplyScript = """
powercfg /setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg /setactive SCHEME_CURRENT
Write-Output 'USB selective suspend disabled on AC power.'
""",
                RevertScript = """
powercfg /setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 1
powercfg /setactive SCHEME_CURRENT
Write-Output 'USB selective suspend restored on AC power.'
"""
            },
            new()
            {
                Id = "power-disable-pcie-saving",
                Category = "Performance",
                Title = "Reduce PCIe power saving",
                Summary = "Turns off PCIe link state power management on AC power.",
                Details = "Can help with some GPU, storage and network adapter stability issues.",
                IsSelected = false,
                ApplyScript = """
powercfg /setacvalueindex SCHEME_CURRENT 501a4d13-42af-4429-9fd1-a8218c268e20 ee12f906-d277-404b-b6da-e5fa1a576df5 0
powercfg /setactive SCHEME_CURRENT
Write-Output 'PCIe link state power management disabled on AC power.'
""",
                RevertScript = """
powercfg /setacvalueindex SCHEME_CURRENT 501a4d13-42af-4429-9fd1-a8218c268e20 ee12f906-d277-404b-b6da-e5fa1a576df5 1
powercfg /setactive SCHEME_CURRENT
Write-Output 'PCIe link state power management restored on AC power.'
"""
            },
            new()
            {
                Id = "power-disable-hibernate",
                Category = "Performance",
                Title = "Disable hibernation",
                Summary = "Turns off hibernation and removes the hibernation file.",
                Details = "Can free disk space on desktops. Laptops may prefer keeping hibernation enabled.",
                IsSelected = false,
                ApplyScript = """
powercfg /hibernate off
Write-Output 'Hibernation disabled.'
""",
                RevertScript = """
powercfg /hibernate on
Write-Output 'Hibernation enabled.'
"""
            },
            new()
            {
                Id = "visual-disable-transparency",
                Category = "Performance",
                Title = "Disable transparency effects",
                Summary = "Turns off Windows transparency effects for a lighter desktop.",
                Details = "Useful for older PCs or users who prefer a simpler interface.",
                IsSelected = false,
                RequiresAdmin = false,
                ApplyScript = """
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name EnableTransparency -Type DWord -Value 0
Write-Output 'Transparency effects disabled for the current user.'
""",
                RevertScript = """
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name EnableTransparency -Type DWord -Value 1
Write-Output 'Transparency effects enabled for the current user.'
"""
            },
            new()
            {
                Id = "gaming-game-mode",
                Category = "Gaming",
                Title = "Enable Game Mode",
                Summary = "Enables Windows Game Mode for the current user.",
                Details = "Helps Windows prioritise gaming workloads where supported.",
                IsSelected = false,
                RequiresAdmin = false,
                ApplyScript = """
New-Item -Path 'HKCU:\Software\Microsoft\GameBar' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\GameBar' -Name AllowAutoGameMode -Type DWord -Value 1
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\GameBar' -Name AutoGameModeEnabled -Type DWord -Value 1
Write-Output 'Game Mode enabled for the current user.'
""",
                RevertScript = """
New-Item -Path 'HKCU:\Software\Microsoft\GameBar' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\GameBar' -Name AllowAutoGameMode -Type DWord -Value 0
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\GameBar' -Name AutoGameModeEnabled -Type DWord -Value 0
Write-Output 'Game Mode disabled for the current user.'
"""
            },
            new()
            {
                Id = "gaming-disable-captures",
                Category = "Gaming",
                Title = "Disable background game recording",
                Summary = "Turns off Game DVR style background capture.",
                Details = "Can reduce background overhead for gaming PCs.",
                IsSelected = false,
                RequiresAdmin = false,
                ApplyScript = """
New-Item -Path 'HKCU:\System\GameConfigStore' -Force | Out-Null
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\System\GameConfigStore' -Name GameDVR_Enabled -Type DWord -Value 0
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Name AppCaptureEnabled -Type DWord -Value 0
Write-Output 'Background game recording disabled.'
""",
                RevertScript = """
New-Item -Path 'HKCU:\System\GameConfigStore' -Force | Out-Null
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\System\GameConfigStore' -Name GameDVR_Enabled -Type DWord -Value 1
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Name AppCaptureEnabled -Type DWord -Value 1
Write-Output 'Background game recording restored.'
"""
            },
            new()
            {
                Id = "privacy-disable-ad-id",
                Category = "Privacy",
                Title = "Disable advertising ID",
                Summary = "Turns off the Windows advertising ID for the current user.",
                Details = "Reduces personalised advertising identifiers in supported Windows experiences.",
                IsSelected = false,
                RequiresAdmin = false,
                ApplyScript = """
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Name Enabled -Type DWord -Value 0
Write-Output 'Advertising ID disabled.'
""",
                RevertScript = """
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Name Enabled -Type DWord -Value 1
Write-Output 'Advertising ID enabled.'
"""
            },
            new()
            {
                Id = "privacy-disable-activity-history",
                Category = "Privacy",
                Title = "Reduce activity history",
                Summary = "Disables activity history publishing for the current machine.",
                Details = "Useful for a cleaner privacy baseline.",
                IsSelected = false,
                ApplyScript = """
New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Force | Out-Null
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Name PublishUserActivities -Type DWord -Value 0
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Name UploadUserActivities -Type DWord -Value 0
Write-Output 'Activity history publishing reduced.'
""",
                RevertScript = """
Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Name PublishUserActivities -ErrorAction SilentlyContinue
Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Name UploadUserActivities -ErrorAction SilentlyContinue
Write-Output 'Activity history policy values removed.'
"""
            },
            new()
            {
                Id = "security-enable-firewall",
                Category = "Security",
                Title = "Enable Windows Firewall",
                Summary = "Makes sure the firewall is enabled on all profiles.",
                Details = "Recommended for almost every PC.",
                IsSelected = true,
                ApplyScript = """
Set-NetFirewallProfile -Profile Domain,Private,Public -Enabled True
Write-Output 'Windows Firewall enabled for all profiles.'
"""
            },
            new()
            {
                Id = "security-disable-smb1",
                Category = "Security",
                Title = "Disable SMBv1",
                Summary = "Disables the legacy SMBv1 file sharing feature.",
                Details = "SMBv1 is old and should usually stay disabled.",
                IsSelected = false,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
Disable-WindowsOptionalFeature -Online -FeatureName SMB1Protocol -NoRestart
Set-SmbServerConfiguration -EnableSMB1Protocol $false -Force
Write-Output 'SMBv1 disable action completed. A restart may be required.'
""",
                RevertScript = """
$ErrorActionPreference = 'Continue'
Enable-WindowsOptionalFeature -Online -FeatureName SMB1Protocol -NoRestart
Write-Output 'SMBv1 enable action completed. A restart may be required.'
"""
            },
            new()
            {
                Id = "security-defender-realtime",
                Category = "Security",
                Title = "Enable Defender real time protection",
                Summary = "Makes sure Microsoft Defender real time protection is enabled.",
                Details = "Keeps Windows security features active instead of trading safety for performance.",
                IsSelected = true,
                ApplyScript = """
$ErrorActionPreference = 'Continue'
Set-MpPreference -DisableRealtimeMonitoring $false
Write-Output 'Defender real time protection enable action completed.'
"""
            },
            new()
            {
                Id = "repair-sfc",
                Category = "Repair",
                Title = "Run System File Checker",
                Summary = "Checks and repairs protected Windows system files.",
                Details = "This can take a while and should not be interrupted.",
                IsSelected = false,
                ApplyScript = """
sfc /scannow
Write-Output 'System File Checker completed.'
"""
            },
            new()
            {
                Id = "repair-dism",
                Category = "Repair",
                Title = "Run DISM health restore",
                Summary = "Repairs the Windows component store.",
                Details = "Useful when Windows updates or system files are damaged.",
                IsSelected = false,
                ApplyScript = """
DISM /Online /Cleanup-Image /RestoreHealth
Write-Output 'DISM health restore completed.'
"""
            },
            new()
            {
                Id = "shell-restart-explorer",
                Category = "Repair",
                Title = "Restart File Explorer",
                Summary = "Restarts the Windows shell without rebooting.",
                Details = "Useful after visual changes or when Explorer feels stuck.",
                IsSelected = false,
                RequiresAdmin = false,
                ApplyScript = """
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Process explorer.exe
Write-Output 'File Explorer restarted.'
"""
            }
        };
    }
}
