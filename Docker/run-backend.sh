#!/bin/bash

echo "Pulling the latest code..."
git pull origin main  # Update this to your branch name if needed

echo ✅ After the container is started the Backend will run at:
echo "   📡 HTTP:  http://localhost:5000"
echo "   🔒 HTTPS: https://localhost:5001"

echo "Starting backend using Docker Compose..."
docker-compose up --build


