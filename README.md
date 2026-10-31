# Go Launcher

A lightweight launcher for games installed through Steam.

## Features
- Finds installed Steam games across Steam library folders.
- Search your installed games.
- Launch games through Steam's official `steam://run/{appid}` protocol.
- Does not ask for or store your Steam password.

## Windows
The Windows app is built with .NET 8 and WPF.

## macOS
A native SwiftUI app is available in `macOS/`. It supports macOS 13 and newer and detects Steam libraries in the standard macOS and Linux locations.

To build locally on a Mac with Xcode Command Line Tools:

```sh
cd macOS
swift build -c release
```

The macOS release workflow creates a universal app for Apple Silicon and Intel when a `macos-v*` tag is pushed. The app is currently unsigned and is not notarized, so macOS may show a security warning on first launch.
