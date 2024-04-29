@echo off

wt -d "Maple2.Server.World" dotnet run ; sp -d "Maple2.Server.Login" dotnet run ; sp -d "Maple2.Server.Web" dotnet run
