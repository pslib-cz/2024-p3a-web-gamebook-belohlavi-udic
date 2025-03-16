#!/bin/bash
set -e

# Ensure data directory exists with proper permissions
mkdir -p /data/logs
chmod -R 777 /data

# Create necessary nginx directories with proper permissions
mkdir -p /var/log/nginx
mkdir -p /var/lib/nginx/body
mkdir -p /run

# Clean up any existing PID files
rm -f /run/nginx.pid
rm -f /var/run/nginx.pid

# Start nginx in the background
nginx &

# Wait a moment for nginx to start
sleep 2

# Run the .NET backend application
cd /app/backend
dotnet GamebookApp.Backend.dll
