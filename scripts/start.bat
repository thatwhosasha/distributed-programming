echo Starting RabbitMQ...
docker-compose up -d rabbitmq-pa3

echo Starting RankCalculator instances...
start /b dotnet RankCalculator\bin\Debug\net8.0\RankCalculator.dll
timeout /t 1 /nobreak >nul
start /b dotnet RankCalculator\bin\Debug\net8.0\RankCalculator.dll

echo Starting EventsLogger instances...
start /b dotnet EventsLogger\bin\Debug\net8.0\EventsLogger.dll
timeout /t 1 /nobreak >nul
start /b dotnet EventsLogger\bin\Debug\net8.0\EventsLogger.dll