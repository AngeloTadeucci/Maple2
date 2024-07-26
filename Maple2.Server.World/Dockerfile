FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# World Service, Global Service
EXPOSE 21001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Maple2.Server.World/Maple2.Server.World.csproj", "Maple2.Server.World/"]
COPY ["Maple2.Tools/Maple2.Tools.csproj", "Maple2.Tools/"]
COPY ["Maple2.Model/Maple2.Model.csproj", "Maple2.Model/"]
COPY ["Maple2.Database/Maple2.Database.csproj", "Maple2.Database/"]
COPY ["Maple2.Server.Core/Maple2.Server.Core.csproj", "Maple2.Server.Core/"]
RUN dotnet restore -r win-x64 "Maple2.Server.World/Maple2.Server.World.csproj"
COPY . .
WORKDIR "/src/Maple2.Server.World"
RUN dotnet build -r win-x64 "Maple2.Server.World.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish -r win-x64 "Maple2.Server.World.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Maple2.Server.World.dll"]
