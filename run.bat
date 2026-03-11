@echo off
cd /d "%~dp0"
echo Starting TaskFlow...
echo.

start "TaskFlow Backend" cmd /k "cd /d %~dp0TaskFlow.API && dotnet run"

echo Waiting for backend to start...
timeout /t 6 /nobreak > nul

start "TaskFlow Frontend" cmd /k "cd /d %~dp0taskflow-frontend && npm run dev"

echo.
echo Backend: http://localhost:5000
echo Frontend: http://localhost:5173
echo.
echo Open http://localhost:5173 in your browser.
pause
