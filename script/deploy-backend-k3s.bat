@echo off
REM Thin launcher: run the Ubuntu/bash implementation from Windows cmd/PowerShell.
REM Prefer running directly in Ubuntu:  bash script/deploy-backend-k3s.sh [options]

cd /d "%~dp0.."
wsl.exe bash ./script/deploy-backend-k3s.sh %*
exit /b %ERRORLEVEL%
