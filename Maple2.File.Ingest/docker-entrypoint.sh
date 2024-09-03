#!/bin/bash

if [ ! -f /root/.dotnet/tools/dotnet-ef ]; then
    echo -n 'Installing dotnet EF... '
    dotnet tool install --global dotnet-ef 2>&1 > /dev/null
    echo 'done'
fi

export PATH="$PATH:/root/.dotnet/tools"

cd /app/Maple2.File.Ingest

dotnet run