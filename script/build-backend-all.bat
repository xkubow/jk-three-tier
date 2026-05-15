@echo off
setlocal EnableExtensions
cd /d "%~dp0.."

set "IMAGE_TAG="
if /i "%~1"=="-v" (
  if "%~2"=="" (
    echo Usage: %~nx0 [-v ^<image-tag^>]
    echo   Example: %~nx0 -v v5
    echo   If -v is omitted, a timestamp tag is generated.
    echo   You can also pass the tag as the first argument: %~nx0 v5
    exit /b 1
  )
  set "IMAGE_TAG=%~2"
) else if not "%~1"=="" (
  if "%~1:~0,1%"=="-" (
    echo Unknown option: %~1
    echo Usage: %~nx0 [-v ^<image-tag^>]  or  %~nx0 ^<image-tag^>
    exit /b 1
  )
  set "IMAGE_TAG=%~1"
)

if not defined IMAGE_TAG (
  for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMddHHmmss"') do set "IMAGE_TAG=%%i"
)
if not defined IMAGE_TAG (
  echo Failed to resolve backend image tag.
  exit /b 1
)

set "TAR_SUFFIX=%IMAGE_TAG::=-%"
if not defined TAR_SUFFIX (
  echo Failed to derive tar filename suffix from tag.
  exit /b 1
)

echo Using backend image tag: %IMAGE_TAG%
> ".backend-image-tag" echo %IMAGE_TAG%
if errorlevel 1 exit /b 1
echo Saved image tag to .backend-image-tag
echo.

echo Updating k8s deployment images to tag %IMAGE_TAG%...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$t='%IMAGE_TAG%'; $base=(Get-Location).Path; foreach ($pair in @(@('configuration.yaml','jk-configuration'),@('messaging.yaml','jk-messaging'),@('offer.yaml','jk-offer'),@('order.yaml','jk-order'))) { $p=[IO.Path]::Combine($base,'k8s',$pair[0]); $n=$pair[1]; $x=[IO.File]::ReadAllText($p); $pat='(' + [regex]::Escape($n) + ':)[^\s]+'; $x2=[regex]::Replace($x,$pat,{param($m) $m.Groups[1].Value+$t}); [IO.File]::WriteAllText($p,$x2) }"
if errorlevel 1 exit /b 1
echo.

echo Building jk-configuration:%IMAGE_TAG%...
docker build -t jk-configuration:%IMAGE_TAG% -f backend/Api/JK.Configuration.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-configuration-%TAR_SUFFIX%.tar...
docker save jk-configuration:%IMAGE_TAG% -o jk-configuration-%TAR_SUFFIX%.tar
if errorlevel 1 exit /b 1

echo Building jk-messaging:%IMAGE_TAG%...
docker build -t jk-messaging:%IMAGE_TAG% -f backend/Api/JK.Messaging.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-messaging-%TAR_SUFFIX%.tar...
docker save jk-messaging:%IMAGE_TAG% -o jk-messaging-%TAR_SUFFIX%.tar
if errorlevel 1 exit /b 1

echo Building jk-offer:%IMAGE_TAG%...
docker build -t jk-offer:%IMAGE_TAG% -f backend/Api/JK.Offer.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-offer-%TAR_SUFFIX%.tar...
docker save jk-offer:%IMAGE_TAG% -o jk-offer-%TAR_SUFFIX%.tar
if errorlevel 1 exit /b 1

echo Building jk-order:%IMAGE_TAG%...
docker build -t jk-order:%IMAGE_TAG% -f backend/Api/JK.Order.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-order-%TAR_SUFFIX%.tar...
docker save jk-order:%IMAGE_TAG% -o jk-order-%TAR_SUFFIX%.tar
if errorlevel 1 exit /b 1

echo.
echo All backend API images built and exported successfully with tag %IMAGE_TAG%.
echo Tar archives: jk-*-%TAR_SUFFIX%.tar
