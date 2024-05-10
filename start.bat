@echo off

wt -d "Maple2.Server.World" dotnet run ; nt -d "Maple2.Server.Login" dotnet run ; nt -d "Maple2.Server.Web" dotnet run ; nt -d "Maple2.Server.Game" dotnet run

