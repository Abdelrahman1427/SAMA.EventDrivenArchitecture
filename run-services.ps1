# Script to run all SAMA services
Write-Host "Starting SAMA Event-Driven Architecture Services..." -ForegroundColor Green

# Start API Gateway
Start-Process -FilePath "dotnet" -ArgumentList "run --project SAMA.API.Gateway" -WindowStyle Normal

Start-Sleep -Seconds 3

# Start Account Service
Start-Process -FilePath "dotnet" -ArgumentList "run --project SAMA.AccountService" -WindowStyle Normal

Start-Sleep -Seconds 2

# Start Notification Service
Start-Process -FilePath "dotnet" -ArgumentList "run --project SAMA.NotificationService" -WindowStyle Normal

Start-Sleep -Seconds 2

# Start Monitoring Service
Start-Process -FilePath "dotnet" -ArgumentList "run --project SAMA.MonitoringService" -WindowStyle Normal

Write-Host "All services started successfully!" -ForegroundColor Green
Write-Host "API Gateway: https://localhost:7001" -ForegroundColor Yellow
Write-Host "Account Service: https://localhost:7002" -ForegroundColor Yellow
Write-Host "Notification Service: https://localhost:7003" -ForegroundColor Yellow
Write-Host "Monitoring Service: https://localhost:7007" -ForegroundColor Yellow