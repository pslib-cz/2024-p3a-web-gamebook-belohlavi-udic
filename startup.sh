#!/bin/bash
set -e

# Make sure nginx directories are writable
mkdir -p /var/log/nginx
mkdir -p /var/lib/nginx
chmod -R 755 /var/log/nginx
chmod -R 755 /var/lib/nginx

# Remove the default pid if it exists
rm -f /var/run/nginx.pid

# Start nginx in the background with proper configuration
nginx -g "daemon off;" &

# Run the .NET backend application
cd /app/backend
dotnet GamebookApp.Backend.dll
