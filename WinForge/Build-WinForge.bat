@echo off
setlocal
cd /d "%~dp0"
title WinForge Build

echo Building WinForge...
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo .NET SDK was not found.
    echo Install the .NET 8 SDK, then run this file again.
    pause
    exit /b 1
)

dotnet restore WinForge.sln
if errorlevel 1 goto failed

dotnet build WinForge.sln -c Release --no-restore
if errorlevel 1 goto failed

dotnet publish src\WinForge.App\WinForge.App.csproj -c Release -r win-x64 --self-contained false -o publish\win-x64
if errorlevel 1 goto failed

if not exist publish\win-x64 mkdir publish\win-x64
copy /Y src\WinForge.App\Assets\WinForge.ico publish\win-x64\WinForge.ico >nul
call Create-Desktop-Shortcut.bat
if errorlevel 1 goto failed

echo.
echo Build complete.
echo Output: publish\win-x64\WinForge.exe
echo Desktop shortcut: %%USERPROFILE%%\Desktop\WinForge.lnk
echo.
pause
exit /b 0

:failed
echo.
echo Build failed. Check the error output above.
pause
exit /b 1
