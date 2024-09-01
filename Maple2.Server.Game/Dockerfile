FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Game Server
#EXPOSE 20002
# Channel Service
#EXPOSE 21002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Maple2.Server.Game/Maple2.Server.Game.csproj", "Maple2.Server.Game/"]
COPY ["Maple2.Tools/Maple2.Tools.csproj", "Maple2.Tools/"]
COPY ["Maple2.Model/Maple2.Model.csproj", "Maple2.Model/"]
COPY ["Maple2.Database/Maple2.Database.csproj", "Maple2.Database/"]
COPY ["Maple2.Server.Core/Maple2.Server.Core.csproj", "Maple2.Server.Core/"]
RUN dotnet restore -r win-x64 "Maple2.Server.Game/Maple2.Server.Game.csproj"
COPY . .
WORKDIR "/src/Maple2.Server.Game"

FROM build AS publish
RUN dotnet publish -r win-x64 "Maple2.Server.Game.csproj" -c Debug

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Maple2.Server.Game/bin/Debug/net8.0/win-x64/publish .
COPY --from=build /src/Maple2.File.Ingest/Navmeshes /Maple2.File.Ingest/Navmeshes

ENTRYPOINT ["dotnet", "Maple2.Server.Game.dll"]
