@echo off
setlocal
cd /d "%~dp0"
title WinForge Clean

if exist publish rmdir /s /q publish
for /d /r %%d in (bin,obj) do if exist "%%d" rmdir /s /q "%%d"
echo Clean complete.
pause
