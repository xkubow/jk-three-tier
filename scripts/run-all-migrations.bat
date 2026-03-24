@echo off
setlocal EnableExtensions

REM Runs all backend database migrations sequentially.
REM Usage:
REM   run-all-migrations.bat
REM   run-all-migrations.bat Local-Dev
REM
REM Required env vars:
REM   set DB_PASSWORD=change-me
REM
REM Optional env vars:
REM   set DB_HOST=172.27.76.33
REM   set DB_PORT=30432
REM   set DB_USERNAME=postgres

set CONFIGURATION=%1
if "%CONFIGURATION%"=="" set CONFIGURATION=Local-Dev

set CLI_PROJECT=./Tools/JK.Migrations.Cli/JK.Migrations.Cli.csproj

if "%DB_HOST%"=="" set DB_HOST=172.27.76.33
if "%DB_PORT%"=="" set DB_PORT=30432
if "%DB_USERNAME%"=="" set DB_USERNAME=postgres

if "%DB_PASSWORD%"=="" (
  echo ERROR: DB_PASSWORD environment variable is required.
  echo Example: set DB_PASSWORD=change-me ^&^& run-all-migrations.bat
  exit /b 1
)

echo ^>^>^> Building migration CLI: %CLI_PROJECT%
dotnet build "%CLI_PROJECT%" -c "%CONFIGURATION%"
if errorlevel 1 goto :error

echo.
call :runModule Configuration jk_configuration ./Modules/Configuration/JK.Configuration.Database/JK.Configuration.Database.csproj ./Modules/Configuration/JK.Configuration.Database/bin/%CONFIGURATION%/net9.0/JK.Configuration.Database.dll
if errorlevel 1 goto :error

echo.
call :runModule Order jk_order ./Modules/Order/JK.Order.Database/JK.Order.Database.csproj ./Modules/Order/JK.Order.Database/bin/%CONFIGURATION%/net9.0/JK.Order.Database.dll
if errorlevel 1 goto :error

echo All migrations finished successfully.
exit /b 0

:runModule
set MODULE_NAME=%~1
set MODULE_DATABASE=%~2
set MODULE_PROJECT=%~3
set MODULE_ASSEMBLY=%~4
set CONNECTION_STRING=Host=%DB_HOST%;Port=%DB_PORT%;Database=%MODULE_DATABASE%;Username=%DB_USERNAME%;Password=%DB_PASSWORD%;

echo ^>^>^> Building %MODULE_NAME% database project: %MODULE_PROJECT%
dotnet build "%MODULE_PROJECT%" -c "%CONFIGURATION%"
if errorlevel 1 exit /b 1

echo ^>^>^> Ensuring database exists and running %MODULE_NAME% migrations on %MODULE_DATABASE%
dotnet run --project "%CLI_PROJECT%" --configuration "%CONFIGURATION%" -- ^
  --connection "%CONNECTION_STRING%" ^
  --assembly "%MODULE_ASSEMBLY%" ^
  --ensure-db
if errorlevel 1 exit /b 1

echo ^>^>^> %MODULE_NAME% migrations finished
exit /b 0

:error
echo Migration script failed.
exit /b 1
