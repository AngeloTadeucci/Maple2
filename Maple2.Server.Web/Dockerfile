FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Web Service (HTTP)
EXPOSE 80
# Web Service (HTTPS)
# EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Maple2.Server.Web/Maple2.Server.Web.csproj", "Maple2.Server.Web/"]
COPY ["Maple2.Database/Maple2.Database.csproj", "Maple2.Database/"]
COPY ["Maple2.Server.Core/Maple2.Server.Core.csproj", "Maple2.Server.Core/"]
COPY ["Maple2.Model/Maple2.Model.csproj", "Maple2.Model/"]
COPY ["Maple2.Tools/Maple2.Tools.csproj", "Maple2.Tools/"]
RUN dotnet restore -r win-x64 "Maple2.Server.Web/Maple2.Server.Web.csproj"
COPY . .
WORKDIR "/src/Maple2.Server.Web"

FROM build AS publish
RUN dotnet publish -r win-x64 "Maple2.Server.Web.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Maple2.Server.Web.dll"]
