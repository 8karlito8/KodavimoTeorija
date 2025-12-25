#!/bin/bash
# Quick start script for Golay Code Docker deployment

echo "==================================================================="
echo "  Golay (23,12,7) Error-Correcting Code - Docker Deployment"
echo "==================================================================="
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Error: Docker is not installed"
    echo "Please install Docker first: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Error: Docker Compose is not installed"
    echo "Please install Docker Compose first"
    exit 1
fi

echo "✓ Docker found: $(docker --version)"
echo "✓ Docker Compose found: $(docker-compose --version)"
echo ""

# Check if ports are available
if lsof -Pi :3000 -sTCP:LISTEN -t >/dev/null 2>&1 ; then
    echo "⚠️  Warning: Port 3000 is already in use"
    echo "   You may need to stop the conflicting service or change the port in docker-compose.yml"
fi

if lsof -Pi :5081 -sTCP:LISTEN -t >/dev/null 2>&1 ; then
    echo "⚠️  Warning: Port 5081 is already in use"
    echo "   You may need to stop the conflicting service or change the port in docker-compose.yml"
fi

echo ""
echo "Building and starting containers..."
echo ""

# Build and start
docker-compose up --build -d

if [ $? -eq 0 ]; then
    echo ""
    echo "==================================================================="
    echo "✅ Application started successfully!"
    echo "==================================================================="
    echo ""
    echo "Frontend (Web UI):  http://localhost:3000"
    echo "Backend API:        http://localhost:5081/golay/matrix-p"
    echo ""
    echo "To view logs:       docker-compose logs -f"
    echo "To stop:            docker-compose down"
    echo ""
    echo "Opening browser in 3 seconds..."
    sleep 3

    # Try to open browser
    if command -v xdg-open &> /dev/null; then
        xdg-open http://localhost:3000
    elif command -v open &> /dev/null; then
        open http://localhost:3000
    elif command -v start &> /dev/null; then
        start http://localhost:3000
    else
        echo "Please open http://localhost:3000 in your browser"
    fi
else
    echo ""
    echo "❌ Error starting containers"
    echo "Check logs with: docker-compose logs"
    exit 1
fi
