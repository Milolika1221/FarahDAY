@echo off
cd /d "%~dp0\Server"
start "" "Server.exe" 127.0.0.1:5000
exit