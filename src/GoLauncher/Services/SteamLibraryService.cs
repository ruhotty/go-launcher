using System.Text.RegularExpressions;
using GoLauncher.Models;
using Microsoft.Win32;

namespace GoLauncher.Services;

public sealed class SteamLibraryService
{
    public string? FindSteamPath()
    {
        var candidates = new[]
        {
            Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string,
            Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam")
        };

        return candidates.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p));
    }

    public IReadOnlyList<SteamGame> GetInstalledGames()
    {
        var steamPath = FindSteamPath();
        if (steamPath is null) return [];

        var libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamPath };
        var libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

        if (File.Exists(libraryFile))
        {
            var text = File.ReadAllText(libraryFile);
            foreach (Match match in Regex.Matches(text, @"""path""\s*""([^""]+)"""))
            {
                var path = match.Groups[1].Value.Replace(@"\\", @"\");
                if (Directory.Exists(path)) libraries.Add(path);
            }
        }

        var games = new List<SteamGame>();
        foreach (var library in libraries)
        {
            var steamApps = Path.Combine(library, "steamapps");
            if (!Directory.Exists(steamApps)) continue;

            foreach (var manifest in Directory.EnumerateFiles(steamApps, "appmanifest_*.acf"))
            {
                try
                {
                    var text = File.ReadAllText(manifest);
                    var appId = Regex.Match(text, @"""appid""\s+""?(\d+)""?").Groups[1].Value;
                    var name = Regex.Match(text, @"""name""\s+""([^""]+)""").Groups[1].Value;
                    var installDir = Regex.Match(text, @"""installdir""\s+""([^""]+)""").Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(name)) continue;

                    games.Add(new SteamGame
                    {
                        AppId = appId,
                        Name = name,
                        InstallDirectory = Path.Combine(steamApps, "common", installDir)
                    });
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        return games.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public bool IsSteamRunning() => System.Diagnostics.Process.GetProcessesByName("steam").Length > 0;
}
