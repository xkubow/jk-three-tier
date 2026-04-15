@echo off
setlocal

:: Configuration for Database Migration
set CONNECTION_STRING="Host=172.27.76.33;Port=30432;Database=jk_messaging"
set CLI_PROJECT="..\..\..\Tools\JK.Migrations.Cli"
set DB_PROJECT="JK.Messaging.Database.csproj"
set ASSEMBLY_PATH="bin\Local-Dev\net9.0\JK.Messaging.Database.dll"

echo Building Migration CLI...
dotnet build %CLI_PROJECT% -c Local-Dev

echo Building Database Project...
dotnet build %DB_PROJECT% -c Local-Dev

echo Running Migrations...
dotnet run --project %CLI_PROJECT% --connection %CONNECTION_STRING% --assembly %ASSEMBLY_PATH% --ensure-db

echo Done.
pause
