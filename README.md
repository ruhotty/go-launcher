# Go Launcher

A lightweight Windows launcher for games already installed through Steam.

## Features
- Finds installed Steam games across Steam library folders.
- Search your installed games.
- Launch games through Steam's official `steam://rungameid/` protocol.
- Does not ask for or store your Steam password.

## Build
Requires the .NET 8 SDK and Windows with the .NET desktop workload.

```powershell
dotnet build src/GoLauncher/GoLauncher.csproj -c Release
dotnet publish src/GoLauncher/GoLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The GitHub Actions workflow creates a Windows ZIP release when a version tag is pushed.
