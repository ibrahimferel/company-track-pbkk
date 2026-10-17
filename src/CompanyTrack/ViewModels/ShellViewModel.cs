using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public partial class NavItem : ObservableObject
{
    public string Key { get; }
    public string Label { get; }
    public string Icon { get; }
    public IRelayCommand Command { get; }

    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private int _badge;

    public bool HasBadge => Badge > 0;
    partial void OnBadgeChanged(int value) => OnPropertyChanged(nameof(HasBadge));

    public NavItem(string key, string label, string icon, Action<string> navigate)
    {
        Key = key;
        Label = label;
        Icon = icon;
        Command = new RelayCommand(() => navigate(key));
    }
}

public partial class ShellViewModel : ViewModelBase, INavigator
{
    private readonly Action _onLogout;

    public List<NavItem> NavItems { get; }

    [ObservableProperty] private ViewModelBase? _currentPage;
    [ObservableProperty] private bool _isDark;

    public string UserName => S.Session.CurrentUser?.Nama ?? "";
    public string RoleLabel => "Peran: " + S.Session.Role.ToLabel();
    public string Initials => string.Concat((S.Session.CurrentUser?.Nama ?? "?").Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0])).ToUpperInvariant();
    public string ThemeLabel => IsDark ? "Mode gelap" : "Mode terang";

    public ShellViewModel(AppServices services, Action onLogout) : base(services)
    {
        _onLogout = onLogout;
        _isDark = services.Theme.IsDark;
        Action<string> go = key => _ = NavigateAsync(key);
        NavItems = new List<NavItem>
        {
            new("dashboard", "Dashboard", "M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z", go),
            new("pengajuan", "Pengajuan", "M4 4h16v16H4z M8 9h8 M8 13h8 M8 17h5", go),
            new("followup", "Follow-up", "M12 3a9 9 0 1 0 0 18a9 9 0 0 0 0-18z M12 7v5l3 2", go),
            new("perusahaan", "Perusahaan", "M4 21V5l8-2v18 M12 9h8v12 M8 9h1 M8 13h1 M8 17h1 M16 13h1 M16 17h1", go),
            new("periode", "Periode", "M5 5h14a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2z M3 10h18 M8 3v4 M16 3v4", go),
            new("laporan", "Laporan", "M4 20V10 M10 20V4 M16 20v-8 M22 20H2", go),
            new("pengaturan", "Pengaturan", "M4 7h10 M18 7h2 M4 17h2 M10 17h10 M16 5a2 2 0 1 0 0 4a2 2 0 0 0 0-4z M8 15a2 2 0 1 0 0 4a2 2 0 0 0 0-4z", go)
        };
    }

    partial void OnIsDarkChanged(bool value)
    {
        S.Theme.Set(value);
        OnPropertyChanged(nameof(ThemeLabel));
    }

    private ViewModelBase CreatePage(string key) => key switch
    {
        "dashboard" => new DashboardViewModel(S, this),
        "pengajuan" => new PengajuanListViewModel(S, this),
        "followup" => new FollowUpViewModel(S, this),
        "perusahaan" => new PerusahaanViewModel(S),
        "periode" => new PeriodeViewModel(S),
        "laporan" => new LaporanViewModel(S),
        "pengaturan" => new PengaturanViewModel(S),
        _ => new DashboardViewModel(S, this)
    };

    private void SetActive(string key)
    {
        foreach (var n in NavItems) n.IsActive = n.Key == key;
    }

    public async Task NavigateAsync(string key)
    {
        SetActive(key);
        var page = CreatePage(key);
        CurrentPage = page;
        await page.LoadAsync();
        await RefreshBadgeAsync();
    }

    public async Task OpenDetailAsync(int pengajuanId)
    {
        SetActive("pengajuan");
        var page = new PengajuanDetailViewModel(S, this, pengajuanId);
        CurrentPage = page;
        await page.LoadAsync();
        await RefreshBadgeAsync();
    }

    /// <summary>Badge menu Follow-up = jumlah yang terlambat atau jatuh tempo hari ini.</summary>
    public async Task RefreshBadgeAsync()
    {
        try
        {
            var items = await S.Pengajuan.ListFollowUpsAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);
            int n = items.Count(f => FollowUpClassifier.Classify(f.TanggalFollowUp, today) is FollowUpBucket.Terlambat or FollowUpBucket.HariIni);
            NavItems.First(i => i.Key == "followup").Badge = n;
        }
        catch (Exception)
        {
            // Badge hanya pelengkap, kegagalan tidak perlu mengganggu pengguna.
        }
    }

    [RelayCommand]
    private void Logout()
    {
        S.Auth.Logout();
        _onLogout();
    }
}
