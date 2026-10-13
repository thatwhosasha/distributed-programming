@echo off
cd /d C:\code\nginx-1.30.0
nginx -s stop

taskkill /F /IM dotnet.exe /FI "MODULES eq Valuator.dll" 2>nul
taskkill /F /IM dotnet.exe /FI "MODULES eq RankCalculator.dll" 2>nul
taskkill /F /IM dotnet.exe /FI "MODULES eq EventsLogger.dll" 2>nul

cd /d C:\code\distributed-programming
docker-compose down