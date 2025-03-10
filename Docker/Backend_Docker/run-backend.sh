#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}📡 Pulling latest code...${NC}"
git pull origin MohamedIslam


echo -e "${GREEN}✅ Backend will run at:${NC}"
echo -e "   🌐 HTTP:  http://localhost:5080"
echo -e "   🔒 HTTPS: https://localhost:5081"

echo -e "${BLUE}🚀 Starting backend...${NC}"
docker-compose up