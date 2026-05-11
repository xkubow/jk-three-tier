@echo off
setlocal
cd /d "%~dp0.."

echo Building jk-configuration:local...
docker build -t jk-configuration:local -f backend/Api/JK.Configuration.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-configuration-local.tar...
docker save jk-configuration:local -o jk-configuration-local.tar
if errorlevel 1 exit /b 1

echo Building jk-messaging:local...
docker build -t jk-messaging:local -f backend/Api/JK.Messaging.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-messaging-local.tar...
docker save jk-messaging:local -o jk-messaging-local.tar
if errorlevel 1 exit /b 1

echo Building jk-offer:local...
docker build -t jk-offer:local -f backend/Api/JK.Offer.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-offer-local.tar...
docker save jk-offer:local -o jk-offer-local.tar
if errorlevel 1 exit /b 1

echo Building jk-order:local...
docker build -t jk-order:local -f backend/Api/JK.Order.CZ/Dockerfile .
if errorlevel 1 exit /b 1
echo Saving jk-order-local.tar...
docker save jk-order:local -o jk-order-local.tar
if errorlevel 1 exit /b 1

echo All backend API images built and exported successfully.
