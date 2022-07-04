﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
# Game Server
EXPOSE 22001
# Channel Service
EXPOSE 22100

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Maple2.Server.Game/Maple2.Server.Game.csproj", "Maple2.Server.Game/"]
COPY ["Maple2.Tools/Maple2.Tools.csproj", "Maple2.Tools/"]
COPY ["Maple2.Model/Maple2.Model.csproj", "Maple2.Model/"]
COPY ["Maple2.Database/Maple2.Database.csproj", "Maple2.Database/"]
COPY ["Maple2.Server.Core/Maple2.Server.Core.csproj", "Maple2.Server.Core/"]
COPY ["Maple2.Script/Maple2.Script.csproj", "Maple2.Script/"]
RUN dotnet restore "Maple2.Server.Game/Maple2.Server.Game.csproj"
COPY . .
WORKDIR "/src/Maple2.Server.Game"
RUN dotnet build "Maple2.Server.Game.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Maple2.Server.Game.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Maple2.Server.Game.dll"]