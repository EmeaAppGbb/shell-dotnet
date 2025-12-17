#!/bin/bash
set -e

echo "Building agentic-shell-dotnet services..."

# Build backend
echo "Building backend (agentic-api)..."
cd src/agentic-api
dotnet restore
dotnet build
cd ../..

# Build frontend
echo "Building frontend (agentic-ui)..."
cd src/agentic-ui
npm install
npm run build
cd ../..

echo "✅ All services built successfully!"
