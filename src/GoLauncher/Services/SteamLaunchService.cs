using System.Diagnostics;
namespace GoLauncher.Services;
public static class SteamLaunchService
{
    public static void Launch(string appId)
    {
        Process.Start(new ProcessStartInfo($"steam://rungameid/{appId}") { UseShellExecute = true });
    }
}