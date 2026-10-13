@echo off
set DB_MAIN=localhost:6000
set DB_RU=localhost:6001
set DB_EU=localhost:6002
set DB_ASIA=localhost:6003

echo Starting infra...
cd /d C:\code\distributed-programming
docker-compose up -d
timeout /t 5 /nobreak >nul

echo Starting RankCalculator...
start /b dotnet RankCalculator\bin\Debug\net8.0\RankCalculator.dll
timeout /t 1 /nobreak >nul
start /b dotnet RankCalculator\bin\Debug\net8.0\RankCalculator.dll

echo Starting EventsLogger...
start /b dotnet EventsLogger\bin\Debug\net8.0\EventsLogger.dll
timeout /t 1 /nobreak >nul
start /b dotnet EventsLogger\bin\Debug\net8.0\EventsLogger.dll

echo Starting Valuator...
cd /d C:\code\distributed-programming\Valuator
start /b dotnet bin\Debug\net8.0\Valuator.dll --urls "http://0.0.0.0:5001"
timeout /t 1 /nobreak >nul
start /b dotnet bin\Debug\net8.0\Valuator.dll --urls "http://0.0.0.0:5002"
timeout /t 1 /nobreak >nul
start /b dotnet bin\Debug\net8.0\Valuator.dll --urls "http://0.0.0.0:5003"

echo Starting Nginx...
cd /d C:\code\nginx-1.30.0
start /b nginx.exe -c conf\nginx.conf

echo All started.