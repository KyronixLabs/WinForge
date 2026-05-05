$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe = Join-Path $root 'publish\win-x64\WinForge.exe'
$icon = Join-Path $root 'publish\win-x64\WinForge.ico'
$sourceIcon = Join-Path $root 'src\WinForge.App\Assets\WinForge.ico'

if (-not (Test-Path $exe)) {
    throw "WinForge.exe was not found. Run Build-WinForge.bat first."
}

if (-not (Test-Path $icon)) {
    Copy-Item -Path $sourceIcon -Destination $icon -Force
}

$desktop = [Environment]::GetFolderPath('DesktopDirectory')
$shortcutPath = Join-Path $desktop 'WinForge.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = Split-Path -Parent $exe
$shortcut.IconLocation = "$icon,0"
$shortcut.Description = 'WinForge Windows optimisation'
$shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath"
Write-Host "Icon used: $icon"
