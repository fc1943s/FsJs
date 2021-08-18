dotnet tool restore
dotnet paket restore
dotnet build --configuration Release
dotnet paket pack bin/Release --build-config Release --symbols --include-referenced-projects
