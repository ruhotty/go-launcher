import SwiftUI
import AppKit

@main
struct GoLauncherMacApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
                .frame(minWidth: 620, minHeight: 520)
                .preferredColorScheme(.dark)
        }
        .windowStyle(.hiddenTitleBar)
    }
}

struct SteamGame: Identifiable, Hashable {
    let id: String
    let name: String
    let library: String
}

@MainActor
final class LauncherModel: ObservableObject {
    @Published var games: [SteamGame] = []
    @Published var search = ""
    @Published var selectedGame: SteamGame?
    @Published var status = "Steam-games zoeken…"

    var filteredGames: [SteamGame] {
        guard !search.isEmpty else { return games }
        return games.filter { $0.name.localizedCaseInsensitiveContains(search) }
    }

    func refresh() {
        let found = SteamLibraryService.findGames()
        games = found.sorted { $0.name.localizedCaseInsensitiveCompare($1.name) == .orderedAscending }
        status = found.isEmpty
            ? "Geen games gevonden. Controleer of Steam is geïnstalleerd."
            : "\(found.count) geïnstalleerde games gevonden"
        if selectedGame == nil { selectedGame = games.first }
    }

    func launch(_ game: SteamGame) {
        guard let url = URL(string: "steam://run/\(game.id)") else { return }
        NSWorkspace.shared.open(url)
    }
}

struct ContentView: View {
    @StateObject private var model = LauncherModel()

    var body: some View {
        VStack(spacing: 0) {
            HStack(spacing: 10) {
                Image(systemName: "gamecontroller.fill")
                    .font(.system(size: 22, weight: .semibold))
                    .foregroundStyle(.white)
                Text("Go Launcher")
                    .font(.system(size: 22, weight: .bold))
                Spacer()
                Button {
                    model.refresh()
                } label: {
                    Label("Vernieuwen", systemImage: "arrow.clockwise")
                }
                .buttonStyle(.bordered)
            }
            .padding(22)

            HStack {
                Image(systemName: "magnifyingglass").foregroundStyle(.secondary)
                TextField("Zoek een game…", text: $model.search)
                    .textFieldStyle(.plain)
            }
            .padding(12)
            .background(Color.white.opacity(0.07), in: RoundedRectangle(cornerRadius: 10))
            .padding(.horizontal, 22)

            HStack {
                Text("JOUW GAMES")
                    .font(.system(size: 11, weight: .bold))
                    .tracking(1.2)
                    .foregroundStyle(.secondary)
                Spacer()
                Text("\(model.filteredGames.count)")
                    .foregroundStyle(.secondary)
                    .font(.caption)
            }
            .padding(.horizontal, 24)
            .padding(.top, 22)
            .padding(.bottom, 8)

            List(selection: $model.selectedGame) {
                ForEach(model.filteredGames) { game in
                    HStack(spacing: 12) {
                        Image(systemName: "gamecontroller")
                            .font(.system(size: 18))
                            .frame(width: 34, height: 34)
                            .background(Color.white.opacity(0.08), in: RoundedRectangle(cornerRadius: 8))
                        VStack(alignment: .leading, spacing: 3) {
                            Text(game.name).fontWeight(.medium)
                            Text("Steam • App ID \(game.id)")
                                .font(.caption)
                                .foregroundStyle(.secondary)
                        }
                        Spacer()
                        if model.selectedGame?.id == game.id {
                            Image(systemName: "checkmark.circle.fill").foregroundStyle(.green)
                        }
                    }
                    .padding(.vertical, 4)
                    .tag(game)
                    .listRowBackground(model.selectedGame?.id == game.id ? Color.accentColor.opacity(0.16) : Color.clear)
                }
            }
            .listStyle(.plain)

            HStack {
                Text(model.status)
                    .font(.caption)
                    .foregroundStyle(.secondary)
                Spacer()
                Button {
                    if let game = model.selectedGame { model.launch(game) }
                } label: {
                    Label("Play", systemImage: "play.fill")
                        .fontWeight(.semibold)
                        .frame(minWidth: 110)
                }
                .buttonStyle(.borderedProminent)
                .disabled(model.selectedGame == nil)
            }
            .padding(20)
            .background(Color.black.opacity(0.18))
        }
        .background(Color(nsColor: .windowBackgroundColor))
        .onAppear { model.refresh() }
    }
}

enum SteamLibraryService {
    static func findGames() -> [SteamGame] {
        let fm = FileManager.default
        let home = fm.homeDirectoryForCurrentUser
        let candidates = [
            home.appendingPathComponent("Library/Application Support/Steam"),
            home.appendingPathComponent(".steam/steam"),
            home.appendingPathComponent(".local/share/Steam")
        ]

        var roots: [URL] = []
        for root in candidates where fm.fileExists(atPath: root.path) {
            roots.append(root)
        }

        var libraries = Set<URL>()
        for root in roots {
            libraries.insert(root)
            let vdf = root.appendingPathComponent("steamapps/libraryfolders.vdf")
            if let text = try? String(contentsOf: vdf, encoding: .utf8) {
                let pattern = #""path"\s*"((?:\\\\.|[^"])*)""#
                if let regex = try? NSRegularExpression(pattern: pattern) {
                    let ns = text as NSString
                    for match in regex.matches(in: text, range: NSRange(location: 0, length: ns.length)) {
                        guard match.numberOfRanges > 1 else { continue }
                        let raw = ns.substring(with: match.range(at: 1))
                            .replacingOccurrences(of: "\\\\", with: "\\")
                            .replacingOccurrences(of: "\\/", with: "/")
                        libraries.insert(URL(fileURLWithPath: raw))
                    }
                }
            }
        }

        var result: [SteamGame] = []
        var seen = Set<String>()
        for library in libraries {
            let apps = library.appendingPathComponent("steamapps")
            guard let files = try? fm.contentsOfDirectory(at: apps, includingPropertiesForKeys: nil) else { continue }
            for file in files where file.lastPathComponent.hasPrefix("appmanifest_") && file.pathExtension == "acf" {
                guard let contents = try? String(contentsOf: file, encoding: .utf8) else { continue }
                let appID = file.deletingPathExtension().lastPathComponent
                    .replacingOccurrences(of: "appmanifest_", with: "")
                let name = value(for: "name", in: contents)
                guard !name.isEmpty, seen.insert(appID).inserted else { continue }
                result.append(SteamGame(id: appID, name: name, library: library.path))
            }
        }
        return result
    }

    private static func value(for key: String, in text: String) -> String {
        let escaped = NSRegularExpression.escapedPattern(for: key)
        let pattern = #"""# + escaped + #""\s*"((?:\\.|[^"])*)""#
        guard let regex = try? NSRegularExpression(pattern: pattern),
              let match = regex.firstMatch(in: text, range: NSRange(text.startIndex..., in: text)),
              let range = Range(match.range(at: 1), in: text) else { return "" }
        return String(text[range]).replacingOccurrences(of: #"\\""#, with: #"""#)
    }
}
