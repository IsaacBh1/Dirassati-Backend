@echo off
echo 🔄 Pulling the latest code...
git pull origin main

echo ✅ After the container is started the Backend will run at:
echo    📡 HTTP:  http://localhost:5080
echo    🔒 HTTPS: https://localhost:5080

echo 🚀 Starting backend using Docker Compose...
start cmd /k "docker-compose build"

start cmd /k "docker-compose up"

:: Wait a few seconds for the backend to start (adjust if needed)
timeout /t 5 /nobreak >nul

:: Print backend URL

pause