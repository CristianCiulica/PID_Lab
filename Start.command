#!/bin/zsh
cd -- "${0:A:h}"
exec /usr/local/share/dotnet/dotnet run --project Framework/Framework.csproj --no-restore
