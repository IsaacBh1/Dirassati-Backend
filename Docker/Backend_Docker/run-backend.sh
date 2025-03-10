#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to check if command succeeded
check_error() {
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Error: $1${NC}"
        exit 1
    fi
}

# Navigate to project root
echo -e "${BLUE}📡 Pulling latest code...${NC}"
cd ../../
check_error "Failed to navigate to project root"

# Pull latest changes
git pull origin main
check_error "Failed to pull latest changes"

# Return to Docker directory
cd Docker/Backend_Docker
check_error "Failed to navigate to Docker directory"

# Print service URLs
echo -e "${GREEN}✅ Backend will run at:${NC}"
echo -e "   🌐 HTTP:  http://localhost:5080"
echo -e "${YELLOW}✅ Email Client will run at:${NC}"
echo -e "   📧 Web UI: http://localhost:6080"

# Start services
echo -e "${BLUE}🚀 Starting backend...${NC}"
docker compose up --build
check_error "Failed to start Docker services"