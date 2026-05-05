@echo off
setlocal
cd /d "%~dp0"
title WinForge Shortcut

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Create-Desktop-Shortcut.ps1"
if errorlevel 1 (
    echo.
    echo Shortcut creation failed.
    pause
    exit /b 1
)

echo.
echo Desktop shortcut updated with the WinForge icon.
pause
exit /b 0
