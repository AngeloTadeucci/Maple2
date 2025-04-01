@echo off

dotnet build

wt -d "Maple2.Server.World" --title "World Server" dotnet run --no-build ; ^
sp -d "Maple2.Server.Login" --title "Login Server" dotnet run --no-build ; ^
sp -d "Maple2.Server.Web" --title "Web Server" dotnet run --no-build
