# TaskFlow - Run both backend and frontend
# No PostgreSQL or Redis needed - uses SQLite and in-memory cache

Write-Host "Starting TaskFlow..." -ForegroundColor Cyan

# Start backend in background
$backend = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "TaskFlow.API" -PassThru -NoNewWindow
Start-Sleep -Seconds 5

# Start frontend
Write-Host "Backend: http://localhost:5000" -ForegroundColor Green
Write-Host "Frontend: http://localhost:5173" -ForegroundColor Green
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Set-Location taskflow-frontend
npm run dev
