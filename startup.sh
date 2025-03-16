#!/bin/bash
set -e

# Create necessary directories with proper permissions
mkdir -p /var/log/nginx
mkdir -p /var/lib/nginx/body
mkdir -p /run

# Set proper permissions
chmod -R 755 /var/log/nginx
chmod -R 755 /var/lib/nginx
chmod 755 /run

# Remove any existing PID files
rm -f /run/nginx.pid
rm -f /var/run/nginx.pid

# Start nginx properly (not as a daemon so container can monitor it)
nginx &
nginx_pid=$!

# Run the .NET backend application
cd /app/backend
dotnet GamebookApp.Backend.dll
