using CommunityToolkit.Mvvm.ComponentModel;

namespace CompanyTrack.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly AppServices _services;

    public Notifier Notifier => _services.Notifier;

    [ObservableProperty] private ViewModelBase? _current;

    public MainWindowViewModel(AppServices services)
    {
        _services = services;
        ShowLogin();
    }

    private void ShowLogin() => Current = new LoginViewModel(_services, ShowShell);

    private async void ShowShell()
    {
        var shell = new ShellViewModel(_services, ShowLogin);
        Current = shell;
        await shell.NavigateAsync("dashboard");
    }
}
