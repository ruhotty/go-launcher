using System.Text.RegularExpressions;
using Microsoft.Win32;
using GoLauncher.Models;
namespace GoLauncher.Services;
public sealed class SteamLibraryService
{
    public IReadOnlyList<SteamGame> GetInstalledGames()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var path = key?.GetValue("SteamPath") as string;
            if (!string.IsNullOrWhiteSpace(path)) roots.Add(path.Replace('/', '\\'));
        } catch { }
        roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
        var games = new List<SteamGame>();
        foreach (var root in roots) {
            var libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { root };
            var vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf)) {
                var text = File.ReadAllText(vdf);
                foreach (Match m in Regex.Matches(text, @"""path""\s*""([^""]+)"""))
                    libraries.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
            }
            foreach (var lib in libraries) {
                var folder = Path.Combine(lib, "steamapps");
                if (!Directory.Exists(folder)) continue;
                foreach (var file in Directory.EnumerateFiles(folder, "appmanifest_*.acf")) {
                    try {
                        var data = File.ReadAllText(file);
                        string Get(string key) => Regex.Match(data, $@"""{key}""\s*""([^""]*)""").Groups[1].Value;
                        var id = Get("appid"); var name = Get("name"); var dir = Get("installdir");
                        if (id.Length > 0 && name.Length > 0) games.Add(new SteamGame(id, name, dir));
                    } catch { }
                }
            }
        }
        return games.GroupBy(g => g.AppId).Select(g => g.First()).OrderBy(g => g.Name).ToList();
    }
}