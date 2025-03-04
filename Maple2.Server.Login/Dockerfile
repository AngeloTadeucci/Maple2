FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Login Server
EXPOSE 20001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Maple2.Server.Login/Maple2.Server.Login.csproj", "Maple2.Server.Login/"]
COPY ["Maple2.Tools/Maple2.Tools.csproj", "Maple2.Tools/"]
COPY ["Maple2.Model/Maple2.Model.csproj", "Maple2.Model/"]
COPY ["Maple2.Database/Maple2.Database.csproj", "Maple2.Database/"]
COPY ["Maple2.Server.Core/Maple2.Server.Core.csproj", "Maple2.Server.Core/"]
RUN dotnet restore "Maple2.Server.Login/Maple2.Server.Login.csproj"
COPY . .
WORKDIR "/src/Maple2.Server.Login"

FROM build AS publish
RUN dotnet publish "Maple2.Server.Login.csproj" -c Debug

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Maple2.Server.Login/bin/Debug/net8.0/publish .

ENTRYPOINT ["dotnet", "Maple2.Server.Login.dll"]
