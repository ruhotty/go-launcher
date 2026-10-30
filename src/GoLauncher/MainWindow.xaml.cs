using System.Windows;
using System.Windows.Controls;
using GoLauncher.Models;
using GoLauncher.Services;
namespace GoLauncher;
public partial class MainWindow : Window
{
    private readonly SteamLibraryService _steam = new();
    private List<SteamGame> _games = new();
    public MainWindow() { InitializeComponent(); Loaded += (_, _) => RefreshGames(); }
    private void RefreshGames()
    {
        try { _games = _steam.GetInstalledGames().ToList(); ApplyFilter(); }
        catch (Exception ex) { MessageBox.Show($"Could not read your Steam library.\n{ex.Message}", "Go Launcher", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
    private void ApplyFilter()
    {
        var q = SearchBox.Text.Trim();
        GamesList.ItemsSource = string.IsNullOrEmpty(q) ? _games : _games.Where(g => g.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshGames();
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is string id) {
            try { SteamLaunchService.Launch(id); }
            catch (Exception ex) { MessageBox.Show($"Could not launch game.\n{ex.Message}", "Go Launcher"); }
        }
    }
}