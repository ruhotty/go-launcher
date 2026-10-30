using System.Windows;
using System.Windows.Controls;
using GoLauncher.Models;
using GoLauncher.Services;

namespace GoLauncher;

public partial class MainWindow : Window
{
    private readonly SteamLibraryService _library = new();
    private readonly SteamLaunchService _launcher = new();
    private List<SteamGame> _games = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => RefreshLibrary();

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshLibrary();

    private void RefreshLibrary()
    {
        try
        {
            var steamPath = _library.FindSteamPath();
            if (steamPath is null)
            {
                _games = [];
                GamesList.ItemsSource = _games;
                LibraryHeading.Text = "STEAM NOT FOUND";
                StatusText.Text = "Install Steam to use Go Launcher.";
                SteamButton.Content = "Open Steam";
                return;
            }

            _games = _library.GetInstalledGames().ToList();
            ApplyFilter();
            LibraryHeading.Text = $"YOUR INSTALLED GAMES  ·  {_games.Count}";
            StatusText.Text = _library.IsSteamRunning()
                ? "Steam is running · Ready to play"
                : "Steam is not running · It will open when you launch a game";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Couldn't read the Steam library: {ex.Message}";
        }
    }

    private void ApplyFilter()
    {
        var query = SearchBox.Text.Trim();
        GamesList.ItemsSource = string.IsNullOrEmpty(query)
            ? _games
            : _games.Where(g => g.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not SteamGame game) return;
        try
        {
            _launcher.Launch(game);
            StatusText.Text = $"Starting {game.Name} through Steam...";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Couldn't start {game.Name}: {ex.Message}";
        }
    }

    private void SteamButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _launcher.OpenSteam();
            StatusText.Text = "Opening the Steam client...";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Couldn't open Steam: {ex.Message}";
        }
    }
}
