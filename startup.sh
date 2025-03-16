#!/bin/bash
set -e

# Start nginx in the background
service nginx start

# Run the .NET backend application
cd /app/backend
dotnet GamebookApp.Backend.dll
