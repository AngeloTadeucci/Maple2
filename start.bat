@echo off

dotnet build

where wt >nul 2>&1
if %errorlevel%==0 (
    wt -d "Maple2.Server.World" --title "World Server" dotnet run --no-build ; ^
    nt -d "Maple2.Server.Login" --title "Login Server" dotnet run --no-build ; ^
    nt -d "Maple2.Server.Web" --title "Web Server" dotnet run --no-build ; ^
    nt -d "Maple2.Server.Game" --title "Game Server" dotnet run --no-build
) else (
    start "World Server" /d "Maple2.Server.World" cmd /k dotnet run --no-build
    start "Login Server" /d "Maple2.Server.Login" cmd /k dotnet run --no-build
    start "Web Server" /d "Maple2.Server.Web" cmd /k dotnet run --no-build
    start "Game Server" /d "Maple2.Server.Game" cmd /k dotnet run --no-build
)
