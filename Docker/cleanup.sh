#!/bin/bash
echo "Stopping and removing containers..."
docker-compose down -v
echo "Removing old images..."
docker system prune -f
