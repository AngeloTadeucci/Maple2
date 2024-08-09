@echo off

wt -d "Maple2.Server.World" --title "World Server" dotnet run ; ^
nt -d "Maple2.Server.Login" --title "Login Server" dotnet run ; ^
nt -d "Maple2.Server.Web" --title "Web Server" dotnet run ; ^
nt -d "Maple2.Server.Game" --title "Game Server" dotnet run
