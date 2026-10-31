// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "GoLauncherMac",
    platforms: [.macOS(.v13)],
    products: [
        .executable(name: "GoLauncherMac", targets: ["GoLauncherMac"])
    ],
    targets: [
        .executableTarget(name: "GoLauncherMac")
    ]
)
