using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CompanyTrack.Core.Data;
using CompanyTrack.Core.Services;

namespace CompanyTrack.ViewModels;

/// <summary>Composition root manual: semua service dibuat sekali di sini lalu dibagikan ke ViewModel.</summary>
public class AppServices
{
    public Session Session { get; }
    public Notifier Notifier { get; } = new();
    public ThemeService Theme { get; } = new();
    public AuthService Auth { get; }
    public MasterService Master { get; }
    public PengajuanService Pengajuan { get; }
    public DashboardService Dashboard { get; }

    private AppServices(string databasePath)
    {
        var db = new Db(databasePath);
        var users = new UserRepository(db);
        var master = new MasterRepository(db);
        var pengajuan = new PengajuanRepository(db);
        Session = new Session();
        Auth = new AuthService(users, Session);
        Master = new MasterService(master, Session);
        Pengajuan = new PengajuanService(pengajuan, Session);
        Dashboard = new DashboardService(pengajuan, master);
    }

    public static AppServices Create() => new(ResolveDatabasePath());

    /// <summary>
    /// Urutan: variabel lingkungan COMPANYTRACK_DB, lalu folder data aplikasi pengguna
    /// (Windows: %APPDATA%\CompanyTrack, macOS: ~/Library/Application Support/CompanyTrack).
    /// </summary>
    private static string ResolveDatabasePath()
    {
        string? env = Environment.GetEnvironmentVariable("COMPANYTRACK_DB");
        if (!string.IsNullOrWhiteSpace(env)) return env;
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CompanyTrack", "companytrack.db");
    }
}

/// <summary>Pesan singkat (toast) yang muncul di bagian bawah aplikasi lalu hilang sendiri.</summary>
public partial class Notifier : ObservableObject
{
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isError;

    private int _ticket;

    public void Success(string message) => Show(message, false);
    public void Error(string message) => Show(message, true);

    private async void Show(string message, bool isError)
    {
        int mine = ++_ticket;
        Message = message;
        IsError = isError;
        IsVisible = true;
        await Task.Delay(isError ? 6000 : 3500);
        if (mine == _ticket) IsVisible = false;
    }
}

public class ThemeService
{
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CompanyTrack", "theme.txt");

    public bool IsDark { get; private set; } = true;

    public ThemeService()
    {
        try
        {
            if (File.Exists(FilePath)) IsDark = File.ReadAllText(FilePath).Trim() != "light";
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    public void Set(bool dark)
    {
        IsDark = dark;
        if (Application.Current is not null)
            Application.Current.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, dark ? "dark" : "light");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
