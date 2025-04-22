@echo off
setlocal enabledelayedexpansion

:: Function to check if the previous command succeeded
:check_error
if %errorlevel% neq 0 (
    echo [31m❌ Error: %~1[0m
    exit /b 1
)
goto :eof

:: Navigate to project root
echo [34m📡 Pulling latest code...[0m
cd ..\..
call :check_error "Failed to navigate to project root"

:: Pull latest changes
git pull origin main
call :check_error "Failed to pull latest changes"

:: Return to Docker directory
cd Docker\Backend_Docker
call :check_error "Failed to navigate to Docker directory"

:: Print service URLs
echo [32m✅ Backend will run at:[0m
echo    🌐 HTTP:  http://localhost:5080
echo [33m✅ Email Client will run at:[0m
echo    📧 Web UI: http://localhost:6080

:: Start services
echo [34m🚀 Starting backend...[0m
docker compose up --build
call :check_error "Failed to start Docker services"

endlocal