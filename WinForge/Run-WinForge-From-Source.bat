@echo off
setlocal
cd /d "%~dp0"
title WinForge Source Run

dotnet run --project src\WinForge.App\WinForge.App.csproj -c Release
pause
