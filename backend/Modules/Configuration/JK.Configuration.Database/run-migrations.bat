@echo off
setlocal

REM Configuration for Database Migration
set CONNECTION_STRING=Host=172.27.76.33;Port=30432;Database=jk_configuration
set CLI_PROJECT=../../../Tools/JK.Migrations.Cli
set DB_PROJECT=JK.Configuration.Database.csproj
set ASSEMBLY_PATH=bin/Debug/net9.0/JK.Configuration.Database.dll

echo Building Migration CLI...
dotnet build "%CLI_PROJECT%" -c Local-Dev
if errorlevel 1 goto :error

echo Building Database Project...
dotnet build "%DB_PROJECT%" -c Local-Dev
if errorlevel 1 goto :error

echo Ensuring database exists and running migrations...
dotnet run --project "%CLI_PROJECT%" -- ^
  --connection "%CONNECTION_STRING%" ^
  --assembly "%ASSEMBLY_PATH%" ^
  --ensure-db
if errorlevel 1 goto :error

echo Done.
goto :eof

:error
echo Migration script failed.
exit /b 1