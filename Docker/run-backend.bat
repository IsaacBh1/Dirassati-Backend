@echo off
echo 🔄 Pulling the latest code...
git pull origin main

echo ✅ After the container is started the Backend will run at:
echo    📡 HTTP:  http://localhost:5000
echo    🔒 HTTPS: https://localhost:5001

echo 🚀 Starting backend using Docker Compose...
start cmd /k "docker-compose up --build  --force-recreate"

:: Wait a few seconds for the backend to start (adjust if needed)
timeout /t 5 /nobreak >nul

:: Print backend URL

pause