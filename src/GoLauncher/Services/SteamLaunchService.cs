using System.Diagnostics;
using GoLauncher.Models;

namespace GoLauncher.Services;

public sealed class SteamLaunchService
{
    public void Launch(SteamGame game)
    {
        var info = new ProcessStartInfo($"steam://rungameid/{game.AppId}")
        {
            UseShellExecute = true
        };
        Process.Start(info);
    }

    public void OpenSteam()
    {
        Process.Start(new ProcessStartInfo("steam://open/main") { UseShellExecute = true });
    }
}
