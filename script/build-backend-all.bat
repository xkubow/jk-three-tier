@echo off
setlocal
cd /d "%~dp0.."

set "IMAGE_TAG=%~1"
if not defined IMAGE_TAG (
  for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMddHHmmss"') do set "IMAGE_TAG=%%i"
)
if not defined IMAGE_TAG (
  echo Failed to resolve backend image tag.
  exit /b 1
)

echo Using backend image tag: %IMAGE_TAG%
> ".backend-image-tag" echo %IMAGE_TAG%
if errorlevel 1 exit /b 1
echo Saved image tag to .backend-image-tag
echo.

echo Building jk-configuration:%IMAGE_TAG%...
docker build -t jk-configuration:%IMAGE_TAG% -f backend/Api/JK.Configuration.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-configuration-local.tar...
docker save jk-configuration:%IMAGE_TAG% -o jk-configuration-local.tar
if errorlevel 1 exit /b 1

echo Building jk-messaging:%IMAGE_TAG%...
docker build -t jk-messaging:%IMAGE_TAG% -f backend/Api/JK.Messaging.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-messaging-local.tar...
docker save jk-messaging:%IMAGE_TAG% -o jk-messaging-local.tar
if errorlevel 1 exit /b 1

echo Building jk-offer:%IMAGE_TAG%...
docker build -t jk-offer:%IMAGE_TAG% -f backend/Api/JK.Offer.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-offer-local.tar...
docker save jk-offer:%IMAGE_TAG% -o jk-offer-local.tar
if errorlevel 1 exit /b 1

echo Building jk-order:%IMAGE_TAG%...
docker build -t jk-order:%IMAGE_TAG% -f backend/Api/JK.Order.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-order-local.tar...
docker save jk-order:%IMAGE_TAG% -o jk-order-local.tar
if errorlevel 1 exit /b 1

echo.
echo All backend API images built and exported successfully with tag %IMAGE_TAG%.
