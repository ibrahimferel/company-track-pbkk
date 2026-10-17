using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CompanyTrack.ViewModels;
using CompanyTrack.Views;

namespace CompanyTrack;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = AppServices.Create();
            RequestedThemeVariant = services.Theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(services) };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
